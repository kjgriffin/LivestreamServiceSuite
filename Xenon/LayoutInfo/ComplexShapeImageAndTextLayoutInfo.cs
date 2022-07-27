using System.Collections.Generic;
using System.Text.Json;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class ComplexShapeImageAndTextLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<ComplexShapeImageAndTextLayoutInfo>
    {
        public List<ComplexTextboxLayout> ComplexBoxes { get; set; } = new List<ComplexTextboxLayout>();
        public List<TextboxLayout> Textboxes { get; set; } = new List<TextboxLayout>();
        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();

        public List<DrawingBoxLayout> Images { get; set; } = new List<DrawingBoxLayout>();
        public List<DrawingBoxLayout> Branding { get; set; } = new List<DrawingBoxLayout>();


        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }
        public ComplexShapeImageAndTextLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<ComplexShapeImageAndTextLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }
        public ComplexShapeImageAndTextLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<ComplexShapeImageAndTextLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
