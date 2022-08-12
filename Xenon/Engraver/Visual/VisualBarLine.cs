using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;

using Xenon.Engraver.DataModel;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class VisualBarLine : IEngravingRenderable
    {
        private float XOffset { get; set; } = 0f;
        private float YOffset { get; set; } = 0f;
        private float Height { get; set; } = 80f;

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

        public RectangleF ComputeMaxLayoutBounds()
        {
            return new RectangleF(XOffset, YOffset, Width, Height);
        }

        public void PerformLayout(float X, float Y, EngravingLayoutInfo layout)
        {
            XOffset = X;
            YOffset = Y;
        }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            if (Type == BarType.Single)
            {
                ibmp.Mutate(ctx =>
                {
                    ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(XOffset, YOffset), new PointF(XOffset, Height + YOffset));
                });
            }
            if (Type == BarType.Double)
            {
                ibmp.Mutate(ctx =>
                {
                    ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(XOffset, YOffset), new PointF(XOffset, Height + YOffset));
                    ctx.DrawLines(Pens.Solid(Color.Black, 8), new PointF(XOffset + 10, YOffset), new PointF(XOffset + 10, +Height + YOffset));
                });
            }
        }

    }



}
