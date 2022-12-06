using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Xenon.FontManagement;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal static class CommonPoetryTextRenderer
    {

        public static void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, PoetryLinesTextboxLayout layout, List<string> lines, bool preSymbol = false, string preSymbolText = "", string preSymbolFont = "")
        {
            CommonTextBoxRenderer.RenderUnfilled(ibmp, ikbmp, layout);

            if (lines == null || !lines.Any())
            {
                return;
            }

            double lhmax = lines.Max(x => TextMeasurer.Measure(x, new TextOptions(FontManager.GetFont(layout.Font)) { Dpi = 96 }).Height);

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


            TextOptions tops = new TextOptions(FontManager.GetFont(layout.Font))
            {
                HorizontalAlignment = layout.HorizontalAlignment.HALIGN(),
                VerticalAlignment = VerticalAlignment.Top,
                WrappingLength = layout.Textbox.Size.Width,
                Dpi = 96,
            };


            var f = FontManager.GetFont(new LWJFont(preSymbolFont, layout.Font.Size, layout.Font.Style));
            TextOptions ptops = new TextOptions(f);
            ptops.HorizontalAlignment = HorizontalAlignment.Left;
            ptops.Dpi = 96;
            ptops.Origin = new Vector2(layout.Textbox.Origin.X, layout.Textbox.Origin.Y + (float)yoff);
            float pxoff = TextMeasurer.Measure(preSymbolText, ptops).Width;
            float doxx = TextMeasurer.Measure(lines.FirstOrDefault(""), tops).Width;

            float xoff = preSymbol ? pxoff : 0;

            switch (layout.HorizontalAlignment)
            {
                case LWJHAlign.Center:
                    xoff = layout.Textbox.Size.Width / 2f;
                    ptops.Origin = new Vector2(ptops.Origin.X + layout.Textbox.Size.Width / 2 - (doxx / 2) - pxoff, ptops.Origin.Y);
                    break;
                case LWJHAlign.Right:
                    xoff = layout.Textbox.Size.Width;
                    ptops.Origin = new Vector2(ptops.Origin.X + layout.Textbox.Size.Width - doxx - pxoff, ptops.Origin.Y);
                    break;
            }



            ibmp.Mutate(ctx =>
            {
                tops.Origin = new System.Numerics.Vector2(layout.Textbox.Origin.X + xoff, layout.Textbox.Origin.Y);
                int i = 0;
                foreach (var line in lines)
                {
                    tops.Origin = new System.Numerics.Vector2(tops.Origin.X, tops.Origin.Y + (float)yoff);

                    if (i == 0 && preSymbol)
                    {
                        ctx.DrawText(ptops, preSymbolText, layout.FColor.ToColor());
                    }

                    ctx.DrawText(tops, line, layout.FColor.ToColor());
                    yoff = interspace + lhmax;
                    i++;
                }
            });

            ikbmp.Mutate(ctx =>
            {
                tops.Origin = new System.Numerics.Vector2(layout.Textbox.Origin.X + xoff, layout.Textbox.Origin.Y);
                int i = 0;
                foreach (var line in lines)
                {
                    tops.Origin = new System.Numerics.Vector2(tops.Origin.X, tops.Origin.Y + (float)yoff);

                    if (i == 0 && preSymbol)
                    {
                        ctx.DrawText(ptops, preSymbolText, layout.FColor.ToColor());
                    }

                    ctx.DrawText(tops, line, layout.FColor.ToAlphaColor());
                    yoff = interspace + lhmax;
                    i++;
                }
            });
        }

        public static void RenderLayoutPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, PoetryLinesTextboxLayout layout)
        {
            Render(ibmp, ikbmp, layout, new List<string> { "Line 1", "Line 2", "Line 3", "Line 4" });
        }

    }
}
