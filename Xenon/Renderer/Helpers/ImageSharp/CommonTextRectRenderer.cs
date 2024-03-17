using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Xenon.FontManagement;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

using IBrush = SixLabors.ImageSharp.Drawing.Processing.Brush;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal static class CommonTextRectRenderer
    {

        private static System.Numerics.Vector2 GDI_Compensate(LWJHAlign HAligh, LWJVAlign VAlign, RectangleF rect)
        {
            float xadjust = rect.X;
            float yadjust = rect.Y;

            switch (HAligh)
            {
                case LWJHAlign.Center:
                    xadjust += rect.Width / 2f;
                    break;
                case LWJHAlign.Right:
                    xadjust += rect.Width;
                    break;
            }

            switch (VAlign)
            {
                case LWJVAlign.Center:
                    yadjust += rect.Height / 2f;
                    break;
                case LWJVAlign.Bottom:
                    yadjust += rect.Height;
                    break;
            }
            return new System.Numerics.Vector2(xadjust, yadjust);
        }

        private static void DrawText(this IImageProcessingContext ctx, string text, Font font, Color fcolor, RectangleF rect, LWJHAlign HAlign, LWJVAlign VAlign, float linespace = 1f)
        {
            DrawingOptions otps = new DrawingOptions();
            /*
            TextOptions tops = new TextOptions(font)
            {
                HorizontalAlignment = HAlign.HALIGN(), // to simulate GDI+ behaviour: move the origin to compensate
                VerticalAlignment = VAlign.VALIGN(), // to simulate GDI+ behaviour: move the origin to compensate 
                WordBreaking = WordBreaking.Standard,
                LineSpacing = linespace,
                TextDirection = TextDirection.LeftToRight,
                TextAlignment = TextAlignment.Start,
                WrappingLength = rect.Width,
                Dpi = 96, // we'll statically assume DPI of 96 everywhere
                Origin = GDI_Compensate(HAlign, VAlign, rect),
            };
            */
            RichTextOptions rtops = new RichTextOptions(font)
            {
                HorizontalAlignment = HAlign.HALIGN(), // to simulate GDI+ behaviour: move the origin to compensate
                VerticalAlignment = VAlign.VALIGN(), // to simulate GDI+ behaviour: move the origin to compensate 
                WordBreaking = WordBreaking.Standard,
                LineSpacing = linespace,
                TextDirection = TextDirection.LeftToRight,
                TextAlignment = TextAlignment.Start,
                WrappingLength = rect.Width,
                Dpi = 96, // we'll statically assume DPI of 96 everywhere
                Origin = GDI_Compensate(HAlign, VAlign, rect),
            };
            SolidBrush brush = new SolidBrush(fcolor);

            if (HAlign == LWJHAlign.Centered)
            {
                DrawText_ManualOverflowCenter(ctx, text, otps, rtops, brush, rect, VAlign);
            }
            else
            {
                ctx.DrawText(otps, rtops, text, brush, null);
            }
        }

        private static void DrawText_ManualOverflowCenter(this IImageProcessingContext ctx, string text, DrawingOptions opts, RichTextOptions topts, IBrush brush, RectangleF rect, LWJVAlign valign)
        {

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            // 1. split text by whitespace (or some other split policy....)
            // 2. Try an put as much text on a line as posible
            // 3. once lines are determined, center each individually

            var words = Regex.Split(text, @"\s");

            List<string> lines = new List<string>();
            StringBuilder sb = new StringBuilder();

            // temporarily disable wrapping so we'll know truly how long something is...
            topts.WrappingLength = -1;
            topts.HorizontalAlignment = HorizontalAlignment.Left;
            topts.VerticalAlignment = VerticalAlignment.Top;

            foreach (var word in words)
            {
                // 1. see if we can add it to the existing line and still fit
                var tmp = "";
                var cur = sb.ToString();
                if (!string.IsNullOrWhiteSpace(cur))
                {
                    tmp += " " + word;
                }
                else
                {
                    tmp = word;
                }
                if (TextMeasurer.MeasureSize(cur + tmp, topts).Width <= rect.Width)
                {
                    // -- if so add it
                    sb.Append(tmp);
                }
                else
                {
                    // else finish the line
                    lines.Add(sb.ToString());
                    // start a new line with this word
                    sb.Clear();
                    sb.Append(word);
                }
            }
            var extra = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(extra))
            {
                lines.Add(extra);
            }

            // measure all lines
            List<FontRectangle> linesizes = new List<FontRectangle>();
            foreach (var line in lines)
            {
                linesizes.Add(TextMeasurer.MeasureSize(line, topts));
            }

            // need to space them correctly

            float yinit = 0;
            float ylineheight = linesizes.MaxBy(x => x.Height).Height;
            float yspace = ylineheight; // ... need to figure out a default space that 'looks' nice

            // calculate the vertical position based on VAlign

            yspace = yspace * topts.LineSpacing;
            float drawnheight = lines.Count * yspace;

            switch (valign)
            {
                case LWJVAlign.Top:
                    yinit = 0;
                    break;
                case LWJVAlign.Center:
                    yinit = (rect.Height - drawnheight) / 2;
                    break;
                case LWJVAlign.Bottom:
                    yinit = rect.Height - drawnheight;
                    break;
                case LWJVAlign.Equidistant:
                    yspace = yspace * topts.LineSpacing; // TODO: change this
                    yinit = 0;
                    break;
                default:
                    break;
            }


            // draw all lines
            int i = 0;
            float xorigctr = rect.Width / 2;
            foreach (var line in lines)
            {
                float x = rect.X;
                float y = rect.Y;

                // modify the origin to place the text where we want it

                // adjust x off to actually center it!
                x += (xorigctr - (linesizes[i].Width / 2));
                // ... hmmm add height offsets...
                y += yinit + yspace * i;


                topts.Origin = new System.Numerics.Vector2(x, y);

                ctx.DrawText(opts, topts, line, brush, null);
                i++;
            }

        }

        internal static void DrawText(this IImageProcessingContext ctx, string text, TextboxLayout layout)
        {
            ctx.DrawText(text, FontManager.GetFont(layout.Font), layout.FColor.ToColor(), layout.Textbox.RectangleF, layout.HorizontalAlignment, layout.VerticalAlignment, layout.LineSpacing);
        }

        internal static void DrawKeyText(this IImageProcessingContext ctx, string text, TextboxLayout layout)
        {
            ctx.DrawText(text, FontManager.GetFont(layout.Font), layout.FColor.RGBFromAlpha(), layout.Textbox.RectangleF, layout.HorizontalAlignment, layout.VerticalAlignment, layout.LineSpacing);
        }

    }
}
