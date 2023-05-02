using CCU.Config;

using Microsoft.Win32;

using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CCUI_UI
{
    /// <summary>
    /// Interaction logic for MockCameraDriver.xaml
    /// </summary>
    public partial class MockCameraDriver : Window
    {
        ICCPUPresetMonitor m_monitor;

        public MockCameraDriver(ICCPUPresetMonitor monitor)
        {
            InitializeComponent();

            m_monitor = monitor;
        }

        private void click_LoadConfig(object sender, RoutedEventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load CCPU Config - Extended";
            ofd.AddExtension = true;
            ofd.DefaultExt = ".json";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    using (var reader = new StreamReader(ofd.FileName))
                    {
                        var json = reader.ReadToEnd();
                        var cfg = JsonSerializer.Deserialize<CCPUConfig_Extended>(json);
                        m_monitor?.LoadConfig(cfg);
                    }
                }
                catch (Exception ex)
                {
                }
            }



        }
    }
}
