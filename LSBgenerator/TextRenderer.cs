using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace LSBgenerator
{

    [Serializable]
    public class TextRendererLayout
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

    }

    public class TextRenderer
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

        public LiturgyLineState LLState = new LiturgyLineState();

        public Graphics gfx;
        public StringFormat format = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };
        public Rectangle ETextRect;
        public Rectangle TextRect;
        public Rectangle TextboxRect;

        public Dictionary<Speaker, Font> SpeakerFonts = new Dictionary<Speaker, Font>();
        public Dictionary<Speaker, string> SpeakerText = new Dictionary<Speaker, string>();
        public Dictionary<Speaker, bool> SpeakerFills = new Dictionary<Speaker, bool>();




        public int blocksize;

        public TextRenderer(int displaywidth, int displayheight, int textboxwidth, int textboxheight, int pleft, int pright, int pcol, int ptop, int pbot, Font f)
        {
            DisplayHeight = displayheight;
            DisplayWidth = displaywidth;
            TextboxHeight = textboxheight;
            TextboxWidth = textboxwidth;
            PaddingLeft = pleft;
            PaddingRight = pright;
            PaddingCol = pcol;
            PaddingTop = ptop;
            PaddingBottom = pbot;
            Font = f;

            create();
            createSpeakers();
        }

        public TextRenderer(TextRendererLayout layout)
        {
            DisplayHeight = layout.DisplayHeight;
            DisplayWidth = layout.DisplayWidth;
            TextboxHeight = layout.TextboxHeight;
            TextboxWidth = layout.TextboxWidth;
            PaddingLeft = layout.PaddingLeft;
            PaddingRight = layout.PaddingRight;
            PaddingCol = layout.PaddingCol;
            PaddingTop = layout.PaddingTop;
            PaddingBottom = layout.PaddingBottom;
            Font = layout.Font;
            create();
            createSpeakers();
        }

        public TextRendererLayout GetLayoutParams()
        {
            TextRendererLayout layoutparams = new TextRendererLayout();

            layoutparams.DisplayHeight = DisplayHeight;
            layoutparams.DisplayWidth = DisplayWidth;
            layoutparams.TextboxHeight = TextboxHeight;
            layoutparams.TextboxWidth = TextboxWidth;
            layoutparams.PaddingLeft = PaddingLeft;
            layoutparams.PaddingRight = PaddingRight;
            layoutparams.PaddingCol = PaddingCol;
            layoutparams.PaddingTop = PaddingTop;
            layoutparams.PaddingBottom = PaddingBottom;
            layoutparams.Font = Font;

            return layoutparams;

        }

        private void create()
        {
            bmp = new Bitmap(DisplayWidth, DisplayHeight);
            gfx = Graphics.FromImage(bmp);
            blocksize = (int)(Font.Size * gfx.DpiX / 72);
            TextRect = new Rectangle((DisplayWidth - TextboxWidth + PaddingLeft) / 2, DisplayHeight - TextboxHeight + PaddingTop, TextboxWidth - (PaddingLeft + PaddingRight), TextboxHeight - (PaddingTop + PaddingBottom));
            ETextRect = new Rectangle(TextRect.X + blocksize + PaddingCol, TextRect.Y, TextRect.Width - blocksize - PaddingCol, TextRect.Height);
            TextboxRect = new Rectangle((DisplayWidth - TextboxWidth) / 2, DisplayHeight - TextboxHeight, TextboxWidth, TextboxHeight);
        }

        private void createSpeakers()
        {
            SpeakerFonts.Add(Speaker.Pastor, new Font(Font, FontStyle.Regular));
            SpeakerFonts.Add(Speaker.Leader, new Font(Font, FontStyle.Regular));
            SpeakerFonts.Add(Speaker.Congregation, new Font(Font, FontStyle.Bold));
            SpeakerFonts.Add(Speaker.Assistant, new Font(Font, FontStyle.Regular));

            SpeakerFills.Add(Speaker.Pastor, false);
            SpeakerFills.Add(Speaker.Leader, false);
            SpeakerFills.Add(Speaker.Congregation, true);
            SpeakerFills.Add(Speaker.Assistant, false);

            SpeakerText.Add(Speaker.Pastor, "P");
            SpeakerText.Add(Speaker.Leader, "L");
            SpeakerText.Add(Speaker.Congregation, "C");
            SpeakerText.Add(Speaker.Assistant, "A");
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

        public Bitmap Render_LayoutLumaKey()
        {
            Bitmap bmp = new Bitmap(DisplayWidth, DisplayHeight);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.Clear(Color.Black);
            gfx.FillRectangle(Brushes.White, TextboxRect);
            return bmp;
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
                currentSlide = line.TypesetSlide(currentSlide, this);
            }

            if (!currentSlide.Blank)
            {
                Slides.Add(FinalizeSlide(currentSlide));
            }

        }

        public RenderSlide FinalizeSlide(RenderSlide slide)
        {
            // now that all the renderlines are determined, layout the lines to be centered vertically

            // compute vertical spacing height that's spare
            // divide this up
            int spareheight = ETextRect.Height;
            foreach (var rl in slide.RenderLines)
            {
                if (rl.RenderLayoutMode == LayoutMode.Auto)
                {
                    spareheight -= rl.Height;
                }
            }
            int verticalspace = (int)(spareheight / (slide.RenderLines.Count + 1d));

            // set y-offsets
            int yoff = verticalspace;
            foreach (var rl in slide.RenderLines)
            {
                if (rl.RenderLayoutMode == LayoutMode.Auto)
                {
                    rl.RenderY = yoff;
                    yoff += rl.Height + verticalspace;
                }
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

                target.Render(slide, this);
                //// draw text
                //slide.gfx.DrawString(target.Text, target.Font, target.TextBrush, new Rectangle(ETextRect.X + target.RenderX, ETextRect.Y + target.RenderY, ETextRect.Size.Width, ETextRect.Size.Height), format);
                //// draw speaker
                //if (target.ShowSpeaker)
                //{
                //    Font speakerfont = new Font(target.Font, FontStyle.Bold);
                //    int speakeroffsety = (int)Math.Floor(Math.Ceiling((slide.gfx.MeasureString(SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont).Height - blocksize) / 2) - 1);
                //    Rectangle speakerblock = new Rectangle(TextboxRect.X + PaddingCol, ETextRect.Y + target.RenderY + speakeroffsety, blocksize, blocksize);

                //    if (target.Speaker == Speaker.None)
                //    {
                //        continue;
                //    }

                //    if (SpeakerFills.TryGetVal(target.Speaker, false))
                //    {
                //        slide.gfx.FillPath(Brushes.Red, RoundedRect(speakerblock, 2));
                //        slide.gfx.DrawString(SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont, Brushes.White, speakerblock, format);
                //    }
                //    else
                //    {
                //        slide.gfx.FillPath(Brushes.White, RoundedRect(speakerblock, 2));
                //        slide.gfx.DrawPath(Pens.Red, RoundedRect(speakerblock, 2));
                //        slide.gfx.DrawString(SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont, Brushes.Red, speakerblock, format);
                //    }

                //}
            }

        }



        public GraphicsPath RoundedRect(Rectangle bounds, int borderradius)
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
