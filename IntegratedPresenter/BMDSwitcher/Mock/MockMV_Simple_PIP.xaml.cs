using IntegratedPresenter.Main;

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
        string m_source = null;
        MainWindow.MediaPlaybackEventArgs.State m_playbackState = MainWindow.MediaPlaybackEventArgs.State.Stop;
        bool m_activeVideo = false;


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

            m_activeVideo = true;
            PerformPlaybackChange(m_playbackState);
        }


        public void UpdateSource(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                videoPlayerA.Visibility = Visibility.Hidden;
                imagePlayerA.Visibility = Visibility.Hidden;

                videoPlayerA.LoadedBehavior = MediaState.Manual;
                videoPlayerA.Stop();
                m_activeVideo = false;
                m_playbackState = MainWindow.MediaPlaybackEventArgs.State.Stop;

                m_source = source;
                return;
            }

            //if (source == m_source)
            //{
            //    // do nothing?
            //    return;
            //}
            if (System.IO.Path.GetExtension(source) == ".mp4" && source != m_source)
            {
                // load video
                videoPlayerA.LoadedBehavior = MediaState.Play; // trick it to auto load
                videoPlayerA.Source = new Uri(source);

                videoPlayerA.Visibility = Visibility.Visible;
                imagePlayerA.Visibility = Visibility.Hidden;

                m_source = source;
            }
            else
            {
                UpdateWithImage(new BitmapImage(new Uri(source)));
            }
        }

        public void UpdateWithVideo(string source)
        {
            if (m_source == source)
            {
                return;
            }

            UpdateSource(source);
        }

        public void UpdateWithImage(BitmapImage img)
        {
            m_source = "";
            // for now assume images
            imagePlayerA.Source = img;

            videoPlayerA.LoadedBehavior = MediaState.Manual;
            videoPlayerA.Stop();
            m_activeVideo = false;
            m_playbackState = MainWindow.MediaPlaybackEventArgs.State.Stop;


            videoPlayerA.Visibility = Visibility.Hidden;
            imagePlayerA.Visibility = Visibility.Visible;
        }

        public Brush GetOutput()
        {
            return new VisualBrush(displaySum);
        }

        internal void UpdatePlaybackState(MainWindow.MediaPlaybackEventArgs e)
        {
            m_playbackState = e.PlaybackState;

            if (m_activeVideo)
            {
                PerformPlaybackChange(e.PlaybackState);
            }
        }

        private void PerformPlaybackChange(MainWindow.MediaPlaybackEventArgs.State state)
        {
            videoPlayerA.LoadedBehavior = MediaState.Manual;
            switch (state)
            {
                case MainWindow.MediaPlaybackEventArgs.State.Play:
                    videoPlayerA.Play();
                    break;
                case MainWindow.MediaPlaybackEventArgs.State.Stop:
                    videoPlayerA.Stop();
                    break;
                case MainWindow.MediaPlaybackEventArgs.State.Pause:
                    videoPlayerA.Pause();
                    break;
                case MainWindow.MediaPlaybackEventArgs.State.Restart:
                    videoPlayerA.Stop();
                    videoPlayerA.Play();
                    break;
                default:
                    break;
            }

        }

    }
}
