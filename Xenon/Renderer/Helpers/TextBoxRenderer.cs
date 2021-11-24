using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers
{
    internal static class TextBoxRenderer
    {
        public static void Render(Graphics gfx, Graphics kgfx, string text, TextboxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.BColor.GetColor()), layout.Textbox.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KColor.GetColor()), layout.Textbox.GetRectangle());

            gfx.DrawString(text, layout.Font.GetFont(), new SolidBrush(layout.FColor.GetColor()), layout.Textbox.GetRectangle(), layout.GetTextAlignment());

            Color grayalpha = Color.FromArgb(255, layout.FColor.Alpha, layout.FColor.Alpha, layout.FColor.Alpha);
            kgfx.DrawString(text, layout.Font.GetFont(), new SolidBrush(grayalpha), layout.Textbox.GetRectangle(), layout.GetTextAlignment());
        }
    }
}
