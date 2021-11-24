using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Renderer.Helpers
{
    internal static class DrawingBoxRenderer
    {
        public static void Render(Graphics gfx, Graphics kgfx, DrawingBoxLayout layout)
        {
            gfx.FillRectangle(new SolidBrush(layout.FillColor.GetColor()), layout.Box.GetRectangle());
            kgfx.FillRectangle(new SolidBrush(layout.KeyColor.GetColor()), layout.Box.GetRectangle());
        }

    }
}
