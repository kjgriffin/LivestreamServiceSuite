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

        List<PresetControl> m_presets = new List<PresetControl>();

        public CamPresetControl()
        {
            InitializeComponent();
        }

        private void btnReStart_Click(object sender, RoutedEventArgs e)
        {
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
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
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
            var ctrl = new PresetControl(presetName);
            ctrl.OnRunPreset += RunPreset;
            ctrl.OnRemovePreset += RemovePreset;
            m_presets.Add(ctrl);
            lvPresets.Items.Add(ctrl);
        }

        private void RemovePreset(string pName)
        {
            OnDeletePresetRequest?.Invoke(CamName, pName);
            var ctrl = m_presets.FirstOrDefault(p => p.PresetName == pName);
            if (ctrl != null)
            {
                ctrl.OnRemovePreset -= RemovePreset;
                ctrl.OnRunPreset -= RunPreset;
                m_presets.Remove(ctrl);
                lvPresets.Items.Remove(ctrl);
            }
        }

        private void RunPreset(string pName)
        {
            if (LockedSettings && int.TryParse(tbPresetSpeed.Text, out int speed) && speed < 0x18 && speed > 0)
            {
                OnFirePresetRequest?.Invoke(CamName, pName, speed);
            }
        }
    }
}
