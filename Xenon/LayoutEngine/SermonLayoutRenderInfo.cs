using System.Drawing;

namespace Xenon.LayoutEngine
{
    public class SermonLayoutRenderInfo
    {
        public SermonLayoutRenderInfo(string fontname = "Arial", int fontsize = 36)
        {
            BoldFont = new Font(fontname, fontsize, FontStyle.Bold);
            RegularFont = new Font(fontname, fontsize, FontStyle.Regular);
            ItalicFont = new Font(fontname, fontsize, FontStyle.Italic);
        }

        public Font BoldFont { get; set; }
        public Font RegularFont { get; set; }
        public Font ItalicFont { get; set; }
    }
}
