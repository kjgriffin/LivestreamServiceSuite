using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;

namespace Presenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PresenterWindow : Window
    {
        public List<(string path, SlideType type)> Slides { get; set; }

        private int _slideNum;
        public PresenterWindow(List<(string path, SlideType type)> slides)
        {
            InitializeComponent();
            _slideNum = 0;
            Slides = slides;

            // open control panel
            PresenterControl controlPanel = new PresenterControl(this);
            controlPanel.Show();


            // start presentation at slide 0
            ShowSlide();
        }

        public void NextSlide()
        {
            if (_slideNum + 1 <= Slides.Count - 1)
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
                videoPlayer.Play();
            }
        }

        public void PauseMediaPlayback()
        {
            if (Slides[_slideNum].type == SlideType.Video)
            {
                videoPlayer.Pause();
            }
        }

        public void RestartMediaPlayback()
        {
            if (Slides[_slideNum].type == SlideType.Video)
            {
                videoPlayer.Position = TimeSpan.Zero;
                videoPlayer.Play();
            }
        }


        private void ShowSlide()
        {
            if (_slideNum >= 0 && _slideNum < Slides.Count - 1)
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
            // arrest all video playback
            videoPlayer.Stop();
            // hide videoplayer
            videoPlayer.Visibility = Visibility.Hidden;
            // show image
            imagePlayer.Visibility = Visibility.Visible;
            imagePlayer.Source = new BitmapImage(new Uri(Slides[_slideNum].path));

        }

        private void ShowVideo()
        {
            // hide image
            imagePlayer.Visibility = Visibility.Hidden;
            // show videoplayer
            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Visibility = Visibility.Visible;
            videoPlayer.Source = new Uri(Slides[_slideNum].path);
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
    }
}
