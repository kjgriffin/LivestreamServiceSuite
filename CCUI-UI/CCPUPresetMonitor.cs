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
    public class CCPUPresetMonitor : ICCPUPresetMonitor_Executor
    {

        MainUI m_UIWindow;
        ISimpleCamServer m_server;
        internal bool m_usingFake;
        ILog? m_log;

        public CCPUPresetMonitor(bool headless = false, bool fakeClients = false, ILog log = null)
        {
            m_log = log;
            m_usingFake = fakeClients;
            Spinup(headless, fakeClients);
        }

        private void Spinup(bool headless, bool fakeClients)
        {
            if (!headless)
            {
                m_UIWindow = new MainUI(this);
                m_UIWindow.Show();
            }
            if (fakeClients)
            {
                m_server = ISimpleCamServer.Instantiate_Mock(m_log);
            }
            else
            {
                m_server = ISimpleCamServer.Instantiate(m_log);
            }
            m_server.Start();
            m_server.OnPresetSavedSuccess += m_server_OnPresetSavedSuccess;
        }

        private void m_server_OnPresetSavedSuccess(string cname, string pname)
        {
            m_UIWindow?.AddKnownPreset(cname, pname);
        }

        public void Shutdown()
        {
            m_server.OnPresetSavedSuccess -= m_server_OnPresetSavedSuccess;
            m_server?.Shutdown();
            m_UIWindow.Close();
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

        internal void ReSpinWithFake()
        {
            bool ui = m_UIWindow?.IsVisible ?? false;
            var cfg = ExportStateToConfig();
            Shutdown();
            m_usingFake = true;
            Spinup(headless: !ui, fakeClients: true);
            LoadConfig(cfg);
        }

        internal void ReSpinWithReal()
        {
            bool ui = m_UIWindow?.IsVisible ?? false;
            var cfg = ExportStateToConfig();
            Shutdown();
            m_usingFake = false;
            Spinup(headless: !ui, fakeClients: false);
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
