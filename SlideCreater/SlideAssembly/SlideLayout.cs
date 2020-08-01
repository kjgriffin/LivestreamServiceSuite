using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class SlideLayout
    {
        public VideoLayout VideoLayout { get; set; } = new VideoLayout() { Size = new Size(1920, 1080) };
        public LiturgyLayout LiturgyLayout { get; set; } = new LiturgyLayout() { Size = new Size(1920, 1080), Key = new Rectangle(0, 1000, 1920, 80), Speaker = new Rectangle(10, 10, 60, 60), Text = new Rectangle(80, 10, 1830, 60) };
    }
}
