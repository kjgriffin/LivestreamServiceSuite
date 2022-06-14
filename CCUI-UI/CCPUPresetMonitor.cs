using CameraDriver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CCUI_UI
{
    public class CCPUPresetMonitor
    {

        MainUI m_UIWindow;
        ISimpleCamServer m_server;

        public CCPUPresetMonitor(bool headless = false, bool fakeClients = false)
        {
            if (!headless)
            {
                m_UIWindow = new MainUI(this);
                m_UIWindow.Show();
            }
            if (fakeClients)
            {
                m_server = ISimpleCamServer.Instantiate_Mock();
            }
            else
            {
                m_server = ISimpleCamServer.Instantiate();
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
            m_server?.Shutdown();
            m_server.OnPresetSavedSuccess -= m_server_OnPresetSavedSuccess;
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

    }
}
