using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
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

        internal static void DrawText(this IImageProcessingContext ctx, string text, Font font, Color fcolor, RectangleF rect, LWJHAlign HAlign, LWJVAlign VAlign)
        {
            DrawingOptions otps = new DrawingOptions();
            TextOptions tops = new TextOptions(font)
            {
                HorizontalAlignment = HAlign.HALIGN(), // to simulate GDI+ behaviour: move the origin to compensate
                VerticalAlignment = VAlign.VALIGN(), // to simulate GDI+ behaviour: move the origin to compensate 
                WordBreaking = WordBreaking.Normal,
                LineSpacing = 1,
                TextDirection = TextDirection.LeftToRight,
                TextAlignment = TextAlignment.Start,
                WrappingLength = rect.Width,
                //Dpi = System.Drawing.Graphics.FromHwnd(IntPtr.Zero).DpiX, // - might not need this...
                Origin = GDI_Compensate(HAlign, VAlign, rect),
            };
            SolidBrush brush = new SolidBrush(fcolor);

            ctx.DrawText(otps, tops, text, brush, null);
        }

        internal static void DrawText(this IImageProcessingContext ctx, string text, TextboxLayout layout)
        {
            ctx.DrawText(text, FontManager.GetFont(layout.Font), layout.FColor.ToColor(), layout.Textbox.RectangleF, layout.HorizontalAlignment, layout.VerticalAlignment);
        }

        internal static void DrawKeyText(this IImageProcessingContext ctx, string text, TextboxLayout layout)
        {
            ctx.DrawText(text, FontManager.GetFont(layout.Font), layout.KColor.ToColor(), layout.Textbox.RectangleF, layout.HorizontalAlignment, layout.VerticalAlignment);
        }

    }
}
