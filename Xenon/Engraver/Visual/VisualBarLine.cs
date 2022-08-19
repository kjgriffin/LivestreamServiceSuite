using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xenon.Engraver.DataModel;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class VisualBarLine : IEngravingRenderable
    {
        internal float XOffset { get; set; } = 0f;
        internal float YOffset { get; set; } = 0f;
        internal float Height { get; set; } = 80f;

        internal float Width
        {
            get
            {
                switch (Type)
                {
                    case BarType.Single:
                        return 20;
                    case BarType.Double:
                        return 40;
                    case BarType.Repeat:
                        return 50;
                    default:
                        return 0;
                }
            }
        }

        internal BarType Type { get; set; } = BarType.None;

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, bool debug = false)
        {
            if (Type == BarType.Single)
            {
                ibmp.Mutate(ctx =>
                {
                    ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(X + XOffset, Y + YOffset), new PointF(X + XOffset, Y + Height + YOffset));
                });
            }
            if (Type == BarType.Double)
            {
                ibmp.Mutate(ctx =>
                {
                    ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(X + XOffset, Y + YOffset), new PointF(X + XOffset, Y + Height + YOffset));
                    ctx.DrawLines(Pens.Solid(Color.Black, 8), new PointF(X + XOffset + 10, Y + YOffset), new PointF(X + XOffset + 10, Y + Height + YOffset));
                });
            }

            if (debug)
            {
                var bounds = CalculateBounds(X, Y);
                bounds.Render(ibmp, ikbmp);
            }
        }

        public VisualBarLineLayoutBounds CalculateBounds(float X, float Y)
        {
            float xprim = X + XOffset;
            float yprim = Y + YOffset;

            VisualBarLineLayoutBounds bounds = new VisualBarLineLayoutBounds
            {
                MaxBounds = new RectangleF(xprim, yprim, Width, Height),
            };

            return bounds;
        }


    }

    internal class VisualBarLineLayoutBounds
    {
        public RectangleF MaxBounds { get; set; }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp)
        {
            ibmp.Mutate(ctx =>
            {
                DrawRectangleExtensions.Draw(ctx, Pens.Dot(Color.Red, 1f), MaxBounds);
            });
        }
    }




}
