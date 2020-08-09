using SlideCreater.LayoutEngine;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SlideCreater.Renderer
{
    class SermonTitleSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(SermonLayoutRenderInfo renderInfo, Slide slide)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

            Bitmap bmp = new Bitmap(Layouts.SermonLayout.Size.Width, Layouts.SermonLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.Black);
            gfx.FillRectangle(Brushes.White, Layouts.SermonLayout.Key);

            // put 'sermon' in top left
            gfx.DrawString("Sermon", renderInfo.RegularFont, Brushes.Black, Layouts.SermonLayout.TopLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            // put reference in top right
            gfx.DrawString(slide.Lines[1].Content[0].Data, renderInfo.ItalicFont, Brushes.Black, Layouts.SermonLayout.TopLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);
            // put title in center mid
            gfx.DrawString(slide.Lines[0].Content[0].Data, renderInfo.BoldFont, Brushes.Black, Layouts.SermonLayout.MainLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location), GraphicsHelper.CenterAlign);

            res.Bitmap = bmp;
            return res;
        }
    }
}
