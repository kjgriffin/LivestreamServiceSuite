using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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


        private void Internal_Loop()
        {
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


        public void Cam_RecallPresetPosition(string cnameID, string presetName)
        {
            m_dispatchCommands.Enqueue(() =>
            {
                if (m_presets.TryGetValue(cnameID, out var presets))
                {
                    if (presets?.TryGetValue(presetName, out var preset) == true)
                    {
                        if (m_clients.TryGetValue(cnameID, out IFullClient? client))
                        {
                            client?.SendCommand(CMD_PanTiltAbsPos.CMD_ABS_POS(preset.Pan, preset.Tilt, preset.Speed).PackagePayload());
                        }
                    }
                }
            });
        }

        public void Cam_SaveCurentPosition(string cnameID, string presetName)
        {
            m_dispatchCommands.Enqueue(() =>
            {
                if (m_clients.TryGetValue(cnameID, out IFullClient? client))
                {
                    var cmd = INQ_PanTilt_Position.Create();
                    client?.SendRequest(cmd.PackagePayload(), 0, (byte[] resp) =>
                    {
                        var preset = cmd.Parse(resp);
                        Internal_UpdatePreset(cnameID, presetName, preset);
                    });
                }
            });
        }

        public void Cam_SaveRawPosition(string cnameID, string presetName, RESP_PanTilt_Position preset)
        {
            m_dispatchCommands.Enqueue(() => Internal_UpdatePreset(cnameID, presetName, preset));
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
            IFullClient client = new TCPFullClient(endpoint);
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

        public void Stop()
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

    public interface ISimpleCamServer
    {

        void Start();
        void Stop();

        void StartCamClient(string cnameID, IPEndPoint endpoint);
        void StopCamClient(string cnameID);

        void Cam_SaveCurentPosition(string cnameID, string presetName);
        void Cam_SaveRawPosition(string cnameID, string presetName, RESP_PanTilt_Position value);
        void Cam_RecallPresetPosition(string cnameID, string presetName);


        public static ISimpleCamServer Instantiate()
        {
            return new SimpleCamServer();
        }

    }

}
