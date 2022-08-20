using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Engraver.Theory;

namespace Xenon.Engraver.DataModel
{
    internal class TimeSignatureMetadata
    {
        public string STimeDividend { get; set; } = "4";
        public string STimeDivisor { get; set; } = "4";
        public int TimeDividend { get; set; } = 4;
        public int TimeDivisor { get; set; } = 4;

        public TimeSignatureType Type { get; set; } = TimeSignatureType.SIMPLE;

        public (int dividend, int divisor) GetSignature()
        {
            return (TimeDividend, TimeDivisor);
        }

        public NoteLength Nominal
        {
            get
            {
                switch (TimeDivisor)
                {
                    case 2:
                        return NoteLength.HALF;
                    case 4:
                        return NoteLength.QUARTER;
                    case 8:
                        return NoteLength.EIGHTH;
                }
                return NoteLength.QUARTER;
            }
        }

        public double ComputeLVal(Note note)
        {
            double ratio = TheoryRules.GetCalcLength(note.Length);

            double clength = ratio;
            double mod = ratio / 2;

            for (int i = 1; i <= note.LengthDots; i++)
            {
                clength += mod;
                mod /= 2f;
            }

            return clength;
        }

        public double ComputeBNVal()
        {
            return TheoryRules.GetCalcLength(Nominal);
        }

        public double ComputeBLVal()
        {
            return ComputeBNVal() * TimeDividend;
        }

    }

    internal enum TimeSignatureType
    {
        SIMPLE,
        COMPOUND,
    }


}
