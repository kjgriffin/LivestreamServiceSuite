﻿using System.Collections.Generic;
using System.Text.Json;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    class ResponsiveLiturgySlideLayoutInfo : ASlideLayoutInfo, ILayoutInfoResolver<ResponsiveLiturgySlideLayoutInfo>
    {
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
