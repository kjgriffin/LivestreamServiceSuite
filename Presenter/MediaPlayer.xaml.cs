using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Presenter
{


    public class MediaPlaybackEventArgs : EventArgs
    {
        public Duration Length { get; set; }
        public TimeSpan Current { get; set; }
        public Duration Remaining { get; set; }

    }

    public delegate void MediaPlaybackEventHandler(object source, MediaPlaybackEventArgs e);

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

        public event MediaPlaybackEventHandler OnMediaPlayback;

        private void _playbacktimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_type == SlideType.Video)
            {
                TimeSpan currentpos = videoPlayer.Position;
                Duration total = videoPlayer.NaturalDuration;
                Duration remaining = total - currentpos.Duration();

                OnMediaPlayback(this, new MediaPlaybackEventArgs() { Current = currentpos, Length = total, Remaining = remaining });
            }
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

        TimeSpan length;
        TimeSpan _remaining;

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

        private void ShowImage()
        {
            _playbacktimer.Stop();
            videoPlayer.Stop();
            videoPlayer.Visibility = Visibility.Hidden;

            imagePlayer.Visibility = Visibility.Visible;
            imagePlayer.Source = new BitmapImage(_source);
        }

        private void ShowVideo()
        {
            imagePlayer.Visibility = Visibility.Hidden;

            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Visibility = Visibility.Visible;
            videoPlayer.Source = _source;

            // enable and reset timer
            _playbacktimer.Start();
        }

    }
}
