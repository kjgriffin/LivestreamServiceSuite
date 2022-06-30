using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Clients.Execution;
using DVIPProtocol.Protocol.Lib.Command.CamCTRL;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;
using DVIPProtocol.Protocol.Lib.Inquiry;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using log4net;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CameraDriver
{
    internal class RobustCamServer : IRobustCamServer
    {
        private ConcurrentDictionary<string, IRobustClient> m_clients = new ConcurrentDictionary<string, IRobustClient>();
        private ConcurrentQueue<Action> m_dispatchCommands = new ConcurrentQueue<Action>();
        private ManualResetEvent m_workAvailable = new ManualResetEvent(false);
        private CancellationTokenSource m_cancel = new CancellationTokenSource();
        private Thread? m_workerThread;
        private ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>> m_presets = new ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>>();

        ILog? m_log;

        public event CameraPresetSaved OnPresetSavedSuccess;

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
        public void Cam_RunZoomChrip(string cnameID, int direction, int chirps)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID))
            {
                return;
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing zoom chrip job");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running zoom chrip job");
                if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                {
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chrip [{direction}] for camera {cnameID}");
                    var zoom = CMD_Zoom_Variable.Create(direction == -1 ? ZoomDir.WIDE : direction == 1 ? ZoomDir.TELE : ZoomDir.STOP, 0x00);
                    var stop = CMD_Zoom_Std.Create(ZoomDir.STOP);

                    long sentChirps = 0;

                    RobustSequence rcmd = new RobustSequence
                    {
                        First = zoom.PackagePayload(),
                        Second = stop.PackagePayload(),
                        DelayMS = 200, // TODO: figure out what chirp interval makes sense
                        // ok- so it seeems that we'd like to do this in small increments, but the ~minimum reliable time to gaurantee completed matters
                        // so we'll sacrafice resolution for time
                        // that, or we'll need to be able to pipe direct times in rather than chirps
                        // or convert chips to a time unit
                        // and then specify a minimum we can gaurantee
                        // TODO: if variable zoom works then figure out which speed makes sense. Slower should be more tolerant to timing slop, but perhaps is too costly to run real-time
                        OnCompleted = (int attempts, byte[] resp) =>
                        {
                            Task.Run(() =>
                            {
                                var after = Interlocked.Read(ref sentChirps);
                                if (after != -1)
                                {
                                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chirp ({after} of {chirps}) [{direction}] for camera {cnameID} COMPLETED");
                                    if (Interlocked.Increment(ref sentChirps) == chirps)
                                    {
                                        OnWorkCompleted?.Invoke(cnameID, "CHIRP", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"after {attempts} tries");
                                    }
                                }
                            });
                        },
                        OnFail = (int attempts) =>
                        {
                            Task.Run(() =>
                            {
                                var after = Interlocked.Exchange(ref sentChirps, -1);
                                m_log?.Info($"[{Thread.CurrentThread.Name}] requesting zoom chrip [{direction}] for camera {cnameID} on chirp {after} of {chirps} FAILED");
                                OnWorkFailed?.Invoke(cnameID, "CHIRP", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP", $"on chirp {after} of {chirps}");
                            });
                        },
                        RetryAttempts = 3,
                    };

                    OnWorkStarted?.Invoke(cnameID, "CHIRP", $"x{chirps}", direction == -1 ? "WIDE" : direction == 1 ? "TELE" : "STOP");

                    for (int i = 0; i < chirps; i++)
                    {
                        client?.DoRobustWork(rcmd);
                    }
                }
            });
            m_workAvailable.Set();
        }


        public void Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(presetName) || speed < 0 || speed > 0x18) // might actually be 18 dec or 0x12
            {
                return;
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] enqueing recall preset job");
            m_dispatchCommands.Enqueue(() =>
            {
                m_log?.Info($"[{Thread.CurrentThread.Name}] running recall preset job");
                if (m_presets.TryGetValue(cnameID, out var presets))
                {
                    if (presets?.TryGetValue(presetName, out var preset) == true)
                    {
                        if (m_clients.TryGetValue(cnameID, out IRobustClient? client))
                        {
                            m_log?.Info($"[{Thread.CurrentThread.Name}] requesting abs position [{presetName}] for camera {cnameID}");
                            var cmd = CMD_PanTiltAbsPos.CMD_ABS_POS(preset.Pan, preset.Tilt, speed);
                            RobustCommand rcmd = new RobustCommand
                            {
                                Data = cmd.PackagePayload(),
                                OnCompleted = (int attempts, byte[] resp) =>
                                {
                                    Task.Run(() =>
                                    {
                                        m_log?.Info($"[{Thread.CurrentThread.Name}] requesting abs position [{presetName}] for camera {cnameID} COMPLETED");
                                        OnWorkCompleted?.Invoke(cnameID, "DRIVE.ABSPOS", presetName, $"after {attempts} tries");
                                    });
                                },
                                OnFail = (int attempts) =>
                                {
                                    Task.Run(() =>
                                    {
                                        m_log?.Info($"[{Thread.CurrentThread.Name}] requesting abs position [{presetName}] for camera {cnameID} FAILED");
                                        OnWorkFailed?.Invoke(cnameID, "DRIVE.ABSPOS", presetName);
                                    });
                                },
                                RetryAttempts = 3,
                            };
                            OnWorkStarted?.Invoke(cnameID, "DRIVE.ABSPOS", presetName);
                            client?.DoRobustWork(rcmd);
                        }
                    }
                }
            });
            m_workAvailable.Set();
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
                                    if (Internal_UpdatePreset(cnameID, presetName, preset))
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
            Internal_UpdatePreset(cnameID, presetName, preset);
        }

        private bool Internal_UpdatePreset(string cnameID, string presetName, RESP_PanTilt_Position pos)
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

        public List<(string camName, IPEndPoint endpoint)> GetClientConfig()
        {
            return m_clients?.Select(x => (x.Key, x.Value.Endpoint)).ToList() ?? new List<(string, IPEndPoint)>();
        }
    }


}
