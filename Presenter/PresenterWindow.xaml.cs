using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Runtime.CompilerServices;
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

namespace Presenter
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PresenterWindow : Window
    {
        public List<(string path, SlideType type)> Slides { get; set; }
        public int CurrentSlideNum
        {
            get
            {
                return IsForked ? _slideNum + 1 : _trueSlideNum + 1;
            }
        }

        public int OutputSlideNum
        {
            get
            {
                return _trueSlideNum + 1;
            }
        }


        public (string path, SlideType type) CurrentSlide { get => Slides[_trueSlideNum]; }

        public bool IsForked { get; private set; }

        public bool IsMute { get => mediaPlayer.IsMute; }

        private int _trueSlideNum;
        private int _slideNum;



        private PresenterControl _controlPanel;

        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdated;

        public PresenterWindow(List<(string path, SlideType type)> slides)
        {
            InitializeComponent();
            _trueSlideNum = 0;
            Slides = slides;


            AttachControlPanel();


            // start presentation at slide 0
            ShowSlide();


        }

        private void AttachControlPanel()
        {
            // open control panel
            _controlPanel = new PresenterControl(this);
            _controlPanel.Show();
            _controlPanel.OnWindowClosing += _controlPanel_OnWindowClosing;
            mediaPlayer.OnMediaPlaybackTimeUpdate += MediaPlayer_OnMediaPlaybackTimeUpdate;
        }

        private void _controlPanel_OnWindowClosing(object sender, EventArgs e)
        {
            mediaPlayer.OnMediaPlaybackTimeUpdate -= MediaPlayer_OnMediaPlaybackTimeUpdate;
            _controlPanel.OnWindowClosing -= _controlPanel_OnWindowClosing;
            _controlPanel = null;
        }

        private void MediaPlayer_OnMediaPlaybackTimeUpdate(object sender, MediaPlaybackTimeEventArgs e)
        {
            _controlPanel.Dispatcher.Invoke(() =>
            {
                OnMediaPlaybackTimeUpdated?.Invoke(this, e);
            });
        }

        public void NextSlide()
        {
            if (IsForked)
            {
                if (_slideNum + 1 < Slides.Count)
                {
                    _slideNum += 1;
                }
            }
            else
            {
                if (_trueSlideNum + 1 < Slides.Count)
                {
                    _trueSlideNum += 1;
                    ShowSlide();
                }
            }
        }

        public void PrevSlide()
        {
            if (IsForked)
            {
                if (_slideNum - 1 >= 0)
                {
                    _slideNum -= 1;
                }
            }
            else
            {
                if (_trueSlideNum - 1 >= 0)
                {
                    _trueSlideNum -= 1;
                    ShowSlide();
                }
            }
        }

        public void Fork()
        {
            IsForked = true;
            _slideNum = _trueSlideNum;
        }

        public void Merge()
        {
            _trueSlideNum = _slideNum;
            IsForked = false;
            ShowSlide();
        }

        public void StartMediaPlayback()
        {
            if (Slides[_trueSlideNum].type == SlideType.Video)
            {
                mediaPlayer.PlayMedia();
            }
        }

        public void PauseMediaPlayback()
        {
            if (Slides[_trueSlideNum].type == SlideType.Video)
            {
                mediaPlayer.PauseMedia();
            }
        }

        public void RestartMediaPlayback()
        {
            if (Slides[_trueSlideNum].type == SlideType.Video)
            {
                mediaPlayer.ReplayMedia();
            }
        }

        public void ResetMediaPlayback()
        {

            if (Slides[_trueSlideNum].type == SlideType.Video)
            {
                mediaPlayer.ResetMedia();
            }
        }


        private void ShowSlide()
        {
            if (_trueSlideNum >= 0 && _trueSlideNum < Slides.Count)
            {
                // try showing either picture or video
                if (Slides[_trueSlideNum].type == SlideType.Image)
                {
                    ShowImage();
                }
                else
                {
                    ShowVideo();
                }
            }
        }


        private void ShowImage()
        {
            if (FillBlack)
            {
                mediaPlayer.SetMediaUnderBlack(new Uri(Slides[_trueSlideNum].path), SlideType.Image);
            }
            else
            {
                mediaPlayer.SetMedia(new Uri(Slides[_trueSlideNum].path), SlideType.Image);
            }
        }

        private void ShowVideo()
        {
            if (FillBlack)
            {
                mediaPlayer.SetMediaUnderBlack(new Uri(Slides[_trueSlideNum].path), SlideType.Video);
            }
            else
            {
                mediaPlayer.SetMedia(new Uri(Slides[_trueSlideNum].path), SlideType.Video);
            }
        }


        public void Mute()
        {
            mediaPlayer.Mute();
        }

        public void UnMute()
        {
            mediaPlayer.UnMute();
        }


        public bool FillBlack { get; private set; } = false;
        public void ToggleFillBlack()
        {
            FillBlack = !FillBlack;

            if (FillBlack)
            {
                mediaPlayer.ShowBlackSource();
            }
            else
            {
                mediaPlayer.HideBlackSource();
            }

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                ToggleFullScreen();
            }
            if (e.Key == Key.Escape)
            {
                ExitFullscreen();
            }
            if (e.Key == Key.C)
            {
                if (_controlPanel == null)
                {
                    AttachControlPanel();
                }
            }

        }

        private void ExitFullscreen()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
        }

        private void EnterFullscreen()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }

        private void ToggleFullScreen()
        {
            if (WindowState == WindowState.Normal)
            {
                EnterFullscreen();
            }
            else
            {
                ExitFullscreen();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _controlPanel?.Close();
        }
    }
}
