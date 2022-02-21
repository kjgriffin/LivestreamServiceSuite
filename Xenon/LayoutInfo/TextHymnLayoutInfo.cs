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
    internal class TextHymnLayoutInfo : ALayoutInfo, ILayoutInfoResolver<TextHymnLayoutInfo>
    {

        public TextboxLayout HymnNameBox { get; set; }
        public TextboxLayout HymnTitleBox { get; set; }
        public TextboxLayout HymnNumberBox { get; set; }
        public TextboxLayout HymnTuneBox { get; set; }
        public TextboxLayout CopyrightBox { get; set; }
        public TextboxLayout VerseInfoBox { get; set; }
        public PoetryLinesTextboxLayout HymnContentBox { get; set; }

        public List<LWJPolygon> Shapes { get; set; } = new List<LWJPolygon>();

        public override string GetDefaultJson()
        {
            return JsonSerializer.Serialize(_Internal_GetDefaultInfo(), new JsonSerializerOptions() { WriteIndented = true });
        }

        public TextHymnLayoutInfo GetLayoutInfo(Slide slide)
        {
            return ILayoutInfoResolver<TextHymnLayoutInfo>._InternalDefault_GetLayoutInfo(slide);
        }

        public TextHymnLayoutInfo _Internal_GetDefaultInfo(string overrideDefault = "")
        {
            return ILayoutInfoResolver<TextHymnLayoutInfo>.GetDefaultInfo();
        }
    }
}
