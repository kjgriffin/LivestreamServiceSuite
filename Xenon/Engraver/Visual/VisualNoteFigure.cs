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
                    return 25;
                }
                else if (NValue?.Length == NoteLength.HALF)
                {

                    return 25;
                }
                return 20;
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
                        return 50;
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
                float lv = 40 * 40;
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
                    lv += dval;
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

            float xprim = X + XOffset;
            float yprim = X + XOffset + yloff;

            bool upstem = SitsUp(yloff);

            ComputeStemPos(xprim, yloff, yprim, out _, out _, out var p_tip);

            VisualNoteFigureLayoutBounds bounds = new VisualNoteFigureLayoutBounds
            {
                BodyOrigin = new PointF(xprim, yprim),
                BodyBounds = new EllipsePolygon(xprim, yprim, NoteBodyWidth, NHeight).Bounds,
                FigureBounds = new RectangleF(xprim, upstem ? yprim - StemHeight : yprim + StemHeight, StemHeight, 5),
                StemTip = p_tip,
                NoteBounds = new RectangleF(),
                TimeSpace = new RectangleF(),
            };

            return bounds;
        }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            float XAcc = X + XOffset;
            float XPrim = X + XOffset + NoteAccidentalWidth;

            var yloff = YCalc();

            float YPrim = Y + YOffset + yloff;

            EllipsePolygon npoly = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth * 0.8f, NHeight * 0.8f);
            EllipsePolygon nepoly = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth * 1.35f, NHeight);
            EllipsePolygon nepoly1 = new EllipsePolygon(XPrim, YPrim, NoteBodyWidth * 1.2f, NHeight);
            ibmp.Mutate(ctx =>
            {
                var pts = nepoly.Scale(0.55f, 0.85f).RotateDegree(-38).Flatten().FirstOrDefault()?.Points.ToArray() ?? new PointF[0];
                var pts2 = nepoly.RotateDegree(-25).Flatten().FirstOrDefault()?.Points.ToArray() ?? new PointF[0];

                var pts2_narrow = nepoly.Scale(0.85f, 1).RotateDegree(-25).Flatten().FirstOrDefault()?.Points.ToArray() ?? new PointF[0];

                if (NValue.Length == NoteLength.HALF)
                {
                    FillPathBuilderExtensions.Fill(ctx, Color.Black, (x) =>
                       {
                           x.AddLines(pts);
                           x.AddLines(pts2_narrow);
                           x.CloseFigure();
                       });
                }
                else if (NValue.Length == NoteLength.WHOLE)
                {
                    FillPathBuilderExtensions.Fill(ctx, Color.Black, (x) =>
                       {
                           x.AddLines(npoly.RotateDegree(40).Flatten().FirstOrDefault().Points.ToArray());
                           x.AddLines(nepoly1.Points.ToArray());
                           x.CloseFigure();
                       });
                }
                else
                {
                    ctx.FillPolygon(Brushes.Solid(Color.Black), pts2);
                }
            });
            ibmp.Mutate(ctx =>
            {


                // TODO: draw ledger
                if (yloff < 0)
                {

                }
                if (yloff > 80)
                {

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
                    float id = NoteAccidentalWidth * 0.6f - NoteBodyWidth / 2f;
                    ctx.DrawLines(Pens.Solid(Color.Black, 2f), new PointF(XAcc + 5, YPrim - 25), new PointF(XAcc + 5, YPrim + 25));
                    ctx.DrawLines(Pens.Solid(Color.Black, 2f), new PointF(XAcc - 5 + id, YPrim - 25 - 5), new PointF(XAcc - 5 + id, YPrim + 25 - 5));

                    ctx.DrawLines(Pens.Solid(Color.Black, 6f), new PointF(XAcc, YPrim - 10), new PointF(XAcc + id, YPrim - 10 - 5));
                    ctx.DrawLines(Pens.Solid(Color.Black, 6f), new PointF(XAcc, YPrim + 10), new PointF(XAcc + id, YPrim + 10 - 5));
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

        }

        private void ComputeStemPos(float XPrim, float yloff, float YPrim, out float mod, out PointF p_note, out PointF p_tip)
        {
            float TOFFSET = 3;
            TOFFSET += SitsUp(yloff) ? -1 : 0;

            if (NValue.Length == NoteLength.HALF && SitsUp(yloff))
            {
                TOFFSET -= 2.9f;
            }
            else if (NValue.Length == NoteLength.HALF)
            {
                TOFFSET -= 1.6f;
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
    }

}
