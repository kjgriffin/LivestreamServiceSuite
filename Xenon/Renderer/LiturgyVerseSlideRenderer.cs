using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Xenon.Helpers;

namespace Xenon.Renderer
{
    class LiturgyVerseSlideRenderer
    {

        public SlideLayout Layouts { get; set; }

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

            Font LSBSymbolFont = new System.Drawing.Font("LSBSymbol", 36, System.Drawing.FontStyle.Regular);

            gfx.Clear(Color.Black);
            kgfx.Clear(Color.Black);

            gfx.FillRectangle(Brushes.White, Layouts.LiturgyLayout.Key);
            kgfx.FillRectangle(Brushes.White, Layouts.LiturgyLayout.Key);

            StringFormat topleftalign = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
            StringFormat centeralign = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

            System.Drawing.Rectangle text = Layouts.LiturgyLayout.Text.Move(Layouts.LiturgyLayout.Key.Location);

            System.Drawing.Rectangle speaker = Layouts.LiturgyLayout.Speaker.Move(Layouts.LiturgyLayout.Key.Location);

            //gfx.DrawRectangle(Pens.Red, text);
            //gfx.DrawRectangle(Pens.Purple, speaker);

            // compute line spacing
            int textlinecombinedheight = 0;
            foreach (var line in slide.Lines)
            {
                textlinecombinedheight += (int)(float)line.Content[0].Attributes["height"];
            }
            int interspace = (renderInfo.TextBox.Height - textlinecombinedheight) / (slide.Lines.Count + 1);

            int linenum = 0;
            int linepos = interspace;
            foreach (var line in slide.Lines)
            {

                gfx.DrawString(line.Content[0].Data, renderInfo.RegularFont, Brushes.Black, text.Move(0, linepos + interspace * linenum).Location, topleftalign);


                linenum++;
                linepos += (int)(float)line.Content[0].Attributes["height"];
            }

            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();

            return res;
        }
    }
}
