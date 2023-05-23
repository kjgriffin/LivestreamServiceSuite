using System.Drawing;

using Xenon.LayoutEngine;

namespace Xenon.SlideAssembly
{
    public class TextHymnLayout
    {
        public Size Size { get; set; }
        public Rectangle NameBox { get; set; }
        public Rectangle TextBox { get; set; }
        public Rectangle NumberBox { get; set; }
        public Rectangle CopyrightBox { get; set; }
        public Rectangle TitleBox { get; set; }

        public HymnTextVerseRenderInfo GetRenderInfo()
        {
            return new HymnTextVerseRenderInfo();
        }
    }
}
