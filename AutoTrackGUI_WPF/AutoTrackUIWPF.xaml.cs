using DirectShowLib;

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

namespace AutoTrackGUI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AutoTrackUIWPF : Window
    {
        Dictionary<string, int> devices = new Dictionary<string, int>();


        public AutoTrackUIWPF()
        {
            InitializeComponent();

            int i = 0;
            foreach (var cam in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                devices[cam.Name] = i;
                i++;
            }

            foreach (var item in devices)
            {
                cbVideoSources.Items.Add(item.Key);
            }

        }

        private void ClickStart(object sender, RoutedEventArgs e)
        {
            if (cbVideoSources.SelectedIndex != -1)
            {
                // perhaps this is ok?
                capture.Start(cbVideoSources.SelectedIndex);
            }
        }

        private void ClickStop(object sender, RoutedEventArgs e)
        {
            capture?.Stop();
        }
    }
}
