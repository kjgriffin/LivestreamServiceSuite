using Integrated_Presenter.ViewModels;

using IntegratedPresenter.BMDSwitcher;
using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections.Generic;
using System.IO;
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

namespace IntegratedPresenter.Main
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


        public void ShowHideShortcuts(bool show)
        {
            if (!ShowIfMute)
                return;
            Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    ksc_m.Visibility = Visibility.Visible;
                }
                else
                {
                    ksc_m.Visibility = Visibility.Collapsed;
                }
            });
        }

        public void MarkMuted()
        {
            if (ShowIfMute)
            {
                Dispatcher.Invoke(() =>
                {
                    MuteIcon.Visibility = Visibility.Visible;
                });
            }
        }

        public void MarkUnMuted()
        {
            Dispatcher.Invoke(() =>
            {
                MuteIcon.Visibility = Visibility.Hidden;
            });
        }

        private void Mute()
        {
            videoPlayer.Volume = 0;
        }

        private void UnMute()
        {
            videoPlayer.Volume = 1;
        }

        public void MuteAudioPlayback()
        {

        }
        public void UnMuteAudioPlayback()
        {

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

        bool _showAutomationPreviews = false;
        public bool ShowAutomationPreviews
        {
            get => _showAutomationPreviews;
            set
            {
                _showAutomationPreviews = value;
                ChangeShowAutomationPreviews();
            }
        }

        private bool _showpostset = false;
        public bool ShowPostset
        {
            get => _showpostset;
            set
            {
                _showpostset = value;
                SetPostset(_postsetid);
            }
        }

        private int _postsetid = -1;
        public void SetPostset(int id)
        {
            _postsetid = id;
            if (!_showpostset)
            {
                postset.Visibility = Visibility.Hidden;
            }
            else
            {
                if (_postsetid > -1)
                {
                    postset.SetPostset(_postsetid, _mvconfig);
                    postset.Visibility = Visibility.Visible;
                }
                else
                {
                    postset.Visibility = Visibility.Hidden;
                }
            }
        }

        BMDMultiviewerSettings _mvconfig = BMDMultiviewerSettings.Default();

        public void SetMVConfigForPostset(BMDMultiviewerSettings config)
        {
            _mvconfig = config;
            SetPostset(_postsetid);
        }

        public void ShowTileBackground(bool show = true)
        {
            if (show)
            {
                backgroundfill.Style = Resources["CheckBackgroundFill"] as Style;
            }
            else
            {
                backgroundfill.Fill = new SolidColorBrush(Colors.Black);
            }
        }

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
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
                if (videoPlayer.NaturalDuration != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
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
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
                if (videoPlayer.NaturalDuration != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
                {
                    if (videoPlayer.NaturalDuration.HasTimeSpan)
                    {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
                        if (videoPlayer.Position != null)
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
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

        Slide _curentSlide = null;
        bool _asKey = false;

        public void ChangeShowAutomationPreviews()
        {
            if (ShowAutomationPreviews)
            {
                if (_curentSlide?.Type == SlideType.Liturgy)
                {
                    AutomationPreviewGraphics.Visibility = Visibility.Visible;
                    return;
                }
            }
            AutomationPreviewGraphics.Visibility = Visibility.Hidden;
        }
        public void FireOnSwitcherStateChangedForAutomation(BMDSwitcherState state, BMDSwitcherConfigSettings config)
        {
            Dispatcher.Invoke(() =>
            {
                ChangeShowAutomationPreviews();
                AutomationPreviewGraphics?.FireOnSwitcherStateChanged(state, config);
            });
        }

        private void ChangeSlide(Slide newSlide, bool asKey)
        {
            ClearActionPreviews();
            _asKey = asKey;
            if (_curentSlide != null)
            {
                // un-register handler
                _curentSlide.OnActionUpdated -= _curentSlide_OnActionUpdated;
            }

            _curentSlide = newSlide;
            // register new handler
            _curentSlide.OnActionUpdated += _curentSlide_OnActionUpdated;

            // show/hide previews
            ChangeShowAutomationPreviews();
        }

        private void _curentSlide_OnActionUpdated(TrackedAutomationAction updatedAction)
        {
            // for now we'll update all the text
            Dispatcher.Invoke(() =>
            {
                ShowActionsText(_curentSlide);
            });
        }

        Dictionary<string, bool> Conditionals = new Dictionary<string, bool>();
        public void FireOnActionConditionsUpdated(Dictionary<string, bool> conditionals)
        {
            Conditionals = conditionals;
            Dispatcher.Invoke(() =>
            {
                ShowActionsText(_curentSlide);
            });
        }

        public void SetMedia(Slide slide, bool asKey)
        {
            ChangeSlide(slide, asKey);
            if (slide.Type == SlideType.Action)
            {
                ShowActionCommands(slide, asKey);
            }
            else if (slide.Source != string.Empty)
            {
                if (asKey)
                {
                    if (slide.KeySource != null && slide.KeySource != "")
                    {
                        SetMedia(new Uri(slide.KeySource), slide.Type);
                    }
                    else
                    {
                        ShowBlackSource();
                    }
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

        private void ShowActionCommands(Slide slide, bool asKey)
        {
            // check action slide listed an alternate display/key source (other than black)
            bool validaltsource = false;
            if (slide.AltSources)
            {
                // load them
                if (File.Exists(slide.AltSource) && File.Exists(slide.AltKeySource))
                {
                    if (asKey)
                    {
                        if (slide.AltKeySource.EndsWith(".png"))
                        {
                            SetMedia(new Uri(slide.AltKeySource), SlideType.Full);
                        }
                        else if (slide.AltKeySource.EndsWith(".mp4"))
                        {
                            SetMedia(new Uri(slide.AltKeySource), SlideType.Video);
                        }
                    }
                    else
                    {
                        if (slide.AltSource.EndsWith(".png"))
                        {
                            SetMedia(new Uri(slide.AltSource), SlideType.Full);
                        }
                        else if (slide.AltKeySource.EndsWith(".mp4"))
                        {
                            SetMedia(new Uri(slide.AltSource), SlideType.Video);
                        }
                    }
                    validaltsource = true;
                }
            }
            if (!validaltsource)
            {
                ShowBlackSource();
            }

            if (!ShowBlackForActions)
            {
                ShowActionsText(slide);

                SequenceLabel.Text = slide.Title;

                SeqType.Text = slide.AutoOnly ? "AUTO" : "SEQ";

                SequenceLabel.Visibility = Visibility.Visible;
                ActionIndicator.Visibility = Visibility.Visible;
                SeqType.Visibility = Visibility.Visible;
            }
        }

        private void ShowActionsText(Slide slide)
        {
            if (!ShowBlackForActions)
            {
                ClearActionPreviews();
                if (slide == null)
                {
                    return;
                }

                foreach (var action in slide.SetupActions)
                {
                    spActionView.Children.Add(new AutomationActionMonitor(action, Conditionals));
                }

                foreach (var action in slide.Actions)
                {
                    spActionView.Children.Add(new AutomationActionMonitor(action, Conditionals));
                }

                //// Show Commands
                //string description = "";
                //foreach (var action in slide.Actions)
                //{
                //    if (action?.Action?.Message != "")
                //    {
                //        description += $"[{action.State}] {action.Action.Message}" + Environment.NewLine;
                //    }
                //}
                //MainMessages.Text = description.Trim();
                //MainMessages.Visibility = Visibility.Visible;

                //description = "";
                //foreach (var action in slide.SetupActions)
                //{
                //    if (action?.Action?.Message != "")
                //    {
                //        description += $"[{action.State}] {action.Action.Message}" + Environment.NewLine;
                //    }
                //}
                //SetupMessages.Text = description.Trim();
                //SetupMessages.Visibility = Visibility.Visible;

                //SetupMessages.Foreground = Brushes.LightGreen;
                //MainMessages.Foreground = Brushes.Orange;
            }
        }

        private void ClearActionPreviews()
        {
            Dispatcher.Invoke(() =>
            {
                MainMessages.Visibility = Visibility.Hidden;
                SetupMessages.Visibility = Visibility.Hidden;
                spActionView.Children.Clear();
            });
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
