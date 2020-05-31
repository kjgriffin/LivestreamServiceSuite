using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace LSBgenerator
{
    [Serializable]
    public class LiturgyLine : ITypesettable
    {
        public Speaker Speaker { get; set; }
        public string Text { get; set; }

        public int SubSplit { get; set; } = 0;

        public bool IsSubsplit { get; set; } = false;


        /// <summary>
        /// Computes the display length of the string
        /// </summary>
        /// <returns></returns>
        public int ComputeWidth(Font f)
        {
            return 0;
        }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            LiturgyLine line = this;


            // update state if not in wrapmode
            if (!r.LLState.SpeakerWrap)
            {
                r.LLState.LastSpeaker = line.Speaker;
            }
            else
            {
                line.Speaker = r.LLState.LastSpeaker;
            }

            // try to fit to current slide
            int lineheight = (int)Math.Ceiling(slide.gfx.MeasureString("CPALTgy", r.SpeakerFonts.TryGetVal(line.Speaker, r.Font)).Height);

            // check if whole line will fit

            SizeF s = slide.gfx.MeasureString(line.Text, r.SpeakerFonts.TryGetVal(line.Speaker, r.Font), r.TextboxRect.Width);

            if (slide.YOffset + s.Height <= r.TextboxRect.Height)
            {
                // it fits, add this line at position and compute
                RenderLine rl = new RenderLine() { Height = (int)s.Height, Width = (int)s.Width, ShowSpeaker = line.SubSplit == 0 || slide.Lines == 0, Speaker = line.Speaker, Text = line.Text, RenderX = 0, RenderY = 0, RenderLayoutMode = LayoutMode.Auto, LineNum = slide.Lines++, Font = r.SpeakerFonts.TryGetVal(line.Speaker, r.Font) };
                slide.RenderLines.Add(rl);
                slide.YOffset += (int)s.Height;
                // wrap only works for one line
                r.LLState.SpeakerWrap = false;
                return slide;
            }
            // if slide isn't blank, then try it on a blank slide
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                // wrap only works for one line
                r.LLState.SpeakerWrap = false;
                return line.TypesetSlide(new RenderSlide() { Order = slide.Order + 1 }, r);
            }
            // if doesn't fit on a blank slide
            // try to split into sentences. put as many sentences as fit on one slide until done


            // If have already split into sentences... split into chunks of words?
            if (line.IsSubsplit)
            {
                RenderSlide errorslide;
                if (!slide.Blank)
                {
                    r.Slides.Add(r.FinalizeSlide(slide));
                    errorslide = new RenderSlide() { Order = slide.Order + 1 };
                }
                else
                {
                    errorslide = slide;
                }
                // for now abandon
                string emsg = "ERROR";
                Size esize = slide.gfx.MeasureString(emsg, r.Font).ToSize();
                RenderLine errorline = new RenderLine() { Height = esize.Height, Width = esize.Width, ShowSpeaker = r.LLState.SpeakerWrap, Speaker = Speaker.None, Text = emsg, RenderX = 0, RenderY = 0, RenderLayoutMode = LayoutMode.Auto, LineNum = 0, Font = r.Font, TextBrush = Brushes.Red };
                errorslide.RenderLines.Add(errorline);
                r.Slides.Add(r.FinalizeSlide(errorslide));
                // wrap only works for one line
                r.LLState.SpeakerWrap = false;
                return new RenderSlide() { Order = errorslide.Order + 1 };

            }


            // split line text into sentences
            var sentences = line.Text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            var tmp = Regex.Matches(line.Text, @"((?<sentence>[^\.;!?]+)(?<delimiter>[\.;!?])+)");



            // keep trying to stuff a sentence
            List<LiturgyLine> sublines = new List<LiturgyLine>();
            int splitnum = 0;
            foreach (var sentence in tmp)
            {
                // create a bunch of sub-liturgy-lines and try to typeset them all
                string t = sentence.ToString();
                sublines.Add(new LiturgyLine() { Speaker = line.Speaker, Text = t.Trim(), SubSplit = splitnum++, IsSubsplit = true });
            }

            foreach (var sl in sublines)
            {
                slide = sl.TypesetSlide(slide, r);
            }


            // wrap only works for one line
            r.LLState.SpeakerWrap = false;
            return slide;

        }

    }
}
