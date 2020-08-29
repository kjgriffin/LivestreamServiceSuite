using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System.Drawing;
using Xenon.Helpers;

namespace Xenon.Renderer
{
    class HymnTextVerseRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(HymnTextVerseRenderInfo renderInfo, Slide slide)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(Layouts.TextHymnLayout.Size.Width, Layouts.TextHymnLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.White);

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

            double spacing = Layouts.TextHymnLayout.TextBox.Height / (slide.Lines.Count + 1);

            double offset = spacing;

            Rectangle vline = new Rectangle(0, (int)offset, Layouts.TextHymnLayout.TextBox.Width, (int)spacing);

            foreach (var line in slide.Lines)
            {
                gfx.DrawString(line.Content[0].Data, renderInfo.VerseFont, Brushes.Black, vline.Move(new Point(0, (int)offset)), GraphicsHelper.CenterAlign );
                offset += spacing;
            }




            res.Bitmap = bmp;
            return res;
        }

    }
}