using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

using Xenon.LayoutInfo.Serialization;

namespace Xenon.LayoutInfo.BaseTypes
{
    internal class DrawingBoxLayout
    {
        public LWJRect Box { get; set; }
        public LWJColor FillColor { get; set; }
        public LWJColor KeyColor { get; set; }
    }
}
