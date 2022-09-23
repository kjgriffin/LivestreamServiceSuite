using System.Text.Json.Serialization;

namespace Xenon.LayoutInfo.BaseTypes
{
    internal class ComplexTextboxLayout : TextboxLayout
    {
        public bool NewLineForBlocks { get; set; } = true;
        public bool VPaddingEnabled { get; set; } = false;
        public float MinInterLineSpace { get; set; } = 10;
        public float MaxInterLineSpace { get; set; } = 40;
        public bool EvenSpill { get; set; } = false;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LWJVAlign VAlign { get; set; } = LWJVAlign.Top;
    }
}
