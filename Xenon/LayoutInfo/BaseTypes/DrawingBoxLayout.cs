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
        public bool ForceBasic { get; set; } = false;
        public bool AutoCrop { get; set; } = false;
        public LWJColor CropExclude { get; set; } = new LWJColor(Color.Black);
        public bool CropExcludeAlpha { get; set; } = false;
        public bool InvertColors { get; set; } = false;
        public bool ForceSolidKey { get; set; } = false;
        public bool AlphaReplace { get; set; } = true;
        public bool AlphaToGrey { get; set; } = false;
    }
}
