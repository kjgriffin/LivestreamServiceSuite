using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.FontManagement;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

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
            TextOptions tops = new TextOptions(font)
            {
                HorizontalAlignment = HAlign.HALIGN(), // to simulate GDI+ behaviour: move the origin to compensate
                VerticalAlignment = VAlign.VALIGN(), // to simulate GDI+ behaviour: move the origin to compensate 
                WordBreaking = WordBreaking.Normal,
                LineSpacing = linespace,
                TextDirection = TextDirection.LeftToRight,
                TextAlignment = TextAlignment.Start,
                WrappingLength = rect.Width,
                Dpi = 96, // we'll statically assume DPI of 96 everywhere
                Origin = GDI_Compensate(HAlign, VAlign, rect),
            };
            SolidBrush brush = new SolidBrush(fcolor);

            ctx.DrawText(otps, tops, text, brush, null);
        }

        private static void DrawText_ManualOverflowCenter(this IImageProcessingContext ctx, string text, Font font, Color fcolor, RectangleF rect, LWJVAlign VAlign, float linespace = 1f)
        {

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            // 1. split text by whitespace (or some other split policy....)
            // 2. Try an put as much text on a line as posible
            // 3. once lines are determined, center each individually

            TextOptions opts = new TextOptions(font)
            {
                WrappingLength = rect.Width,
                Dpi = 96,
            };

            var words = Regex.Split(text, @"\s");

            List<string> lines = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (var word in words)
            {
                var str = sb.ToString() + " " + word;
                if (TextMeasurer.Measure(str, opts).Width <= rect.Width)
                {
                    sb.Append(" ");
                    sb.Append(word);
                }
                else
                {
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
            }
            lines.Add(sb.ToString());

            // measure all lines
            List<FontRectangle> linesizes = new List<FontRectangle>();
            foreach (var line in lines)
            {
                linesizes.Add(TextMeasurer.Measure(line, opts));
            }

            // draw all lines
            // need to space them correctly
            
            // calculate the vertical position based on VAlign



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
