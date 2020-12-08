using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class TitledLiturgyVerseSlideRenderer
    {

        public SlideLayout Layouts { get; set; }


        public RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";
            // draw it

            // for now just draw the layout
            Bitmap bmp = new Bitmap(Layouts.TitleLiturgyVerseLayout.Size.Width, Layouts.TitleLiturgyVerseLayout.Size.Height);

            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.Gray);

            gfx.FillRectangle(Brushes.Black, Layouts.TitleLiturgyVerseLayout.Key);

            List<LiturgyTextLine> Lines = (List<LiturgyTextLine>)slide.Data["lines"];

            Font bf = new Font(Layouts.TitleLiturgyVerseLayout.Font, FontStyle.Bold);
            Font itf = new Font(Layouts.TitleLiturgyVerseLayout.Font, FontStyle.Italic);

            gfx.DrawString((string)slide.Data["title"], bf, Brushes.White, Layouts.TitleLiturgyVerseLayout.TitleLine.Move(Layouts.TitleLiturgyVerseLayout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            gfx.DrawString((string)slide.Data["reference"], itf, Brushes.White, Layouts.TitleLiturgyVerseLayout.TitleLine.Move(Layouts.TitleLiturgyVerseLayout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);

            float alltextheight = 0;
            foreach (var line in Lines)
            {
                alltextheight += line.Height;
            }

            float interspace = (Layouts.TitleLiturgyVerseLayout.Textbox.Height - alltextheight) / (Lines.Count + 1);

            float vspace = interspace;
            int linenum = 0;

            string lastspeaker = "";

            Font fregular = new Font("Arial", 36, FontStyle.Regular);
            Font fbold = new Font("Arial", 36, FontStyle.Bold);
            Font fitalic = new Font("Arial", 36, FontStyle.Italic);

            Font flsbregular = new Font("LSBSymbol", 36, FontStyle.Regular);
            Font flsbbold = new Font("LSBSymbol", 36, FontStyle.Bold);
            Font flsbitalic = new Font("LSBSymbol", 36, FontStyle.Italic);


            foreach (var line in Lines)
            {
                float xoffset = 0;
                // center the text
                xoffset = (Layouts.TitleLiturgyVerseLayout.Textbox.Width / 2) - (line.Width / 2);

                // draw speaker
                if ((string)slide.Data["drawspeaker"] == "true" && lastspeaker != line.Speaker)
                {
                    SizeF speakersize = gfx.MeasureStringCharacters(line.Speaker + " ", flsbregular, new RectangleF(0, 0, 100, 100));
                    float jog = 0.07f * (gfx.DpiY * speakersize.Height / 72);
                    xoffset = (Layouts.TitleLiturgyVerseLayout.Textbox.Width / 2) - ((line.Width) + speakersize.Width) / 2;
                    gfx.DrawString(line.Speaker + " ", flsbregular, Brushes.Teal, Layouts.TitleLiturgyVerseLayout.Textbox.Move(Layouts.TitleLiturgyVerseLayout.Key.Location).Move((int) xoffset, (int)(vspace + interspace * linenum)).Move(0, (int)-jog).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += speakersize.Width;
                }

                lastspeaker = line.Speaker;



                foreach (var word in line.Words)
                {
                    Font f = word.IsLSBSymbol ? (word.IsBold ? flsbbold : flsbregular) : (word.IsBold ? fbold : fregular);
                    gfx.DrawString(word.Value, f, Brushes.White, Layouts.TitleLiturgyVerseLayout.Textbox.Move(Layouts.TitleLiturgyVerseLayout.Key.Location).Move((int)xoffset, (int)(vspace + interspace * linenum)).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += word.Size.Width;
                }
                linenum++;
                vspace += line.Height;
            }

            res.Bitmap = bmp;
            return res;
        }

    }
}
