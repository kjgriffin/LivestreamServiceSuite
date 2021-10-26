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


        static Font fregular = new Font("Arial", 36, FontStyle.Regular);
        static Font fbold = new Font("Arial", 36, FontStyle.Bold);
        static Font fitalic = new Font("Arial", 36, FontStyle.Italic);

        static Font flsbregular = new Font("LSBSymbol", 36, FontStyle.Regular);
        static Font flsbbold = new Font("LSBSymbol", 36, FontStyle.Bold);
        static Font flsbitalic = new Font("LSBSymbol", 36, FontStyle.Italic);


        public RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

            TitledLiturgyVerseLayout layout = Layouts.TitleLiturgyVerseLayout;

            if (slide.Data.ContainsKey("layoutoverride"))
            {
                layout = (TitledLiturgyVerseLayout)slide.Data["layoutoverride"];
            }

            // draw it

            // for now just draw the layout
            Bitmap bmp = new Bitmap(layout.Size.Width, layout.Size.Height);
            Bitmap kbmp = new Bitmap(layout.Size.Width, layout.Size.Height);

            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.Gray);
            kgfx.Clear(Color.Black);

            gfx.FillRectangle(Brushes.Black, layout.Key);
            kgfx.FillRectangle(new SolidBrush(slide.Colors["keytrans"]), layout.Key);

            List<LiturgyTextLine> Lines = (List<LiturgyTextLine>)slide.Data["lines"];

            Font bf = new Font(layout.Font, FontStyle.Bold);
            Font itf = new Font(layout.Font, FontStyle.Italic);

            gfx.DrawString((string)slide.Data["title"], bf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            kgfx.DrawString((string)slide.Data["title"], bf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            gfx.DrawString((string)slide.Data["reference"], itf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);
            kgfx.DrawString((string)slide.Data["reference"], itf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);

            float alltextheight = 0;
            foreach (var line in Lines)
            {
                alltextheight += line.Height;
            }

            float interspace = (layout.Textbox.Height - alltextheight) / (Lines.Count + 1);

            float vspace = interspace;
            int linenum = 0;

            string lastspeaker = "";



            foreach (var line in Lines)
            {
                float xoffset = 0;
                // center the text
                xoffset = (layout.Textbox.Width / 2) - (line.Width / 2);

                // draw speaker
                if ((string)slide.Data["drawspeaker"] == "true" && lastspeaker != line.Speaker)
                {
                    SizeF speakersize = gfx.MeasureStringCharacters(line.Speaker, ref flsbregular, new RectangleF(0, 0, 100, 100));
                    float jog = 0.07f * (gfx.DpiY * speakersize.Height / 72);
                    xoffset = (layout.Textbox.Width / 2) - ((line.Width) + speakersize.Width) / 2;
                    gfx.DrawString(line.Speaker, flsbregular, Brushes.Teal, layout.Textbox.Move(layout.Key.Location).Move((int)xoffset, (int)(vspace + interspace * linenum)).Move(0, (int)-jog).Location, GraphicsHelper.DefaultStringFormat());
                    kgfx.DrawString(line.Speaker, flsbregular, Brushes.White, layout.Textbox.Move(layout.Key.Location).Move((int)xoffset, (int)(vspace + interspace * linenum)).Move(0, (int)-jog).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += speakersize.Width;
                }

                lastspeaker = line.Speaker;

                xoffset += 30;

                foreach (var word in line.Words)
                {
                    Font f = word.IsLSBSymbol ? (word.IsBold ? flsbbold : flsbregular) : (word.IsBold ? fbold : fregular);
                    gfx.DrawString(word.Value, f, Brushes.White, layout.Textbox.Move(layout.Key.Location).Move((int)xoffset, (int)(vspace + interspace * linenum)).Location, GraphicsHelper.DefaultStringFormat());
                    kgfx.DrawString(word.Value, f, Brushes.White, layout.Textbox.Move(layout.Key.Location).Move((int)xoffset, (int)(vspace + interspace * linenum)).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += word.Size.Width;
                }
                linenum++;
                vspace += line.Height;
            }

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }

    }
}
