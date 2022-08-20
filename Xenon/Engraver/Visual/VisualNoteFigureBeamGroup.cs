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

using Xenon.Engraver.DataModel;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class VisualNoteFigureBeamGroup : IEngravingRenderable
    {

        public float XOffset { get; set; } = 0;
        public float YOffset { get; set; } = 0;

        public List<Note> ChildNotes { get; set; } = new List<Note>();
        public Clef Clef { get; set; }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, HashSet<string> debug = null)
        {
            var bounds = CalculateBounds(X, Y);


            bounds.Render(ibmp, ikbmp, debug);
        }

        internal VisualNoteFigureBeamGroupLayoutBounds CalculateBounds(float X, float Y)
        {
            float xprim = X + XOffset;
            float yprim = Y + YOffset;
            return new VisualNoteFigureBeamGroupLayoutBounds
            {
                Origin = new PointF(xprim, yprim),
                MaxBounds = new RectangleF(xprim, yprim, ChildNotes.Count * 60, 40),
            };
        }
    }

    internal class VisualNoteFigureBeamGroupLayoutBounds
    {
        public PointF Origin { get; set; }
        public RectangleF MaxBounds { get; set; }
        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, HashSet<string> debug = null)
        {
            ibmp.Mutate(ctx =>
            {
                if (debug?.HasFlag(DebugEngravingRenderableExtensions.DFlags.Origin) == true)
                {
                    ctx.FillPolygon(Brushes.Solid(Color.Red), new EllipsePolygon(Origin, 5).Points.ToArray());
                }
                if (debug?.HasFlag(DebugEngravingRenderableExtensions.DFlags.Bounds) == true)
                {
                    DrawRectangleExtensions.Draw(ctx, Pens.Dot(Color.Red, 1f), MaxBounds);
                }
            });
        }

    }

}
