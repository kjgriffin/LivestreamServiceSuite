using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter
{
    /// <summary>
    /// Interaction logic for PresenterDisplay.xaml
    /// </summary>
    public partial class PresenterDisplay : Window
    {

        MainWindow _control;

        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdated;

        public PresenterDisplay(MainWindow parent)
        {
            InitializeComponent();
            _control = parent;
            mediaPlayer.OnMediaPlaybackTimeUpdate += MediaPlayer_OnMediaPlaybackTimeUpdate;
        }

        private void _controlPanel_OnWindowClosing(object sender, EventArgs e)
        {
            mediaPlayer.OnMediaPlaybackTimeUpdate -= MediaPlayer_OnMediaPlaybackTimeUpdate;
        }

        private void MediaPlayer_OnMediaPlaybackTimeUpdate(object sender, MediaPlaybackTimeEventArgs e)
        {
            _control.Dispatcher.Invoke(() =>
            {
                OnMediaPlaybackTimeUpdated?.Invoke(this, e);
            });
        }

        public void StartMediaPlayback()
        {
            if (_control.Presentation.Current.Type == Integrated_Presenter.SlideType.Video)
            {
                mediaPlayer.PlayMedia();
            }
        }

        public void PauseMediaPlayback()
        {
            if (_control.Presentation.Current.Type == Integrated_Presenter.SlideType.Video)
            {
                mediaPlayer.PauseMedia();
            }
        }

        public void RestartMediaPlayback()
        {
            if (_control.Presentation.Current.Type == Integrated_Presenter.SlideType.Video)
            {
                mediaPlayer.ReplayMedia();
            }
        }


        public void ShowSlide()
        {
            if (_control.Presentation.Current.Type == SlideType.Video)
            {
                ShowVideo();
            }
            else
            {
                ShowImage();
            }
        }


        private void ShowImage()
        {
            mediaPlayer.SetMedia(_control.Presentation.Current);
        }

        private void ShowVideo()
        {
            mediaPlayer.SetMedia(_control.Presentation.Current);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {

        }
    }
}
