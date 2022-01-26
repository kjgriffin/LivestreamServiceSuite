using System.Collections.Generic;

using Xenon.Compiler.SubParsers;

namespace Xenon.LayoutEngine.L2
{
    class CandidateBlock
    {
        public List<ResponsiveStatement> Lines { get; set; } = new List<ResponsiveStatement>();
    }
}
