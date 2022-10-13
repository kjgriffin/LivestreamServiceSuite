using CameraDriver;

using CCU.Config;

using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using log4net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CCUI_UI
{

    public delegate void CCUEvent(string cName, params string[] args);

    public class CCPUPresetMonitor : ICCPUPresetMonitor
    {

        Window _parent = null;
        MainUI m_UIWindow;
        IRobustCamServer m_server;
        internal bool m_usingFake;
        ILog? m_log;

        public event CCUEvent OnCommandUpdate;

        public CCPUPresetMonitor(bool headless = false, ILog log = null, Window parent = null)
        {
            _parent = parent;
            m_log = log;
            Spinup(headless);
        }

        private void Spinup(bool headless)
        {
            if (!headless)
            {
                m_UIWindow = new MainUI(this, _parent);
                m_UIWindow.Show();
            }
            m_server = ISimpleCamServer.InstantiateRobust(m_log);
            m_server.Start();
            m_server.OnPresetSavedSuccess += m_server_OnPresetSavedSuccess;
            m_server.OnZoomSavedSuccess += m_server_OnZoomSavedSuccess;
            m_server.OnWorkCompleted += m_server_OnWorkCompleted;
            m_server.OnWorkFailed += m_server_OnWorkFailed;
            m_server.OnWorkStarted += m_server_OnWorkStarted;
        }

        private void m_server_OnWorkStarted(string cnameID, params string[] args)
        {
            if (args.Length > 1)
            {
                m_UIWindow?.UpdateCamStatus(cnameID, getDetails(args), getStatus("STARTED", args), true);
            }
            OnCommandUpdate?.Invoke(cnameID, new string[] { "STARTED" }.Concat(args).ToArray());
        }

        private void m_server_OnWorkFailed(string cnameID, params string[] args)
        {
            if (args.Length > 1)
            {
                m_UIWindow?.UpdateCamStatus(cnameID, getDetails(args), getStatus("FAILED", args), false);
            }
            OnCommandUpdate?.Invoke(cnameID, new string[] { "FAILED" }.Concat(args).ToArray());
        }

        private void m_server_OnWorkCompleted(string cnameID, params string[] args)
        {
            if (args.Length > 1)
            {
                m_UIWindow?.UpdateCamStatus(cnameID, getDetails(args), getStatus("COMPLETED", args), true);
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
        private void m_server_OnZoomSavedSuccess(string cname, string pname)
        {
            m_UIWindow?.AddKnownZoom(cname, pname);
        }


        public void Shutdown()
        {
            m_server.OnPresetSavedSuccess -= m_server_OnPresetSavedSuccess;
            m_server.OnZoomSavedSuccess -= m_server_OnZoomSavedSuccess;
            m_server.OnWorkCompleted -= m_server_OnWorkCompleted;
            m_server.OnWorkFailed -= m_server_OnWorkFailed;
            m_server?.Shutdown();
            m_UIWindow?.Close();
        }

        public void ShowUI()
        {
            if (m_UIWindow == null)
            {
                m_UIWindow = new MainUI(this, _parent);
                m_UIWindow.Show();
            }
            if (m_UIWindow?.IsVisible == false)
            {
                m_UIWindow?.Close();
                m_UIWindow = new MainUI(this, _parent);
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

        public void SaveZoomLevel(string camname, string presetname, int zlevel, string mode)
        {
            m_server?.Cam_SaveZoomPresetProgram(camname, presetname, zlevel, mode);
        }

        public void FirePreset(string camname, string presetname, int speed)
        {
            FirePreset_Internal(camname, presetname, (byte)speed);
        }
        public void FireZoomLevel(string camname, string presetname)
        {
            FireZoomLevel_Internal(camname, presetname);
        }

        internal Guid FirePreset_Internal(string camname, string presetname, int speed)
        {
            return m_server?.Cam_RecallPresetPosition(camname, presetname, (byte)speed) ?? Guid.Empty;
        }

        internal Guid FireZoomLevel_Internal(string camname, string presetname)
        {
            return m_server?.Cam_RecallZoomPresetPosition(camname, presetname) ?? Guid.Empty;
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
            ChirpZoom_Internal(camname, direction, duration);
        }

        internal Guid ChirpZoom_Internal(string camname, int direction, int duration)
        {
            // reject invalid duration, or greater than 10 sec
            if (duration < 0 || duration > 10000)
            {
                return Guid.Empty;
            }
            return m_server?.Cam_RunZoomChrip(camname, direction, duration) ?? Guid.Empty;
        }

        public CCPUConfig ExportStateToConfig()
        {
            var cams = m_server?.GetClientNamesWithPresets() ?? new List<string>();
            Dictionary<string, Dictionary<string, RESP_PanTilt_Position>> keyedPresets = new Dictionary<string, Dictionary<string, RESP_PanTilt_Position>>();
            Dictionary<string, Dictionary<string, ZoomProgram>> keyedZooms = new Dictionary<string, Dictionary<string, ZoomProgram>>();
            foreach (var cam in cams)
            {
                var presets = m_server?.GetKnownPresetsForClient(cam);
                keyedPresets[cam] = presets ?? new Dictionary<string, RESP_PanTilt_Position>();
                var zooms = m_server?.GetKnownZoomPresetsForClient(cam);
                keyedZooms[cam] = zooms ?? new Dictionary<string, ZoomProgram>();
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
                KeyedZooms = keyedZooms,
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
                if (cfg.KeyedPresets?.TryGetValue(client.Name, out var presets) == true)
                {
                    foreach (var preset in presets)
                    {
                        m_server?.Cam_SaveRawPosition(client.Name, preset.Key, preset.Value);
                    }
                }
                if (cfg.KeyedZooms?.TryGetValue(client.Name, out var zooms) == true)
                {
                    foreach (var zoom in zooms)
                    {
                        m_server?.Cam_SaveZoomPresetProgram(client.Name, zoom.Key, zoom.Value.ZoomMS, zoom.Value.Mode);
                    }
                }
            }

            // any UI's should be re-created
            m_UIWindow?.ReConfigure();

            // hack to auto-refresh the ui after we're pretty sure all cfg got loaded
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                m_UIWindow?.ReConfigure();
            });
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

        void ICCPUPresetMonitor_Executor.FirePreset(string camname, string presetname, int speed)
        {
            FirePreset(camname, presetname, speed);
        }

        Guid ICCPUPresetMonitor_Executor.FirePreset_Tracked(string camname, string presetname, int speed)
        {
            return FirePreset_Internal(camname, presetname, speed);
        }

        Guid ICCPUPresetMonitor_Executor.FireZoom_Tracked(string camname, int zoomdir, int msdur)
        {
            return ChirpZoom_Internal(camname, zoomdir, msdur);
        }
        Guid ICCPUPresetMonitor_Executor.FireZoomLevel_Tracked(string camname, string presetname)
        {
            return FireZoomLevel_Internal(camname, presetname);
        }

        (Guid move, Guid zoom) ICCPUPresetMonitor_Executor.FirePresetWithZoom_Tracked(string camname, string presetname, int speed, int zoomdir, int msdur)
        {
            Guid m = FirePreset_Internal(camname, presetname, speed);
            Guid z = ChirpZoom_Internal(camname, zoomdir, msdur);
            return (m, z);
        }
    }


}
