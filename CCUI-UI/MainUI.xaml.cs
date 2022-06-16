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

        public MainUI(CCPUPresetMonitor monitor)
        {
            InitializeComponent();
            m_monitor = monitor;
            Setup();
        }

        private void Setup()
        {
            cam1.OnRestartRequest += Cam_OnRestartRequest;
            cam1.OnStopRequest += Cam_OnStopRequest;
            cam1.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam1.OnFirePresetRequest += Cam_OnFirePresetRequest;
            cam1.OnDeletePresetRequest += Cam_OnDeletePresetRequest;

            cam2.OnRestartRequest += Cam_OnRestartRequest;
            cam2.OnStopRequest += Cam_OnStopRequest;
            cam2.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam2.OnFirePresetRequest += Cam_OnFirePresetRequest;
            cam2.OnDeletePresetRequest += Cam_OnDeletePresetRequest;

            cam3.OnRestartRequest += Cam_OnRestartRequest;
            cam3.OnStopRequest += Cam_OnStopRequest;
            cam3.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam3.OnFirePresetRequest += Cam_OnFirePresetRequest;
            cam3.OnDeletePresetRequest += Cam_OnDeletePresetRequest;

            miFakeClients.IsChecked = m_monitor?.m_usingFake ?? false;

            ReConfigure();
        }

        internal void ReConfigure()
        {
            var cfg = m_monitor?.ExportStateToConfig() ?? new CCPUConfig();

            var c1 = cfg.Clients.Count > 0 ? cfg.Clients[0] : null;
            var c2 = cfg.Clients.Count > 1 ? cfg.Clients[1] : null;
            var c3 = cfg.Clients.Count > 2 ? cfg.Clients[2] : null;

            if (c1 != null)
            {
                cfg.KeyedPresets.TryGetValue(c1.Name, out var presets);
                cam1.Reconfigure(c1.Name, c1.IPAddress, c1.Port.ToString(), presets?.Select(x => x.Key).ToList() ?? new List<string>());
            }
            if (c2 != null)
            {
                cfg.KeyedPresets.TryGetValue(c2.Name, out var presets);
                cam2.Reconfigure(c2.Name, c2.IPAddress, c2.Port.ToString(), presets?.Select(x => x.Key).ToList() ?? new List<string>());
            }
            if (c3 != null)
            {
                cfg.KeyedPresets.TryGetValue(c3.Name, out var presets);
                cam3.Reconfigure(c3.Name, c3.IPAddress, c3.Port.ToString(), presets?.Select(x => x.Key).ToList() ?? new List<string>());
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

        private void Window_Closed(object sender, EventArgs e)
        {
            cam1.OnRestartRequest -= Cam_OnRestartRequest;
            cam1.OnStopRequest -= Cam_OnStopRequest;
            cam1.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam1.OnFirePresetRequest -= Cam_OnFirePresetRequest;
            cam1.OnDeletePresetRequest -= Cam_OnDeletePresetRequest;

            cam2.OnRestartRequest -= Cam_OnRestartRequest;
            cam2.OnStopRequest -= Cam_OnStopRequest;
            cam2.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam2.OnFirePresetRequest -= Cam_OnFirePresetRequest;
            cam2.OnDeletePresetRequest -= Cam_OnDeletePresetRequest;

            cam3.OnRestartRequest -= Cam_OnRestartRequest;
            cam3.OnStopRequest -= Cam_OnStopRequest;
            cam3.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam3.OnFirePresetRequest -= Cam_OnFirePresetRequest;
            cam3.OnDeletePresetRequest -= Cam_OnDeletePresetRequest;
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
                m_monitor?.ReSpinWithFake();
                miFakeClients.IsChecked = true;
            }
        }
    }
}
