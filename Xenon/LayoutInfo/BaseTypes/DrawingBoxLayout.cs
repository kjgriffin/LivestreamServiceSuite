using System.Drawing;
using System.Text.Json.Serialization;

namespace Xenon.LayoutInfo.BaseTypes
{
    internal class DrawingBoxLayout
    {
        public LWJRect Box { get; set; }
        public LWJColor FillColor { get; set; }
        public LWJColor KeyColor { get; set; }
        public bool InvertAll { get; set; } = false;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJHAlign HorizontalAlignment { get; set; } = LWJHAlign.Center;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJVAlign VerticalAlignment { get; set; } = LWJVAlign.Center;
        public bool OverrideKeyImg { get; set; } = false;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJHAlign KeyHorizontalAlignmentOverride { get; set; } = LWJHAlign.Center;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJVAlign KeyVerticalAlignmentOverride { get; set; } = LWJVAlign.Center;
        public LWJRect KeyBoxOverride { get; set; }
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
