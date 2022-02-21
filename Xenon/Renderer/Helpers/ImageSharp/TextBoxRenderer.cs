using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.LayoutInfo.BaseTypes;
using Xenon.FontManagement;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal class CommonTextBoxRenderer
    {

        public static void RenderUnfilled(Image<Bgra32> bmp, Image<Bgra32> kbmp, TextboxLayout layout)
        {
            bmp.Mutate(g =>
            {
                g.BackgroundColor(layout.BColor.ToColor());
            });
            kbmp.Mutate(g =>
            {
                g.BackgroundColor(layout.KColor.ToColor());
            });
        }

        public static void Render(Image<Bgra32> bmp, Image<Bgra32> kbmp, TextboxLayout layout, string text)
        {
            RenderUnfilled(bmp, kbmp, layout);

            bmp.Mutate(g =>
            {
                g.DrawText(text, FontManager.GetFont(layout.Font), layout.FColor.ToColor(), layout.Textbox.Origin.PointF);
            });
            kbmp.Mutate(g =>
            {
                g.DrawText(text, FontManager.GetFont(layout.Font), layout.FColor.ToAlphaColor(), layout.Textbox.Origin.PointF);
            });
        }
    }
}
