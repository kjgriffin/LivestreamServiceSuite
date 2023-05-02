using System.Collections.Generic;
using System.Text.Json;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class TitledResponsiveLiturgySlideLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<TitledResponsiveLiturgySlideLayoutInfo>
    {
        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();
        public List<TextboxLayout> TitleBoxes { get; set; } = new List<TextboxLayout>();
        public LiturgyTextboxLayout ContentBox { get; set; } = new LiturgyTextboxLayout();


        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions { WriteIndented = true });
        }

        public TitledResponsiveLiturgySlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<TitledResponsiveLiturgySlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public TitledResponsiveLiturgySlideLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<TitledResponsiveLiturgySlideLayoutInfo>.GetDefaultInfo(overrideDefault);
        }
    }
}
