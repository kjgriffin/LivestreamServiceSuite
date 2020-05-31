using System;
using System.Drawing;

namespace LSBgenerator
{
    [Serializable]
    public class SermonTitle : ITypesettable
    {
        public string Title { get; set; }
        public string SermonName { get; set; }
        public string SermonText { get; set; }

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
            Font sermontitlefont = new Font(r.Font, FontStyle.Bold);
            Font reffont = new Font(r.Font, FontStyle.Bold | FontStyle.Italic);

            // add render lines for title and for reference
            Size tsize = rslide.gfx.MeasureString(Title, r.Font).ToSize();
            Size rsize = rslide.gfx.MeasureString(SermonText, reffont).ToSize();
            Size nsize = rslide.gfx.MeasureString(SermonName, r.Font).ToSize();

            int yslack = r.TextboxRect.Height - Math.Max(tsize.Height, rsize.Height) - nsize.Height;

            int titley = yslack / 3;

            RenderLine titleline = new RenderLine() { Height = tsize.Height, Width = tsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = Title, RenderX = 0, RenderY = titley, Font = titlefont };

            // add equal column spacing
            int padright = r.TextboxRect.X - r.KeyRect.X + r.PaddingCol;
            int refx = r.TextboxRect.Width - rsize.Width - padright;
            int refy = yslack / 3;
            RenderLine refline = new RenderLine() { Height = rsize.Height, Width = rsize.Width, LineNum = 0, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = SermonText, RenderX = refx, RenderY = refy, Font = reffont };

            rslide.RenderLines.Add(titleline);
            rslide.RenderLines.Add(refline);

            int stitley = yslack / 3 * 2 + Math.Max(tsize.Height, rsize.Height);
            int stitlex = r.TextboxRect.Width / 2 - padright - nsize.Width / 2;
            RenderLine sermontitleline = new RenderLine() { Height = nsize.Height, Width = nsize.Width, LineNum = 1, RenderLayoutMode = LayoutMode.Fixed, ShowSpeaker = false, Speaker = Speaker.None, Text = SermonName, RenderX = stitlex, RenderY = stitley, Font = sermontitlefont };

            rslide.RenderLines.Add(sermontitleline);



            r.Slides.Add(r.FinalizeSlide(rslide));

            // return a new black slide
            return new RenderSlide() { Order = rslide.Order + 1 };

        }
    }
}
