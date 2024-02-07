using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Xenon.Compiler.SubParsers;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.LayoutEngine.L2
{
    internal class ComplexTextLayoutEngine
    {

        class LayoutLine
        {
            public List<SizedTextBlurb> Blurbs { get; private set; } = new List<SizedTextBlurb>();
            public LayoutLine(List<SizedTextBlurb> blurbs)
            {
                Blurbs = blurbs;
            }

            public float MaxHeight => Blurbs.Any() ? Blurbs.Max(x => x.Size.Height) : 0;
            public float MinWidth => Blurbs.Any() ? Blurbs.Sum(x => x.Size.Width) : 0;

            internal List<SizedTextBlurb> PlaceLine(TextboxLayout layout, float ypos)
            {
                float x = layout.Textbox.Rectangle.X;

                foreach (var blurb in Blurbs)
                {
                    blurb.Place(new Point((int)x, (int)ypos));
                    x += blurb.Size.Width;
                }

                return Blurbs;
            }
        }

        class LayoutBox
        {
            public List<LayoutLine> Lines { get; private set; } = new List<LayoutLine>();
            public LayoutBox(List<LayoutLine> lines)
            {
                Lines = lines;
            }

            internal List<SizedTextBlurb> PlaceLines(ComplexTextboxLayout layout)
            {
                float yoff = 0;
                float vspace = layout.MinInterLineSpace;

                if (layout.VAlign == LWJVAlign.Equidistant)
                {
                    vspace = Math.Min(Math.Max((layout.Textbox.Size.Height - Lines.Sum(x => x.MaxHeight)) / Math.Max(Lines.Count - 1, 1), layout.MinInterLineSpace), layout.MaxInterLineSpace);
                }
                else if (layout.VAlign == LWJVAlign.Bottom)
                {
                    yoff = layout.Textbox.Size.Height - Lines.Sum(x => x.MaxHeight + layout.LineSpacing * vspace);
                }
                else if (layout.VAlign == LWJVAlign.Center)
                {
                    yoff = (layout.Textbox.Size.Height - Lines.Sum(x => x.MaxHeight + layout.LineSpacing * vspace)) / 2;
                }

                List<SizedTextBlurb> lines = new List<SizedTextBlurb>();
                float y = layout.Textbox.Rectangle.Y + yoff;
                foreach (var line in Lines)
                {
                    lines.AddRange(line.PlaceLine(layout, y));
                    y += line.MaxHeight + layout.LineSpacing * vspace;
                }
                return lines;
            }
        }



        public static List<List<SizedTextBlurb>> GenerateSlides(List<TextBlockPara> paragraphs, ComplexTextboxLayout layout)
        {
            List<List<SizedTextBlurb>> slides = new List<List<SizedTextBlurb>>();

            if (!paragraphs.Any())
            {
                return slides;
            }

            /*
                    1. handle each line
                        - we'll greedily fit words on a line
                        - but we'll also treat non-break annotations

                    2. once we've filled a line greedily, we'll perform the x axis layout (taking into account hlayout justification)

                    3. once we've filled a slide greeditly with lines, we'll perform the y axis layout (taking into account vlayout justification)
             */

            // need to measure all the text

            // measurement accounts for script-type
            // here is where super/sub script's get re-sized
            // for super/sub script placement: will use it's re-sized measurement. apply x (based on size) and y (line height) offsets

            List<SizedTextBlurb> measuredBlurbs = new List<SizedTextBlurb>();
            foreach (var paragraph in paragraphs)
            {
                foreach (var blurb in paragraph.ContentBlurbs(layout))
                {
                    measuredBlurbs.Add(SizedTextBlurb.CreateMeasured(blurb,
                                                                 layout.Font.Name,
                                                                 layout.Font.Size,
                                                                 layout.Font.GetStyle(),
                                                                 layout.Textbox.GetRectangleF()));
                }
            }


            List<LayoutLine> filledLines = new List<LayoutLine>();

            List<SizedTextBlurb> cline = new List<SizedTextBlurb>();
            double xwidth = 0;
            for (int i = 0; i < measuredBlurbs.Count; i++)
            {
                var blurb = measuredBlurbs[i];

                // check if we break lines here
                if (layout.NewLineForBlocks && blurb.Rules?.Contains("br") == true)
                {
                    filledLines.Add(new LayoutLine(cline));
                    cline = new List<SizedTextBlurb>();
                    xwidth = 0;
                }


                // TODO: check rules for non-breaking and if so account for that when splitting lines
                double nwidth = blurb.Size.Width;
                bool nbsp = false;
                if (blurb.Rules?.Contains("nbspn") == true && i + 1 < measuredBlurbs.Count)
                {
                    nwidth += measuredBlurbs[i + 1].Size.Width;
                    nbsp = true;
                }

                if (xwidth + nwidth > layout.Textbox.Size.Width)
                {
                    filledLines.Add(new LayoutLine(cline));
                    cline = new List<SizedTextBlurb>();
                    xwidth = 0;
                }

                // for now ignore leading whitespace
                if (cline.Any() || !string.IsNullOrWhiteSpace(blurb.Text) && !blurb.IsWhitespace)
                {
                    cline.Add(blurb);
                    if (nbsp)
                    {
                        cline.Add(measuredBlurbs[i + 1]);
                        i += 1;
                    }
                    xwidth += nwidth;
                }
            }
            if (cline.Any())
            {
                filledLines.Add(new LayoutLine(cline));
            }


            // we've now got lines laid out
            // NOW split the lines across slides
            List<LayoutBox> filledboxes = new List<LayoutBox>();
            List<LayoutLine> cbox = new List<LayoutLine>();

            // perhaps some consideration should be paid
            // such that we don't put an unbalanced number of lines on each slide
            // i.e. if possible avoid having less than a 30% difference between the fullest/emptiest slide

            // compute rough avg line height
            float avgLineHeight = filledLines.Select(x => x.MaxHeight + layout.MinInterLineSpace).DefaultIfEmpty(0).Average();
            // compute rough approx max lines/slide
            int approxMaxLinesPerSlide = (int)Math.Floor(layout.Textbox.Size.Height / avgLineHeight);
            // figure out how many slides are needed
            int expectedSlides = (int)Math.Ceiling(filledLines.Count / (double)approxMaxLinesPerSlide);
            // figure out how many leftover lines would be on the last slide
            int expectedLastSlideLines = filledLines.Count % approxMaxLinesPerSlide;

            bool adjustRequired = false;
            var targetLines = -1;

            double PercentDiff = 0.3;

            if ((approxMaxLinesPerSlide - expectedLastSlideLines) / approxMaxLinesPerSlide < PercentDiff)
            {
                adjustRequired = layout.EvenSpill;
                // compute how to adjust based on max number of slides needed
                // then re-balance against this higher number
                targetLines = (int)Math.Ceiling(filledLines.Count / (double)expectedSlides);// still bias against earlier slides
            }


            double yheight = 0;
            int slines = 0;
            foreach (var line in filledLines)
            {
                if (yheight + line.MaxHeight > layout.Textbox.Size.Height || (adjustRequired && slines > targetLines))
                {
                    filledboxes.Add(new LayoutBox(cbox));
                    cbox = new List<LayoutLine>();
                    yheight = 0;
                    slines = 0;
                }
                yheight += line.MaxHeight + layout.MinInterLineSpace; // NOTE: adjustRequired should force this to work in other modes since the PlaceLines call will actually figure stuff out
                cbox.Add(line);
                slines++;
            }
            if (cbox.Any())
            {
                filledboxes.Add(new LayoutBox(cbox));
            }

            // now just need to place the blurbs
            foreach (var fbox in filledboxes)
            {
                slides.Add(fbox.PlaceLines(layout));
            }


            return slides;
        }


    }
}
