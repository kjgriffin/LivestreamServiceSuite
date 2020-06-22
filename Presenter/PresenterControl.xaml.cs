using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presenter
{

    /// <summary>
    /// Interaction logic for PresenterControl.xaml
    /// </summary>
    public partial class PresenterControl : Window
    {
        PresenterWindow _window;

        System.Timers.Timer realtimeclk;
        public PresenterControl(PresenterWindow window)
        {
            InitializeComponent();
            _window = window;
            SlideNumView = $"{_window.CurrentSlideNum}/{_window.Slides.Count}";
            _window.OnMediaPlaybackTimeUpdated += _window_OnMediaPlaybackTimeUpdated;
            realtimeclk = new System.Timers.Timer(1000);
            realtimeclk.Elapsed += Realtimeclk_Elapsed;
            realtimeclk.Start();

            UpdateFillState();

            // mute playback monitor
            NowSlide.Mute();
            UpdateMuteButton();

            SlideChanged();
        }

        private void Realtimeclk_Elapsed(object sender, ElapsedEventArgs e)
        {
            LocalTime.Dispatcher.Invoke(() =>
            {
                LocalTime.Content = DateTime.Now.ToString("hh:mm:ss tt");
            });
        }

        private void _window_OnMediaPlaybackTimeUpdated(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateLength(e.Length);
            UpdateCurrentTime(e.Current);
            UpdateTimeRemaining(e.Remaining);
        }

        public void UpdateLength(TimeSpan length)
        {
            Media_length.Content = length.ToString("mm\\:ss");
        }

        public void UpdateTimeRemaining(TimeSpan rem)
        {
            Media_rem.Content = rem.ToString("mm\\:ss");
        }

        public void UpdateCurrentTime(TimeSpan cur)
        {
            Media_at.Content = cur.ToString("mm\\:ss");
        }

        public bool IsMuted { get => _window.IsMute; }

        string slideNumView = "";
        public string SlideNumView
        {
            get => slideNumView;
            set
            {
                slideNumView = value;
                slidenumview_label.Content = value;
            }
        }

        private void ShowHideMediaControls()
        {
            if (_window.CurrentSlide.type == SlideType.Video)
            {
                // show media controls
                PlaybackControls.Visibility = Visibility.Visible;
                PlaybackTime.Visibility = Visibility.Visible;
                PlaybackTime_1.Visibility = Visibility.Visible;
            }
            else
            {
                // hide media controls
                PlaybackControls.Visibility = Visibility.Hidden;
                PlaybackTime.Visibility = Visibility.Hidden;
                PlaybackTime_1.Visibility = Visibility.Hidden;
            }
        }

        private void SlideChanged()
        {
            SlideNumView = $"{_window.CurrentSlideNum}/{_window.Slides.Count}";
            ShowHideMediaControls();



            UpdatePrevSlide();
            UpdateNowSlide();
            UpdateNextSlide();
            UpdateAfterSlide();
        }

        private void UpdateNowSlide()
        {
            NowSlide.SetMedia(new Uri(_window.CurrentSlide.path), _window.CurrentSlide.type);
        }

        private void UpdatePrevSlide()
        {
            if (_window.CurrentSlideNum - 2 < 0)
            {
                PrevSlide.ShowBlackSource();
            }
            else
            {
                PrevSlide.SetMedia(new Uri(_window.Slides[_window.CurrentSlideNum - 2].path), _window.Slides[_window.CurrentSlideNum - 2].type);
            }
        }

        private void UpdateNextSlide()
        {
            if (_window.CurrentSlideNum < _window.Slides.Count)
            {
                NextSlide.SetMedia(new Uri(_window.Slides[_window.CurrentSlideNum].path), _window.Slides[_window.CurrentSlideNum].type);
            }
            else
            {
                NextSlide.ShowBlackSource();
            }
        }

        private void UpdateAfterSlide()
        {
            if (_window.CurrentSlideNum + 1 < _window.Slides.Count)
            {
                AfterSlide.SetMedia(new Uri(_window.Slides[_window.CurrentSlideNum + 1].path), _window.Slides[_window.CurrentSlideNum + 1].type);
            }
            else
            {
                AfterSlide.ShowBlackSource();
            }
        }


        private void Next_Slide(object sender, RoutedEventArgs e)
        {
            GoNextSlide();
        }

        private void Prev_Slide(object sender, RoutedEventArgs e)
        {
            GoPrevSlide();
        }

        private void GoNextSlide()
        {
            _window.NextSlide();
            SlideChanged();
        }

        private void GoPrevSlide()
        {
            _window.PrevSlide();
            SlideChanged();
        }

        private void Play_Media(object sender, RoutedEventArgs e)
        {
            _window.StartMediaPlayback();
            NowSlide.PlayMedia();
        }

        private void Pause_Media(object sender, RoutedEventArgs e)
        {
            _window.PauseMediaPlayback();
            NowSlide.PauseMedia();
        }

        private void Replay_Media(object sender, RoutedEventArgs e)
        {
            _window.RestartMediaPlayback();
            NowSlide.ReplayMedia();
        }

        public event EventHandler OnWindowClosing;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _window.Dispatcher.Invoke(() =>
            {
                OnWindowClosing?.Invoke(this, e);
            });
        }

        private void Reset_Media(object sender, RoutedEventArgs e)
        {
            _window.ResetMediaPlayback();
            NowSlide.ResetMedia();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                GoPrevSlide();
            }
            if (e.Key == Key.Right)
            {
                GoNextSlide(); 
            }
        }

        private void ToggleMute(object sender, RoutedEventArgs e)
        {
            if (IsMuted)
            {
                _window.UnMute();
            }
            else
            {
                _window.Mute();
            }
            UpdateMuteButton();
        }

        private void UpdateMuteButton()
        {
            if (IsMuted)
            {
                Muted_icon.Visibility = Visibility.Visible;
                Unmuted_icon.Visibility = Visibility.Hidden;
            }
            else
            {
                Muted_icon.Visibility = Visibility.Hidden;
                Unmuted_icon.Visibility = Visibility.Visible;
            }
        }


        private void FillBlack_Click(object sender, RoutedEventArgs e)
        {
            _window.ToggleFillBlack();
            UpdateFillState();
        }

        private void UpdateFillState()
        {
            if (_window.FillBlack)
            {
                FillBlack.Foreground = Brushes.Red;
            }
            else
            {
                FillBlack.Foreground = Brushes.Black;
            }
        }

        

        private void Merge_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Fork_btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
