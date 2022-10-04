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

namespace IntegratedPresenter.Main
{
    /// <summary>
    /// Interaction logic for PresenterDisplay.xaml
    /// </summary>
    public partial class PresenterDisplay : Window
    {

        MainWindow _control;

        public bool IsWindowVisilbe { get; set; }

        public event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdated;

        private int activeplayer;
        private int prevplayer;
        private int nextplayer;

        private Guid curentslide;
        private Guid prevslide;
        private Guid nextslide;

        private bool ShowKey;

        public PresenterDisplay(MainWindow parent, bool ShowKey)
        {
            InitializeComponent();
            _control = parent;
            mediaPlayerA.OnMediaPlaybackTimeUpdate += MediaPlayer_OnMediaPlaybackTimeUpdateA;
            mediaPlayerB.OnMediaPlaybackTimeUpdate += MediaPlayer_OnMediaPlaybackTimeUpdateB;
            mediaPlayerC.OnMediaPlaybackTimeUpdate += MediaPlayer_OnMediaPlaybackTimeUpdateC;

            mediaPlayerA.ShowTileBackground(false);
            mediaPlayerB.ShowTileBackground(false);
            mediaPlayerC.ShowTileBackground(false);

            activeplayer = 2;
            prevplayer = 1;
            nextplayer = 3;

            blackcover.Visibility = Visibility.Hidden;

            this.ShowKey = ShowKey;
            ShowSlide(this.ShowKey);

            Title = ShowKey ? "Presentation Key Source" : "Presentation Display";

        }

        private void _controlPanel_OnWindowClosing(object sender, EventArgs e)
        {
            mediaPlayerA.OnMediaPlaybackTimeUpdate -= MediaPlayer_OnMediaPlaybackTimeUpdateA;
            mediaPlayerB.OnMediaPlaybackTimeUpdate -= MediaPlayer_OnMediaPlaybackTimeUpdateB;
            mediaPlayerC.OnMediaPlaybackTimeUpdate -= MediaPlayer_OnMediaPlaybackTimeUpdateC;
        }

        private void MediaPlayer_OnMediaPlaybackTimeUpdateA(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateMediaPlaybackTime(1, e);
        }

        private void MediaPlayer_OnMediaPlaybackTimeUpdateB(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateMediaPlaybackTime(2, e);
        }

        private void MediaPlayer_OnMediaPlaybackTimeUpdateC(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateMediaPlaybackTime(3, e);
        }


        private void UpdateMediaPlaybackTime(int player, MediaPlaybackTimeEventArgs e)
        {
            if (activeplayer == player)
            {
                _control.Dispatcher.Invoke(() =>
                {
                    OnMediaPlaybackTimeUpdated?.Invoke(this, e);
                });
            }
        }

        private void StopNonActiveMedia()
        {
            switch (activeplayer)
            {
                case 1:
                    mediaPlayerB.StopMedia();
                    mediaPlayerC.StopMedia();
                    break;
                case 2:
                    mediaPlayerA.StopMedia();
                    mediaPlayerC.StopMedia();
                    break;
                case 3:
                    mediaPlayerA.StopMedia();
                    mediaPlayerB.StopMedia();
                    break;
            }
        }

        public void StartMediaPlayback()
        {
            StopNonActiveMedia();
            if (_control.Presentation.EffectiveCurrent.Type == IntegratedPresenter.Main.SlideType.Video || _control.Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
            {
                switch (activeplayer)
                {
                    case 1:
                        mediaPlayerA.PlayMedia();
                        break;
                    case 2:
                        mediaPlayerB.PlayMedia();
                        break;
                    case 3:
                        mediaPlayerC.PlayMedia();
                        break;
                }
            }
        }

        public void PauseMediaPlayback()
        {
            StopNonActiveMedia();
            if (_control.Presentation.EffectiveCurrent.Type == IntegratedPresenter.Main.SlideType.Video || _control.Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
            {
                switch (activeplayer)
                {
                    case 1:
                        mediaPlayerA.PauseMedia();
                        break;
                    case 2:
                        mediaPlayerB.PauseMedia();
                        break;
                    case 3:
                        mediaPlayerC.PauseMedia();
                        break;
                }
            }
        }

        public void RestartMediaPlayback()
        {
            StopNonActiveMedia();
            if (_control.Presentation.EffectiveCurrent.Type == IntegratedPresenter.Main.SlideType.Video || _control.Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
            {
                switch (activeplayer)
                {
                    case 1:
                        mediaPlayerA.ReplayMedia();
                        break;
                    case 2:
                        mediaPlayerB.ReplayMedia();
                        break;
                    case 3:
                        mediaPlayerC.ReplayMedia();
                        break;
                }
            }
        }

        public void StopMediaPlayback()
        {
            StopNonActiveMedia();
            if (_control.Presentation.EffectiveCurrent.Type == SlideType.Video || _control.Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
            {
                switch (activeplayer)
                {
                    case 1:
                        mediaPlayerA.StopMedia();
                        break;
                    case 2:
                        mediaPlayerB.StopMedia();
                        break;
                    case 3:
                        mediaPlayerC.StopMedia();
                        break;
                }
            }
        }

        public void MuteMedia()
        {
            mediaPlayerA.videoPlayer.Volume = 0;
            mediaPlayerB.videoPlayer.Volume = 0;
            mediaPlayerC.videoPlayer.Volume = 0;
        }

        public void UnMuteMedia()
        {
            mediaPlayerA.videoPlayer.Volume = 1;
            mediaPlayerB.videoPlayer.Volume = 1;
            mediaPlayerC.videoPlayer.Volume = 1;
        }


        public void ShowSlide(bool asKey)
        {
            ISlide slidetoshow = _control.Presentation.EffectiveCurrent;

            if (slidetoshow.Guid == curentslide)
            {
                // we're displaying it!!
            }
            else if (slidetoshow.Guid == nextslide)
            {
                // we've cued it on our nextplayer
                var swap = activeplayer;
                activeplayer = nextplayer;
                nextplayer = swap;
                ShowActivePlayer();
            }
            else if (slidetoshow.Guid == prevslide)
            {
                // we've cued it on our prevplayer
                var swap = activeplayer;
                activeplayer = prevplayer;
                prevplayer = swap;
                ShowActivePlayer();
            }
            else
            {
                // we don't have this one cued
                // hot-swap into the active player
                // show black for now
                blackcover.Visibility = Visibility.Visible;
                SetSlideForPlayer(activeplayer, slidetoshow);
                // show it from next player
                ShowActivePlayer();
                blackcover.Visibility = Visibility.Hidden;
            }




            // Update next/prev players if needed
            if (_control.Presentation.Next.Guid != nextslide)
            {
                // pre-cue the next slide into the next player
                SetSlideForPlayer(nextplayer, _control.Presentation.Next);
            }
            if (_control.Presentation.Prev.Guid != prevslide)
            {
                // pre-cue the next slide into the prev player
                SetSlideForPlayer(prevplayer, _control.Presentation.Prev);
            }




            curentslide = _control.Presentation.EffectiveCurrent.Guid;
            nextslide = _control.Presentation.Next.Guid;
            prevslide = _control.Presentation.Prev.Guid;
        }

        private void ShowActivePlayer()
        {
            StopNonActiveMedia();
            if (activeplayer == 1)
            {
                mediaPlayerA.Visibility = Visibility.Visible;
                mediaPlayerB.Visibility = Visibility.Hidden;
                mediaPlayerC.Visibility = Visibility.Hidden;
            }
            else if (activeplayer == 2)
            {
                mediaPlayerB.Visibility = Visibility.Visible;
                mediaPlayerA.Visibility = Visibility.Hidden;
                mediaPlayerC.Visibility = Visibility.Hidden;
            }
            else if (activeplayer == 3)
            {
                mediaPlayerC.Visibility = Visibility.Visible;
                mediaPlayerB.Visibility = Visibility.Hidden;
                mediaPlayerA.Visibility = Visibility.Hidden;
            }
        }


        private void SetSlideForPlayer(int player, ISlide slide)
        {
            switch (player)
            {
                case 1:
                    mediaPlayerA.SetMedia(slide, ShowKey);
                    break;
                case 2:
                    mediaPlayerB.SetMedia(slide, ShowKey);
                    break;
                case 3:
                    mediaPlayerC.SetMedia(slide, ShowKey);
                    break;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsWindowVisilbe = true;
        }

        private void window_Closed(object sender, EventArgs e)
        {
            IsWindowVisilbe = false;
        }
    }
}
