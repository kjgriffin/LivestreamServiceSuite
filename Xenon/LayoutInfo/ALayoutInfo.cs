using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.LayoutInfo
{
    internal abstract class ALayoutInfo
    {
        public LWJSize SlideSize { get; set; } = new LWJSize() { Height = 1920, Width = 1080 };
        public LWJColor BackgroundColor { get; set; } = new LWJColor() { Hex = "#ffffffff" };
        public LWJColor KeyColor { get; set; } = new LWJColor() { Hex = "#ffffffff" };
        public string SlideType { get; set; } = "";
        virtual public string GetDefaultJson()
        {
            return "";
        }
    }
}
