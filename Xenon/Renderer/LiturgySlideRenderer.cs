using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Xenon.Helpers;
using System.Printing;

namespace Xenon.Renderer
{
    class LiturgySlideRenderer
    {

        public SlideLayout Layouts { get; set; }

        static Font fregular = new Font("Arial", 36, FontStyle.Regular);
        static Font fbold = new Font("Arial", 36, FontStyle.Bold);
        static Font fitalic = new Font("Arial", 36, FontStyle.Italic);

        static Font flsbregular = new Font("LSBSymbol", 36, FontStyle.Regular);
        static Font flsbbold = new Font("LSBSymbol", 36, FontStyle.Bold);
        static Font flsbitalic = new Font("LSBSymbol", 36, FontStyle.Italic);


        public RenderedSlide RenderSlide(LiturgyLayoutRenderInfo renderInfo, Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";
            // draw it

            // for now just draw the layout
            Bitmap bmp = new Bitmap(Layouts.LiturgyLayout.Size.Width, Layouts.LiturgyLayout.Size.Height);
            Bitmap kbmp = new Bitmap(Layouts.LiturgyLayout.Size.Width, Layouts.LiturgyLayout.Size.Height);

            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(slide.Colors["background"]);
            // most of slide should be transparent
            kgfx.Clear(Color.Black);


            SolidBrush textcol = new SolidBrush(slide.Colors["text"]);
            SolidBrush speakercol = new SolidBrush(slide.Colors["alttext"]);

            SolidBrush sb = new SolidBrush(slide.Colors["keybackground"]);
            SolidBrush kb = new SolidBrush(slide.Colors["keytrans"]);

            gfx.FillRectangle(sb, Layouts.LiturgyLayout.Key);
            // make black somewhat transparent
            kgfx.FillRectangle(kb, Layouts.LiturgyLayout.Key);

            StringFormat topleftalign = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
            StringFormat centeralign = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            RectangleF text = Layouts.LiturgyLayout.Text.Move(Layouts.LiturgyLayout.Key.Location);

            RectangleF speaker = Layouts.LiturgyLayout.Speaker.Move(Layouts.LiturgyLayout.Key.Location);

            //gfx.DrawRectangle(Pens.Red, text);
            //gfx.DrawRectangle(Pens.Purple, speaker);

            // compute line spacing
            int textlinecombinedheight = 0;
            foreach (var line in slide.Lines)
            {
                textlinecombinedheight += (int)(float)line.Content[1].Attributes["height"];
            }
            int interspace = (renderInfo.TextBox.Height - textlinecombinedheight) / (slide.Lines.Count + 1);

            int linenum = 0;
            int linepos = interspace;
            string lastspeaker = "";



            foreach (var line in slide.Lines)
            {
                bool drawspeaker = false;
                if (line.Content[0].Data != lastspeaker && line.Content[0].Data != "$")
                {
                    drawspeaker = true;
                }
                lastspeaker = line.Content[0].Data;

                LiturgyTextLine linewords = (LiturgyTextLine)line.Content[1].Attributes["textline"];

                // draw line
                float xoffset = 0;
                // center the text
                //xoffset = (Layouts.TitleLiturgyVerseLayout.Textbox.Width / 2) - (linewords.Width / 2);


                RectangleF speakerblock = new RectangleF(speaker.Move(0, linepos + interspace * linenum).Location, new Size(60, 60));

                // draw speaker
                if (drawspeaker)
                {
                    float jog = 0.07f * (gfx.DpiY * gfx.MeasureStringCharacters(linewords.Speaker, ref flsbregular, speakerblock).Height / 72);
                    gfx.DrawString(linewords.Speaker, flsbregular, speakercol, speakerblock.Move(0, -jog), centeralign);
                    // make speaker fully opaque
                    kgfx.DrawString(linewords.Speaker, flsbregular, Brushes.White, speakerblock.Move(0, -jog), centeralign);
                }


                foreach (var word in linewords.Words)
                {
                    Font f = word.IsLSBSymbol ? (word.IsBold ? flsbbold : flsbregular) : (word.IsBold ? fbold : fregular);
                    float jog = word.IsLSBSymbol ? 0.07f * (gfx.DpiY * gfx.MeasureStringCharacters(linewords.Speaker, ref f, speakerblock).Height / 72) : 0;
                    gfx.DrawString(word.Value, f, textcol, text.Move(xoffset, linepos + interspace * linenum).Move(0, -jog).Location, GraphicsHelper.DefaultStringFormat());
                    // make text fully opaque 
                    kgfx.DrawString(word.Value, f, Brushes.White, text.Move(xoffset, linepos + interspace * linenum).Move(0, -jog).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += word.Size.Width;
                }


                linenum++;
                linepos += (int)linewords.Height;



            }


            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;

            return res;
        }
    }
}
