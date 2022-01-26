using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Xenon.LayoutInfo;

namespace Xenon.LayoutEngine.L2
{
    class SizedCandidateBlock
    {
        public List<SizedResponsiveStatement> Lines { get; set; } = new List<SizedResponsiveStatement>();

        public float MaxHeight
        {
            get
            {
                return Lines.Max(x => x.MaxHeight);
            }
        }

        public int NumLines { get; set; }

        public static SizedCandidateBlock CreateSized(CandidateBlock block, Graphics gfx, ResponsiveLiturgySlideLayoutInfo layout)
        {
            SizedCandidateBlock sblock = new SizedCandidateBlock();
            foreach (var line in block.Lines)
            {
                sblock.Lines.Add(SizedResponsiveStatement.CreateSized(line, gfx, layout));
            }
            return sblock;
        }
    }
}
