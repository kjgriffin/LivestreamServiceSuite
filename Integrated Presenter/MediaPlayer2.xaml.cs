using System;
using System.Collections.Generic;
using System.Text;
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

namespace Integrated_Presenter
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
    /// Interaction logic for MediaPlayer2.xaml
    /// </summary>
    public partial class MediaPlayer2 : UserControl
    {
        public MediaPlayer2()
        {
            InitializeComponent();
            _playbacktimer = new Timer(500);
            _playbacktimer.Elapsed += _playbacktimer_Elapsed;
            videoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            videoPlayer.MediaEnded += VideoPlayer_MediaEnded;

        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (AutoSilentReplay)
            {
                videoPlayer.Volume = 0;
                ReplayMedia();
            }
        }

        public bool AutoSilentReplay { get; set; } = false;
        public bool AutoSilentPlayback { get; set; } = false;

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (AutoSilentPlayback)
            {
                videoPlayer.Volume = 0;
                PlayMedia();
            }
            OnMediaLoaded?.Invoke(this, new MediaPlaybackTimeEventArgs(videoPlayer.Position, videoPlayer.NaturalDuration.TimeSpan, (videoPlayer.NaturalDuration - videoPlayer.Position).TimeSpan));
        }

        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaLoaded;
        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdate;

        public TimeSpan MediaLength
        {
            get
            {
                if (videoPlayer.NaturalDuration != null)
                {
                    if (videoPlayer.NaturalDuration.HasTimeSpan)
                    {
                        return videoPlayer.NaturalDuration.TimeSpan;
                    }
                }
                return TimeSpan.Zero;
            }
        }

        private void _playbacktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Parent.Dispatcher.Invoke(() =>
            {
                try
                {
                    OnMediaPlaybackTimeUpdate?.Invoke(this, new MediaPlaybackTimeEventArgs(videoPlayer.Position, videoPlayer.NaturalDuration.TimeSpan, (videoPlayer.NaturalDuration - videoPlayer.Position).TimeSpan));
                }
                catch { }
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

        public void StopMedia()
        {
            if (_type == SlideType.Video)
            {
                videoPlayer.Stop();
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
                case SlideType.Full:
                    ShowImage();
                    break;
                case SlideType.Liturgy:
                    ShowImage();
                    break;
                case SlideType.Empty:
                    ShowBlackSource();
                    break;
                default:
                    break;
            }
        }

        public void SetMedia(Slide slide)
        {
            if (slide.Source != string.Empty)
            {
                SetMedia(new Uri(slide.Source), slide.Type);
            }
            else
            {
                ShowBlackSource();
            }
        }

        private void ShowImage()
        {
            BlackSource.Visibility = Visibility.Hidden;
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

        private void ShowVideo()
        {
            BlackSource.Visibility = Visibility.Hidden;
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

        public void ShowBlackSource()
        {
            BlackSource.Visibility = Visibility.Visible;
        }

    }
}
