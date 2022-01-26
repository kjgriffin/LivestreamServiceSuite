using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.LayoutInfo.LiturgyTypes;
using Xenon.Layouts;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    class ResponsiveLiturgySlideLayoutInfo : ALayoutInfo, ILayoutInfoResolver<ResponsiveLiturgySlideLayoutInfo>
    {
        public LWJSize SlideSize { get; set; }
        public LWJColor BackgroundColor { get; set; }
        public LWJColor KeyColor { get; set; }
        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();
        public List<LiturgyTextboxLayout> Textboxes { get; set; } = new List<LiturgyTextboxLayout>();

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public ResponsiveLiturgySlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<ResponsiveLiturgySlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public ResponsiveLiturgySlideLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<ResponsiveLiturgySlideLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
