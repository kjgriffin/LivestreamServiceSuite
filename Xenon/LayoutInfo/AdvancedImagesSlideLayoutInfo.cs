using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class AdvancedImagesSlideLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<AdvancedImagesSlideLayoutInfo>
    {
        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();
        public List<AdvancedDrawingBoxLayout> Images { get; set; } = new List<AdvancedDrawingBoxLayout>();

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions { WriteIndented = true });
        }
        public AdvancedImagesSlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<AdvancedImagesSlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public AdvancedImagesSlideLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<AdvancedImagesSlideLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
