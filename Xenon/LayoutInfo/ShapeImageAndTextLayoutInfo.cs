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
    internal class ShapeImageAndTextLayoutInfo : ALayoutInfo, ILayoutInfoResolver<ShapeImageAndTextLayoutInfo>
    {
        public List<TextboxLayout> Textboxes { get; set; } = new List<TextboxLayout>();
        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();

        public List<DrawingBoxLayout> Images { get; set; } = new List<DrawingBoxLayout>();
        public List<DrawingBoxLayout> Branding { get; set; } = new List<DrawingBoxLayout>();

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public ShapeImageAndTextLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<ShapeImageAndTextLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public ShapeImageAndTextLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<ShapeImageAndTextLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
