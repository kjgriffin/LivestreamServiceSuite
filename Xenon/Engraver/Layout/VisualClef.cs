using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xenon.Engraver.DataModel;
using Xenon.Engraver.Visual;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Layout
{
    internal class VisualClef : IEngravingRenderable
    {


        internal float XOffset { get; set; }
        internal float YOffset { get; set; }

        internal Clef ClefType { get; set; }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, bool debug = false)
        {
            var bounds = CalculateBounds(X, Y);

            switch (ClefType)
            {
                case Clef.Unkown:
                    break;
                case Clef.Trebble:
                    Render_Trebble(X, Y, ibmp, ikbmp, layout, bounds);
                    break;
                case Clef.Base:
                    break;
                default:
                    break;
            }

            if (debug)
            {
                bounds.Render(ibmp, ikbmp);
            }
        }

        private void Render_Trebble(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, VisualClefLayoutBounds bounds)
        {
            PointF mainStemTop = new PointF(bounds.MaxBounds.Left + bounds.MaxBounds.Width / 3f, bounds.MaxBounds.Top + bounds.MaxBounds.Height / 3.5f);
            PointF mainStemBot = new PointF(bounds.MaxBounds.Left + (bounds.MaxBounds.Width / 5f) * 3f, bounds.MaxBounds.Bottom - (bounds.MaxBounds.Height / 6f));

            PointF top = new PointF(bounds.MaxBounds.Left + (bounds.MaxBounds.Width / 2f), bounds.MaxBounds.Top + bounds.MaxBounds.Height / 10f);

            ibmp.Mutate(ctx =>
            {

                // draw mainstem
                DrawLineExtensions.DrawLines(ctx, Pens.Solid(Color.Black, 2), mainStemTop, mainStemBot);


                // draw L-top outer
                PointF btopctrl1 = new PointF(bounds.MaxBounds.Left + (bounds.MaxBounds.Width / 3f), bounds.MaxBounds.Top + bounds.MaxBounds.Height / 7f);
                PointF btopctrl2 = new PointF(bounds.MaxBounds.Left + (bounds.MaxBounds.Width / 2f), bounds.MaxBounds.Top + bounds.MaxBounds.Height / 9f);

                // L-top inner
                float top_sub_offset = 6f;
                PointF ltop1 = new PointF(mainStemTop.X + 2, mainStemTop.Y + top_sub_offset);
                PointF ltop2 = new PointF(top.X, top.Y + top_sub_offset);
                PointF ltopctrl1 = new PointF(btopctrl1.X, btopctrl1.Y + top_sub_offset);
                PointF ltopctrl2 = new PointF(btopctrl2.X, btopctrl2.Y + top_sub_offset);



                // draw L outersweep outer
                PointF outersweep_top = new PointF(mainStemTop.X + 5, bounds.Origin.Y - 5 - 10);
                PointF outersweep_bot = new PointF(mainStemBot.X - 5, mainStemBot.Y - 15);

                PointF outersweep_ctrl1 = new PointF(mainStemTop.X + 5 - 20, bounds.Origin.Y - 5 + 7);
                PointF outersweep_ctrl2 = new PointF(mainStemBot.X - 5 - 40, mainStemBot.Y - 15);
                //DrawBezierExtensions.DrawBeziers(ctx, Pens.Solid(Color.Black, 1), outersweep_top, outersweep_ctrl1, outersweep_ctrl2, outersweep_bot);




                // draw R-top inner
                PointF ritop_ctrl1 = new PointF(ltop2.X + 7, ltop2.Y + 2);
                PointF ritop_ctrl2 = new PointF(outersweep_top.X + 12, outersweep_top.Y - 10);

                // draw R-top outer
                PointF rotop_ctrl1 = new PointF(top.X + 12, top.Y + 7);
                PointF rotop_ctrl2 = new PointF(outersweep_top.X + 15, outersweep_top.Y - 3);
                PointF robot = new PointF(outersweep_top.X + 2, outersweep_top.Y + 8);

                // draw L outersweep inner
                PointF ioutersweep_top = new PointF(robot.X + 1, robot.Y - 1);
                PointF ioutersweep_bot = new PointF(outersweep_bot.X - 2, outersweep_bot.Y + 1);

                PointF ioutersweep_ctrl1 = new PointF(outersweep_ctrl1.X + 6, outersweep_ctrl1.Y + 3);
                PointF ioutersweep_ctrl2 = new PointF(outersweep_ctrl2.X + 3, outersweep_ctrl2.Y - 3);


                // draw r sweep
                PointF orsweep_bot = new PointF(outersweep_bot.X, outersweep_bot.Y);
                PointF orsweep_top = new PointF(orsweep_bot.X + 4, orsweep_bot.Y - 37);

                PointF orsweep_ctrl1 = new PointF(orsweep_bot.X + 24, orsweep_bot.Y - 2);
                PointF orsweep_ctrl2 = new PointF(orsweep_top.X + 22, orsweep_top.Y - 1);
                //DrawBezierExtensions.DrawBeziers(ctx, Pens.Solid(Color.Black, 1), orsweep_bot, orsweep_ctrl1, orsweep_ctrl2, orsweep_top);


                // draw outer close sweep
                PointF oclose_top = new PointF(orsweep_top.X, orsweep_top.Y);
                PointF oclose_bot = new PointF(outersweep_bot.X - 6, outersweep_bot.Y - 9);

                PointF oclose_ctrl1 = new PointF(oclose_top.X - 25, oclose_top.Y + 2);
                PointF oclose_ctrl2 = new PointF(oclose_bot.X - 14, oclose_bot.Y - 2);

                //DrawBezierExtensions.DrawBeziers(ctx, Pens.Solid(Color.Black, 1), oclose_top, oclose_ctrl1, oclose_ctrl2, oclose_bot);


                // draw inner close sweep
                PointF iclose_top = new PointF(oclose_top.X, oclose_top.Y + 6);
                PointF iclose_bot = new PointF(oclose_bot.X, oclose_bot.Y);

                PointF iclose_ctrl1 = new PointF(iclose_bot.X - 10, iclose_bot.Y - 1);
                PointF iclose_ctrl2 = new PointF(iclose_top.X - 20, iclose_top.Y + 3);

                //DrawBezierExtensions.DrawBeziers(ctx, Pens.Solid(Color.Black, 1), iclose_bot, iclose_ctrl1, iclose_ctrl2, iclose_top);

                // draw inner r sweep
                PointF irsweep_top = new PointF(iclose_top.X, iclose_top.Y);
                PointF irsweep_bot = new PointF(ioutersweep_bot.X, ioutersweep_bot.Y);

                PointF irsweep_ctrl1 = new PointF(irsweep_bot.X + 20, irsweep_bot.Y - 2);
                PointF irsweep_ctrl2 = new PointF(irsweep_top.X + 20, irsweep_top.Y + 1);
                //DrawBezierExtensions.DrawBeziers(ctx, Pens.Solid(Color.Black, 1), irsweep_bot, irsweep_ctrl1, irsweep_ctrl2, irsweep_top);


                FillPathBuilderExtensions.Fill(ctx, Color.Black, p =>
                {

                    p.AddBezier(mainStemTop, btopctrl1, btopctrl2, top); // ltop outer
                    p.AddBezier(top, rotop_ctrl1, rotop_ctrl2, robot); // rtop outer
                    p.AddLine(robot, outersweep_top); // add line to join
                    p.AddBezier(outersweep_top, ritop_ctrl2, ritop_ctrl1, ltop2); // rtop inner
                    p.AddBezier(ltop2, ltopctrl2, ltopctrl1, ltop1); // ltop inner
                    p.CloseFigure();

                    p.AddBezier(outersweep_top, outersweep_ctrl1, outersweep_ctrl2, outersweep_bot);
                    p.AddLine(outersweep_bot, ioutersweep_bot);
                    p.AddBezier(ioutersweep_bot, ioutersweep_ctrl2, ioutersweep_ctrl1, ioutersweep_top);
                    p.AddLine(ioutersweep_top, outersweep_top);
                    p.CloseFigure();

                    p.AddBezier(orsweep_bot, orsweep_ctrl1, orsweep_ctrl2, orsweep_top);
                    p.AddBezier(oclose_top, oclose_ctrl1, oclose_ctrl2, oclose_bot);
                    p.AddBezier(iclose_bot, iclose_ctrl1, iclose_ctrl2, iclose_top);
                    p.AddBezier(irsweep_top, irsweep_ctrl2, irsweep_ctrl1, irsweep_bot);
                    //p.AddLine(irsweep_top, irsweep_bot);
                    p.CloseFigure();

                });

                // draw bottom hang
                float hangHeight = 14;
                DrawBezierExtensions.DrawBeziers(ctx, Pens.Solid(Color.Black, 2), mainStemBot, new PointF(mainStemBot.X - 1, mainStemBot.Y + hangHeight), new PointF(mainStemBot.X - 17, mainStemBot.Y + hangHeight), new PointF(mainStemBot.X - 25, mainStemBot.Y + 5));

                ctx.FillPolygon(Color.Black, new EllipsePolygon(mainStemBot.X - 25 + 5.7f, mainStemBot.Y + 2, 7).Points.ToArray());

            });
        }


        public VisualClefLayoutBounds CalculateBounds(float X, float Y)
        {
            var xprim = X + XOffset;
            var yprim = Y + YOffset;

            float HHeight = 80;

            return new VisualClefLayoutBounds
            {
                Origin = new PointF(xprim, yprim),
                MaxBounds = new RectangleF(xprim, yprim - HHeight, 80, HHeight * 2f),
            };
        }

    }

    internal class VisualClefLayoutBounds
    {

        public PointF Origin { get; set; }
        public RectangleF MaxBounds { get; set; }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp)
        {
            ibmp.Mutate(ctx =>
            {
                ctx.FillPolygon(Brushes.Solid(Color.Red), new EllipsePolygon(Origin, 5).Points.ToArray());
                DrawRectangleExtensions.Draw(ctx, Pens.Dot(Color.Red, 1f), MaxBounds);
            });
        }


    }

}