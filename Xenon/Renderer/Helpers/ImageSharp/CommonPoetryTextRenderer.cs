using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.FontManagement;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal static class CommonPoetryTextRenderer
    {

        public static void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, PoetryLinesTextboxLayout layout, List<string> lines)
        {
            CommonTextBoxRenderer.RenderUnfilled(ibmp, ikbmp, layout);

            if (lines == null || !lines.Any())
            {
                return;
            }

            double lhmax = lines.Max(x => TextMeasurer.Measure(x, new TextOptions(FontManager.GetFont(layout.Font))).Height);

            // compute interline spacing
            double interspace = (layout.Textbox.Size.Height - (lines.Count * lhmax)) / (layout.VPaddingEnabled ? lines.Count + 1 : Math.Max(lines.Count - 1, 1));

            if (layout.VerticalAlignment == LWJVAlign.Equidistant)
            {
                interspace = Math.Max(interspace, layout.MinInterLineSpace);
            }
            else
            {
                interspace = layout.MinInterLineSpace;
            }

            double tlheight = lines.Count * lhmax + (layout.VPaddingEnabled ? (lines.Count + 1) * interspace : (lines.Count - 1) * interspace);
            double yoff = 0;

            if (layout.VerticalAlignment == LWJVAlign.Top || layout.VerticalAlignment == LWJVAlign.Equidistant)
            {
                yoff = 0;
            }
            else if (layout.VerticalAlignment == LWJVAlign.Bottom)
            {
                yoff = layout.Textbox.Size.Height - tlheight;
            }
            else if (layout.VerticalAlignment == LWJVAlign.Center)
            {
                double hrem = layout.Textbox.Size.Height - tlheight;
                yoff = hrem / 2;
            }
            yoff += layout.VPaddingEnabled ? interspace : 0;

            ibmp.Mutate(ctx =>
            {
                RectangleF vline = new RectangleF(layout.Textbox.Origin.X, layout.Textbox.Origin.Y, layout.Textbox.Size.Width, (float)lhmax);
                foreach (var line in lines)
                {
                    vline = new RectangleF(vline.X, vline.Y + (float)yoff, vline.Width, vline.Height);
                    ctx.DrawText(line, layout);
                    yoff = interspace + lhmax;
                }
            });

            ikbmp.Mutate(ctx =>
            {
                RectangleF vline = new RectangleF(layout.Textbox.Origin.X, layout.Textbox.Origin.Y, layout.Textbox.Size.Width, (float)lhmax);
                foreach (var line in lines)
                {
                    vline = new RectangleF(vline.X, vline.Y + (float)yoff, vline.Width, vline.Height);
                    ctx.DrawKeyText(line, layout);
                    yoff = interspace + lhmax;
                }
            });
        }

        public static void RenderLayoutPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, PoetryLinesTextboxLayout layout)
        {
            Render(ibmp, ikbmp, layout, new List<string> { "Line 1", "Line 2", "Line 3", "Line 4" });
        }

    }
}
