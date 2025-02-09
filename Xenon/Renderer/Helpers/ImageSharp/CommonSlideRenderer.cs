﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xenon.LayoutInfo;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal class CommonSlideRenderer
    {
        public static void Render(out Image<Bgra32> bmp, out Image<Bgra32> kbmp, ASlideLayoutInfo layout)
        {
            bmp = new Image<Bgra32>(layout.SlideSize.Width, layout.SlideSize.Height);
            kbmp = new Image<Bgra32>(layout.SlideSize.Width, layout.SlideSize.Height);

            bmp.Mutate(gfx =>
            {
                gfx.BackgroundColor(layout.BackgroundColor.ToColor());
            });

            kbmp.Mutate(gfx =>
            {
                gfx.BackgroundColor(layout.KeyColor.ToColor());
            });
        }

    }
}
