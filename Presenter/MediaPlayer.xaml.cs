using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Schema;

namespace Presenter
{


    public class MediaPlaybackTimeEventArgs : EventArgs
    {
        public TimeSpan Current { get; }
        public TimeSpan Length { get; }
        public TimeSpan Remaining { get; }
        public MediaPlaybackTimeEventArgs(TimeSpan current, TimeSpan length, TimeSpan remaining)
        {
            Current = current;
            Length = length;
            Remaining = remaining;
        }
    }


    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl
    {

        public MediaPlayer()
        {
            InitializeComponent();
            _playbacktimer = new Timer(1000);
            _playbacktimer.Elapsed += _playbacktimer_Elapsed;
        }

        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdate;

        private void _playbacktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Parent.Dispatcher.Invoke(() =>
            {
                OnMediaPlaybackTimeUpdate?.Invoke(this, new MediaPlaybackTimeEventArgs(videoPlayer.Position, videoPlayer.NaturalDuration.TimeSpan, (videoPlayer.NaturalDuration - videoPlayer.Position).TimeSpan));
            });
        }

        public void PlayMedia()
        {
            if (_type == SlideType.Video)
            {
                videoPlayer.Play();
            }
        }

        public void PauseMedia()
        {
            if (_type == SlideType.Video)
            {
                videoPlayer.Pause();
            }
        }

        public void ReplayMedia()
        {
            if (_type == SlideType.Video)
            {
                videoPlayer.Position = TimeSpan.Zero;
                videoPlayer.Play();
            }
        }


        Uri _source;
        SlideType _type;

        Timer _playbacktimer;

        public void SetMedia(Uri source, SlideType type)
        {
            _source = source;
            _type = type;
            switch (type)
            {
                case SlideType.Video:
                    ShowVideo();
                    break;
                case SlideType.Image:
                    ShowImage();
                    break;
                default:
                    break;
            }
        }

        public void SetMediaUnderBlack(Uri source, SlideType type)
        {
            _source = source;
            _type = type;
            switch (type)
            {
                case SlideType.Video:
                    ShowVideo(underBlack: true);
                    break;
                case SlideType.Image:
                    ShowImage(underBlack: true);
                    break;
                default:
                    break;
            }
        }

        private void ShowImage(bool underBlack = false)
        {
            if (underBlack)
            {
                BlackSource.Visibility = Visibility.Visible;
            }
            else
            {
                BlackSource.Visibility = Visibility.Hidden;
            }

            _playbacktimer.Stop();
            videoPlayer.Stop();
            videoPlayer.Visibility = Visibility.Hidden;

            imagePlayer.Visibility = Visibility.Visible;
            try
            {
                imagePlayer.Source = new BitmapImage(_source);
            }
            catch (Exception)
            {
                ShowBlackSource();
            }
        }

        private void ShowVideo(bool underBlack = false)
        {
            if (underBlack)
            {
                BlackSource.Visibility = Visibility.Visible;
            }
            else
            {
                BlackSource.Visibility = Visibility.Hidden;
            }
            _playbacktimer.Start();
            imagePlayer.Visibility = Visibility.Hidden;

            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Visibility = Visibility.Visible;
            try
            {
                videoPlayer.Source = _source;
            }
            catch (Exception)
            {
                ShowBlackSource();
            }

        }

        public void ResetMedia()
        {
            if (_type == SlideType.Video)
            {
                videoPlayer.Stop();
            }
        }

        public void ShowBlackSource()
        {
            BlackSource.Visibility = Visibility.Visible;
        }

        public void HideBlackSource()
        {
            BlackSource.Visibility = Visibility.Hidden;
        }

        public bool IsMute { get; private set; }

        public void Mute()
        {
            IsMute = true;
            videoPlayer.Volume = 0;
        }
        public void UnMute()
        {
            IsMute = false;
            videoPlayer.Volume = 0.5;
        }


    }
}
