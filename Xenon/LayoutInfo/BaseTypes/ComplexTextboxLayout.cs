namespace Xenon.LayoutInfo.BaseTypes
{
    internal class ComplexTextboxLayout : TextboxLayout
    {
        public bool NewLineForBlocks { get; set; } = true;
        public bool VPaddingEnabled { get; set; } = false;
        public float MinInterLineSpace { get; set; } = 10;
    }
}
