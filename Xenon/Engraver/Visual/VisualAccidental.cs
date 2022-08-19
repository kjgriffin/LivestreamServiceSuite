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

    internal class VisualAccidentalLayoutBounds
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
                    FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(200, 0, 200, 100)), MaxBounds);
                }
            });
        }
    }


    internal class VisualAccidental : IEngravingRenderable
    {

        //public float XOffset { get; set; }
        //public float YOffset { get; set; }

        public float Width { get; set; }

        public static float CalculateWidth(Accidental val)
        {
            switch (val)
            {
                case Accidental.None:
                    return 0;
                case Accidental.Sharp:
                    return 20;
                case Accidental.Flat:
                    return 25;
                case Accidental.Natural:
                    return 50;
                default:
                    return 0;
            }
        }

        public Accidental Accidental { get; set; }

        public VisualAccidentalLayoutBounds CalculateBounds(float X, float Y)
        {
            var width = CalculateWidth(Accidental);
            var xprim = X;

            switch (Accidental)
            {
                case Accidental.Sharp:
                    xprim += -Width + 5;
                    break;
                case Accidental.Flat:
                    xprim += -Width + 10;
                    break;
                case Accidental.Natural:
                    break;
            }


            var yprim = Y;
            float height = Accidental == Accidental.None ? 0 : 64;
            return new VisualAccidentalLayoutBounds
            {
                Origin = new PointF(xprim, yprim),
                MaxBounds = new RectangleF(xprim, yprim - height / 2f, width, height),
            };
        }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, HashSet<string> debug = null)
        {
            var bounds = CalculateBounds(X, Y);
            bounds.Render(ibmp, ikbmp, debug);

            if (Accidental == Accidental.Sharp)
            {
                float id = Width / 1.2f;
                float XAccTrue = X - Width + 5;
                ibmp.Mutate(ctx =>
                {
                    ctx.DrawLines(Pens.Solid(Color.Black, 2f), new PointF(XAccTrue + 5, Y - 25), new PointF(XAccTrue + 5, Y + 25));
                    ctx.DrawLines(Pens.Solid(Color.Black, 2f), new PointF(XAccTrue - 5 + id, Y - 25 - 5), new PointF(XAccTrue - 5 + id, Y + 25 - 5));

                    ctx.DrawLines(Pens.Solid(Color.Black, 6f), new PointF(XAccTrue, Y - 10), new PointF(XAccTrue + id, Y - 10 - 5));
                    ctx.DrawLines(Pens.Solid(Color.Black, 6f), new PointF(XAccTrue, Y + 10), new PointF(XAccTrue + id, Y + 10 - 5));
                });
            }
            else if (Accidental == Accidental.Flat)
            {
                float id = Width / 1.2f;
                float XAccTrue = X - Width + 10;
                ibmp.Mutate(ctx =>
                {
                    PointF stop = new PointF(XAccTrue + 5, Y - 28);
                    PointF sbot = new PointF(XAccTrue + 5, Y + 13);

                    ctx.DrawLines(Pens.Solid(Color.Black, 2.7f), stop, sbot);

                    PointF icurve_top = new PointF(stop.X, Y - 5);
                    PointF icurve_bot = new PointF(stop.X, sbot.Y);
                    PointF icurve_c1 = new PointF(icurve_top.X + 15, icurve_top.Y - 5);
                    PointF icurve_c2 = new PointF(icurve_bot.X + 9, icurve_bot.Y - 3);

                    PointF ocurve_top = new PointF(stop.X, Y - 7);
                    PointF ocurve_bot = new PointF(stop.X, sbot.Y + 1);
                    PointF ocurve_c1 = new PointF(ocurve_top.X + 23, ocurve_top.Y - 5);
                    PointF ocurve_c2 = new PointF(ocurve_bot.X + 14, ocurve_bot.Y - 4);

                    FillPathBuilderExtensions.Fill(ctx, Color.Black, p =>
                    {
                        p.AddBezier(icurve_top, icurve_c1, icurve_c2, icurve_bot);
                        p.AddLine(icurve_bot, ocurve_bot);
                        p.AddBezier(ocurve_bot, ocurve_c2, ocurve_c1, ocurve_top);
                        p.AddLine(ocurve_top, icurve_top);
                        p.CloseFigure();
                    });

                });
            }

        }

    }
}
