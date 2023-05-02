using System.Drawing;

namespace Xenon.LayoutEngine
{
    public class LiturgyLayoutRenderInfo
    {

        public LiturgyLayoutRenderInfo(string fontname = "Arial", int fontsize = 36)
        {
            BoldFont = new Font(fontname, fontsize, System.Drawing.FontStyle.Bold);
            RegularFont = new Font(fontname, fontsize, System.Drawing.FontStyle.Regular);
        }

        public Font BoldFont { get; set; }
        public Font RegularFont { get; set; }
        public Rectangle TextBox { get; set; }
        public Rectangle SpeakerBox { get; set; }

    }
}
