using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MediaEngine.WPFPlayout
{
    internal class MediaCueRequest
    {
        public Guid MediaID { get; set; }
        public Uri? MediaSource { get; set; }
        public int CuePriorityOrder { get; set; }
        public UIElement Player { get; set; } 


    }
    internal enum MediaType
    {
        Image,
        Video,
        Unsupported,
    }

    public enum CueRequestResult
    {
        CueRequestSubmitted,
        CueRejected_NoAvailablePlayer,
        CueRejected_MediaUnsupported,
    }

}
