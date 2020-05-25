using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace LSBgenerator
{
    class TextRenderer
    {


        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }


        public int TextboxWidth { get; set; }
        public int TextboxHeight { get; set; }

        //public int PaddingH { get; set; }
        public int PaddingLeft { get; set; }
        public int PaddingRight { get; set; }
        //public int PaddingV { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingBottom { get; set; }

        public int PaddingCol { get; set; }

        public Font Font { get; set; }

        public Bitmap bmp { get; set; }

        public List<RenderSlide> Slides { get; set; } = new List<RenderSlide>();

        Graphics gfx;
        StringFormat format = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };
        Rectangle ETextRect;
        Rectangle TextRect;
        Rectangle TextboxRect;

        Dictionary<Speaker, Font> SpeakerFonts = new Dictionary<Speaker, Font>();
        Dictionary<Speaker, string> SpeakerText = new Dictionary<Speaker, string>();
        Dictionary<Speaker, bool> SpeakerFills = new Dictionary<Speaker, bool>();




        int blocksize;

        public TextRenderer(int displaywidth, int displayheight, int textboxwidth, int textboxheight, int pleft, int pright, int pcol, int ptop, int pbot, Font f)
        {
            DisplayHeight = displayheight;
            DisplayWidth = displaywidth;
            TextboxHeight = textboxheight;
            TextboxWidth = textboxwidth;
            PaddingLeft   = pleft;
            PaddingRight = pright;
            PaddingCol = pcol;
            PaddingTop = ptop;
            PaddingBottom = pbot;
            Font = f;
            bmp = new Bitmap(displaywidth, displayheight);
            gfx = Graphics.FromImage(bmp);
            blocksize = (int)(f.Size * gfx.DpiX / 72);
            TextRect = new Rectangle(DisplayWidth - TextboxWidth + PaddingLeft , DisplayHeight - TextboxHeight + PaddingTop, TextboxWidth - (PaddingLeft + PaddingRight), TextboxHeight - (PaddingTop + PaddingBottom));
            ETextRect = new Rectangle(TextRect.X + blocksize + PaddingCol, TextRect.Y, TextRect.Width - blocksize - PaddingCol, TextRect.Height);
            TextboxRect = new Rectangle((DisplayWidth - TextboxWidth) / 2, DisplayHeight - TextboxHeight, TextboxWidth, TextboxHeight);

            SpeakerFonts.Add(Speaker.Pastor, new Font(f, FontStyle.Regular));
            SpeakerFonts.Add(Speaker.Congregation, new Font(f, FontStyle.Bold));

            SpeakerFills.Add(Speaker.Pastor, false);
            SpeakerFills.Add(Speaker.Congregation, true);

            SpeakerText.Add(Speaker.Pastor, "P");
            SpeakerText.Add(Speaker.Congregation, "C");
        }


        public void Render_LayoutPreview(TextData td, Font f)
        {

            // fill background black
            gfx.Clear(Color.Black);

            // draw display

            gfx.FillRectangle(Brushes.White, TextboxRect);

            // draw layout padding
            Pen p = new Pen(Brushes.Red);
            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            //gfx.DrawRectangle(p, TextRect);

            // draw column
            gfx.DrawRectangle(p, new Rectangle(TextRect.X, TextRect.Y, blocksize, TextRect.Height));

            // draw overlap for effective text
            gfx.DrawRectangle(p, ETextRect);

        }
        public void Typeset_Text(TextData td)
        {
            // create a series of 'slides' given some liturgy lines


            // figure out how many 'lines' fit in the box
            // compute size and posititon of words
            // generate a renderlist for the text
            // this is broken into slides


            // go through lines
            // create new slide
            // check if line fits into current 'slide'
            // if so add renderline to slide
            // if not check if can split to fit (ignore for now)
            // create new slide etc.

            Slides.Clear();

            int slideCount = 0;
            RenderSlide currentSlide = new RenderSlide() { Order = slideCount++ };

            int linestartX = ETextRect.X;
            int linestartY = ETextRect.Y;


            foreach (var line in td.LineData)
            {
                // typeset the line 
                currentSlide = TypesetSlide(currentSlide, line);
            }

            if (!currentSlide.Blank)
            {
                Slides.Add(FinalizeSlide(currentSlide));
            }

        }

        private RenderSlide TypesetSlide(RenderSlide slide, LiturgyLine line)
        {
            // try to fit to current slide
            int lineheight = (int)Math.Ceiling(slide.gfx.MeasureString("CPALTgy", SpeakerFonts.TryGetVal(line.Speaker, Font)).Height);

            // check if whole line will fit

            SizeF s = slide.gfx.MeasureString(line.Text, SpeakerFonts.TryGetVal(line.Speaker, Font), ETextRect.Width);

            if (slide.YOffset + s.Height <= ETextRect.Height)
            {
                // it fits, add this line at position and compute
                RenderLine rl = new RenderLine() { Height = (int)s.Height, Width = (int)s.Width, ShowSpeaker = line.SubSplit == 0 || slide.Lines == 0, Speaker = line.Speaker, Text = line.Text, RenderX = 0, RenderY = 0, RenderLayoutMode = LayoutMode.Auto, LineNum = slide.Lines++ };
                slide.RenderLines.Add(rl);
                slide.YOffset += (int)s.Height;
                return slide;
            }
            // if slide isn't blank, then try it on a blank slide
            if (!slide.Blank)
            {
                Slides.Add(FinalizeSlide(slide));
                return TypesetSlide(new RenderSlide() { Order = slide.Order + 1 }, line);
            }
            // if doesn't fit on a blank slide
            // try to split into sentences. put as many sentences as fit on one slide until done

            
            // If have already split into sentences... split into chunks of words?
            if (line.IsSubsplit)
            {
                RenderSlide errorslide;
                if (!slide.Blank)
                {
                    Slides.Add(FinalizeSlide(slide));
                    errorslide = new RenderSlide() { Order = slide.Order + 1 };
                }                    
                else
                {
                    errorslide = slide;
                }
                // for now abandon
                string emsg = "ERROR";
                Size esize = slide.gfx.MeasureString(emsg, Font).ToSize();
                RenderLine errorline = new RenderLine() { Height = esize.Height, Width = esize.Width, ShowSpeaker = false, Speaker = Speaker.None, Text = emsg, RenderX = 0, RenderY = 0, RenderLayoutMode = LayoutMode.Auto, LineNum = 0 };
                errorslide.RenderLines.Add(errorline);
                Slides.Add(FinalizeSlide(errorslide));
                return new RenderSlide() { Order = errorslide.Order + 1 };

            }


            // split line text into sentences
            var sentences = line.Text.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            // keep trying to stuff a sentence
            List<LiturgyLine> sublines = new List<LiturgyLine>();
            int splitnum = 0;
            foreach (var sentence in sentences)
            {
                // create a bunch of sub-liturgy-lines and try to typeset them all
                sublines.Add(new LiturgyLine() { Speaker = line.Speaker, Text = sentence.Trim() + ".", SubSplit = splitnum++, IsSubsplit = true });
            }

            foreach (var sl in sublines)
            {
                slide = TypesetSlide(slide, sl);
            }

            return slide;

        }

        private RenderSlide FinalizeSlide(RenderSlide slide)
        {
            // now that all the renderlines are determined, layout the lines to be centered vertically

            // compute vertical spacing height that's spare
            // divide this up
            int spareheight = ETextRect.Height;
            foreach (var rl in slide.RenderLines)
            {
                spareheight -= rl.Height;
            }
            int verticalspace = (int)(spareheight / (slide.RenderLines.Count + 1d));

            // set y-offsets
            int yoff = verticalspace;
            foreach (var rl in slide.RenderLines)
            {
                rl.RenderY = yoff;
                yoff += rl.Height + verticalspace;
            }


            // center horizontally?

            // replace characters

            return slide;

        }

        public void RenderSlides()
        {
            foreach (var s in Slides)
            {
                Render_Slide(s);
            }
        }

        private void Render_Slide(RenderSlide slide)
        {

            // clear background
            slide.gfx.Clear(Color.Black);
            // draw textbox
            slide.gfx.FillRectangle(Brushes.White, TextboxRect);


            // go through the renderlist and display it

            foreach (var target in slide.RenderLines)
            {
                // draw text
                slide.gfx.DrawString(target.Text, SpeakerFonts.TryGetVal(target.Speaker, Font), Brushes.Black, new Rectangle(ETextRect.X + target.RenderX, ETextRect.Y + target.RenderY, ETextRect.Size.Width, ETextRect.Size.Height), format);
                // draw speaker
                if (target.ShowSpeaker)
                {
                    Font speakerfont = new Font(Font, FontStyle.Bold);
                    int speakeroffsety = (int)Math.Floor(Math.Ceiling((slide.gfx.MeasureString(SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont).Height - blocksize) / 2) - 1);
                    Rectangle speakerblock = new Rectangle(TextboxRect.X + PaddingCol, ETextRect.Y + target.RenderY + speakeroffsety, blocksize, blocksize);

                    if (SpeakerFills.TryGetVal(target.Speaker, false))
                    {
                        slide.gfx.FillPath(Brushes.Red, RoundedRect(speakerblock, 2));
                        switch (target.Speaker)
                        {
                            case Speaker.Congregation:
                                slide.gfx.DrawString("C", speakerfont, Brushes.White, speakerblock, format);
                                break;
                            default:
                                slide.gfx.DrawString("?", speakerfont, Brushes.White, speakerblock, format);
                                break;
                        }
                    }
                    else
                    {
                        slide.gfx.FillPath(Brushes.White, RoundedRect(speakerblock, 2));
                        slide.gfx.DrawPath(Pens.Red, RoundedRect(speakerblock, 2));
                        switch (target.Speaker)
                        {
                            case Speaker.Pastor:
                                slide.gfx.DrawString("P", speakerfont, Brushes.Red, speakerblock, format);
                                break;
                            default:
                                slide.gfx.DrawString("?", speakerfont, Brushes.Red, speakerblock, format);
                                break;
                        }
                    }

                }
            }

        }



        private GraphicsPath RoundedRect(Rectangle bounds, int borderradius)
        {

            int diameter = borderradius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (borderradius == 0)
            {
                path.AddRectangle(arc);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;

        }





    }
}
