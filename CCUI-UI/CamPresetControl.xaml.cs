using CameraDriver;

using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CCUI_UI
{

    internal delegate void OnRestartEvent(string cname, IPEndPoint endpoint);
    internal delegate void OnStopEvent(string cname);
    internal delegate void SavePresetEvent(string cname, string presetname);
    internal delegate void SaveZoomEvent(string cname, string presetname, int zoom, string mode);
    internal delegate void FirePresetEvent(string cname, string presetname, int speed);
    internal delegate void DeletePresetEvent(string cname, string presetname);
    internal delegate void FireZoomEvent(string cname, string presetname);
    internal delegate void DeleteZoomEvent(string cname, string presetname);

    //internal delegate void RunZoom(string cname, int direction);
    internal delegate void RunZoomProgram(string cname, int direction, int chirps);

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

        private int m_zmode = 0;
        /// <summary>
        /// 1 = TELE, -1 = WIDE, 0 = STOP
        /// </summary>
        private int ZoomMode
        {
            get => m_zmode;
            set
            {
                m_zmode = value;
                Dispatcher.Invoke(() =>
                {
                    switch (m_zmode)
                    {
                        case -1:
                            btnZoomModeTELE.Foreground = Brushes.CornflowerBlue;
                            btnZoomModeWIDE.Foreground = Brushes.Orange;
                            break;
                        case 1:
                            btnZoomModeTELE.Foreground = Brushes.Orange;
                            btnZoomModeWIDE.Foreground = Brushes.CornflowerBlue;
                            break;
                        default:
                            btnZoomModeTELE.Foreground = Brushes.CornflowerBlue;
                            btnZoomModeWIDE.Foreground = Brushes.CornflowerBlue;
                            break;
                    }
                });
            }
        }
        private string ZoomModeSTR => ZoomMode == 0 ? "STOP" : ZoomMode == -1 ? "WIDE" : "TELE";


        internal event OnRestartEvent OnRestartRequest;
        internal event OnStopEvent OnStopRequest;
        internal event SavePresetEvent OnSavePresetRequest;

        internal event SaveZoomEvent OnSaveZoomRequest;
        internal event DeleteZoomEvent OnDeleteZoomRequest;
        internal event FireZoomEvent OnFireZoomRequest;

        internal event FirePresetEvent OnFirePresetRequest;
        internal event DeletePresetEvent OnDeletePresetRequest;

        //internal event RunZoom OnRunZoomRequest;
        internal event RunZoomProgram OnRunZoomProgramRequest;

        Dictionary<string, PresetControl> m_presets = new Dictionary<string, PresetControl>();
        Dictionary<string, PresetControl> m_zooms = new Dictionary<string, PresetControl>();

        Dictionary<string, ZoomProgram> m_zoomprograms = new Dictionary<string, ZoomProgram>();

        public CamPresetControl()
        {
            InitializeComponent();
            ZoomMode = 0;
        }

        internal void Reconfigure(string name, string ip, string port, List<string> pos_presets, Dictionary<string, ZoomProgram> z_presets)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => Reconfigure(name, ip, port, pos_presets, z_presets));
                return;
            }

            tbCamName.Text = name;
            tbCamIP.Text = ip;
            tbCamPort.Text = port;

            CamName = name;
            m_camIP = ip;
            m_camPort = port;

            ZoomMode = -1; // seems to be a nice default??

            LockedSettings = true;

            // clear any presets
            foreach (var pst in m_presets.Values)
            {
                pst.OnRemovePreset -= RemovePreset;
                pst.OnRunPreset -= RunPreset;
            }
            lvPresets.Items.Clear();
            m_presets.Clear();
            // clear any presets
            foreach (var pst in m_zooms.Values)
            {
                pst.OnRemovePreset -= RemoveZPreset;
                pst.OnRunPreset -= RunZPreset;
            }
            lvZoomPresets.Items.Clear();
            m_zooms.Clear();


            foreach (var preset in pos_presets)
            {
                NewPresetAdded(preset);
            }

            foreach (var preset in z_presets.Keys)
            {
                NewZoomAdded(preset);
            }
            m_zoomprograms = z_presets;
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
        private void btnSavePresetZoom_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings && !string.IsNullOrWhiteSpace(tbZPstName.Text))
            {
                if (int.TryParse(tbZProgDurMs.Text, out var zrunms))
                {
                    //var pstMatch = Regex.Match(tbPresetName.Text, "(?<name>.*);(?<mode>.*)");
                    //OnSaveZoomRequest?.Invoke(CamName, pstMatch.Groups["name"].Value, zrunms, pstMatch.Groups["mode"].Value);
                    OnSaveZoomRequest?.Invoke(CamName, tbZPstName.Text, zrunms, ZoomModeSTR);
                }
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

        internal void NewZoomAdded(string presetName)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => NewZoomAdded(presetName));
                return;
            }
            if (!m_zooms.ContainsKey(presetName))
            {
                var ctrl = new PresetControl(presetName);
                ctrl.OnRunPreset += RunZPreset;
                ctrl.OnRemovePreset += RemoveZPreset;
                ctrl.OnPresetSelected += SelectZPreset;
                m_zooms[presetName] = ctrl;
                lvZoomPresets.Items.Add(ctrl);
            }
        }

        private void SelectZPreset(string pName)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => SelectZPreset(pName));
                return;
            }
            tbZPstName.Text = pName;
            if (m_zoomprograms.TryGetValue(pName, out var zprog))
            {
                switch (zprog.Mode)
                {
                    case "WIDE":
                        ZoomMode = -1;
                        break;
                    case "TELE":
                        ZoomMode = 1;
                        break;
                }
                tbZProgDurMs.Text = zprog.ZoomMS.ToString();
            }
        }

        private void RemoveZPreset(string pName)
        {
            OnDeletePresetRequest?.Invoke(CamName, pName);
            if (m_zooms.TryGetValue(pName, out var ctrl))
            {
                ctrl.OnRemovePreset -= RemoveZPreset;
                ctrl.OnRunPreset -= RunZPreset;
                ctrl.OnPresetSelected -= SelectZPreset;
                m_zooms.Remove(pName);
                lvZoomPresets.Items.Remove(ctrl);
            }
        }

        private void RunZPreset(string pName)
        {
            if (LockedSettings)
            {
                OnFireZoomRequest?.Invoke(CamName, pName);
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
            if (LockedSettings && int.TryParse(tbZProgDurMs.Text, out int chirps) && chirps > 0)
            {
                OnRunZoomProgramRequest?.Invoke(CamName, 1, chirps);
            }
        }

        private void btnChirpWide_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings && int.TryParse(tbZProgDurMs.Text, out int chirps) && chirps > 0)
            {
                OnRunZoomProgramRequest?.Invoke(CamName, -1, chirps);
            }
        }

        private void btnSetZoomModeWIDE(object sender, RoutedEventArgs e)
        {
            ZoomMode = -1;
        }

        private void bntSetZoomModeTELE(object sender, RoutedEventArgs e)
        {
            ZoomMode = 1;
        }

        private void btnRunZoom_Click(object sender, RoutedEventArgs e)
        {
            if (LockedSettings && !string.IsNullOrWhiteSpace(tbZPstName.Text))
            {
                if (int.TryParse(tbZProgDurMs.Text, out var zrunms))
                {
                    OnRunZoomProgramRequest?.Invoke(CamName, ZoomMode, zrunms);
                }
            }

        }
    }
}
