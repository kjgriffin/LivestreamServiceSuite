using System;
using System.Drawing;

using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers
{
    [Obsolete("Switch to imagesharp. Use CommonTextBoxRenderer instead.")]
    internal static class TextBoxRenderer
    {
        public static void RenderUnFilled(Graphics gfx, Graphics kgfx, TextboxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.BColor.GetColor()), layout.Textbox.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KColor.GetColor()), layout.Textbox.GetRectangle());
        }

        public static void Render(Graphics gfx, Graphics kgfx, string text, TextboxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.BColor.GetColor()), layout.Textbox.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KColor.GetColor()), layout.Textbox.GetRectangle());

            gfx.DrawString(text, layout.Font.GetFont(), new SolidBrush(layout.FColor.GetColor()), layout.Textbox.GetRectangle(), layout.GetTextAlignment());

            Color grayalpha = Color.FromArgb(255, layout.FColor.Alpha, layout.FColor.Alpha, layout.FColor.Alpha);
            kgfx.DrawString(text, layout.Font.GetFont(), new SolidBrush(grayalpha), layout.Textbox.GetRectangle(), layout.GetTextAlignment());
        }
        public static void RenderLayoutPreview(Graphics gfx, Graphics kgfx, TextboxLayout layout, string text = "Example Text")
        {
            gfx.FillRectangle(new SolidBrush(layout.BColor.GetColor()), layout.Textbox.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KColor.GetColor()), layout.Textbox.GetRectangle());

            gfx.DrawString(text, layout.Font.GetFont(), new SolidBrush(layout.FColor.GetColor()), layout.Textbox.GetRectangle(), layout.GetTextAlignment());

            Color grayalpha = Color.FromArgb(255, layout.FColor.Alpha, layout.FColor.Alpha, layout.FColor.Alpha);
            kgfx.DrawString(text, layout.Font.GetFont(), new SolidBrush(grayalpha), layout.Textbox.GetRectangle(), layout.GetTextAlignment());
        }
        public static void RenderLayoutGhostPreview(Graphics gfx, Graphics kgfx, TextboxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.BColor.GetColor()), layout.Textbox.GetRectangle());

            if (layout.BColor.Alpha > 0)
            {
                gfx.DrawRectangle(new Pen(layout.BColor.GetColor(), 5) { DashPattern = new float[] { 10, 5 } }, layout.Textbox.GetRectangle());
            }
            else
            {
                gfx.DrawRectangle(new Pen(Color.FromArgb(200, 200, 200), 5) { DashPattern = new float[] { 10, 5 } }, layout.Textbox.GetRectangle());
            }


            kgfx.FillRectangle(new SolidBrush(layout.KColor.GetColor()), layout.Textbox.GetRectangle());
        }

    }
}
