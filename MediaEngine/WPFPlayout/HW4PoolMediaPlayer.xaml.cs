using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace MediaEngine.WPFPlayout
{
    /// <summary>
    /// Interaction logic for _4PoolMediaPlayer.xaml
    /// </summary>
    public partial class HW4PoolMediaPlayer : UserControl
    {

        readonly List<string> SupportedImageTypes = new List<string> { ".png", ".bmp", ".jpeg", ".jpg" };
        readonly List<string> SupportedVideoTypes = new List<string> { ".mp4", ".avi" };

        public HW4PoolMediaPlayer()
        {
            InitializeComponent();

            AvailableImagePlayers.Enqueue(still_a);
            AvailableImagePlayers.Enqueue(still_b);
            AvailableImagePlayers.Enqueue(still_c);
            AvailableImagePlayers.Enqueue(still_d);

            AvailableVideoPlayers.Enqueue(clip_a);
            AvailableVideoPlayers.Enqueue(clip_b);
            AvailableVideoPlayers.Enqueue(clip_c);
            AvailableVideoPlayers.Enqueue(clip_d);

            clip_a.MediaOpened += ClipPlayerMediaOpened;
            clip_b.MediaOpened += ClipPlayerMediaOpened;
            clip_c.MediaOpened += ClipPlayerMediaOpened;
            clip_d.MediaOpened += ClipPlayerMediaOpened;

            lowResTimer.Interval = TimeSpan.FromMilliseconds(500);
            highRestTimer.Interval = TimeSpan.FromMilliseconds(16);

            lowResTimer.Tick += CheckMediaPlaybackPosition;
            highRestTimer.Tick += CheckMediaPlaybackPosition;
        }


        private void ClipPlayerMediaOpened(object sender, RoutedEventArgs e)
        {
            /* By leaving the media as auto-play on load we can force the media to begin loading immediately after setting the source
             * Once that's forced it to actually begin loading, we'll catch it here and stop it from playing to far
             * 
             * So we now the the media opened, we'll set it back to the first frame
             * At this point we can get a preview of a cued source, and putting this on air should happen immediately.
             */
            MediaElement media = (MediaElement)sender;

            media.Dispatcher.Invoke(() => media.LoadedBehavior = MediaState.Manual);
            media.Dispatcher.Invoke(() => media.Pause());
            media.Dispatcher.Invoke(() => media.Position = TimeSpan.FromMilliseconds(1));
            media.Dispatcher.Invoke(() => media.Volume = 1);

            // mark the request as having been cued and now ready for playback 
            var originatingreq = ActiveMediaCueRequests.FirstOrDefault(r => r.Player == media);
            if (originatingreq != null)
            {
                originatingreq.CueState = CueState.Ready;
            }

        }



        // Track available media playout control resources
        Queue<Image> AvailableImagePlayers = new Queue<Image>();
        Queue<MediaElement> AvailableVideoPlayers = new Queue<MediaElement>();

        List<MediaCueRequest> ActiveMediaCueRequests = new List<MediaCueRequest>();
        MediaCueRequest? OnAirCueRequest = null;
        private int _reqNum = 0;


        // Playback Timers
        public enum TimerMode
        {
            HighRes,
            LowRes,
        }
        DispatcherTimer lowResTimer = new DispatcherTimer();
        DispatcherTimer highRestTimer = new DispatcherTimer();
        public TimerMode PlaybackPositionTimerResolution { get; private set; } = TimerMode.LowRes;
        public void SetPlaybackPositionTimerResolution(TimerMode res)
        {
            if (res == TimerMode.HighRes)
            {
                if (lowResTimer.IsEnabled)
                {
                    lowResTimer.Stop();
                    highRestTimer.Start();
                }
            }
            else if (res == TimerMode.LowRes)
            {
                if (highRestTimer.IsEnabled)
                {
                    highRestTimer.Stop();
                    lowResTimer.Start();
                }
            }
        }
        private void CheckMediaPlaybackPosition(object? sender, EventArgs e)
        {
            var player = ActivePlayer as MediaElement;
            if (player != null)
            {
                TimeSpan currentpos = TimeSpan.Zero;
                TimeSpan duration = TimeSpan.Zero;
                player.Dispatcher.Invoke(() => currentpos = player.Position);
                player.Dispatcher.Invoke(() => duration = player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : TimeSpan.Zero);

                OnMediaPlaybackPositionChanged?.Invoke(currentpos, duration);
            }
        }

        public event MediaPlaybackPositionArgs? OnMediaPlaybackPositionChanged;


        private UIElement? ActivePlayer = null;
        private int GetNextReqNum()
        {
            return _reqNum++;
        }

        public Brush? GetOnAirVisual()
        {
            VisualBrush? b = null;
            if (OnAirCueRequest != null)
            {
                OnAirCueRequest.Player?.Dispatcher.Invoke(() => b = new VisualBrush(OnAirCueRequest.Player));
            }
            return b;
        }

        public Brush? GetVisualForCueRequest(Guid mediaID)
        {
            VisualBrush? b = null;
            var req = ActiveMediaCueRequests.FirstOrDefault(x => x.MediaID == mediaID);
            if (req != null)
            {
                req?.Player?.Dispatcher.Invoke(() => b = new VisualBrush(req.Player));
            }
            return b;
        }

        private void ManiuplateActiveVideoPlayer(Action<MediaElement> a)
        {
            MediaElement? aplayer = ActivePlayer as MediaElement;
            if (aplayer != null)
            {
                aplayer.Dispatcher.Invoke(() => a(aplayer));
            }
        }
        public bool PlayCurrent()
        {
            ManiuplateActiveVideoPlayer((MediaElement e) => e.Position = TimeSpan.FromSeconds(1));
            ManiuplateActiveVideoPlayer((MediaElement e) => e.Play());
            return true;
        }

        public bool PauseCurrent()
        {
            ManiuplateActiveVideoPlayer((MediaElement e) => e.Pause());
            return true;
        }
        public bool StopCurrent()
        {
            ManiuplateActiveVideoPlayer((MediaElement e) => e.Stop());
            return true;
        }

        public ShowCuedResult ShowCuedMedia(Guid mediaID)
        {
            var cuereq = ActiveMediaCueRequests.FirstOrDefault(r => r.MediaID == mediaID);
            if (cuereq != null)
            {
                var lastonair = OnAirCueRequest;

                if (cuereq.CueState != CueState.Ready)
                {
                    return ShowCuedResult.Failed_StillCueing;
                }

                // stop current if needed
                PauseCurrent();

                // Stop playback timers
                highRestTimer.Stop();
                lowResTimer.Stop();

                // show it
                if (display_a.Visibility == Visibility.Hidden)
                {
                    display_a.Dispatcher.Invoke(() => display_a.Background = new VisualBrush(cuereq.Player));
                    display_a.Dispatcher.Invoke(() => display_a.Visibility = Visibility.Visible);
                    display_b.Dispatcher.Invoke(() => display_b.Visibility = Visibility.Hidden);
                }
                else if (display_b.Visibility == Visibility.Hidden)
                {
                    display_b.Dispatcher.Invoke(() => display_b.Background = new VisualBrush(cuereq.Player));
                    display_b.Dispatcher.Invoke(() => display_b.Visibility = Visibility.Visible);
                    display_a.Dispatcher.Invoke(() => display_a.Visibility = Visibility.Hidden);
                }
                // Re-enable timers if video
                if (cuereq.MediaType == MediaType.Video)
                {
                    if (PlaybackPositionTimerResolution == TimerMode.HighRes)
                    {
                        highRestTimer.Start();
                    }
                    else if (PlaybackPositionTimerResolution == TimerMode.LowRes)
                    {
                        lowResTimer.Start();
                    }
                }

                ActivePlayer = cuereq.Player;
                OnAirCueRequest = cuereq;

                // un-cue last played
                ActiveMediaCueRequests.Remove(OnAirCueRequest);
                ReleaseCuePlayer(lastonair);

                return ShowCuedResult.Success_OnAir;
            }
            return ShowCuedResult.Failed_NotCued;
        }

        public CueRequestResult TryCueMedia(Uri mediaSource, Guid mediaID)
        {
            // generate cuerequest for internal tracking and have it assigned to a playout control
            // if no playout control available we'll reject the request return false
            // check the source and see what type it is

            var mtype = InspectMediaForType(mediaSource);
            // cue on available player
            if (mtype == MediaType.Image)
            {
                return TryCueImage(mediaSource, mediaID);
            }
            else if (mtype == MediaType.Video)
            {
                return TryCueVideo(mediaSource, mediaID);
            }
            return CueRequestResult.CueRejected_MediaUnsupported;
        }

        private CueRequestResult TryCueImage(Uri imageSource, Guid mediaId)
        {
            // if we have an available image player we'll cue on that, otherwise we cant cue
            if (AvailableImagePlayers.Any())
            {
                var player = AvailableImagePlayers.Dequeue();
                // TODO: I think images do cue-immediately, but should probably check
                MediaCueRequest req = new MediaCueRequest() { CuePriorityOrder = GetNextReqNum(), MediaID = mediaId, MediaSource = imageSource, CueState = CueState.Uncued, MediaType = MediaType.Image };
                player.Dispatcher.Invoke(() =>
                {
                    req.Player = player;
                });
                ActiveMediaCueRequests.Add(req);
                player.Dispatcher.Invoke(() => player.Source = new BitmapImage(req.MediaSource));
                req.CueState = CueState.Ready;
                return CueRequestResult.CueRequestSubmitted;
            }
            return CueRequestResult.CueRejected_NoAvailablePlayer;
        }

        private CueRequestResult TryCueVideo(Uri videoSource, Guid mediaId)
        {
            // if we have an available image player we'll cue on that, otherwise we cant cue
            if (AvailableVideoPlayers.Any())
            {
                var player = AvailableVideoPlayers.Dequeue();
                MediaCueRequest req = new MediaCueRequest() { CuePriorityOrder = GetNextReqNum(), MediaID = mediaId, MediaSource = videoSource, CueState = CueState.Uncued, MediaType = MediaType.Video };
                player.Dispatcher.Invoke(() =>
                {
                    req.Player = player;
                });
                ActiveMediaCueRequests.Add(req);


                /*
                 *  Bit of a hack here: we can force the media player to begin loading now (even though it shouldn't be 'on-air'
                 *  we make sure we don't get unintended audio playout
                 *  then (here's the magic!) we set the player to play as the loaded behaviour. this will force it to begin loading the media when we set it
                 *  we'll set the media and we (elsewhere) catch the loaded event and stop playout and hold it ready on the first frame
                 */
                player.Dispatcher.Invoke(() => player.Volume = 0);
                player.Dispatcher.Invoke(() => player.LoadedBehavior = MediaState.Play);
                player.Dispatcher.Invoke(() => player.Source = req.MediaSource);

                req.CueState = CueState.Cueing;


                return CueRequestResult.CueRequestSubmitted;
            }
            return CueRequestResult.CueRejected_NoAvailablePlayer;
        }

        private void ReleaseCuePlayer(MediaCueRequest? cuereq)
        {
            if (cuereq?.Player != null)
            {
                if (cuereq.Player is MediaElement)
                {
                    AvailableVideoPlayers.Enqueue((MediaElement)cuereq.Player);
                }
                if (cuereq.Player is Image)
                {
                    AvailableImagePlayers.Enqueue((Image)cuereq.Player);
                }
            }
        }




        private MediaType InspectMediaForType(Uri source)
        {
            if (source.IsFile)
            {
                var ext = System.IO.Path.GetExtension(source.AbsolutePath);
                if (SupportedImageTypes.Contains(ext))
                {
                    return MediaType.Image;
                }
                else if (SupportedVideoTypes.Contains(ext))
                {
                    return MediaType.Video;
                }
            }
            return MediaType.Unsupported;
        }



    }
}
