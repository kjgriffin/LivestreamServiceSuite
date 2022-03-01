using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Xenon.Compiler.SubParsers;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.LayoutEngine.L2
{
    class SizedResponsiveStatement
    {
        public SizedTextBlurb Speaker { get; set; }
        public List<SizedTextBlurb> Content { get; set; } = new List<SizedTextBlurb>();

        public float MaxHeight
        {
            get
            {
                return Math.Max(Speaker.Size.Height, Content.DefaultIfEmpty().Max(x => x?.Size.Height ?? 0f));
            }
        }

        public static SizedResponsiveStatement CreateSized(ResponsiveStatement statement, Graphics gfx, LiturgyTextboxLayout layout)
        {
            SizedResponsiveStatement srs = new SizedResponsiveStatement();

            srs.Speaker = SizedTextBlurb.CreateMeasured(statement.SpeakerBlurb(layout),
                                                         layout.Font.Name,
                                                         layout.Font.Size,
                                                         layout.Font.GetStyle(),
                                                         layout.Textbox.GetRectangleF());

            foreach (var word in statement.ContentBlurbs(layout))
            {
                srs.Content.Add(SizedTextBlurb.CreateMeasured(word,
                                                             layout.Font.Name,
                                                             layout.Font.Size,
                                                             layout.Font.GetStyle(),
                                                             layout.Textbox.GetRectangleF()));
            }

            return srs;
        }
    }
}
