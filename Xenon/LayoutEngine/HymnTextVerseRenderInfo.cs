using System.Drawing;

namespace Xenon.LayoutEngine
{
    public class HymnTextVerseRenderInfo
    {
        public HymnTextVerseRenderInfo(string fontname = "Arial", int fontsize = 36)
        {
            TitleFont = new Font(fontname, 20, FontStyle.Regular);
            VerseFont = new Font(fontname, fontsize, FontStyle.Bold);
            CopyrightFont = new Font(fontname, 12, FontStyle.Regular);
            NameFont = new Font(fontname, 40, FontStyle.Bold);
            NumberFont = new Font(fontname, 30, FontStyle.Bold | FontStyle.Italic);
        }

        public Font TitleFont { get; set; }
        public Font VerseFont { get; set; }
        public Font CopyrightFont { get; set; }
        public Font NameFont { get; set; }
        public Font NumberFont { get; set; }


    }
}