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

namespace Integrated_Presenter.BMDSwitcher.Mock
{
    /// <summary>
    /// Interaction logic for MockMV_Simple_PIP.xaml
    /// </summary>
    public partial class MockMV_Simple_PIP : UserControl
    {
        public MockMV_Simple_PIP()
        {
            InitializeComponent();
            videoPlayerA.MediaOpened += VideoPlayerA_MediaOpened;
        }

        private void VideoPlayerA_MediaOpened(object sender, RoutedEventArgs e)
        {
            // pause and reset
            videoPlayerA.LoadedBehavior = MediaState.Manual;
            videoPlayerA.Pause();
            videoPlayerA.Position = TimeSpan.FromMilliseconds(1);

            // perhaps we hide the last image only once media has loaded?
        }

        string m_source = "";

        public void UpdateSource(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                videoPlayerA.Visibility = Visibility.Hidden;
                imagePlayerA.Visibility = Visibility.Hidden;

                videoPlayerA.LoadedBehavior = MediaState.Manual;
                videoPlayerA.Stop();
            }

            if (source == m_source)
            {
                // do nothing?
                return;
            }
            m_source = source;
            if (System.IO.Path.GetExtension(source) == ".mp4")
            {
                // load video
                videoPlayerA.LoadedBehavior = MediaState.Play; // trick it to auto load
                videoPlayerA.Source = new Uri(source);

                videoPlayerA.Visibility = Visibility.Visible;
                imagePlayerA.Visibility = Visibility.Hidden;
            }
            else
            {
                UpdateWithImage(new BitmapImage(new Uri(source)));
            }
        }

        public void UpdateWithImage(BitmapImage img)
        {
            // for now assume images
            imagePlayerA.Source = img;

            videoPlayerA.LoadedBehavior = MediaState.Manual;
            videoPlayerA.Stop();

            videoPlayerA.Visibility = Visibility.Hidden;
            imagePlayerA.Visibility = Visibility.Visible;
        }

    }
}
