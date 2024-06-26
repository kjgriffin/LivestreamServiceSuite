﻿using System.Collections.Generic;
using System.Drawing;

using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class SermonTitleSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(SermonLayoutRenderInfo renderInfo, Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy-t1restart";

            Bitmap bmp = new Bitmap(Layouts.SermonLayout.Size.Width, Layouts.SermonLayout.Size.Height);
            Bitmap kbmp = new Bitmap(Layouts.SermonLayout.Size.Width, Layouts.SermonLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.Gray);
            kgfx.Clear(Color.Black);
            gfx.FillRectangle(Brushes.Black, Layouts.SermonLayout.Key);
            kgfx.FillRectangle(new SolidBrush(slide.Colors["keytrans"]), Layouts.SermonLayout.Key);

            // put 'preacher' in top left
            gfx.DrawString(slide.Lines[2].Content[0].Data, renderInfo.RegularFont, Brushes.White, Layouts.SermonLayout.TopLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location).Move(100, 0), GraphicsHelper.LeftVerticalCenterAlign);
            kgfx.DrawString(slide.Lines[2].Content[0].Data, renderInfo.RegularFont, Brushes.White, Layouts.SermonLayout.TopLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location).Move(100, 0), GraphicsHelper.LeftVerticalCenterAlign);
            // put reference in top right
            gfx.DrawString(slide.Lines[1].Content[0].Data, renderInfo.ItalicFont, Brushes.White, Layouts.SermonLayout.TopLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location).Move(-100, 0), GraphicsHelper.RightVerticalCenterAlign);
            kgfx.DrawString(slide.Lines[1].Content[0].Data, renderInfo.ItalicFont, Brushes.White, Layouts.SermonLayout.TopLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location).Move(-100, 0), GraphicsHelper.RightVerticalCenterAlign);
            // put title in center mid
            gfx.DrawString(slide.Lines[0].Content[0].Data, renderInfo.BoldFont, Brushes.White, Layouts.SermonLayout.MainLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location), GraphicsHelper.CenterAlign);
            kgfx.DrawString(slide.Lines[0].Content[0].Data, renderInfo.BoldFont, Brushes.White, Layouts.SermonLayout.MainLine.Move(Layouts.SermonLayout.TextAera.Location).Move(Layouts.SermonLayout.Key.Location), GraphicsHelper.CenterAlign);

            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            res.KeyBitmap = kbmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            return res;
        }
    }
}
