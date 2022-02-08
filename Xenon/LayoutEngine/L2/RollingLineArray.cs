using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.LayoutEngine.L2
{
    internal class RollingLineArray
    {

        public List<RollingLine> Lines { get; set; } = new List<RollingLine>();

        public double LineWidth { get; set; } = 0;

        public RollingLineArray(double lwidth)
        {
            LineWidth = lwidth;
        }


        public void Append(SizedTextBlurb blurb)
        {
            if (Lines.LastOrDefault() == null || !Lines.Last().TryAppend(blurb))
            {
                RollingLine newline = new RollingLine(LineWidth);
                newline.TryAppend(blurb);
                Lines.Add(newline);
            }
        }

        internal void Append(List<SizedTextBlurb> content)
        {
            foreach (var item in content)
            {
                Append(item);
            }
        }

        internal void KickandSpill(int lineindex, int blurbindex)
        {
            if (lineindex < Lines.Count && blurbindex < Lines[lineindex].Blurbs.Count)
            {
                // get words to kick
                var kicked = Lines[lineindex].Kick(blurbindex);

                // propagate the rollover
                RollingSpill(lineindex + 1, kicked);
            }
        }

        private void RollingSpill(int spillinto, List<SizedTextBlurb> overflow)
        {
            if (!overflow.Any())
            {
                return;
            }
            if (Lines.Count - 1 < spillinto)
            {
                // we need a new line
                RollingLine newline = new RollingLine(LineWidth);
                Lines.Add(newline);
                Append(overflow);
            }
            else
            {
                var spilled = Lines[spillinto].RollingInsert(overflow);
                // recursively spill onto the next line
                RollingSpill(spillinto + 1, spilled);
            }
        }

    }

    internal class RollingLine
    {
        public List<SizedTextBlurb> Blurbs { get; set; } = new List<SizedTextBlurb>();
        public double LineWidth { get; set; }

        public double CWidth
        {
            get
            {
                return Blurbs.Sum(x => x.Size.Width);
            }
        }

        public RollingLine(double lwidth)
        {
            LineWidth = lwidth;
        }

        /// <summary>
        /// Attempts to add to end of line.
        /// </summary>
        /// <param name="blurb">Blurb to append.</param>
        /// <returns>True if successful.</returns>
        public bool TryAppend(SizedTextBlurb blurb)
        {
            ThrowIfImpossibleToFit(blurb);
            if (CWidth + blurb.Size.Width <= LineWidth)
            {
                Blurbs.Add(blurb);
                return true;
            }
            return false;
        }

        public List<SizedTextBlurb> RollingInsert(SizedTextBlurb blurb)
        {
            ThrowIfImpossibleToFit(blurb);


            List<SizedTextBlurb> spill = new List<SizedTextBlurb>();

            Blurbs = Blurbs.Prepend(blurb).ToList();

            // performance wise this is stupid- just get the full width once, then iterate backwords and locally compute the new width
            // but hey- this is easy for now and we can profile before optimization

            while (CWidth > LineWidth && Blurbs.Any())
            {
                var b = Blurbs.Last();
                spill.Add(b);
                Blurbs.Remove(b);
            }

            // keep blurbs in order
            spill.Reverse();
            return spill;
        }
        internal List<SizedTextBlurb> RollingInsert(List<SizedTextBlurb> insert)
        {
            var overflow = new List<List<SizedTextBlurb>>();
            insert.Reverse();
            foreach (var blurb in insert)
            {
                overflow.Add(RollingInsert(blurb));
            }
            overflow.Reverse();
            List<SizedTextBlurb> orderedoverlow = new List<SizedTextBlurb>();
            foreach (var item in overflow)
            {
                orderedoverlow.AddRange(item);
            }
            return orderedoverlow;
        }

        internal List<SizedTextBlurb> Kick(int index)
        {
            if (index < Blurbs.Count)
            {
                int num = Blurbs.Count - index;
                var kicked = Blurbs.GetRange(index, num);
                Blurbs.RemoveRange(index, num);
                return kicked;
            }
            return new List<SizedTextBlurb>();
        }

        internal void Trim()
        {
            // trim start
            while (Blurbs.Any() && string.IsNullOrWhiteSpace(Blurbs.First().Text))
            {
                Blurbs.RemoveAt(0);
            }
            //trim end
            while(Blurbs.Any() && string.IsNullOrWhiteSpace(Blurbs.Last().Text))
            {
                Blurbs.RemoveAt(Blurbs.Count - 1);
            }
        }


        private void ThrowIfImpossibleToFit(SizedTextBlurb blurb)
        {
            if (blurb.Size.Width > LineWidth)
            {
                // impossible to fit... what to do here I'm not sure
                throw new Exception($"Can't figure out how to fit blurb {blurb.Text} with width: {blurb.Size.Width} into a line with width: {LineWidth}");
            }
        }

        public override string ToString()
        {
            return string.Concat(Blurbs.Select(b => b.Text).ToArray());
        }

    }


}
