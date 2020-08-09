﻿using SlideCreater.LayoutEngine;
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
        public Font Font { get; set; }

        public LiturgyLayoutRenderInfo GetRenderInfo()
        {
            var res = new LiturgyLayoutRenderInfo
            {
                BoldFont = new Font(Font, FontStyle.Bold),
                RegularFont = Font,
                SpeakerBox = Speaker,
                TextBox = Text
            };
            return res;
        }
    }
}