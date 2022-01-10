using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.LayoutInfo.Serialization;
using Xenon.Layouts;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class StitchedImageSlideLayoutInfo : ALayoutInfo, ILayoutInfoResolver<StitchedImageSlideLayoutInfo>
    {
        public LWJSize SlideSize { get; set; }
        public LWJColor BackgroundColor { get; set; }
        public LWJColor KeyColor { get; set; }

        public TextboxLayout TitleBox { get; set; }
        public TextboxLayout NameBox { get; set; }
        public TextboxLayout NumberBox { get; set; }
        public TextboxLayout CopyrightBox { get; set; }
        public DrawingBoxLayout MusicBox { get; set; }


        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true});
        }

        public StitchedImageSlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<StitchedImageSlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public StitchedImageSlideLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<StitchedImageSlideLayoutInfo>.GetDefaultInfo();
        }
    }
}
