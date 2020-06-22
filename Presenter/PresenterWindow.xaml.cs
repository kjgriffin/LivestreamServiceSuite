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
        public int CurrentSlideNum { get => _slideNum + 1; }

        public (string path, SlideType type) CurrentSlide { get => Slides[_slideNum]; }

        public bool IsMute { get => mediaPlayer.IsMute; }

        private int _slideNum;



        private PresenterControl _controlPanel;

        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdated;

        public PresenterWindow(List<(string path, SlideType type)> slides)
        {
            InitializeComponent();
            _slideNum = 0;
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
            if (_slideNum + 1 < Slides.Count)
            {
                _slideNum += 1;
                ShowSlide();
            }
        }

        public void PrevSlide()
        {
            if (_slideNum - 1 >= 0)
            {
                _slideNum -= 1;
                ShowSlide();
            }
        }

        public void StartMediaPlayback()
        {
            if (Slides[_slideNum].type == SlideType.Video)
            {
                mediaPlayer.PlayMedia();
            }
        }

        public void PauseMediaPlayback()
        {
            if (Slides[_slideNum].type == SlideType.Video)
            {
                mediaPlayer.PauseMedia();
            }
        }

        public void RestartMediaPlayback()
        {
            if (Slides[_slideNum].type == SlideType.Video)
            {
                mediaPlayer.ReplayMedia();
            }
        }

        public void ResetMediaPlayback()
        {

            if (Slides[_slideNum].type == SlideType.Video)
            {
                mediaPlayer.ResetMedia();
            }
        }


        private void ShowSlide()
        {
            if (_slideNum >= 0 && _slideNum < Slides.Count)
            {
                // try showing either picture or video
                if (Slides[_slideNum].type == SlideType.Image)
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
                mediaPlayer.SetMediaUnderBlack(new Uri(Slides[_slideNum].path), SlideType.Image);
            }
            else
            {
                mediaPlayer.SetMedia(new Uri(Slides[_slideNum].path), SlideType.Image);
            }
        }

        private void ShowVideo()
        {
            if (FillBlack)
            {
                mediaPlayer.SetMediaUnderBlack(new Uri(Slides[_slideNum].path), SlideType.Video);
            }
            else
            {
                mediaPlayer.SetMedia(new Uri(Slides[_slideNum].path), SlideType.Video);
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
