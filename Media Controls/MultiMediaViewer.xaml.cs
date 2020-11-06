using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Media_Controls
{


    public delegate void ImageAvailableArgs(object sender);
    public delegate void VideoAvailableArgs(object sender);
    public delegate void VideoPlaybackPositionUpdate(object sender, TimeSpan currentPosition, TimeSpan duration, TimeSpan remaining);

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MultiMediaViewer : UserControl
    {
        public MultiMediaViewer()
        {
            InitializeComponent();
            VideoPlayer_A.LoadedBehavior = MediaState.Manual;
            VideoPlayer_B.LoadedBehavior = MediaState.Manual;

            VideoPlayer_A.MediaOpened += VideoPlayer_A_MediaOpened;
            VideoPlayer_B.MediaOpened += VideoPlayer_B_MediaOpened;

            VideoPlayer_A.MediaEnded += VideoPlayer_A_MediaEnded;
            VideoPlayer_B.MediaEnded += VideoPlayer_B_MediaEnded;

            MediaPlaybackTimer = new Timer(500);
            MediaPlaybackTimer.Elapsed += MediaPlaybackTimer_Elapsed;

            imageactive = false;
            videoactive = false;
            activeimageplayer = 2;
            activevideoplayer = 2;
        }

        private bool imageactive;
        private int activeimageplayer;
        private bool videoactive;
        private int activevideoplayer;
        private bool loopvideoa;
        private bool loopvideob;
        private bool activecolor;

        private Timer MediaPlaybackTimer;


        public event VideoAvailableArgs OnNextVideoCued;
        public event ImageAvailableArgs OnNextImageCued;
        public event VideoPlaybackPositionUpdate OnVideoPlaybackPositionUpdated;



        private void MediaPlaybackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MediaElement player;
            if (videoactive)
            {
                if (activevideoplayer == 1)
                {
                    player = VideoPlayer_A; 
                }
                else
                {
                    player = VideoPlayer_B;
                }

                TimeSpan remaining = player.NaturalDuration.TimeSpan - player.Position;
                OnVideoPlaybackPositionUpdated?.Invoke(this, player.Position, player.NaturalDuration.TimeSpan, remaining);
            }
        }

        private void VideoPlayer_B_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (loopvideob)
            {
                VideoPlayer_B.Position = TimeSpan.Zero;
                VideoPlayer_B.Play();
            }
        }

        private void VideoPlayer_A_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (loopvideob)
            {
                VideoPlayer_A.Position = TimeSpan.Zero;
                VideoPlayer_A.Play();
            }
        }

        private void VideoPlayer_B_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (activevideoplayer != 2)
            {
                OnNextVideoCued?.Invoke(this);
            }
        }

        private void VideoPlayer_A_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (activevideoplayer != 1)
            {
                OnNextVideoCued?.Invoke(this);
            }
        }



        /// <summary>
        /// Cue up the next image to display. Raises an ImageReady event when image is loaded.
        /// </summary>
        /// <param name="ImageSource"></param>
        public void CueNextImage(Uri ImageSource)
        {
            if (activeimageplayer == 1)
            {
                ImagePlayer_B.Source = new BitmapImage(ImageSource);
                OnNextImageCued?.Invoke(this);
            }
            else
            {
                ImagePlayer_A.Source = new BitmapImage(ImageSource);
                OnNextImageCued?.Invoke(this);
            }
        }

        /// <summary>
        /// Displays the cued image if available, stopping video playback if nessecary. Otherwise does nothing.
        /// </summary>
        public void ShowCuedImage()
        {
            MediaPlaybackTimer.Stop();
            videoactive = false;
            VideoPlayer_A.Pause();
            VideoPlayer_B.Pause();

            if (activeimageplayer == 1)
            {
                ImagePlayer_B.Visibility = Visibility.Visible;
                ImagePlayer_A.Visibility = Visibility.Hidden;
                activeimageplayer = 2;
            }
            else
            {
                ImagePlayer_A.Visibility = Visibility.Visible;
                ImagePlayer_B.Visibility = Visibility.Hidden;
                activeimageplayer = 1;
            }

            VideoPlayer_A.Visibility = Visibility.Hidden;
            VideoPlayer_B.Visibility = Visibility.Hidden;
            ColorRect.Visibility = Visibility.Hidden;

            activecolor = false;
            imageactive = true;
        }

        /// <summary>
        /// Cue up the next video to display. Raises a VideoReady event when video is loaded.
        /// </summary>
        /// <param name="VideoSource"></param>
        public void CueNextVideo(Uri VideoSource)
        {
            if (activevideoplayer == 1)
            {
                VideoPlayer_B.Source = VideoSource;
                loopvideob = false;
            }
            else
            {
                VideoPlayer_A.Source = VideoSource;
                loopvideoa = true;
            }
        }

        /// <summary>
        /// Displays the cued video. Sets position to start of video. Will not start playback.
        /// </summary>
        public void ShowCuedVideo()
        {
            MediaPlaybackTimer.Start();

            if (activevideoplayer == 1)
            {
                VideoPlayer_A.Pause();
                VideoPlayer_B.Visibility = Visibility.Visible;
                VideoPlayer_A.Visibility = Visibility.Hidden;
                activevideoplayer = 2;
            }
            else
            {
                VideoPlayer_B.Pause();
                VideoPlayer_A.Visibility = Visibility.Visible;
                VideoPlayer_B.Visibility = Visibility.Hidden;
                activevideoplayer = 1;
            }

            videoactive = true;
            activecolor = false;
            imageactive = false;
        }

        /// <summary>
        /// Will start playback from current position.
        /// </summary>
        public void PlayVideo()
        {
            if (videoactive)
            {
                if (activevideoplayer == 1)
                {
                    VideoPlayer_A.Play();
                }
                else
                {
                    VideoPlayer_B.Play();
                }
            }
        }

        /// <summary>
        /// Pauses the displayed video.
        /// </summary>
        public void PauseVideo()
        {
            if (activevideoplayer == 1)
            {
                VideoPlayer_A.Pause();
            }
            else
            {
                VideoPlayer_B.Pause();
            }

        }

        /// <summary>
        /// Stops the displayed video, resetting the position to the beginning.
        /// </summary>
        public void StopVideo()
        {
            if (activevideoplayer == 1)
            {
                VideoPlayer_A.Stop();
                loopvideoa = false;
            }
            else
            {
                VideoPlayer_B.Stop();
                loopvideob = false;
            }
        }

        /// <summary>
        /// Resets the position of the displayed video to the beginning then begins playback.
        /// </summary>
        public void RestartVideo()
        {
            if (activevideoplayer == 1)
            {
                VideoPlayer_A.Stop();
                VideoPlayer_A.Play();
            }
            else
            {
                VideoPlayer_B.Stop();
                VideoPlayer_B.Play();
            }
        }

        /// <summary>
        /// Sets the position of the displayed video to the beginning then begins looped playback.
        /// </summary>
        public void LoopVideo()
        {
            if (activevideoplayer == 1)
            {
                VideoPlayer_A.Play();
                loopvideoa = true;
            }
            else
            {
                VideoPlayer_B.Play();
                loopvideoa = true;
            }
        }

        /// <summary>
        /// Immediatley overrides the current display to show the specified color. Stops all current media playback.
        /// </summary>
        public void ShowColor(Color color)
        {
            MediaPlaybackTimer.Stop();
            BackgroundFill.Color = color;
            videoactive = false;
            VideoPlayer_A.Pause();
            VideoPlayer_B.Pause();
            ColorRect.Visibility = Visibility.Visible;
            VideoPlayer_A.Visibility = Visibility.Hidden;
            VideoPlayer_B.Visibility = Visibility.Hidden;
            ImagePlayer_A.Visibility = Visibility.Hidden;
            ImagePlayer_B.Visibility = Visibility.Hidden;
            activecolor = true;
            imageactive = false;
        }





    }
}