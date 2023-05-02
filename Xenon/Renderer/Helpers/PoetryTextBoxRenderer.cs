using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers
{
    [Obsolete("Use ImageSharp for drawing. CommonPoetryTextRenderer is equivalent.")]
    internal static class PoetryTextBoxRenderer
    {

        public static void Render(Graphics gfx, Graphics kgfx, List<string> lines, PoetryLinesTextboxLayout layout)
        {
            // draw backgrounds still
            gfx.FillRectangle(new SolidBrush(layout.BColor.GetColor()), layout.Textbox.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KColor.GetColor()), layout.Textbox.GetRectangle());

            Color grayalpha = Color.FromArgb(255, layout.FColor.Alpha, layout.FColor.Alpha, layout.FColor.Alpha);

            if (lines != null && lines.Count > 0)
            {
                // draw content lines

                // get height of tallest lines
                double lhmax = lines.Max(x => gfx.MeasureString(x, layout.Font.GetFont()).Height);

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

                RectangleF vline = new RectangleF(layout.Textbox.Origin.X, layout.Textbox.Origin.Y, layout.Textbox.Size.Width, (float)lhmax);

                foreach (var line in lines)
                {
                    vline = vline.Move(0, (float)yoff);
                    gfx.DrawString(line, layout.Font.GetFont(), new SolidBrush(layout.FColor.GetColor()), vline, layout.GetHTextAlignment());
                    kgfx.DrawString(line, layout.Font.GetFont(), new SolidBrush(grayalpha), vline, layout.GetHTextAlignment());
                    yoff = interspace + lhmax;
                }
            }
        }

        public static void RenderLayoutPreview(Graphics gfx, Graphics kgfx, PoetryLinesTextboxLayout layout)
        {
            Render(gfx, kgfx, new List<string> { "Line 1", "Line 2", "Line 3", "Line 4" }, layout);
        }

    }
}
