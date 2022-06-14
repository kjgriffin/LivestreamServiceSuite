using System;
using System.Collections.Generic;
using System.Linq;
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

            cam2.OnRestartRequest += Cam_OnRestartRequest;
            cam2.OnStopRequest += Cam_OnStopRequest;
            cam2.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam2.OnFirePresetRequest += Cam_OnFirePresetRequest;

            cam3.OnRestartRequest += Cam_OnRestartRequest;
            cam3.OnStopRequest += Cam_OnStopRequest;
            cam3.OnSavePresetRequest += Cam_OnSavePresetRequest;
            cam3.OnFirePresetRequest += Cam_OnFirePresetRequest;
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

            cam2.OnRestartRequest -= Cam_OnRestartRequest;
            cam2.OnStopRequest -= Cam_OnStopRequest;
            cam2.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam2.OnFirePresetRequest -= Cam_OnFirePresetRequest;

            cam3.OnRestartRequest -= Cam_OnRestartRequest;
            cam3.OnStopRequest -= Cam_OnStopRequest;
            cam3.OnSavePresetRequest -= Cam_OnSavePresetRequest;
            cam3.OnFirePresetRequest -= Cam_OnFirePresetRequest;
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
    }
}
