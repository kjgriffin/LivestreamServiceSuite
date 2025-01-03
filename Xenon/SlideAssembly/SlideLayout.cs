﻿using System.Drawing;

namespace Xenon.SlideAssembly
{
    public class SlideLayout
    {
        public VideoLayout VideoLayout { get; set; } = new VideoLayout()
        {
            Size = new Size(1920, 1080)
        };

        public AnthemTitleLayout AnthemTitleLayout { get; set; } = new AnthemTitleLayout()
        {
            Size = new Size(1920, 1080),
            Key = new Rectangle(0, 864, 1920, 216),
            Font = new Font("Arial", 36, FontStyle.Regular),
            TopLine = new Rectangle(50, 50, 1820, 50),
            MainLine = new Rectangle(50, 120, 1820, 50)
        };

        public TwoPartTitleLayout TwoPartTitleLayout { get; set; } = new TwoPartTitleLayout()
        {
            Size = new Size(1920, 1080),
            Key = new Rectangle(0, 864, 1920, 216),
            Font = new Font("Arial", 36, FontStyle.Regular),
            MainLine = new Rectangle(80, 0, 1760, 50),
            Line1 = new Rectangle(50, 50, 1820, 50),
            Line2 = new Rectangle(50, 120, 1820, 50)
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

        public TitledLiturgyVerseLayout TitleLiturgyVerseLayout { get; set; } = new TitledLiturgyVerseLayout()
        {
            Size = new Size(1920, 1080),
            Key = new Rectangle(0, 864, 1920, 216),
            TitleLine = new Rectangle(50, 10, 1820, 50),
            Textbox = new Rectangle(50, 56, 1820, 160),
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

        public SermonTitleLayout SermonLayout { get; set; } = new SermonTitleLayout()
        {
            Size = new Size(1920, 1080),
            Font = new Font("Arial", 36, FontStyle.Regular),
            Key = new Rectangle(0, 864, 1920, 216),
            TextAera = new Rectangle(50, 3, 1820, 210),
            TopLine = new Rectangle(0, 20, 1820, 70),
            MainLine = new Rectangle(0, 70, 1820, 140)
        };

        public TextHymnLayout TextHymnLayout { get; set; } = new TextHymnLayout()
        {
            Size = new Size(1920, 1080),
            NameBox = new Rectangle(576, 0, 768, 162),
            TitleBox = new Rectangle(0, 0, 576, 162),
            NumberBox = new Rectangle(1344, 0, 576, 162),
            CopyrightBox = new Rectangle(0, 1026, 1920, 54),
            TextBox = new Rectangle(0, 162, 1920, 864)
        };

        public PrefabLayout PrefabLayout { get; set; } = new PrefabLayout()
        {
            Size = new Size(1920, 1080)
        };

    }
}
