using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class TwoPartTitleSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

            Bitmap bmp = new Bitmap(Layouts.TwoPartTitleLayout.Size.Width, Layouts.TwoPartTitleLayout.Size.Height);
            Bitmap kbmp = new Bitmap(Layouts.TwoPartTitleLayout.Size.Width, Layouts.TwoPartTitleLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.Gray);
            kgfx.Clear(Color.Black);
            gfx.FillRectangle(Brushes.Black, Layouts.TwoPartTitleLayout.Key);
            kgfx.FillRectangle(new SolidBrush(slide.Colors["keytrans"]), Layouts.TwoPartTitleLayout.Key);

            Font bf = new Font(Layouts.TwoPartTitleLayout.Font, FontStyle.Bold);

            var lineheight = gfx.MeasureStringCharacters(slide.Lines[0].Content[0].Data, ref bf, Layouts.AnthemTitleLayout.Key);

            if ((string)slide.Data["orientation"] == "vertical")
            {
                gfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.TwoPartTitleLayout.Line1.Move(Layouts.TwoPartTitleLayout.Key.Location), GraphicsHelper.CenterAlign);
                kgfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.TwoPartTitleLayout.Line1.Move(Layouts.TwoPartTitleLayout.Key.Location), GraphicsHelper.CenterAlign);
                gfx.DrawString(slide.Lines[1].Content[0].Data, Layouts.TwoPartTitleLayout.Font, Brushes.White, Layouts.TwoPartTitleLayout.Line2.Move(Layouts.TwoPartTitleLayout.Key.Location), GraphicsHelper.CenterAlign);
                kgfx.DrawString(slide.Lines[1].Content[0].Data, Layouts.TwoPartTitleLayout.Font, Brushes.White, Layouts.TwoPartTitleLayout.Line2.Move(Layouts.TwoPartTitleLayout.Key.Location), GraphicsHelper.CenterAlign);
            }
            else
            {
                int ycord = (int)((Layouts.TwoPartTitleLayout.Key.Height / 2) - (lineheight.Height / 2));
                gfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.TwoPartTitleLayout.MainLine.Move(Layouts.TwoPartTitleLayout.Key.Location).Move(0, ycord), GraphicsHelper.LeftVerticalCenterAlign);
                kgfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, Layouts.TwoPartTitleLayout.MainLine.Move(Layouts.TwoPartTitleLayout.Key.Location).Move(0, ycord), GraphicsHelper.LeftVerticalCenterAlign);
                gfx.DrawString(slide.Lines[1].Content[0].Data, Layouts.TwoPartTitleLayout.Font, Brushes.White, Layouts.TwoPartTitleLayout.MainLine.Move(Layouts.TwoPartTitleLayout.Key.Location).Move(0, ycord), GraphicsHelper.RightVerticalCenterAlign);
                kgfx.DrawString(slide.Lines[1].Content[0].Data, Layouts.TwoPartTitleLayout.Font, Brushes.White, Layouts.TwoPartTitleLayout.MainLine.Move(Layouts.TwoPartTitleLayout.Key.Location).Move(0, ycord), GraphicsHelper.RightVerticalCenterAlign);
            }


            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }
    }
}
