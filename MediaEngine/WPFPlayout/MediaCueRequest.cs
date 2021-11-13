using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MediaEngine.WPFPlayout
{
    public delegate void MediaPlaybackPositionArgs(TimeSpan currentPos, TimeSpan duration);
    internal class MediaCueRequest
    {
        public Guid MediaID { get; set; }
        public Uri? MediaSource { get; set; }
        public int CuePriorityOrder { get; set; }
        public UIElement? Player { get; set; }
        public CueState CueState { get; set; }
        public MediaType MediaType { get; set; }


    }
    internal enum MediaType
    {
        Image,
        Video,
        Unsupported,
    }

    internal enum CueState
    {
        Uncued,
        Cueing,
        Ready,
    }

    public enum CueRequestResult
    {
        CueRequestSubmitted,
        CueRejected_NoAvailablePlayer,
        CueRejected_MediaUnsupported,
        CueRequest_AlreadySubmitted,
    }

    public enum ShowCuedResult
    {
        Success_OnAir,
        Failed_NotCued,
        Failed_StillCueing,
    }


}
