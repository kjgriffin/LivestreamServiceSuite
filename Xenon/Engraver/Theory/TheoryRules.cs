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

        internal static (int Sharps, int Flats) GetSharpsAndFlats(KeySignature key)
        {
            switch (key)
            {
                case KeySignature.NONE:
                    return (0, 0);

                case KeySignature.C_MAJOR:
                    return (0, 0);

                case KeySignature.G_MAJOR:
                    return (1, 0);
                case KeySignature.D_MAJOR:
                    return (2, 0);
                case KeySignature.A_MAJOR:
                    return (3, 0);
                case KeySignature.E_MAJOR:
                    return (4, 0);
                case KeySignature.B_MAJOR:
                    return (5, 0);
                case KeySignature.F_SHARP_MAJOR:
                    return (6, 0);
                case KeySignature.C_SHARP_MAJOR:
                    return (7, 0);

                case KeySignature.F_MAJOR:
                    return (0, 1);
                case KeySignature.B_FLAT_MAJOR:
                    return (0, 2);
                case KeySignature.E_FLAT_MAJOR:
                    return (0, 3);
                case KeySignature.A_FLAT_MAJOR:
                    return (0, 4);
                case KeySignature.D_FLAT_MAJOR:
                    return (0, 5);
                case KeySignature.G_FLAT_MAJOR:
                    return (0, 6);
                case KeySignature.C_FLAT_MAJOR:
                    return (0, 7);

                default:
                    return (0, 0);
            }

        }




    }
}
