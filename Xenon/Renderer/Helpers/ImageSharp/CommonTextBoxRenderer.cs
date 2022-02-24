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
using Xenon.LayoutInfo;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal class CommonTextBoxRenderer
    {

        public static void RenderUnfilled(Image<Bgra32> bmp, Image<Bgra32> kbmp, TextboxLayout layout)
        {
            bmp.Mutate(g =>
            {
                g.Fill(layout.BColor.ToColor(), layout.Textbox.RectangleF);
            });
            kbmp.Mutate(g =>
            {
                g.Fill(layout.KColor.ToColor(), layout.Textbox.RectangleF);
            });
        }

        public static void Render(Image<Bgra32> bmp, Image<Bgra32> kbmp, TextboxLayout layout, string text)
        {
            RenderUnfilled(bmp, kbmp, layout);

            bmp.Mutate(g =>
            {
                g.DrawText(text, layout);
            });
            kbmp.Mutate(g =>
            {
                g.DrawKeyText(text, layout);
            });
        }

        internal static void RenderLayoutPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, TextboxLayout layout, string exampleText = "Example Text")
        {
            Render(ibmp, ikbmp, layout, exampleText);
        }

        internal static void RenderLayoutGhostPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, LiturgyTextboxLayout layout)
        {
            RenderUnfilled(ibmp, ikbmp, layout);
            if (layout.BColor.Alpha > 0)
            {
                ibmp.Mutate(ctx => ctx.Draw(Pens.Dash(layout.BColor.ToColor(), 5), layout.Textbox.RectangleF));
            }
            else
            {
                ibmp.Mutate(ctx => ctx.Draw(Pens.Dash(Color.FromRgb(200, 200, 200), 5), layout.Textbox.RectangleF));
            }
        }
    }
}
