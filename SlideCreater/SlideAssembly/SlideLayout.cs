using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class SlideLayout
    {
        public VideoLayout VideoLayout { get; set; } = new VideoLayout() { Size = new Size(1920, 1080) };
        public LiturgyLayout LiturgyLayout { get; set; } = new LiturgyLayout() { Size = new Size(1920, 1080), Key = new Rectangle(0, 864, 1920, 216), Speaker = new Rectangle(50, 3, 60, 210), Text = new Rectangle(120, 3, 1810, 210), InterLineSpacing = 5 };
    }
}
