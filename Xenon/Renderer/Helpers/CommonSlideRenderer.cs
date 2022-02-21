using System;
using System.Collections.Generic;
using GDI = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.LayoutInfo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Xenon.Renderer.Helpers
{
    internal class CommonSlideRenderer
    {

        public static void Render(GDI.Graphics gfx, GDI.Graphics kgfx, ALayoutInfo layout)
        {
            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.BackgroundColor.GetColor());
        }

        public static void Render(out Image<Bgra32> bmp, out Image<Bgra32> kbmp, ALayoutInfo layout)
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

        public static void RenderLayoutPreview(GDI.Graphics gfx, GDI.Graphics kgfx, ALayoutInfo layout)
        {

        }

    }
}
