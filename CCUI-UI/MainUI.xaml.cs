using CCU.Config;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CCUI_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainUI : Window
    {

        CCPUPresetMonitor m_monitor;
        Window _parent = null;

        public bool AlwaysOnTop = true;

        public MainUI(CCPUPresetMonitor monitor, Window parent = null)
        {
            InitializeComponent();
            _parent = parent;
            m_monitor = monitor;
            this.Topmost = AlwaysOnTop;
            this.miOnTop.IsChecked = AlwaysOnTop;
            Setup();
        }

        private void Setup()
        {
            cam1.OnRestartRequest += Cam_OnRestartRequest;
            cam1.OnStopRequest += Cam_OnStopRequest;
            cam1.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam1.OnSaveZoomRequest += Cam_OnSaveZoomRequest;
            cam1.OnFirePresetRequest += Cam_OnFirePresetRequest;
            cam1.OnFireZoomRequest += Cam_OnFireZoomRequest;
            cam1.OnDeletePresetRequest += Cam_OnDeletePresetRequest;
            cam1.OnRunZoomRequest += Cam_OnRunZoomRequest;
            cam1.OnChirpZoomRequest += Cam_OnChripZoomRequest;

            cam2.OnRestartRequest += Cam_OnRestartRequest;
            cam2.OnStopRequest += Cam_OnStopRequest;
            cam2.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam2.OnSaveZoomRequest += Cam_OnSaveZoomRequest;
            cam2.OnFirePresetRequest += Cam_OnFirePresetRequest;
            cam2.OnFireZoomRequest += Cam_OnFireZoomRequest;
            cam2.OnDeletePresetRequest += Cam_OnDeletePresetRequest;
            cam2.OnRunZoomRequest += Cam_OnRunZoomRequest;
            cam2.OnChirpZoomRequest += Cam_OnChripZoomRequest;

            cam3.OnRestartRequest += Cam_OnRestartRequest;
            cam3.OnStopRequest += Cam_OnStopRequest;
            cam3.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam3.OnSaveZoomRequest += Cam_OnSaveZoomRequest;
            cam3.OnFirePresetRequest += Cam_OnFirePresetRequest;
            cam3.OnFireZoomRequest += Cam_OnFireZoomRequest;
            cam3.OnDeletePresetRequest += Cam_OnDeletePresetRequest;
            cam3.OnRunZoomRequest += Cam_OnRunZoomRequest;
            cam3.OnChirpZoomRequest += Cam_OnChripZoomRequest;

            miFakeClients.IsChecked = m_monitor?.m_usingFake ?? false;

            ReConfigure();
        }

        private void Cam_OnSaveZoomRequest(string cname, string presetname, int zoom, string mode)
        {
            m_monitor?.SaveZoomLevel(cname, presetname, zoom, mode);
        }

        private void Cam_OnChripZoomRequest(string cname, int direction, int chirps)
        {
            m_monitor?.ChirpZoom(cname, direction, chirps);
        }

        private void Cam_OnRunZoomRequest(string cname, int direction)
        {
            m_monitor?.RunZoom(cname, direction);
        }

        internal void ReConfigure()
        {
            var cfg = m_monitor?.ExportStateToConfig() ?? new CCPUConfig();

            CCPUConfig.ClientConfig c1 = null;
            CCPUConfig.ClientConfig c2 = null;
            CCPUConfig.ClientConfig c3 = null;

            var sorted = cfg.Clients.OrderBy(c => c.IPAddress).ToList();
            for (int i = 0; i < 3; i++)
            {
                if (sorted.Count > i)
                {
                    switch (i)
                    {
                        case 0:
                            c1 = sorted[i];
                            break;
                        case 1:
                            c2 = sorted[i];
                            break;
                        case 2:
                            c3 = sorted[i];
                            break;
                    }
                }
            }

            if (c1 != null)
            {
                cfg.KeyedPresets.TryGetValue(c1.Name, out var presets);
                cfg.KeyedZooms.TryGetValue(c1.Name, out var zpresets);
                cam1.Reconfigure(c1.Name, c1.IPAddress, c1.Port.ToString(), presets?.Select(x => x.Key).ToList() ?? new List<string>(), zpresets?.Select(x => x.Key).ToList() ?? new List<string>());
            }
            if (c2 != null)
            {
                cfg.KeyedPresets.TryGetValue(c2.Name, out var presets);
                cfg.KeyedZooms.TryGetValue(c2.Name, out var zpresets);
                cam2.Reconfigure(c2.Name, c2.IPAddress, c2.Port.ToString(), presets?.Select(x => x.Key).ToList() ?? new List<string>(), zpresets?.Select(x => x.Key).ToList() ?? new List<string>());
            }
            if (c3 != null)
            {
                cfg.KeyedPresets.TryGetValue(c3.Name, out var presets);
                cfg.KeyedZooms.TryGetValue(c3.Name, out var zpresets);
                cam3.Reconfigure(c3.Name, c3.IPAddress, c3.Port.ToString(), presets?.Select(x => x.Key).ToList() ?? new List<string>(), zpresets?.Select(x => x.Key).ToList() ?? new List<string>());
            }

        }

        private void Cam_OnDeletePresetRequest(string cname, string presetname)
        {
            m_monitor?.RemovePreset(cname, presetname);
        }

        private void Cam_OnSavePresetRequest(string cname, string presetname)
        {
            m_monitor?.SavePreset(cname, presetname);
        }

        private void Cam_OnStopRequest(string cname)
        {
            m_monitor?.StopCamera(cname);
        }

        private void Cam_OnRestartRequest(string cname, System.Net.IPEndPoint endpoint)
        {
            m_monitor?.StartCamera(cname, endpoint);
        }

        private void Cam_OnFirePresetRequest(string cname, string presetname, int speed)
        {
            m_monitor?.FirePreset(cname, presetname, speed);
        }

        private void Cam_OnFireZoomRequest(string cname, string presetname)
        {
            m_monitor?.FireZoomLevel(cname, presetname);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            cam1.OnRestartRequest -= Cam_OnRestartRequest;
            cam1.OnStopRequest -= Cam_OnStopRequest;
            cam1.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam1.OnSaveZoomRequest -= Cam_OnSaveZoomRequest;
            cam1.OnFirePresetRequest -= Cam_OnFirePresetRequest;
            cam1.OnFireZoomRequest -= Cam_OnFireZoomRequest;
            cam1.OnDeletePresetRequest -= Cam_OnDeletePresetRequest;
            cam1.OnRunZoomRequest -= Cam_OnRunZoomRequest;
            cam1.OnChirpZoomRequest -= Cam_OnChripZoomRequest;

            cam2.OnRestartRequest -= Cam_OnRestartRequest;
            cam2.OnStopRequest -= Cam_OnStopRequest;
            cam2.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam2.OnSaveZoomRequest -= Cam_OnSaveZoomRequest;
            cam2.OnFirePresetRequest -= Cam_OnFirePresetRequest;
            cam2.OnFireZoomRequest -= Cam_OnFireZoomRequest;
            cam2.OnDeletePresetRequest -= Cam_OnDeletePresetRequest;
            cam2.OnRunZoomRequest -= Cam_OnRunZoomRequest;
            cam2.OnChirpZoomRequest -= Cam_OnChripZoomRequest;

            cam3.OnRestartRequest -= Cam_OnRestartRequest;
            cam3.OnStopRequest -= Cam_OnStopRequest;
            cam3.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam3.OnSaveZoomRequest -= Cam_OnSaveZoomRequest;
            cam3.OnFirePresetRequest -= Cam_OnFirePresetRequest;
            cam3.OnFireZoomRequest -= Cam_OnFireZoomRequest;
            cam3.OnDeletePresetRequest -= Cam_OnDeletePresetRequest;
            cam3.OnRunZoomRequest -= Cam_OnRunZoomRequest;
            cam3.OnChirpZoomRequest -= Cam_OnChripZoomRequest;
        }

        internal void AddKnownPreset(string cname, string pname)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => AddKnownPreset(cname, pname));
                return;
            }
            if (cam1.LockedSettings && cam1.CamName == cname)
            {
                cam1.NewPresetAdded(pname);
            }
            else if (cam2.LockedSettings && cam2.CamName == cname)
            {
                cam2.NewPresetAdded(pname);
            }
            else if (cam3.LockedSettings && cam3.CamName == cname)
            {
                cam3.NewPresetAdded(pname);
            }
        }
        internal void AddKnownZoom(string cname, string pname)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => AddKnownZoom(cname, pname));
                return;
            }
            if (cam1.LockedSettings && cam1.CamName == cname)
            {
                cam1.NewZoomAdded(pname);
            }
            else if (cam2.LockedSettings && cam2.CamName == cname)
            {
                cam2.NewZoomAdded(pname);
            }
            else if (cam3.LockedSettings && cam3.CamName == cname)
            {
                cam3.NewZoomAdded(pname);
            }
        }


        private void Export()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export Cameras and Presets";
            sfd.AddExtension = true;
            sfd.DefaultExt = ".json";
            sfd.FileName = "CCU-Config";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // get list of all presets
                    var cfg = m_monitor?.ExportStateToConfig() ?? new CCPUConfig();

                    string json = JsonSerializer.Serialize(cfg);

                    using (var writer = new StreamWriter(sfd.FileName))
                    {
                        writer.Write(json);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void ClickExport(object sender, RoutedEventArgs e)
        {
            Export();
        }

        private void ClickLoad(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.Title = "Load Cameras and Presets";
            sfd.AddExtension = true;
            sfd.DefaultExt = ".json";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var reader = new StreamReader(sfd.FileName))
                    {
                        var json = reader.ReadToEnd();
                        var cfg = JsonSerializer.Deserialize<CCPUConfig>(json);
                        m_monitor?.LoadConfig(cfg);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void ClickToggleMock(object sender, RoutedEventArgs e)
        {
            if (m_monitor?.m_usingFake == true)
            {
                m_monitor?.ReSpinWithReal();
                miFakeClients.IsChecked = false;
            }
            else
            {
                //m_monitor?.ReSpinWithFake();
                //miFakeClients.IsChecked = true;
            }
        }

        internal void UpdateCamStatus(string cname, string cmd, string status, bool ok)
        {
            if (cname == cam1?.CamName)
            {
                cam1.UpdateLastStatus(cmd, status, ok);
            }
            else if (cname == cam2?.CamName)
            {
                cam2.UpdateLastStatus(cmd, status, ok);
            }
            else if (cname == cam3?.CamName)
            {
                cam3.UpdateLastStatus(cmd, status, ok);
            }
        }

        private void ClickRefresh(object sender, RoutedEventArgs e)
        {
            ReConfigure();
        }

        private void ClickOnTop(object sender, RoutedEventArgs e)
        {
            AlwaysOnTop = !AlwaysOnTop;
            
            miOnTop.IsChecked = AlwaysOnTop;
            this.Topmost = AlwaysOnTop;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _parent?.Focus();
            }
        }
    }
}
