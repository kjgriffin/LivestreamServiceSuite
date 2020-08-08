using SlideCreater.LayoutEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class ReadingLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle TextAera { get; set; }
        public Font Font { get; set; }

        public ReadingLayoutRenderInfo GetRenderInfo()
        {
            return new ReadingLayoutRenderInfo();
        }

    }
}
