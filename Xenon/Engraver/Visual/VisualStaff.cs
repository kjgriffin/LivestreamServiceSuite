using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class VisualStaff : IEngravingRenderable
    {
        float X;
        float Y;
        public int Lines { get; set; } = 5;
        public float Width { get; set; } = 0;

        public float LineSpace { get; set; } = 20;

        public RectangleF ComputeMaxLayoutBounds()
        {
            return new RectangleF(X, y, Width, Lines * LineSpace);
        }

        public void PerformLayout(float X, float Y, EngravingLayoutInfo layout)
        {
            this.X = X;
            this.Y = Y;
        }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            for (int i = 0; i < Lines; i++)
            {
                ibmp.Mutate(ctx =>
                {
                    ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(X, Y + i * LineSpace), new PointF(X + Width, Y + i * LineSpace));
                });
            }
        }

    }



}
