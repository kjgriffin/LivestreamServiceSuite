using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;

namespace SlideCreater.LayoutEngine
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
