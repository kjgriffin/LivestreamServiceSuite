using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.LayoutInfo;

namespace Xenon.Renderer.Helpers.ImageSharp
{
    internal static class CommonPolygonRenderer
    {
        public static void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, LWJPolygon layout)
        {
            var points = layout.Verticies.ApplyTransformSet(layout.Transforms);

            ibmp.Mutate(ctx =>
            {
                ctx.FillPolygon(layout.FillColor.ToColor(), points);
                ctx.DrawPolygon(layout.BorderColor.ToColor(), layout.BorderWidth, points);
            });

            ibmp.Mutate(ctx =>
            {
                ctx.FillPolygon(layout.KeyFillColor.ToColor(), points);
                ctx.DrawPolygon(layout.KeyBorderColor.ToColor(), layout.BorderWidth, points);
            });
        }

        public static void RenderLayoutPreview(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, LWJPolygon layout)
        {
            Render(ibmp, ikbmp, layout);
        }

    }
}
