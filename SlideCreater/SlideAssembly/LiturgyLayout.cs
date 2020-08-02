using SlideCreater.LayoutEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class LiturgyLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle Speaker { get; set; }
        public Rectangle Text { get; set; }
        public int InterLineSpacing { get; set; }

        public LiturgyLayoutRenderInfo GetRenderInfo()
        {
            var res = new LiturgyLayoutRenderInfo();
            res.SpeakerBox = Speaker;
            res.TextBox = Text;
            return res;
        }
    }
}
