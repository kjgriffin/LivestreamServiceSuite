using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;

using Xenon.Engraver.DataModel;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class VisualNoteFigure : IEngravingRenderable
    {
        private float XOffset { get; set; } = 0f;
        private float YOffset { get; set; } = 0f;

        internal float Width
        {
            get
            {
                return 20;
            }
        }

        internal float Height
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
                int rbasis = 4;

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

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            var yloff = YCalc();
            EllipsePolygon npoly = new EllipsePolygon(X + XOffset, Y + YOffset + yloff, Width, Width / 1.25f);
            EllipsePolygon nepoly = new EllipsePolygon(X + XOffset, Y + YOffset + yloff, Width * 1.2f, Width / 1.25f);
            ibmp.Mutate(ctx =>
            {
                ctx.SetDrawingTransform(Matrix3x2Extensions.CreateRotationDegrees(-30, new PointF(X + XOffset, Y + YOffset + yloff)));

                ctx.DrawPolygon(Pens.Solid(Color.Black, 1), npoly.Points.ToArray());
                ctx.DrawPolygon(Pens.Solid(Color.Black, 2), nepoly.Points.ToArray());

                // fill ??
                if (NValue.Length > NoteLength.HALF)
                {
                    ctx.FillPolygon(Brushes.Solid(Color.Black), nepoly.Points.ToArray());
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
                    const float X_STEM_OFFSET = 1.8f;
                    if (SitsUp(yloff))
                    {
                        ctx.DrawLines(Pens.Solid(Color.Black, 1.2f), new PointF(X + XOffset + Width / 2 + X_STEM_OFFSET, Y + YOffset + yloff), new PointF(X + XOffset + Width / 2 + X_STEM_OFFSET, Y - Height + YOffset + yloff));
                    }
                    else
                    {
                        ctx.DrawLines(Pens.Solid(Color.Black, 1.2f), new PointF(X + XOffset - Width / 2 - X_STEM_OFFSET, Y + YOffset + yloff), new PointF(X + XOffset - Width / 2 - X_STEM_OFFSET, Y + Height + YOffset + yloff));
                    }
                }


                // draw tail


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
                    EllipsePolygon dpoly = new EllipsePolygon(X + XOffset + Width + 2 + 6 * i, Y + YOffset + yloff + ymerge, 6, 6);
                    ctx.FillPolygon(Brushes.Solid(Color.Black), dpoly.Points.ToArray());
                }


            });

        }

        public RectangleF ComputeMaxLayoutBounds()
        {
            throw new NotImplementedException();
        }

        public void PerformLayout(float X, float Y, EngravingLayoutInfo layout)
        {
            throw new NotImplementedException();
        }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            throw new NotImplementedException();
        }
    }



}
