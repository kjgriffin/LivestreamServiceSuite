using System;
using System.Drawing;

using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers
{
    [Obsolete("Switch to imagesharp. Use CommonDrawingBoxRenderer instead.")]
    internal static class DrawingBoxRenderer
    {
        public static void Render(Graphics gfx, Graphics kgfx, DrawingBoxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.FillColor.GetColor()), layout.Box.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KeyColor.GetColor()), layout.Box.GetRectangle());
        }
        public static void RenderLayoutPreview(Graphics gfx, Graphics kgfx, DrawingBoxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.FillColor.GetColor()), layout.Box.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KeyColor.GetColor()), layout.Box.GetRectangle());

            // draw dashed border?

            gfx.DrawImage(ProjectResources.Icons.ImageIcon, layout.Box.GetRectangle(), new Rectangle(Point.Empty, ProjectResources.Icons.ImageIcon.Size), GraphicsUnit.Pixel);

        }


    }
}
