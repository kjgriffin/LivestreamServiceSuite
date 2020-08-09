using SlideCreater.LayoutEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class SermonTitleLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle TextAera { get; set; }
        public Rectangle TopLine { get; set; }
        public Rectangle MainLine { get; set; }
        public Font Font { get; set; }
    
        public SermonLayoutRenderInfo GetRenderInfo()
        {
            return new SermonLayoutRenderInfo();
        }
    }
}
