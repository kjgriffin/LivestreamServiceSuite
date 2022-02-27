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

    internal class AdvancedDrawingBoxLayout : DrawingBoxLayout
    {
        public bool AutoCrop { get; set; } = true;
        public LWJColor CropExclude { get; set; }
        public bool CropExcludeAlpha { get; set; } = true;
        public bool InvertColors { get; set; } = true;
    }
}
