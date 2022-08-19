using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Linq;

using Xenon.Engraver.DataModel;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class VisualNoteFigure : IEngravingRenderable
    {
        internal float XOffset { get; set; } = 0f;
        internal float YOffset { get; set; } = 0f;

        internal float Width
        {
            get
            {
                return NoteBodyWidth + NoteAccidentalWidth;
            }
        }

        private float NoteBodyWidth
        {
            get
            {
                if (NValue?.Length == NoteLength.WHOLE)
                {
                    return 30;
                }
                else if (NValue?.Length == NoteLength.HALF)
                {

                    return 25;
                }
                return 27;
            }
        }
        private float NoteAccidentalWidth
        {
            get
            {
                switch (NValue.Accidental)
                {
                    case Accidental.None:
                        return 0;
                    case Accidental.Sharp:
                        return 20;
                    case Accidental.Flat:
                        return 50;
                    case Accidental.Natural:
                        return 50;
                    default:
                        return 0;
                }
            }
        }


        internal float LWidth
        {
            get
            {
                float nval = 60;
                float lv = nval * nval;
                float dval = 0;
                switch (NValue.Length)
                {
                    case NoteLength.WHOLE:
                        lv *= 4;
                        dval = 2;
                        break;
                    case NoteLength.HALF:
                        lv *= 2;
                        dval = 1;
                        break;
                    case NoteLength.QUARTER:
                        lv *= 1;
                        dval = 0.5f;
                        break;
                    case NoteLength.EIGHTH:
                        lv *= 0.5f;
                        dval = 0.25f;
                        break;
                    case NoteLength.SIXTEENTH:
                        lv *= 0.25f;
                        dval = 0.125f;
                        break;
                }
                for (int i = 0; i < NValue.LengthDots; i++)
                {
                    lv += nval * nval * dval;
                    dval /= 2f;
                }
                return (float)Math.Sqrt(lv);
            }
        }

        internal float NHeight
        {
            get
            {
                return 16f;
            }
        }

        internal float StemHeight
        {
            get
            {
                return 60;
            }
        }

        internal Note NValue { get; set; } = new Note();
        internal Clef Clef { get; set; }

        private float YCalc()
        {
            // assumes a 5 line staff
            // Y-0 == top line
            float y = 0;
            const float L_H_HEIGHT = 10;

            if (Clef == Clef.Trebble)
            {
                float ybasis = 0 + 100;

                float yoct = 70;

                if (NValue.Register > 4)
                {
                    // need to go above
                    var o = NValue.Register - 4;
                    ybasis -= o * yoct;
                }
                else if (NValue.Register < 4)
                {
                    // go below
                    var o = 4 - NValue.Register;
                    ybasis += o * yoct;
                }

                // calculate note offset
                switch (NValue.Name)
                {
                    case NoteName.A:
                        ybasis -= L_H_HEIGHT * 5;
                        break;
                    case NoteName.B:
                        ybasis -= L_H_HEIGHT * 6;
                        break;
                    case NoteName.C:
                        ybasis += L_H_HEIGHT * 0;
                        break;
                    case NoteName.D:
                        ybasis -= L_H_HEIGHT * 1;
                        break;
                    case NoteName.E:
                        ybasis -= L_H_HEIGHT * 2;
                        break;
                    case NoteName.F:
                        ybasis -= L_H_HEIGHT * 3;
                        break;
                    case NoteName.G:
                        ybasis -= L_H_HEIGHT * 4;
                        break;
                }

                y += ybasis;
            }

            return y;
        }

        private bool SitsUp(float y)
        {
            if (Clef == Clef.Trebble)
            {
                if (y > 40)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public VisualNoteFigureLayoutBounds CalculateBounds(float X, float Y)
        {
            float yloff = YCalc();

            float xprim = X + XOffset + NoteAccidentalWidth;
            float yprim = Y + YOffset + yloff;

            bool upstem = SitsUp(yloff);

            float tlength = NValue.Length == NoteLength.EIGHTH && upstem ? 14 : 0;

            ComputeStemPos(xprim, yloff, yprim, out _, out _, out var p_tip);

            VisualNoteFigureLayoutBounds bounds = new VisualNoteFigureLayoutBounds
            {
                BodyOrigin = new PointF(xprim, yprim),
                BodyBounds = new EllipsePolygon(xprim, yprim, NoteBodyWidth, NHeight).Bounds,
                FigureBounds = new RectangleF(xprim - NoteBodyWidth / 2f - 2, upstem ? yprim - NHeight / 2f - StemHeight : yprim - NHeight / 2, NoteBodyWidth + tlength, StemHeight + NHeight),
                StemTip = p_tip,
                NoteBounds = new RectangleF(xprim - NoteBodyWidth / 2f - NoteAccidentalWidth - 2, upstem ? yprim - NHeight / 2f - StemHeight : yprim - NHeight / 2 + (NoteAccidentalWidth > 0 ? -30 : 0), NoteBodyWidth + tlength + NoteAccidentalWidth, StemHeight + NHeight + (NoteAccidentalWidth > 0 ? 30 : 0)),
                TimeSpace = new RectangleF(xprim - NoteBodyWidth / 2f - 2 + NoteBodyWidth + tlength, yprim - NHeight / 2f, LWidth, NHeight),
            };

            return bounds;
        }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, bool debug = false)
        {
            var bounds = CalculateBounds(X, Y);
            if (debug)
            {
                bounds.RenderBounds(ibmp, ikbmp, true);
            }

            float XAcc = X + XOffset;
            float XPrim = X + XOffset + NoteAccidentalWidth;

            var yloff = YCalc();

            float YPrim = Y + YOffset + yloff;

            PointF[] whole_note = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth, NHeight).Points.ToArray();
            PointF[] whole_inner = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth * 0.5f, NHeight * 0.8f).RotateDegree(40).Flatten().FirstOrDefault().Points.ToArray();

            PointF[] half_note = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth, NHeight).RotateDegree(-25).Flatten().FirstOrDefault()?.Points.ToArray() ?? new PointF[0];
            PointF[] half_inner = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth * 0.75f, NHeight * 0.8f).RotateDegree(-38).Flatten().FirstOrDefault()?.Points.ToArray() ?? new PointF[0];

            EllipsePolygon std_note = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth, NHeight);
            var solid_note = std_note.RotateDegree(-25).Flatten().FirstOrDefault()?.Points.ToArray() ?? new PointF[0];

            ibmp.Mutate(ctx =>
            {

                if (NValue.Length == NoteLength.HALF)
                {
                    FillPathBuilderExtensions.Fill(ctx, Color.Black, (x) =>
                       {
                           x.AddLines(half_inner);
                           x.AddLines(half_note);
                           x.CloseFigure();
                       });
                }
                else if (NValue.Length == NoteLength.WHOLE)
                {
                    FillPathBuilderExtensions.Fill(ctx, Color.Black, (x) =>
                       {
                           x.AddLines(whole_inner);
                           x.AddLines(whole_note);
                           x.CloseFigure();
                       });
                }
                else
                {
                    ctx.FillPolygon(Brushes.Solid(Color.Black), solid_note);
                }
            });
            ibmp.Mutate(ctx =>
            {


                // TODO: draw ledger
                float ledgerwidthextend = 20;
                if (yloff < 0)
                {
                    var llines = Math.Abs(yloff / 20f);
                    for (int i = 1; i <= llines; i++)
                    {
                        ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(bounds.BodyOrigin.X - ledgerwidthextend, Y + YOffset - i * 20), new PointF(bounds.BodyOrigin.X + ledgerwidthextend, Y + YOffset - i * 20));
                    }
                }
                if (yloff > 80)
                {
                    var llines = Math.Abs((yloff - 80) / 20f);
                    for (int i = 1; i <= llines; i++)
                    {
                        ctx.DrawLines(Pens.Solid(Color.Black, 1), new PointF(bounds.BodyOrigin.X - ledgerwidthextend, Y + YOffset + 80 + i * 20), new PointF(bounds.BodyOrigin.X + ledgerwidthextend, Y + YOffset + 80 + i * 20));
                    }
                }

                // draw handle
                if (NValue.Length != NoteLength.WHOLE)
                {
                    float mod;
                    PointF p_note, p_tip;
                    ComputeStemPos(XPrim, yloff, YPrim, out mod, out p_note, out p_tip);

                    ctx.DrawLines(Pens.Solid(Color.Black, 1.2f), p_note, p_tip);

                    // draw tail
                    if (NValue.Length == NoteLength.EIGHTH)
                    {
                        float DSTSCALE = SitsUp(yloff) ? 1f : 0.8f;

                        var pt1 = new PointF(p_tip.X, p_tip.Y + 18 * mod);
                        var pt2 = new PointF(pt1.X + 3.5f, pt1.Y + 5.1f * mod);
                        var pt3 = new PointF(pt1.X + 18, pt1.Y + 18 * mod * DSTSCALE);
                        var pt4 = new PointF(pt1.X + 10, pt1.Y + 32 * mod * DSTSCALE);

                        var pp1 = new PointF(p_tip.X, p_tip.Y + 3 * mod);
                        var pp2 = new PointF(pp1.X + 6, pp1.Y + 13 * mod);
                        var pp3 = new PointF(pp1.X + 18, pp1.Y + 22 * mod * DSTSCALE);

                        FillPathBuilderExtensions.Fill(ctx, Color.Black, (x) =>
                        {
                            x.AddLine(pp1, pt1);
                            x.AddBezier(pt1, pt2, pt3, pt4);
                            x.AddBezier(pt4, pp3, pp2, pp1);
                            x.CloseFigure();
                        });
                    }

                }


                // accidentals
                if (NValue.Accidental == Accidental.Sharp)
                {
                    float id = NoteAccidentalWidth / 1.2f;
                    float XAccTrue = XAcc - NoteAccidentalWidth + 5;// + debug.NoteBounds.Width / 2f;
                    var _ = bounds;
                    ctx.DrawLines(Pens.Solid(Color.Black, 2f), new PointF(XAccTrue + 5, YPrim - 25), new PointF(XAccTrue + 5, YPrim + 25));
                    ctx.DrawLines(Pens.Solid(Color.Black, 2f), new PointF(XAccTrue - 5 + id, YPrim - 25 - 5), new PointF(XAccTrue - 5 + id, YPrim + 25 - 5));

                    ctx.DrawLines(Pens.Solid(Color.Black, 6f), new PointF(XAccTrue, YPrim - 10), new PointF(XAccTrue + id, YPrim - 10 - 5));
                    ctx.DrawLines(Pens.Solid(Color.Black, 6f), new PointF(XAccTrue, YPrim + 10), new PointF(XAccTrue + id, YPrim + 10 - 5));
                }


                // dots?
                for (int i = 0; i < NValue.LengthDots; i++)
                {
                    float ymerge = 0;
                    if ((int)Math.Round(yloff) % 20 == 0)
                    {
                        ymerge = -8;
                    }
                    else
                    {
                        ymerge = 3;
                    }
                    EllipsePolygon dpoly = new EllipsePolygon(X + XOffset + NoteBodyWidth + 2 + 6 * i, YPrim + ymerge, 6, 6);
                    ctx.FillPolygon(Brushes.Solid(Color.Black), dpoly.Points.ToArray());
                }


            });

            if (debug)
            {
                bounds.RenderBounds(ibmp, ikbmp, false);
            }
        }

        private void ComputeStemPos(float XPrim, float yloff, float YPrim, out float mod, out PointF p_note, out PointF p_tip)
        {
            float TOFFSET = -1.2f;
            TOFFSET += SitsUp(yloff) ? -1 : 0;

            if (NValue.Length == NoteLength.HALF && SitsUp(yloff))
            {
                TOFFSET -= -0.9f;
            }
            else if (NValue.Length == NoteLength.HALF)
            {
                TOFFSET -= -0.6f;
            }

            mod = SitsUp(yloff) ? 1 : -1;

            p_note = new PointF(XPrim + ((NoteBodyWidth / 2 + TOFFSET) * mod), YPrim);
            p_tip = new PointF(XPrim + ((NoteBodyWidth / 2 + TOFFSET) * mod), YPrim + (StemHeight * -mod));
        }
    }


    internal class VisualNoteFigureLayoutBounds
    {
        /// <summary>
        /// Center Point of round boi
        /// </summary>
        public PointF BodyOrigin { get; set; }
        /// <summary>
        /// Bounding rectangle that encompases the round boi
        /// </summary>
        public RectangleF BodyBounds { get; set; }
        /// <summary>
        /// Point describing the extreme tip (up/down) of the stem furthest from the <see cref="BodyOrigin"/>
        /// </summary>
        public PointF StemTip { get; set; }
        /// <summary>
        /// Bounding rectangle that encompases the note body, stem, tail and dots.
        /// <para>Excludes accidentals, time leading</para>
        /// </summary>
        public RectangleF FigureBounds { get; set; }

        /// <summary>
        /// Boudning rectangle that encompases the <see cref="FigureBounds"/> and any preceeding accidentals.
        /// </summary>
        public RectangleF NoteBounds { get; set; }

        /// <summary>
        /// Bouding rectangle that defines the whitespace after the note for time-spacing.
        /// </summary>
        public RectangleF TimeSpace { get; set; }

        /// <summary>
        /// Bounding rectangle that encompases the <see cref="NoteBounds"/> and the <see cref="TimeSpace"/>
        /// </summary>
        public RectangleF MaxBounds { get => new RectangleF(NoteBounds.X, NoteBounds.Y, NoteBounds.Width + TimeSpace.Width, NoteBounds.Height); }


        public void RenderBounds(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, bool pre)
        {

            ibmp.Mutate(ctx =>
            {

                // origin
                //ctx.FillPolygon(Brushes.Solid(Color.Red), new EllipsePolygon(BodyOrigin, 5).Points.ToArray());

                // tail point
                ctx.FillPolygon(Brushes.Solid(Color.Red), new EllipsePolygon(StemTip, 5).Points.ToArray());


                FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(255, 255, 0, 80)), TimeSpace);

                if (pre)
                {
                    // max
                    //FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(255, 0, 0, 100)), MaxBounds);
                    DrawRectangleExtensions.Draw(ctx, Pens.Dash(Color.FromRgba(255, 0, 0, 100), 2), MaxBounds);

                    // timespace
                    FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(255, 255, 0, 80)), TimeSpace);

                    // note
                    FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(0, 255, 0, 100)), NoteBounds);

                    // figure
                    FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(80, 100, 100, 80)), FigureBounds);

                    // body
                    FillRectangleExtensions.Fill(ctx, Brushes.Solid(Color.FromRgba(55, 0, 200, 150)), BodyBounds);

                }
            });

        }

    }

}
