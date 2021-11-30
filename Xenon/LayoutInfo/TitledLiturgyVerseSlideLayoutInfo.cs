﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.Layouts;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.LayoutInfo
{
    internal class TitledLiturgyVerseSlideLayoutInfo : ALayoutInfo, ILayoutInfoResolver<TitledLiturgyVerseSlideLayoutInfo>
    {
        public LWJSize SlideSize { get; set; }
        public LWJColor BackgroundColor { get; set; }
        public LWJColor KeyColor { get; set; }
        public TextboxLayout TitleBox { get; set; }
        public TextboxLayout RefBox { get; set; }
        public TextboxLayout ContentTextbox { get; set; }
        public DrawingBoxLayout Banner { get; set; }


        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public TitledLiturgyVerseSlideLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<TitledLiturgyVerseSlideLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public TitledLiturgyVerseSlideLayoutInfo _Internal_GetDefaultInfo()
        {
            return ILayoutInfoResolver<TitledLiturgyVerseSlideLayoutInfo>.GetDefaultInfo();
        }
    }
}