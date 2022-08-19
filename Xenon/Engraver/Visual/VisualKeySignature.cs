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
    internal class VisualKeySignature : IEngravingRenderable
    {

        public float XOffset { get; set; }
        public float YOffset { get; set; }

        public Clef Clef { get; set; }

        public KeySignature Key { get; set; }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, HashSet<string> debug = null)
        {
            var bounds = CalculateBounds(X, Y);
            // for now rely on theory rules not do mixed key signatures
            // so we can render 0 sharps/flats without issue
            var ksig = Theory.TheoryRules.GetSharpsAndFlats(Key);

            float ystart = 0;

            if (Clef == Clef.Trebble)
            {
                // adjust ystart
                ystart = 0;
            }

            Accidental atype = ksig.Sharps > 0 ? Accidental.Sharp : Accidental.Flat;

            float awidth = VisualAccidental.CalculateWidth(atype);
            float axoff = awidth / 1.2f;

            float xprim = X + XOffset + awidth - (atype == Accidental.Sharp ? 5 : 10);
            float yprim = Y + YOffset;

            float yloff = 0;
            float xwoff = 0;

            for (int i = 0; i < ksig.Sharps; i++)
            {
                if (Clef == Clef.Trebble)
                {
                    if (i == 2)
                    {
                        yloff = -10;
                    }
                    else if (i == 5)
                    {
                        yloff = 10;
                    }
                }

                var acc = new VisualAccidental
                {
                    Accidental = atype,
                    Width = awidth,
                };
                acc.Render(xprim + xwoff, yprim + yloff, ibmp, ikbmp, layout, debug);

                xwoff += axoff;
                yloff += 30;
            }

            yloff = 0;
            for (int i = 0; i < ksig.Flats; i++)
            {
                if (Clef == Clef.Trebble)
                {
                    if (i == 0)
                    {
                        yloff = 40;
                    }
                    else if (i == 2)
                    {
                        yloff = 50;
                        xwoff -= axoff / 2f;
                    }
                    else if (i == 4)
                    {
                        yloff = 60;
                        xwoff -= axoff / 2f;
                    }
                    else if (i == 6)
                    {
                        yloff = 70;
                        xwoff -= axoff / 2f;
                    }
                }

                var acc = new VisualAccidental
                {
                    Accidental = atype,
                    Width = awidth,
                };
                acc.Render(xprim + xwoff, yprim + yloff, ibmp, ikbmp, layout, debug);

                xwoff += axoff;
                yloff -= 30;
            }



            bounds.Render(ibmp, ikbmp, debug);
        }


        public VisualKeySignatureLayoutBounds CalculateBounds(float X, float Y)
        {
            var ksig = Theory.TheoryRules.GetSharpsAndFlats(Key);

            float kwidth = 0;
            if (ksig.Sharps > 0)
            {
                kwidth = (VisualAccidental.CalculateWidth(Accidental.Sharp) * ksig.Sharps) / 1.2f;
            }
            else if (ksig.Flats > 0)
            {
                var w = VisualAccidental.CalculateWidth(Accidental.Flat) / 1.2f;

                if (Clef == Clef.Trebble)
                {
                    float xw = 3;
                    for (int i = 0; i < ksig.Flats; i++)
                    {
                        if (i == 2)
                        {
                            xw -= w / 2f;
                        }
                        else if (i == 4)
                        {
                            xw -= w / 2f;
                        }
                        else if (i == 6)
                        {
                            xw -= w / 2f;
                        }

                        xw += w;
                    }
                    kwidth = xw;
                }

            }


            float xprim = X + XOffset;
            float yprim = Y + YOffset;
            return new VisualKeySignatureLayoutBounds
            {
                Origin = new PointF(xprim, yprim),
                MaxBounds = new RectangleF(xprim, yprim - 42, kwidth + 20, 140),
            };

        }

    }

    internal class VisualKeySignatureLayoutBounds
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
