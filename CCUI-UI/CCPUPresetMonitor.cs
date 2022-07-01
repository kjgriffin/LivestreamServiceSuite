using CameraDriver;

using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using log4net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CCUI_UI
{

    public delegate void CCUEvent(string cName, params string[] args);

    public class CCPUPresetMonitor : ICCPUPresetMonitor_Executor
    {

        MainUI m_UIWindow;
        IRobustCamServer m_server;
        internal bool m_usingFake;
        ILog? m_log;

        public event CCUEvent OnCommandUpdate;

        public CCPUPresetMonitor(bool headless = false, ILog log = null)
        {
            m_log = log;
            Spinup(headless);
        }

        private void Spinup(bool headless)
        {
            if (!headless)
            {
                m_UIWindow = new MainUI(this);
                m_UIWindow.Show();
            }
            m_server = ISimpleCamServer.InstantiateRobust(m_log);
            m_server.Start();
            m_server.OnPresetSavedSuccess += m_server_OnPresetSavedSuccess;
            m_server.OnWorkCompleted += m_server_OnWorkCompleted;
            m_server.OnWorkFailed += m_server_OnWorkFailed;
            m_server.OnWorkStarted += m_server_OnWorkStarted;
        }

        private void m_server_OnWorkStarted(string cnameID, params string[] args)
        {
            if (args.Length > 1)
            {
                m_UIWindow.UpdateCamStatus(cnameID, getDetails(args), getStatus("STARTED", args), true);
            }
            OnCommandUpdate?.Invoke(cnameID, new string[] { "STARTED" }.Concat(args).ToArray());
        }

        private void m_server_OnWorkFailed(string cnameID, params string[] args)
        {
            if (args.Length > 1)
            {
                m_UIWindow.UpdateCamStatus(cnameID, getDetails(args), getStatus("FAILED", args), false);
            }
            OnCommandUpdate?.Invoke(cnameID, new string[] { "FAILED" }.Concat(args).ToArray());
        }

        private void m_server_OnWorkCompleted(string cnameID, params string[] args)
        {
            if (args.Length > 1)
            {
                m_UIWindow.UpdateCamStatus(cnameID, getDetails(args), getStatus("COMPLETED", args), true);
            }
            OnCommandUpdate?.Invoke(cnameID, new string[] { "COMPLETED" }.Concat(args).ToArray());
        }

        private string getDetails(string[] args)
        {
            if (args.Length > 1)
            {
                return $"{args[0]} [{args[1]}]";
            }
            return "";
        }

        private string getStatus(string msg, string[] args)
        {
            if (args.Length > 2)
            {
                return $"{msg}{Environment.NewLine}({string.Join(" ", args.Skip(2))})";
            }
            return msg;
        }

        private void m_server_OnPresetSavedSuccess(string cname, string pname)
        {
            m_UIWindow?.AddKnownPreset(cname, pname);
        }

        public void Shutdown()
        {
            m_server.OnPresetSavedSuccess -= m_server_OnPresetSavedSuccess;
            m_server.OnWorkCompleted -= m_server_OnWorkCompleted;
            m_server.OnWorkFailed -= m_server_OnWorkFailed;
            m_server?.Shutdown();
            m_UIWindow?.Close();
        }

        public void ShowUI()
        {
            if (m_UIWindow == null)
            {
                m_UIWindow = new MainUI(this);
                m_UIWindow.Show();
            }
            if (m_UIWindow?.IsVisible == false)
            {
                m_UIWindow?.Close();
                m_UIWindow = new MainUI(this);
                m_UIWindow.Show();
            }
        }

        public void StartCamera(string name, IPEndPoint endpoint)
        {
            m_server?.StartCamClient(name, endpoint);
        }

        public void StopCamera(string name)
        {
            m_server?.StopCamClient(name);
        }

        public void SavePreset(string camname, string presetname)
        {
            m_server?.Cam_SaveCurentPosition(camname, presetname);
        }

        public void FirePreset(string camname, string presetname, int speed)
        {
            m_server?.Cam_RecallPresetPosition(camname, presetname, (byte)speed);
        }

        public void RemovePreset(string camname, string presetname)
        {
            m_server?.RemovePreset(camname, presetname);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camname"></param>
        /// <param name="direction">-1 = WIDE/ 1 = TELE</param>
        public void RunZoom(string camname, int direction)
        {
            m_server?.Cam_RunZoomProgram(camname, direction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camname"></param>
        /// <param name="direction">-1 = WIDE/ 1 = TELE</param>
        /// <param name="duration">Duration in ms. Expected that 200ms is about as small as will work</param>
        public void ChirpZoom(string camname, int direction, int duration)
        {
            // reject invalid duration, or greater than 10 sec
            if (duration < 0 || duration > 10000)
            {
                return;
            }
            m_server?.Cam_RunZoomChrip(camname, direction, duration);
        }

        public CCPUConfig ExportStateToConfig()
        {
            var cams = m_server?.GetClientNamesWithPresets() ?? new List<string>();
            Dictionary<string, Dictionary<string, RESP_PanTilt_Position>> keyedPresets = new Dictionary<string, Dictionary<string, RESP_PanTilt_Position>>();
            foreach (var cam in cams)
            {
                var presets = m_server?.GetKnownPresetsForClient(cam);
                keyedPresets[cam] = presets ?? new Dictionary<string, RESP_PanTilt_Position>();
            }
            var clientscfg = m_server?.GetClientConfig() ?? new List<(string camName, IPEndPoint endpoint)>();

            CCPUConfig cfg = new CCPUConfig
            {
                Clients = clientscfg.Select(x => new CCPUConfig.ClientConfig
                {
                    IPAddress = x.endpoint?.Address.ToString() ?? "",
                    Port = x.endpoint?.Port ?? 0,
                    Name = x.camName,
                }).ToList(),
                KeyedPresets = keyedPresets,
            };
            return cfg;
        }

        public void LoadConfig(CCPUConfig? cfg)
        {
            if (cfg == null)
            {
                return;
            }

            // stop all clients
            m_server?.StopAllCamClients();
            // flush all presets
            m_server?.ClearAllPresets();

            // start all new clients
            foreach (var client in cfg.Clients)
            {
                if (IPEndPoint.TryParse($"{client.IPAddress}:{client.Port}", out var endpoint))
                {
                    m_server?.StartCamClient(client.Name, endpoint);
                }
            }
            // load all new presets
            foreach (var client in cfg.Clients)
            {
                if (cfg.KeyedPresets.TryGetValue(client.Name, out var presets))
                {
                    foreach (var preset in presets)
                    {
                        m_server?.Cam_SaveRawPosition(client.Name, preset.Key, preset.Value);
                    }
                }
            }

            // any UI's should be re-created
            m_UIWindow?.ReConfigure();
        }

        internal void ReSpinWithReal()
        {
            bool ui = m_UIWindow?.IsVisible ?? false;
            var cfg = ExportStateToConfig();
            Shutdown();
            m_usingFake = false;
            Spinup(headless: !ui);
            LoadConfig(cfg);
        }
    }

    public class CCPUConfig
    {

        public class ClientConfig
        {
            public string IPAddress { get; set; } = "";
            public int Port { get; set; } = 0;
            public string Name { get; set; } = "";
        }


        public Dictionary<string, Dictionary<string, RESP_PanTilt_Position>> KeyedPresets { get; set; } = new Dictionary<string, Dictionary<string, RESP_PanTilt_Position>>();
        public List<ClientConfig> Clients { get; set; } = new List<ClientConfig>();

    }


}
