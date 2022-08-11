using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Engraver.DataModel;

namespace Xenon.Engraver.Theory
{
    internal static class TheoryRules
    {
        internal static int NominalRegister(Clef clef)
        {
            switch (clef)
            {
                case Clef.Trebble:
                    return 4;
                default:
                    return 0;
            }
        }




    }
}
