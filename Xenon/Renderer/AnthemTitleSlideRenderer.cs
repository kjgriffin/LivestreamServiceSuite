using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class AnthemTitleSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

            Bitmap bmp = new Bitmap(Layouts.AnthemTitleLayout.Size.Width, Layouts.AnthemTitleLayout.Size.Height);
            Bitmap kbmp = new Bitmap(Layouts.AnthemTitleLayout.Size.Width, Layouts.AnthemTitleLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.Gray);
            kgfx.Clear(Color.Black);
            gfx.FillRectangle(Brushes.Black, Layouts.SermonLayout.Key);
            SolidBrush kb = new SolidBrush(slide.Colors["keytrans"]);
            kgfx.FillRectangle(kb, Layouts.SermonLayout.Key);

            Font bf = new Font(Layouts.AnthemTitleLayout.Font, FontStyle.Bold);

            // musician
            gfx.DrawString(slide.Lines[1].Content[0].Data, bf, Brushes.White, Layouts.AnthemTitleLayout.TopLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            kgfx.DrawString(slide.Lines[1].Content[0].Data, bf, Brushes.White, Layouts.AnthemTitleLayout.TopLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            // anthem title
            gfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.AnthemTitleLayout.TopLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);
            kgfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.AnthemTitleLayout.TopLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);
            // accompanianist 
            gfx.DrawString(slide.Lines[2].Content[0].Data, Layouts.AnthemTitleLayout.Font, Brushes.White, Layouts.AnthemTitleLayout.MainLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            kgfx.DrawString(slide.Lines[2].Content[0].Data, Layouts.AnthemTitleLayout.Font, Brushes.White, Layouts.AnthemTitleLayout.MainLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            // credit
            gfx.DrawString(slide.Lines[3].Content[0].Data, Layouts.AnthemTitleLayout.Font, Brushes.White, Layouts.AnthemTitleLayout.MainLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);
            kgfx.DrawString(slide.Lines[3].Content[0].Data, Layouts.AnthemTitleLayout.Font, Brushes.White, Layouts.AnthemTitleLayout.MainLine.Move(Layouts.AnthemTitleLayout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);

            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            res.KeyBitmap = kbmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            return res;
        }
    }
}
