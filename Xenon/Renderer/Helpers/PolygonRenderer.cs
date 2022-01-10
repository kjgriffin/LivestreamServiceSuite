using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Layouts;

namespace Xenon.Renderer.Helpers
{
    internal static class PolygonRenderer
    {

        public static void Render(Graphics gfx, Graphics kgfx, LWJPolygon layout)
        {
            gfx.FillPolygon(new SolidBrush(layout.FillColor.GetColor()), layout.Verticies.Select(p => p.GetPoint()).ToArray());
            gfx.DrawPolygon(new Pen(layout.BorderColor.GetColor(), layout.BorderWidth), layout.Verticies.Select(p => p.GetPoint()).ToArray());

            kgfx.FillPolygon(new SolidBrush(layout.KeyFillColor.GetColor()), layout.Verticies.Select(p => p.GetPoint()).ToArray());
            kgfx.DrawPolygon(new Pen(layout.KeyBorderColor.GetColor(), layout.BorderWidth), layout.Verticies.Select(p => p.GetPoint()).ToArray());
        }

        public static void RenderLayoutPreview(Graphics gfx, Graphics kgfx, LWJPolygon layout)
        {
            gfx.FillPolygon(new SolidBrush(layout.FillColor.GetColor()), layout.Verticies.Select(p => p.GetPoint()).ToArray());
            gfx.DrawPolygon(new Pen(layout.BorderColor.GetColor(), layout.BorderWidth), layout.Verticies.Select(p => p.GetPoint()).ToArray());

            kgfx.FillPolygon(new SolidBrush(layout.KeyFillColor.GetColor()), layout.Verticies.Select(p => p.GetPoint()).ToArray());
            kgfx.DrawPolygon(new Pen(layout.KeyBorderColor.GetColor(), layout.BorderWidth), layout.Verticies.Select(p => p.GetPoint()).ToArray());
        }

    }
}
