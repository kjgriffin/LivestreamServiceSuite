using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class SlideLayout
    {
        public VideoLayout VideoLayout { get; set; } = new VideoLayout()
        {
            Size = new Size(1920, 1080)
        };
        public LiturgyLayout LiturgyLayout { get; set; } = new LiturgyLayout()
        {
            Size = new Size(1920, 1080),
            Key = new Rectangle(0, 864, 1920, 216),
            Speaker = new Rectangle(50, 3, 60, 210),
            Text = new Rectangle(120, 3, 1750, 210),
            InterLineSpacing = 5,
            Font = new Font("Arial", 36, FontStyle.Regular)
        };
        public ImageLayout FullImageLayout { get; set; } = new ImageLayout()
        {
            Size = new Size(1920, 1080)
        };

        public ReadingLayout ReadingLayout { get; set; } = new ReadingLayout()
        {
            Size = new Size(1920, 1080),
            Font = new Font("Arial", 36, FontStyle.Regular),
            Key = new Rectangle(0, 864, 1920, 216),
            TextAera = new Rectangle(50, 3, 1820, 210)
        };
    }
}
