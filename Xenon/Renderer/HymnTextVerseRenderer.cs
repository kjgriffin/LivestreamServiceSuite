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

            if ((bool)slide.Data.GetOrDefault("is-overlay", false))
            {
                res.RenderedAs = "Full.nodrive";
                RenderOverlayVerse(renderInfo, slide, res, bmp, kbmp, gfx, kgfx);
            }
            else
            {
                RenderNormalVerse(renderInfo, slide, res, bmp, kbmp, gfx, kgfx);
            }
            return res;
        }

        private void RenderOverlayVerse(HymnTextVerseRenderInfo renderInfo, Slide slide, RenderedSlide res, Bitmap bmp, Bitmap kbmp, Graphics gfx, Graphics kgfx)
        {
            gfx.Clear(Color.White);
            // try background transparency at 50%?? (not sure if gray is actualy 50%)
            kgfx.Clear(Color.Gray);

            // debug
            
            // define new boxes for overlays
            Rectangle MainHymnNameBox = new Rectangle((int)(1920 * 0.1), (int)(1080 * 0.03), (int)(1920 * 0.8), (int)(1080 * 0.13));

            //gfx.DrawRectangle(Pens.Green, MainHymnNameBox);

            //gfx.DrawRectangle(Pens.Green, Layouts.TextHymnLayout.CopyrightBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NameBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NumberBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.TitleBox);
            //gfx.DrawRectangle(Pens.Red, Layouts.TextHymnLayout.TextBox);

            // draw name
            string name = (string)slide.Data["name"];
            Font namefont = new Font("Arial", 30, (FontStyle.Bold | FontStyle.Underline));
            gfx.DrawString(name, namefont, Brushes.Black, MainHymnNameBox, GraphicsHelper.CenterAlign);
            kgfx.DrawString(name, namefont, Brushes.White, MainHymnNameBox, GraphicsHelper.CenterAlign);

            // draw title
            Font titlefont = new Font("Arial", 30);
            string titleandsubname = (string)slide.Data["sub-name"] == "" ? (string)slide.Data["title"] : $"{(string)slide.Data["title"]}\r\n{(string)slide.Data["sub-name"]}";
            gfx.DrawString(titleandsubname, titlefont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);
            kgfx.DrawString(titleandsubname, titlefont, Brushes.White, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

            // draw sub-title
            //gfx.DrawString((string)slide.Data["sub-name"], renderInfo.TitleFont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

            // draw number and tune
            string numberandtune = (string)slide.Data["tune"] == "" ? $"{(string)slide.Data["number"]}" : $"{(string)slide.Data["number"]}\r\n{(string)slide.Data["tune"]}";
            gfx.DrawString(numberandtune, renderInfo.NumberFont, Brushes.Black, Layouts.TextHymnLayout.NumberBox, GraphicsHelper.CenterAlign);
            kgfx.DrawString(numberandtune, renderInfo.NumberFont, Brushes.White, Layouts.TextHymnLayout.NumberBox, GraphicsHelper.CenterAlign);

            // draw copyright
            gfx.DrawString((string)slide.Data["copyright"], renderInfo.CopyrightFont, Brushes.Gray, Layouts.TextHymnLayout.CopyrightBox, GraphicsHelper.CenterAlign);
            kgfx.DrawString((string)slide.Data["copyright"], renderInfo.CopyrightFont, Brushes.Gray, Layouts.TextHymnLayout.CopyrightBox, GraphicsHelper.CenterAlign);

            // draw text
            // compute interline spacing


            Rectangle versebox = new Rectangle(0, (int)(1080 * 0.16), 1920, (int)(1080 * 0.7));
            //gfx.DrawRectangle(Pens.Red, versebox);
            Font versefont = new Font("Arial", 56, FontStyle.Bold);

            double lineheight = gfx.MeasureString(slide.Lines[0].Content[0].Data, versefont).Height;
            double interspace = (versebox.Height - (slide.Lines.Count * lineheight)) / (slide.Lines.Count + 1);

            double offset = 0;
            Rectangle vline = new Rectangle(0, versebox.Y, versebox.Width, (int)lineheight).Move(new Point(0, (int)interspace));

            foreach (var line in slide.Lines)
            {
                gfx.DrawString(line.Content[0].Data, versefont, Brushes.Black, vline.Move(new Point(0, (int)offset)), GraphicsHelper.CenterAlign);
                kgfx.DrawString(line.Content[0].Data, versefont, Brushes.White, vline.Move(new Point(0, (int)offset)), GraphicsHelper.CenterAlign);
                offset += interspace + lineheight;
            }

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
        }


        private void RenderNormalVerse(HymnTextVerseRenderInfo renderInfo, Slide slide, RenderedSlide res, Bitmap bmp, Bitmap kbmp, Graphics gfx, Graphics kgfx)
        {
            gfx.Clear(Color.White);
            kgfx.Clear(Color.White);

            // debug

            //gfx.DrawRectangle(Pens.Green, Layouts.TextHymnLayout.CopyrightBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NameBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NumberBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.TitleBox);
            //gfx.DrawRectangle(Pens.Red, Layouts.TextHymnLayout.TextBox);

            // draw name
            string name = (string)slide.Data["name"];
            gfx.DrawString(name, renderInfo.NameFont, Brushes.Black, Layouts.TextHymnLayout.NameBox, GraphicsHelper.CenterAlign);

            // draw title
            string titleandsubname = (string)slide.Data["sub-name"] == "" ? (string)slide.Data["title"] : $"{(string)slide.Data["title"]}\r\n{(string)slide.Data["sub-name"]}";
            gfx.DrawString(titleandsubname, renderInfo.TitleFont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

            // draw sub-title
            //gfx.DrawString((string)slide.Data["sub-name"], renderInfo.TitleFont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

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
                gfx.DrawString(line.Content[0].Data, renderInfo.VerseFont, Brushes.Black, vline.Move(new Point(0, (int)offset)), GraphicsHelper.CenterAlign);
                offset += interspace + lineheight;
            }

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
        }
    }
}