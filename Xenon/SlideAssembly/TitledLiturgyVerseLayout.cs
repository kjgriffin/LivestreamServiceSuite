using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xenon.SlideAssembly
{
    public class TitledLiturgyVerseLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle Textbox { get; set; }
        public Rectangle TitleLine { get; set; }
        public Font Font { get; set; }
    }
}
