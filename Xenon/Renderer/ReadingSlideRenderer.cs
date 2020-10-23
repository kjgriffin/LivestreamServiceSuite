using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;

namespace Xenon.Renderer
{
    class ReadingSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(ReadingLayoutRenderInfo renderInfo, Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

            Bitmap bmp = new Bitmap(Layouts.ReadingLayout.Size.Width, Layouts.ReadingLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.Gray);
            gfx.FillRectangle(Brushes.Black, Layouts.ReadingLayout.Key);


            // put name in center left
            gfx.DrawString(slide.Lines[0].Content[0].Data, renderInfo.RegularFont, Brushes.White, Layouts.ReadingLayout.TextAera.Move(Layouts.ReadingLayout.Key.Location).Move(150, 0), GraphicsHelper.LeftVerticalCenterAlign); 


            // put reference in center right
            gfx.DrawString(slide.Lines[1].Content[0].Data, renderInfo.ItalicFont, Brushes.White, Layouts.ReadingLayout.TextAera.Move(Layouts.ReadingLayout.Key.Location).Move(-180, 0), GraphicsHelper.RightVerticalCenterAlign);


            res.Bitmap = bmp;
            return res;

        }





    }
}
