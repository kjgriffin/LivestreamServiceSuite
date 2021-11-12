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

        }


        // Track available media playout control resources
        Queue<Image> AvailableImagePlayers = new Queue<Image>();
        Queue<MediaElement> AvailableVideoPlayers = new Queue<MediaElement>();

        List<MediaCueRequest> ActiveMediaCueRequests = new List<MediaCueRequest>();
        MediaCueRequest? OnAirCueRequest = null;
        private int _reqNum = 0;


        private UIElement ActivePlayer = null;
        private int GetNextReqNum()
        {
            return _reqNum++;
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

        public bool StopCurrent()
        {
            ManiuplateActiveVideoPlayer((MediaElement e) => e.Pause());
            return true;
        }

        public bool ShowCuedMedia(Guid mediaID)
        {
            var cuereq = ActiveMediaCueRequests.FirstOrDefault(r => r.MediaID == mediaID);
            if (cuereq != null)
            {
                var lastonair = OnAirCueRequest;

                // TODO: check that it is ready to go, else report not read

                // stop current if needed
                StopCurrent();

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
                ActivePlayer = cuereq.Player;
                OnAirCueRequest = cuereq;

                // un-cue last played
                ActiveMediaCueRequests.Remove(OnAirCueRequest);
                ReleaseCuePlayer(lastonair);

            }
            // TODO: should change to report that it isn't cued
            return false;
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
                MediaCueRequest req = new MediaCueRequest() { CuePriorityOrder = GetNextReqNum(), MediaID = mediaId, MediaSource = imageSource };
                player.Dispatcher.Invoke(() =>
                {
                    req.Player = player;
                });
                ActiveMediaCueRequests.Add(req);
                player.Dispatcher.Invoke(() => player.Source = new BitmapImage(req.MediaSource));
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
                MediaCueRequest req = new MediaCueRequest() { CuePriorityOrder = GetNextReqNum(), MediaID = mediaId, MediaSource = videoSource };
                player.Dispatcher.Invoke(() =>
                {
                    req.Player = player;
                });
                ActiveMediaCueRequests.Add(req);
                player.Dispatcher.Invoke(() => player.LoadedBehavior = MediaState.Manual);
                player.Dispatcher.Invoke(() => player.Source = req.MediaSource);
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
