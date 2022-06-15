using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;
using DVIPProtocol.Protocol.Lib.Inquiry;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

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
    internal class SimpleCamServer : ISimpleCamServer
    {
        private ConcurrentDictionary<string, IFullClient> m_clients = new ConcurrentDictionary<string, IFullClient>();
        private ConcurrentQueue<Action> m_dispatchCommands = new ConcurrentQueue<Action>();
        private ManualResetEvent m_workAvailable = new ManualResetEvent(false);
        private CancellationTokenSource m_cancel = new CancellationTokenSource();
        private Thread? m_workerThread;
        private ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>> m_presets = new ConcurrentDictionary<string, Dictionary<string, RESP_PanTilt_Position>>();

        private bool m_fakeClients = false;

        public event CameraPresetSaved OnPresetSavedSuccess;

        internal SimpleCamServer(bool useFakeClients = false)
        {
            m_fakeClients = useFakeClients;
        }

        private void Internal_Loop()
        {

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
                        work?.Invoke();
                    }
                }
                m_workAvailable.Reset();

                // wait for work
                if (!m_cancel.IsCancellationRequested)
                {
                    WaitHandle.WaitAny(new WaitHandle[] { m_cancel.Token.WaitHandle, m_workAvailable });
                }
            }

            // cleanup
            m_cancel.Dispose();
            m_workAvailable.Dispose();
        }


        public void Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10)
        {
            m_dispatchCommands.Enqueue(() =>
            {
                if (m_presets.TryGetValue(cnameID, out var presets))
                {
                    if (presets?.TryGetValue(presetName, out var preset) == true)
                    {
                        if (m_clients.TryGetValue(cnameID, out IFullClient? client))
                        {
                            client?.SendCommand(CMD_PanTiltAbsPos.CMD_ABS_POS(preset.Pan, preset.Tilt, speed));
                        }
                    }
                }
            });
            m_workAvailable.Set();
        }

        public void Cam_SaveCurentPosition(string cnameID, string presetName)
        {
            m_dispatchCommands.Enqueue(() =>
            {
                if (m_clients.TryGetValue(cnameID, out IFullClient? client))
                {
                    var cmd = INQ_PanTilt_Position.Create();
                    client?.SendRequest<RESP_PanTilt_Position>(cmd, 0, (byte[] resp) =>
                    {
                        // this will come back and be run on the client's thread...
                        // wrap this into a task and run it somewhere
                        Task.Run(() =>
                        {
                            try
                            {
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
            m_dispatchCommands.Enqueue(() => Internal_UpdatePreset(cnameID, presetName, preset));
            m_workAvailable.Set();
        }

        private void Internal_UpdatePreset(string cnameID, string presetName, RESP_PanTilt_Position pos)
        {
            // assign it
            if (pos.Valid)
            {
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
            m_dispatchCommands.Enqueue(() => Internal_StartCamClient(cnameID, endpoint));
            m_workAvailable.Set();
        }

        private void Internal_StartCamClient(string cnameID, IPEndPoint endpoint)
        {
            if (m_clients.ContainsKey(cnameID))
            {
                // stop first so we can restart
                StopCamClient(cnameID);
            }
            IFullClient client;
            if (m_fakeClients)
            {
                client = new MockFullClient(endpoint);
            }
            else
            {
                client = new TCPFullClient(endpoint);
            }
            client.Init();
            m_clients.TryAdd(cnameID, client);
        }

        public void StopCamClient(string cnameID)
        {
            m_dispatchCommands.Enqueue(() => Internal_StopCamClient(cnameID));
            m_workAvailable.Set();
        }

        private void Internal_StopCamClient(string cnameID)
        {
            if (m_clients.TryRemove(cnameID, out IFullClient? client))
            {
                client?.Stop();
                client = null;
            }
        }
        public void StopAllCamClients()
        {
            m_dispatchCommands.Enqueue(() =>
            {
                foreach (var client in m_clients.Values)
                {
                    client?.Stop();
                }
                m_clients.Clear();
            });
            m_workAvailable.Set();
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

    }

    public delegate void CameraPresetSaved(string cname, string pname);

    public interface ISimpleCamServer
    {

        event CameraPresetSaved OnPresetSavedSuccess;

        void Start();
        void Shutdown();

        void StartCamClient(string cnameID, IPEndPoint endpoint);
        void StopCamClient(string cnameID);
        void StopAllCamClients();

        void Cam_SaveCurentPosition(string cnameID, string presetName);
        void Cam_SaveRawPosition(string cnameID, string presetName, RESP_PanTilt_Position value);
        void Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10);


        public static ISimpleCamServer Instantiate()
        {
            return new SimpleCamServer();
        }

        public static ISimpleCamServer Instantiate_Mock()
        {
            return new SimpleCamServer(true);
        }

    }

}
