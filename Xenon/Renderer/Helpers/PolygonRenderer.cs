using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.LayoutInfo;

namespace Xenon.Renderer.Helpers
{
    [Obsolete("Switch to imagesharp. Use CommonPolygonRenderer instead.")]
    internal static class PolygonRenderer
    {
        public static void Render(Graphics gfx, Graphics kgfx, LWJPolygon layout)
        {
            var points = layout.Verticies.ApplyTransforms(layout.Transforms);

            gfx.FillPolygon(new SolidBrush(layout.FillColor.GetColor()), points);
            gfx.DrawPolygon(new Pen(layout.BorderColor.GetColor(), layout.BorderWidth), points);

            kgfx.FillPolygon(new SolidBrush(layout.KeyFillColor.GetColor()), points);
            kgfx.DrawPolygon(new Pen(layout.KeyBorderColor.GetColor(), layout.BorderWidth), points);
        }

        public static void RenderLayoutPreview(Graphics gfx, Graphics kgfx, LWJPolygon layout)
        {
            var points = layout.Verticies.ApplyTransforms(layout.Transforms);

            gfx.FillPolygon(new SolidBrush(layout.FillColor.GetColor()), points);
            gfx.DrawPolygon(new Pen(layout.BorderColor.GetColor(), layout.BorderWidth), points);

            kgfx.FillPolygon(new SolidBrush(layout.KeyFillColor.GetColor()), points);
            kgfx.DrawPolygon(new Pen(layout.KeyBorderColor.GetColor(), layout.BorderWidth), points);
        }

    }
}
