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
    internal class ShapeAndTextLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<ShapeAndTextLayoutInfo>
    {
        public List<TextboxLayout> Textboxes { get; set; } = new List<TextboxLayout>();
        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public ShapeAndTextLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<ShapeAndTextLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public ShapeAndTextLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<ShapeAndTextLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
