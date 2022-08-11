using System.Text.Json;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class EngravingLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<EngravingLayoutInfo>
    {

        public EngravingDrawingBoxLayout Engraving { get; set; } = new EngravingDrawingBoxLayout();

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }
        public EngravingLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<EngravingLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }
        public EngravingLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<EngravingLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
