using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using log4net;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace CameraDriver
{
    internal class SimpleCamServer : ISimpleCamServer
    {
        private ConcurrentDictionary<string, IFullClient> m_clients = new ConcurrentDictionary<string, IFullClient>();
        private ConcurrentQueue<Action> m_dispatchCommands = new ConcurrentQueue<Action>();
        private ManualResetEvent m_workAvailable = new ManualResetEvent(false);
        private CancellationTokenSource m_cancel = new CancellationTokenSource();
        private Thread? m_workerThread;
        private ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>> m_presets = new ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>>();

        private bool m_fakeClients = false;

        ILog? m_log;

        public event CameraPresetSaved OnPresetSavedSuccess;

        internal SimpleCamServer(ILog log, bool useFakeClients = false)
        {
            m_log = log;
            m_fakeClients = useFakeClients;
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


        public void Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10)
        {
            if (string.IsNullOrEmpty(cnameID) || string.IsNullOrEmpty(cnameID) || speed < 0 || speed > 0x18)
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
                        if (m_clients.TryGetValue(cnameID, out IFullClient? client))
                        {
                            m_log?.Info($"[{Thread.CurrentThread.Name}] requesting abs position for camera");
                            client?.SendCommand(CMD_PanTiltAbsPos.CMD_ABS_POS(preset.Pan, preset.Tilt, speed));
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
                if (m_clients.TryGetValue(cnameID, out IFullClient? client))
                {
                    var cmd = INQ_PanTilt_Position.Create();
                    m_log?.Info($"[{Thread.CurrentThread.Name}] requesting pantilt position inquiry");
                    client?.SendRequest<RESP_PanTilt_Position>(cmd, RESP_PanTilt_Position.ExpectedResponseLength, (byte[] resp) =>
                    {
                        // this will come back and be run on the client's thread...
                        // wrap this into a task and run it somewhere
                        m_log?.Info($"[{Thread.CurrentThread.Name}] recieved pantilt position inquiry response");
                        Task.Run(() =>
                        {
                            try
                            {
                                m_log?.Info($"[{Thread.CurrentThread.Name}] parsing pantilt position inquiry response");
                                var preset = cmd.Parse(resp) as RESP_PanTilt_Position;
                                Internal_UpdatePreset(cnameID, presetName, preset);
                            }
                            catch (Exception ex)
                            {
                            }
                        });
                    });
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

        private void Internal_UpdatePreset(string cnameID, string presetName, RESP_PanTilt_Position pos)
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
            }
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
            IFullClient client;
            if (m_fakeClients)
            {
                client = new MockFullClient(endpoint);
            }
            else
            {
                client = new TCPFullClient(endpoint, m_log);
            }
            m_log?.Info($"[{Thread.CurrentThread.Name}] starting running cam {cnameID}");
            client.Init();
            m_clients.TryAdd(cnameID, client);
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
                if (m_clients.TryRemove(cnameID, out IFullClient? client))
                {
                    client?.Stop();
                    client = null;
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
