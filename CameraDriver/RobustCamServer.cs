using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Clients.Execution;
using DVIPProtocol.Protocol.ControlCommand.Cmd.PanTiltDrive;
using DVIPProtocol.Protocol.Lib.Command.CamCTRL;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using log4net;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace CameraDriver
{

    public class ZoomProgram
    {

        [System.Text.Json.Serialization.JsonConstructor]
        public ZoomProgram(int ZoomMs, string Mode)
        {
            this.ZoomMS = ZoomMs;
            this.Mode = Mode;
        }

        public bool IsValid()
        {
            return ZoomMS > 0 && ZoomMS < 10000 && (Mode == "WIDE" || Mode == "TELE" || Mode == "STOP");
        }

        public int ZoomMS { get; set; } = 0;
        public string Mode { get; set; } = "STOP";

        internal int Direction()
        {
            switch (Mode.ToUpper())
            {
                case "WIDE":
                    return -1;
                case "TELE":
                    return 1;
                default:
                    return 0;
            }
        }
    }

    internal class RobustCamServer : IRobustCamServer
    {
        private ConcurrentDictionary<string, IRobustClient> m_clients = new ConcurrentDictionary<string, IRobustClient>();
        private ConcurrentQueue<Action> m_dispatchCommands = new ConcurrentQueue<Action>();
        private ManualResetEvent m_workAvailable = new ManualResetEvent(false);
        private CancellationTokenSource m_cancel = new CancellationTokenSource();
        private Thread? m_workerThread;
        private ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>> m_presets = new ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>>();
        private ConcurrentDictionary<string, Dictionary<string, ZoomProgram>> m_zooms = new ConcurrentDictionary<string, Dictionary<string, ZoomProgram>>();

        ILog? m_log;

        public event CameraPresetSaved OnPresetSavedSuccess;
        public event CameraZoomSaved OnZoomSavedSuccess;

        public event RobustReport OnWorkStarted;
        public event RobustReport OnWorkCompleted;
        public event RobustReport OnWorkFailed;

        internal RobustCamServer(ILog log)
        {
            m_log = log;
        }

        private void Internal_Loop()
        {

            m_log?.Info($"[{Thread.CurrentThread.Name}] Started.");
#if DEBUG
            Debug.WriteLine($"[{Thread.CurrentThread.Name}] Started.");
#endif

            while (!m_cancel.IsCancellationRequested)
            {
                // execute all work
                while (m_dispatchCommands.TryDequeue(out Action? work))
                {
                    if (!m_cancel.IsCancellationRequested)
                    {
                        m_log?.Info($"[{Thread.CurrentThread.Name}] Executing work.");
                        work?.Invoke();
                    }
                }
                m_workAvailable.Reset();

                // wait for work
                if (!m_cancel.IsCancellationRequested)
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] waiting for work...");
                    WaitHandle.WaitAny(new WaitHandle[] { m_cancel.Token.WaitHandle, m_workAvailable });
                }
            }

            // cleanup
            m_cancel.Dispose();
            m_workAvailable.Dispose();
        }


        public Guid Cam_RunDriveProgram(string cnameID, PanTiltDirection dir, byte speed, uint msDriveTime)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID))
            {
                return Guid.Empty;
            }
            Guid reqId = Guid.NewGuid();

            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing cam drive job {reqId}");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running cam drive job {reqId}");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting drive [{dir}] for camera {cnameID}");

                    var drive = CMD_PanTiltDrive.UpDownLeftRight(dir, speed);
                    var stop = CMD_Zoom_Std.Create(ZoomDir.STOP);

                    RobustCommand rcmd = new RobustCommand
                    {
                        Data = drive.PackagePayload(),
                        OnCompleted = (x, y) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting drive ({msDriveTime}ms) [{dir}] for camera {cnameID} COMPLETED");
                                OnWorkCompleted?.Invoke(cnameID, "DRIVE", $"{dir} @{msDriveTime}ms", $"after 1 tries", reqId.ToString());
                            });
                        },
                        OnFail = (x) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting drive [{dir}] for camera {cnameID} of {msDriveTime}ms FAILED");
                                OnWorkFailed?.Invoke(cnameID, "DRIVE", $"{dir} @{msDriveTime}ms", $"of {msDriveTime}ms", reqId.ToString());
                            });
                        },
                        RetryAttempts = 1,
                        IgnoreResponse = true,
                    };

                    /*
                    RobustSequence rcmd = new RobustSequence
                    {
                        First = drive.PackagePayload(),
                        Second = stop.PackagePayload(),
                        DelayMS = (int)Math.Clamp(msDriveTime, 0, TimeSpan.FromSeconds(10).Milliseconds),

                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting drive ({msDriveTime}ms) [{dir}] for camera {cnameID} COMPLETED");
                                OnWorkCompleted?.Invoke(cnameID, "DRIVE", $"{dir} @{msDriveTime}ms", $"after {attempts} tries", reqId.ToString());
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting drive [{dir}] for camera {cnameID} of {msDriveTime}ms FAILED");
                                OnWorkFailed?.Invoke(cnameID, "DRIVE", $"{dir} @{msDriveTime}ms", $"of {msDriveTime}ms", reqId.ToString());
                            });
                        },
                        RetryAttempts = 1,
                    };
                    */


                    OnWorkStarted?.Invoke(cnameID, "DRIVE", dir.ToString(), reqId.ToString());

                    client?.DoRobustWork(rcmd);

                }
            });
            m_workAvailable.Set();
            return reqId;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cnameID"></param>
        /// <param name="direction">-1 WIDE / 1 TELE / 0 STOP</param>
        public void Cam_RunZoomProgram(string cnameID, int direction)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID))
            {
                return;
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing zoom program job");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running zoom program job");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom program [{direction}] for camera {cnameID}");
                    var zoom = CMD_Zoom_Variable.Create(direction == -1 ? ZoomDir.WIDE : direction == 1 ? ZoomDir.TELE : ZoomDir.STOP, 0x07);
                    var stop = CMD_Zoom_Std.Create(ZoomDir.STOP);
                    RobustSequence rcmd = new RobustSequence
                    {
                        First = zoom.PackagePayload(),
                        Second = stop.PackagePayload(),
                        DelayMS = 3800, // emperical evidence suggests at fastest zoom speed ~ 3.5 seconds
                        Reset = stop.PackagePayload(), // try to gaurantee zoom in a stopped state even if we fail
                        ResetDelayMS = 0,
                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom program [{direction}] for camera {cnameID} COMPLETED");
                                OnWorkCompleted?.Invoke(cnameID, "ZOOM", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"after {attempts} tries");
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom program [{direction}] for camera {cnameID} FAILED");
                                OnWorkFailed?.Invoke(cnameID, "ZOOM", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP");
                            });
                        },
                        RetryAttempts = 3,
                    };
                    OnWorkStarted?.Invoke(cnameID, "ZOOM", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP");
                    client?.DoRobustWork(rcmd);
                }
            });
            m_workAvailable.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cnameID"></param>
        /// <param name="direction">-1 WIDE / 1 TELE / 0 STOP</param>
        /// <param name="duration">time in MS to run zoom for. Note: 200ms seems to be the smallest gauranteed interval</param>
        public Guid Cam_RunZoomChrip_RELATIVE(string cnameID, int direction, int duration, Guid _reqid = default(Guid))
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID))
            {
                return Guid.Empty;
            }
            Guid reqId;
            if (_reqid == Guid.Empty)
            {
                reqId = Guid.NewGuid();
            }
            else
            {
                reqId = _reqid;
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing zoom chrip job {reqId}");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running zoom chrip job {reqId}");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chrip [{direction}] for camera {cnameID}");
                    //var setup = CMD_Zoom_Variable.Create(direction == -1 ? ZoomDir.TELE : direction == 1 ? ZoomDir.WIDE : ZoomDir.STOP, 0x07);
                    var zoom = CMD_Zoom_Variable.Create(direction == -1 ? ZoomDir.WIDE : direction == 1 ? ZoomDir.TELE : ZoomDir.STOP, 0x00);
                    var stop = CMD_Zoom_Std.Create(ZoomDir.STOP);

                    RobustSequence rcmd = new RobustSequence
                    {
                        First = zoom.PackagePayload(),
                        Second = stop.PackagePayload(),
                        DelayMS = duration, // note it seems that 200ms is about the smallest time we can gaurantee reliably
                                            // Setup full program sequence
                                            // automatically determine the setup sequence required
                                            // if we're going in, then the only initial state that makes sense is from full wide
                                            // likewise going out you need to start all the way in
                                            // use the stop command as a reset.....
                        Setup = null, // setup.PackagePayload(),
                        SetupDelayMS = 0,
                        // TODO: do we *really* need to stop the zoom in once we're maxed in, or can we let it just change direction?
                        Reset = stop.PackagePayload(),
                        ResetDelayMS = 0,

                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chirp ({duration}ms) [{direction}] for camera {cnameID} COMPLETED");
                                OnWorkCompleted?.Invoke(cnameID, "CHIRP", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"after {attempts} tries", reqId.ToString());
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chrip [{direction}] for camera {cnameID} of {duration}ms FAILED");
                                OnWorkFailed?.Invoke(cnameID, "CHIRP", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"of {duration}ms", reqId.ToString());
                            });
                        },
                        RetryAttempts = 1,
                        IgnoreALLResponse = true,
                    };

                    OnWorkStarted?.Invoke(cnameID, "CHIRP", $"{duration}ms", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", reqId.ToString());

                    client?.DoRobustWork(rcmd);
                }
            });
            m_workAvailable.Set();
            return reqId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cnameID"></param>
        /// <param name="direction">-1 WIDE / 1 TELE / 0 STOP</param>
        /// <param name="duration">time in MS to run zoom for. Note: 200ms seems to be the smallest gauranteed interval</param>
        public Guid Cam_RunZoomChrip(string cnameID, int direction, int duration, Guid _reqid = default(Guid))
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID))
            {
                return Guid.Empty;
            }
            Guid reqId;
            if (_reqid == Guid.Empty)
            {
                reqId = Guid.NewGuid();
            }
            else
            {
                reqId = _reqid;
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing zoom chrip job {reqId}");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running zoom chrip job {reqId}");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chrip [{direction}] for camera {cnameID}");
                    var setup = CMD_Zoom_Variable.Create(direction == -1 ? ZoomDir.TELE : direction == 1 ? ZoomDir.WIDE : ZoomDir.STOP, 0x07);
                    var zoom = CMD_Zoom_Variable.Create(direction == -1 ? ZoomDir.WIDE : direction == 1 ? ZoomDir.TELE : ZoomDir.STOP, 0x00);
                    var stop = CMD_Zoom_Std.Create(ZoomDir.STOP);

                    RobustSequence rcmd = new RobustSequence
                    {
                        First = zoom.PackagePayload(),
                        Second = stop.PackagePayload(),
                        DelayMS = duration, // note it seems that 200ms is about the smallest time we can gaurantee reliably
                                            // Setup full program sequence
                                            // automatically determine the setup sequence required
                                            // if we're going in, then the only initial state that makes sense is from full wide
                                            // likewise going out you need to start all the way in
                                            // use the stop command as a reset.....
                        Setup = setup.PackagePayload(),
                        SetupDelayMS = 3500,
                        // TODO: do we *really* need to stop the zoom in once we're maxed in, or can we let it just change direction?
                        Reset = stop.PackagePayload(),
                        ResetDelayMS = 0,

                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chirp ({duration}ms) [{direction}] for camera {cnameID} COMPLETED");
                                OnWorkCompleted?.Invoke(cnameID, "CHIRP", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"after {attempts} tries", reqId.ToString());
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chrip [{direction}] for camera {cnameID} of {duration}ms FAILED");
                                OnWorkFailed?.Invoke(cnameID, "CHIRP", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"of {duration}ms", reqId.ToString());
                            });
                        },
                        RetryAttempts = 2,
                    };

                    OnWorkStarted?.Invoke(cnameID, "CHIRP", $"{duration}ms", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", reqId.ToString());

                    client?.DoRobustWork(rcmd);
                }
            });
            m_workAvailable.Set();
            return reqId;
        }


        public Guid Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(presetName) || speed < 0 || speed > 0x18) // might actually be 18 dec or 0x12
            {
                return Guid.Empty;
            }
            Guid reqId = Guid.NewGuid();
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing recall preset job {reqId}");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running recall preset job {reqId}");
                if (m_presets.TryGetValue(cnameID, out var presets))
                {
                    if (presets?.TryGetValue(presetName, out var preset) == true)
                    {
                        if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                        {
                            var cmd = CMD_PanTiltAbsPos.CMD_ABS_POS(preset.Pan, preset.Tilt, speed);
                            RobustCommand rcmd = new RobustCommand
                            {
                                Data = cmd.PackagePayload(),
                                OnCompleted = (int attempts, byte[] resp) =>
                                {
                                    Task.Run(() =>
                                    {
                                        OnWorkCompleted?.Invoke(cnameID, "DRIVE.ABSPOS", presetName, $"after {attempts} tries", reqId.ToString());
                                    });
                                },
                                OnFail = (int attempts) =>
                                {
                                    Task.Run(() =>
                                    {
                                        OnWorkFailed?.Invoke(cnameID, "DRIVE.ABSPOS", presetName, reqId.ToString());
                                    });
                                },
                                RetryAttempts = 3,
                            };
                            OnWorkStarted?.Invoke(cnameID, "DRIVE.ABSPOS", presetName, reqId.ToString());
                            client?.DoRobustWork(rcmd);
                        }
                    }
                }
            });
            m_workAvailable.Set();
            return reqId;
        }

        public Guid Cam_RecallZoomPresetPosition(string cnameID, string presetName)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(presetName))
            {
                return Guid.Empty;
            }
            Guid reqId = Guid.NewGuid();
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing recall zoom preset job {reqId}");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running recall zoom preset job {reqId}");
                if (m_zooms.TryGetValue(cnameID, out var zooms))
                {
                    if (zooms?.TryGetValue(presetName, out var zoom) == true)
                    {
                        if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                        {
                            Cam_RunZoomChrip(cnameID, zoom.Direction(), zoom.ZoomMS, reqId);
                        }
                    }
                }
            });
            m_workAvailable.Set();
            return reqId;
        }

        public void Cam_SaveCurentPosition(string cnameID, string presetName)
        {
            m_log?.Info($"[{Thread.CurrentThread.Name}] enquing save position job");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running save position job");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    var cmd = INQ_PanTilt_Position.Create();
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting pantilt position inquiry");

                    var onfail = (string reason) =>
                    {
                        Task.Run(() =>
                        {
                            m_log?.Info($"[{Thread.CurrentThread.Name}] attempted to get position of cam {cnameID} FAILED");
                            OnWorkFailed?.Invoke(cnameID, "GET.ABSPOS", presetName, reason);
                        });
                    };

                    RobustCommand rcmd = new RobustCommand
                    {
                        Data = cmd.PackagePayload(),
                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                try
                                {
                                    m_log?.Info($"[{Thread.CurrentThread.Name}] parsing pantilt position inquiry response");
                                    var preset = cmd.Parse(resp) as RESP_PanTilt_Position;
                                    if (Internal_UpdateCamPreset(cnameID, presetName, preset))
                                    {
                                        OnWorkCompleted?.Invoke(cnameID, "GET.ABSPOS", presetName, $"after {attempts} tries");
                                    }
                                    else
                                    {
                                        onfail("invalid response");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    m_log?.Info($"[{Thread.CurrentThread.Name}] failed while parsing pantilt position inquiry response {ex}");
                                    onfail("exception"); // perhaps this is wrong?
                                }
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            onfail("max attempts");
                        },
                        RetryAttempts = 3,
                    };

                    OnWorkStarted?.Invoke(cnameID, "GET.ABSPOS", presetName);
                    client?.DoRobustWork(rcmd);
                }
            });
            m_workAvailable.Set();
        }



        public void Cam_SaveRawPosition(string cnameID, string presetName, RESP_PanTilt_Position preset)
        {
            //m_dispatchCommands.Enqueue(() => Internal_UpdatePreset(cnameID, presetName, preset));
            //m_workAvailable.Set();
            Internal_UpdateCamPreset(cnameID, presetName, preset);
        }

        private bool Internal_UpdateCamPreset(string cnameID, string presetName, RESP_PanTilt_Position pos)
        {
            // assign it
            if (pos.Valid)
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] updating preset position {presetName} for cam {cnameID}");
                m_presets.AddOrUpdate(cnameID, new Dictionary<string, RESP_PanTilt_Position> { [presetName] = pos }, (cid, pdict) =>
                {
                    pdict[presetName] = pos;
                    return pdict;
                });
                OnPresetSavedSuccess?.Invoke(cnameID, presetName);
                return true;
            }
            return false;
        }
        private bool Internal_UpdateZoomPreset(string cnameID, string presetName, int zlevel, string mode)
        {
            // assign it
            string _mode = mode.ToUpper();
            ZoomProgram pst = new ZoomProgram(zlevel, mode);
            // validate the command
            if (pst.IsValid())
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] updating zoom position {presetName} for cam {cnameID}");
                m_zooms.AddOrUpdate(cnameID, new Dictionary<string, ZoomProgram> { [presetName] = pst }, (cid, pdict) =>
                {
                    pdict[presetName] = pst;
                    return pdict;
                });
                OnZoomSavedSuccess?.Invoke(cnameID, presetName);
                return true;
            }
            return false;
        }

        public void StartCamClient(string cnameID, IPEndPoint endpoint)
        {
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing start cam job");
            m_dispatchCommands.Enqueue(() => Internal_StartCamClient(cnameID, endpoint));
            m_workAvailable.Set();
            //Internal_StartCamClient(cnameID, endpoint);
        }

        private void Internal_StartCamClient(string cnameID, IPEndPoint endpoint)
        {
            m_log?.Info($"[{Thread.CurrentThread.Name}] running start cam job for cam {cnameID}");
            if (m_clients.ContainsKey(cnameID))
            {
                // stop first so we can restart
                m_log?.Info($"[{Thread.CurrentThread.Name}] stopping running cam {cnameID}");
                Internal_StopCamClient(cnameID);
            }
            IRobustClient client = new RobustTCPClient(endpoint, m_log);
            m_log?.Info($"[{Thread.CurrentThread.Name}] starting running cam {cnameID}");
            client.Init();
            m_clients.TryAdd(cnameID, client);
            OnWorkCompleted?.Invoke(cnameID, "START.client", "");
        }

        public void StopCamClient(string cnameID)
        {
            m_dispatchCommands.Enqueue(() => Internal_StopCamClient(cnameID));
            m_workAvailable.Set();
            //Internal_StopCamClient(cnameID);
        }

        private void Internal_StopCamClient(string cnameID)
        {
            if (!string.IsNullOrEmpty(cnameID) && m_clients.ContainsKey(cnameID))
            {
                if (m_clients.TryRemove(cnameID, out IRobustClient? client))
                {
                    client?.Stop();
                    client = null;
                    OnWorkCompleted?.Invoke(cnameID, "STOP.client", "");
                }
            }
        }
        public void StopAllCamClients()
        {
            m_log?.Info($"[{Thread.CurrentThread.Name}] stopping all cam clients");
            foreach (var client in m_clients.Values)
            {
                client?.Stop();
            }
            m_clients.Clear();
        }

        public void ClearAllPresets()
        {
            m_log?.Info($"[{Thread.CurrentThread.Name}] clearing all presets");
            m_presets.Clear();
        }

        public void Start()
        {
            if (m_workerThread != null)
            {
                //.... hhmmm bad 
            }
            m_workerThread = new Thread(Internal_Loop);
            m_workerThread.IsBackground = true;
            m_workerThread.Name = "SimpleCamServer-WorkerThread";
            m_workerThread.Start();
        }

        public void Shutdown()
        {
            m_cancel.Cancel();
            // TODO: make sure all commands are finished executing before we kill everything...
            // but for now hard kill
            foreach (var client in m_clients)
            {
                client.Value?.Stop();
            }
        }

        public void RemovePreset(string cnameID, string presetName)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(presetName))
            {
                return;
            }

            m_presets.TryGetValue(cnameID, out var presets);
            if (presets?.ContainsKey(presetName) == true)
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] removeing preset {presetName} for cam {cnameID}");
                presets.Remove(presetName);
            }
        }

        public List<string> GetClientNamesWithPresets()
        {
            return m_presets.Keys.ToList() ?? new List<string>();
        }

        public Dictionary<string, RESP_PanTilt_Position> GetKnownPresetsForClient(string cnameID)
        {
            if (string.IsNullOrEmpty(cnameID))
            {
                return new Dictionary<string, RESP_PanTilt_Position>();
            }
            if (m_presets.TryGetValue(cnameID, out var presets))
            {
                if (presets != null)
                {
                    return new Dictionary<string, RESP_PanTilt_Position>(presets);
                }
            }
            return new Dictionary<string, RESP_PanTilt_Position>();
        }

        public Dictionary<string, ZoomProgram> GetKnownZoomPresetsForClient(string cnameID)
        {
            if (string.IsNullOrEmpty(cnameID))
            {
                return new Dictionary<string, ZoomProgram>();
            }
            if (m_zooms.TryGetValue(cnameID, out var presets))
            {
                if (presets != null)
                {
                    return new Dictionary<string, ZoomProgram>(presets);
                }
            }
            return new Dictionary<string, ZoomProgram>();
        }


        public List<(string camName, IPEndPoint endpoint)> GetClientConfig()
        {
            return m_clients?.Select(x => (x.Key, x.Value.Endpoint)).ToList() ?? new List<(string, IPEndPoint)>();
        }

        void ISimpleCamServer.Cam_RecallPresetPosition(string cnameID, string presetName, byte speed)
        {
            Cam_RecallPresetPosition(cnameID, presetName, speed);
        }

        public void Cam_SaveZoomPresetProgram(string cnameID, string presetName, int zlevel, string mode)
        {
            m_log?.Info($"[{Thread.CurrentThread.Name}] enquing save zoom job");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running save zoom job");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    if (Internal_UpdateZoomPreset(cnameID, presetName, zlevel, mode))
                    {
                        OnWorkCompleted?.Invoke(cnameID, "SAVE.ZOOM", presetName);
                    }
                    else
                    {
                        OnWorkFailed?.Invoke(cnameID, "SAVE.ZOOM", presetName);
                    }
                }
            });
            m_workAvailable.Set();
        }

        public Guid Cam_RunMove_RELATIVE(string cnameID, int dirX, int dirY, int speedX, int speedY, Guid _rid = default)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID))
            {
                return Guid.Empty;
            }
            Guid reqId;
            if (_rid == Guid.Empty)
            {
                reqId = Guid.NewGuid();
            }
            else
            {
                reqId = _rid;
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing move relative job {reqId}");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running move relative job {reqId}");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting move relative [X: {dirX} @{speedX}] [Y: {dirY} @{speedY}] for camera {cnameID}");

                    var dir = PanTitlDriveDirectionBuilder.BuildDir(dirX, dirY);
                    var move = PanTiltDrive_Direction_Command.Create(dir, (byte)speedX, (byte)speedY);
                    var stop = PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Stop, 0, 0);

                    string dstr = $"{(dirX == -1 ? "LEFT" : dirX == 1 ? "RIGHT" : "")} @{speedX}] [Y: {(dirY == -1 ? "UP" : dirY == 1 ? "DOWN" : "")} @{speedY}]";

                    RobustCommand rcmd = new RobustCommand
                    {
                        Data = move.PackagePayload(),
                        IgnoreResponse = true,
                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting move relative [X: {dirX} @{speedX}] [Y: {dirY} @{speedY}] for camera {cnameID} COMPLETED");
                                OnWorkCompleted?.Invoke(cnameID, "MOVE", dstr, $"after {attempts} tries", reqId.ToString());
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            Task.Run(() =>
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting move relative [X: {dirX} @{speedX}] [Y: {dirY} @{speedY}] for camera {cnameID} FAILED");
                                OnWorkFailed?.Invoke(cnameID, "MOVE", dstr, reqId.ToString());
                            });
                        },
                        RetryAttempts = 2,
                    };

                    OnWorkStarted?.Invoke(cnameID, "MOVE", dstr, reqId.ToString());

                    client?.DoRobustWork(rcmd);
                }
            });
            m_workAvailable.Set();
            return reqId;

        }
    }


}
