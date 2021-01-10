using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System.Drawing;
using Xenon.Helpers;

namespace Xenon.Renderer
{
    class HymnTextVerseRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(HymnTextVerseRenderInfo renderInfo, Slide slide, System.Collections.Generic.List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(Layouts.TextHymnLayout.Size.Width, Layouts.TextHymnLayout.Size.Height);
            Bitmap kbmp = new Bitmap(Layouts.TextHymnLayout.Size.Width, Layouts.TextHymnLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.White);
            kgfx.Clear(Color.White);

            // debug

            //gfx.DrawRectangle(Pens.Green, Layouts.TextHymnLayout.CopyrightBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NameBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NumberBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.TitleBox);
            //gfx.DrawRectangle(Pens.Red, Layouts.TextHymnLayout.TextBox);

            // draw name
            gfx.DrawString((string)slide.Data["name"], renderInfo.NameFont, Brushes.Black, Layouts.TextHymnLayout.NameBox, GraphicsHelper.CenterAlign);

            // draw title
            gfx.DrawString((string)slide.Data["title"], renderInfo.TitleFont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

            // draw number and tune
            string numberandtune = (string)slide.Data["tune"] == "" ? $"{(string)slide.Data["number"]}" : $"{(string)slide.Data["number"]}\r\n{(string)slide.Data["tune"]}";
            gfx.DrawString(numberandtune, renderInfo.NumberFont, Brushes.Black, Layouts.TextHymnLayout.NumberBox, GraphicsHelper.CenterAlign);

            // draw copyright
            gfx.DrawString((string)slide.Data["copyright"], renderInfo.CopyrightFont, Brushes.Gray, Layouts.TextHymnLayout.CopyrightBox, GraphicsHelper.CenterAlign);

            // draw text
            // compute interline spacing



            double lineheight = gfx.MeasureString(slide.Lines[0].Content[0].Data, renderInfo.VerseFont).Height;
            double interspace = (Layouts.TextHymnLayout.TextBox.Height - (slide.Lines.Count * lineheight)) / (slide.Lines.Count + 1);

            double offset = 0;
            Rectangle vline = new Rectangle(0, Layouts.TextHymnLayout.TextBox.Y, Layouts.TextHymnLayout.TextBox.Width, (int)lineheight).Move(new Point(0, (int)interspace));

            foreach (var line in slide.Lines)
            {
                gfx.DrawString(line.Content[0].Data, renderInfo.VerseFont, Brushes.Black, vline.Move(new Point(0, (int)offset)), GraphicsHelper.CenterAlign );
                offset += interspace + lineheight;
            }




            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }

    }
}