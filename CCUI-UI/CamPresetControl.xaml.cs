using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

    internal delegate void OnRestartEvent(string cname, IPEndPoint endpoint);
    internal delegate void OnStopEvent(string cname);
    internal delegate void SavePresetEvent(string cname, string presetname);
    internal delegate void FirePresetEvent(string cname, string presetname, int speed);
    internal delegate void DeletePresetEvent(string cname, string presetname);

    internal delegate void RunZoom(string cname, int direction);
    internal delegate void ChirpZoom(string cname, int direction, int chirps);

    /// <summary>
    /// Interaction logic for CamPresetControl.xaml
    /// </summary>
    public partial class CamPresetControl : UserControl
    {

        private bool _locksettings = false;
        internal bool LockedSettings
        {
            get
            {
                return _locksettings;
            }
            private set
            {
                _locksettings = value;
                tbCamName.IsReadOnly = value;
            }
        }

        public string CamName { get; private set; }
        private string m_camIP;
        private string m_camPort;


        internal event OnRestartEvent OnRestartRequest;
        internal event OnStopEvent OnStopRequest;
        internal event SavePresetEvent OnSavePresetRequest;
        internal event FirePresetEvent OnFirePresetRequest;
        internal event DeletePresetEvent OnDeletePresetRequest;
        internal event RunZoom OnRunZoomRequest;
        internal event ChirpZoom OnChirpZoomRequest;

        Dictionary<string, PresetControl> m_presets = new Dictionary<string, PresetControl>();

        public CamPresetControl()
        {
            InitializeComponent();
        }

        internal void Reconfigure(string name, string ip, string port, List<string> presets)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => Reconfigure(name, ip, port, presets));
                return;
            }

            tbCamName.Text = name;
            tbCamIP.Text = ip;
            tbCamPort.Text = port;

            CamName = name;
            m_camIP = ip;
            m_camPort = port;
            LockedSettings = true;

            // reload any presets
            foreach (var pst in m_presets.Values)
            {
                pst.OnRemovePreset -= RemovePreset;
                pst.OnRunPreset -= RunPreset;
            }
            lvPresets.Items.Clear();
            m_presets.Clear();

            foreach (var preset in presets)
            {
                NewPresetAdded(preset);
            }
        }

        private void btnReStart_Click(object sender, RoutedEventArgs e)
        {
            Internal_ReStart();
        }

        private void Internal_ReStart()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(Internal_ReStart);
                return;
            }

            if (!LockedSettings)
            {
                CamName = tbCamName.Text;
                m_camIP = tbCamIP.Text;
                m_camPort = tbCamPort.Text;
            }

            if (!string.IsNullOrEmpty(CamName) && IPEndPoint.TryParse($"{m_camIP}:{m_camPort}", out var endpoint))
            {
                OnRestartRequest?.Invoke(CamName, endpoint);
                LockedSettings = true;
                UpdateLastStatus("Restart", "Waiting for command...", true);
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Internal_Stop();
        }

        private void Internal_Stop()
        {
            OnStopRequest?.Invoke(CamName);
            LockedSettings = false;
        }

        private void btnSavePreset_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings && !string.IsNullOrWhiteSpace(tbPresetName.Text))
            {
                OnSavePresetRequest?.Invoke(CamName, tbPresetName.Text);
            }
        }

        internal void NewPresetAdded(string presetName)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => NewPresetAdded(presetName));
                return;
            }
            if (!m_presets.ContainsKey(presetName))
            {
                var ctrl = new PresetControl(presetName);
                ctrl.OnRunPreset += RunPreset;
                ctrl.OnRemovePreset += RemovePreset;
                ctrl.OnPresetSelected += SelectPreset;
                m_presets[presetName] = ctrl;
                lvPresets.Items.Add(ctrl);
            }
        }

        private void SelectPreset(string pName)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => SelectPreset(pName));
                return;
            }
            tbPresetName.Text = pName;
        }

        private void RemovePreset(string pName)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => RemovePreset(pName));
                return;
            }
            OnDeletePresetRequest?.Invoke(CamName, pName);
            if (m_presets.TryGetValue(pName, out var ctrl))
            {
                ctrl.OnRemovePreset -= RemovePreset;
                ctrl.OnRunPreset -= RunPreset;
                ctrl.OnPresetSelected -= SelectPreset;
                m_presets.Remove(pName);
                lvPresets.Items.Remove(ctrl);
            }
        }

        internal void UpdateLastStatus(string command, string status, bool OK)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateLastStatus(command, status, OK));
                return;
            }

            tbPending.Text = command;
            tbStatus.Text = status;
            tbStatus.Foreground = OK ? Brushes.LimeGreen : Brushes.Red;
        }

        private void RunPreset(string pName)
        {
            if (LockedSettings && int.TryParse(tbPresetSpeed.Text, out int speed) && speed < 0x18 && speed > 0)
            {
                OnFirePresetRequest?.Invoke(CamName, pName, speed);
            }
        }

        private void btnChirpTele_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings && int.TryParse(tbChirps.Text, out int chirps) && chirps > 0)
            {
                OnChirpZoomRequest?.Invoke(CamName, 1, chirps);
            }
        }

        private void btnChirpWide_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings && int.TryParse(tbChirps.Text, out int chirps) && chirps > 0)
            {
                OnChirpZoomRequest?.Invoke(CamName, -1, chirps);
            }
        }

        private void btnRunTele_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings)
            {
                OnRunZoomRequest?.Invoke(CamName, 1);
            }
        }

        private void btnRunWide_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings)
            {
                OnRunZoomRequest?.Invoke(CamName, -1);
            }
        }


    }
}
