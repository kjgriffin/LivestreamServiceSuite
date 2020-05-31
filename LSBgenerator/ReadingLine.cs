using System;
using System.Drawing;

namespace LSBgenerator
{
    [Serializable]
    public class ReadingLine : ITypesettable
    {

        public string Title { get; set; }
        public string Reference { get; set; }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            // force a new slide
            RenderSlide rslide = slide;
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                rslide = new RenderSlide() { Order = slide.Order + 1 };
            }


            // create the slide 


            Font titlefont = new Font(r.Font, FontStyle.Bold);
            Font reffont = new Font(r.Font, FontStyle.Bold | FontStyle.Italic);

            // add render lines for title and for reference
            Size tsize = rslide.gfx.MeasureString(Title, titlefont).ToSize();
            int titley = r.TextboxRect.Height / 2 - (tsize.Height / 2);


            RenderLine titleline = new RenderLine() { Height = tsize.Height, Width = tsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = Title, RenderX = 0, RenderY = titley, Font = titlefont };

            Size rsize = rslide.gfx.MeasureString(Reference, reffont).ToSize();
            // add equal column spacing
            int padright = r.TextboxRect.X - r.KeyRect.X + r.PaddingCol;
            int refx = r.TextboxRect.Width - rsize.Width - padright;
            int refy = r.TextboxRect.Height / 2 - (rsize.Height / 2);
            RenderLine refline = new RenderLine() { Height = rsize.Height, Width = rsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = Reference, RenderX = refx, RenderY = refy, Font = reffont };

            rslide.RenderLines.Add(titleline);
            rslide.RenderLines.Add(refline);


            r.Slides.Add(r.FinalizeSlide(rslide));

            // return a new black slide
            return new RenderSlide() { Order = rslide.Order + 1 };

        }
    }
}
