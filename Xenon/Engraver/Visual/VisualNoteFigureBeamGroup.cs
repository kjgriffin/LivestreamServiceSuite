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

        public List<VisualNoteFigure> ChildNotes { get; set; } = new List<VisualNoteFigure>();
        public Clef Clef { get; set; }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, HashSet<string> debug = null)
        {
            var bounds = CalculateBounds(X, Y);

            // extract all bounds to find all the note position points
            List<BeamNoteInfo> npts = new List<BeamNoteInfo>();

            foreach (var cnote in ChildNotes)
            {
                cnote.Render(X, Y, ibmp, ikbmp, layout, debug);

                var up = cnote.SitsUp();
                var cp = cnote.CalculateBounds(X, Y);
                npts.Add((up, new PointF(cp.StemOrigin.X, cp.StemOrigin.Y), new PointF(cp.StemTip.X, cp.StemTip.Y)));
            }

            // DRAW BEAMS

            bool beamUp = false;

            // if we bridge the mid-staff, we'll go up/down based on the mode, biased in the direction the clef uses for the mid-line note
            // handle the bias
            int ups = 0;
            int downs = 0;
            foreach (var cnote in npts)
            {
                if (cnote.up)
                {
                    ups += 1;
                }
                else
                {
                    downs += 1;
                }
            }
            if ((Clef == Clef.Trebble && ups > downs) || (Clef == Clef.Base && ups >= downs))
            {
                beamUp = true;
            }

            float nStemHeight = VisualNoteFigure.StemHeight;

            // **** rules ****
            // we're NOT allowed to 'shorten' a stem. we must lengthen instead
            // the MAX difference between stem extremes is 1 staff line height

            // we'll adjust the stem points as needed
            // then we'll add a beam

            // 1. adjust all nominal stem-tips to the coallesed direction
            float adjust = 2 * nStemHeight * (beamUp ? -1 : 1);
            for (int i = 0; i < npts.Count; i++)
            {
                if (npts[i].up != beamUp)
                {
                    var cp = ChildNotes[i].CalculateBounds(X, Y, true, beamUp);
                    npts[i] = new BeamNoteInfo(beamUp, cp.StemOrigin, cp.StemTip);
                }
            }


            // 2. now that all the stem tips are going the right way, need to find the min/max Y coords-
            //      based on if it's up/down bound by the 'min' val and then adjust as nessecary to be within the bounds

            PointF Ymin = npts.MinBy(y => y.tpt.Y).tpt;
            PointF Ymax = npts.MaxBy(y => y.tpt.Y).tpt;

            if (Math.Abs(Ymax.Y - Ymin.Y) > 10) // one line on a staff
            {

                for (int i = 0; i < npts.Count; i++)
                {
                    if (beamUp)
                    {
                        if (npts[i].tpt.Y > Ymin.Y + 10)
                        {
                            npts[i] = new BeamNoteInfo(beamUp, npts[i].npt, new PointF(npts[i].tpt.X, Ymin.Y));
                        }
                    }
                    else
                    {
                        if (npts[i].tpt.Y < Ymax.Y - 10)
                        {
                            npts[i] = new BeamNoteInfo(beamUp, npts[i].npt, new PointF(npts[i].tpt.X, Ymax.Y));
                        }
                    }
                }

            }

            PointF Lbound = npts.MinBy(x => x.tpt.X).tpt;
            PointF Rbound = npts.MaxBy(x => x.tpt.X).tpt;

            if (debug?.HasFlag(DebugEngravingRenderableExtensions.DFlags.BPoints) == true)
            {
                ibmp.Mutate(ctx =>
                {
                    foreach (var bni in npts)
                    {
                        ctx.FillPolygon(Brushes.Solid(Color.Purple), new EllipsePolygon(bni.tpt, 3).Points.ToArray());
                        ctx.FillPolygon(Brushes.Solid(Color.Orange), new EllipsePolygon(bni.npt, 3).Points.ToArray());
                        ctx.DrawLines(Pens.Dot(Color.Blue, 2), bni.npt, bni.tpt);

                        ctx.DrawLines(Pens.Dot(Color.Pink, 2), Lbound, Rbound);
                    }
                });
            }


            bounds.Render(ibmp, ikbmp, debug);
        }
        internal void PlaceChildren(float X, float Y)
        {
            float xprim = X + XOffset;
            float yprim = Y + YOffset;

            float iXoff = 0;

            // adjust placement for each child's body iterativley
            for (int i = 0; i < ChildNotes.Count; i++)
            {
                var cnote = ChildNotes[i];

                var bounds = cnote.CalculateBounds(0, 0);
                // adjust figure position based on size
                cnote.XOffset = cnote.XOffset + (bounds.BodyOrigin.X - bounds.FigureBounds.Left) + iXoff;
                iXoff += bounds.MaxBounds.Width;
            }
        }

        internal VisualNoteFigureBeamGroupLayoutBounds CalculateBounds(float X, float Y)
        {
            float xprim = X + XOffset;
            float yprim = Y + YOffset;

            float mbounds = 0;

            foreach (var cnote in ChildNotes)
            {
                mbounds += cnote.CalculateBounds(X, Y).MaxBounds.Width;
            }

            return new VisualNoteFigureBeamGroupLayoutBounds
            {
                Origin = new PointF(xprim, yprim),
                MaxBounds = new RectangleF(xprim, yprim, mbounds, 40),
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

    internal record struct BeamNoteInfo(bool up, PointF npt, PointF tpt)
    {
        public static implicit operator (bool up, PointF npt, PointF tpt)(BeamNoteInfo value)
        {
            return (value.up, value.npt, value.tpt);
        }

        public static implicit operator BeamNoteInfo((bool up, PointF npt, PointF tpt) value)
        {
            return new BeamNoteInfo(value.up, value.npt, value.tpt);
        }

        public static BeamNoteInfo MovedTail(BeamNoteInfo orig, float val)
        {
            return new BeamNoteInfo(orig.up, orig.npt, new PointF(orig.tpt.X, orig.tpt.Y + val));
        }

    }
}
