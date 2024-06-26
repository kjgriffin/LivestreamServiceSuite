﻿using CommonVersionInfo;

namespace Xenon.LayoutInfo
{

    internal abstract class ALayoutInfo
    {
        virtual public string GetDefaultJson()
        {
            return "";
        }
        virtual public ALayoutInfo UpgradeLayoutFromPreviousVersion(BuildVersion originalVersion, BuildVersion targetVersion)
        {
            return this;
        }
    }

    internal class ASlideLayoutInfo : ALayoutInfo
    {
        public LWJSize SlideSize { get; set; } = new LWJSize() { Height = 1920, Width = 1080 };
        public LWJColor BackgroundColor { get; set; } = new LWJColor() { Hex = "#ffffffff" };
        public LWJColor KeyColor { get; set; } = new LWJColor() { Hex = "#ffffffff" };
        public string SlideType { get; set; } = "";
    }
}
