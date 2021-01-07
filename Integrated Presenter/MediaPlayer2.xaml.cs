using System;
using System.Collections.Generic;
using System.Linq;
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
            ShowBlackSource();
            _playbacktimer = new Timer(500);
            _playbacktimer.Elapsed += _playbacktimer_Elapsed;
            videoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            videoPlayer.MediaEnded += VideoPlayer_MediaEnded;

            MuteIcon.Visibility = Visibility.Hidden;
        }

        private bool showingmute = false;

        public void MarkMuted()
        {
            showingmute = true;
            if (ShowIfMute)
            {
                MuteIcon.Visibility = Visibility.Visible;
            }
        }

        public void MarkUnMuted()
        {
            showingmute = false;
            MuteIcon.Visibility = Visibility.Hidden;
        }

        private void Mute()
        {
            videoPlayer.Volume = 0;
        }

        private void UnMute()
        {
            videoPlayer.Volume = 1;
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (AutoSilentReplay)
            {
                Mute();
                ReplayMedia();
            }
        }

        public bool AutoSilentReplay { get; set; } = false;
        public bool AutoSilentPlayback { get; set; } = false;

        public bool ShowIfMute { get; set; } = false;

        public bool ShowBlackForActions { get; set; } = true;

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (AutoSilentPlayback)
            {
                Mute();
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

        public TimeSpan MediaTimeRemaining
        {
            get
            {
                if (videoPlayer.NaturalDuration != null)
                {
                    if (videoPlayer.NaturalDuration.HasTimeSpan)
                    {
                        if (videoPlayer.Position != null)
                        {
                            TimeSpan length = videoPlayer.NaturalDuration.TimeSpan;
                            TimeSpan pos = videoPlayer.Position;
                            if (pos <= length)
                            {
                                return length - pos;
                            }
                            else
                            {
                                return length;
                            }
                        }
                        else
                        {
                            return videoPlayer.NaturalDuration.TimeSpan;
                        }
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
            if (_type == SlideType.Video || _type == SlideType.ChromaKeyVideo)
            {
                videoPlayer.Play();
            }
        }

        public void PauseMedia()
        {
            if (_type == SlideType.Video || _type == SlideType.ChromaKeyVideo)
            {
                videoPlayer.Pause();
            }
        }

        public void ReplayMedia()
        {
            if (_type == SlideType.Video || _type == SlideType.ChromaKeyVideo)
            {
                videoPlayer.Position = TimeSpan.Zero;
                videoPlayer.Play();
            }
        }

        public void StopMedia()
        {
            if (_type == SlideType.Video || _type == SlideType.ChromaKeyVideo)
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
                case SlideType.ChromaKeyVideo:
                    ShowVideo();
                    break;
                case SlideType.ChromaKeyStill:
                    ShowImage();
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

        public void SetMedia(Slide slide, bool asKey)
        {
            if (slide.Type == SlideType.Action)
            {
                ShowActionCommands(slide);
            }
            else if (slide.Source != string.Empty)
            {
                if (asKey)
                {
                    SetMedia(new Uri(slide.KeySource), slide.Type);
                }
                else
                {
                    SetMedia(new Uri(slide.Source), slide.Type);
                }
            }
            else
            {
                ShowBlackSource();
            }
        }

        public void SetupComplete(bool complete = true)
        {
            if (complete)
            {
                SetupMessages.Foreground = Brushes.Gray;
            }
            else
            {
                SetupMessages.Foreground = Brushes.LightGreen;
            }
        }

        public void ActionComplete(bool complete = true)
        {
            if (complete)
            {
                MainMessages.Foreground = Brushes.White;
            }
            else
            {
                MainMessages.Foreground = Brushes.Orange;
            }
        }

        private void ShowActionCommands(Slide slide)
        {
            ShowBlackSource();

            if (!ShowBlackForActions)
            {
                // Show Commands
                string description = "";
                foreach (var action in slide.Actions)
                {
                    if (action?.Message != "")
                    {
                        description += action.Message + Environment.NewLine;
                    }
                }
                MainMessages.Text = description.Trim();
                MainMessages.Visibility = Visibility.Visible;

                description = "";
                foreach (var action in slide.SetupActions)
                {
                    if (action?.Message != "")
                    {
                        description += action.Message + Environment.NewLine;
                    }
                }
                SetupMessages.Text = description.Trim();
                SetupMessages.Visibility = Visibility.Visible;

                SetupMessages.Foreground = Brushes.LightGreen;
                MainMessages.Foreground = Brushes.Orange;


                SequenceLabel.Text = slide.Title;

                SeqType.Text = slide.AutoOnly ? "AUTO" : "SEQ";

                SequenceLabel.Visibility = Visibility.Visible;
                ActionIndicator.Visibility = Visibility.Visible;
            }
        }

        private void ShowImage()
        {
            SequenceLabel.Visibility = Visibility.Hidden;
            SeqType.Visibility = Visibility.Hidden;
            ActionIndicator.Visibility = Visibility.Hidden;
            MainMessages.Visibility = Visibility.Hidden;
            SetupMessages.Visibility = Visibility.Hidden;
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
            SequenceLabel.Visibility = Visibility.Hidden;
            SeqType.Visibility = Visibility.Hidden;
            ActionIndicator.Visibility = Visibility.Hidden;
            MainMessages.Visibility = Visibility.Hidden;
            SetupMessages.Visibility = Visibility.Hidden;
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
            imagePlayer.Visibility = Visibility.Hidden;
            videoPlayer.Visibility = Visibility.Hidden;
            SequenceLabel.Visibility = Visibility.Hidden;
            SeqType.Visibility = Visibility.Hidden;
            ActionIndicator.Visibility = Visibility.Hidden;
            MainMessages.Visibility = Visibility.Hidden;
            SetupMessages.Visibility = Visibility.Hidden;
        }

    }
}
