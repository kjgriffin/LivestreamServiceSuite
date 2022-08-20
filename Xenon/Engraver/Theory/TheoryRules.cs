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


        public static List<NoteGroup> GroupifyNoteStream(List<Note> nstream, TimeSignatureMetadata time)
        {
            if (time.Type == DataModel.TimeSignatureType.SIMPLE)
            {
                return SimpleTimeStreamBeamifyier(nstream, time);
            }

            List<NoteGroup> groups = new List<NoteGroup>();

            foreach (var note in nstream)
            {
                NoteGroup g = new NoteGroup
                {
                    Notes = new List<Note> { note },
                };
                groups.Add(g);
            }

            return groups;
        }

        private static List<NoteGroup> SimpleTimeStreamBeamifyier(List<Note> nstream, TimeSignatureMetadata time)
        {
            List<NoteGroup> groups = new List<NoteGroup>();

            double std = time.ComputeBNVal();
            double blength = time.ComputeBLVal();
            double forcesplitat = blength / 2;

            double gfillval = 0;

            NoteGroup g = new NoteGroup();

            foreach (var note in nstream)
            {
                var nlen = time.ComputeLVal(note);

                if (note.Length < NoteLength.EIGHTH) // only tailed notes are factored for beaming
                {
                    groups.Add(g);
                    g = new NoteGroup();
                    groups.Add(new NoteGroup { Notes = new List<Note> { note } });
                    gfillval = 0;
                }
                else
                {
                    if (gfillval + nlen > forcesplitat)
                    {
                        groups.Add(g);
                        g = new NoteGroup();
                        gfillval = 0;
                    }

                    g.Notes.Add(note);
                    gfillval += nlen;
                }
            }

            if (g.Notes.Any())
            {
                groups.Add(g);
            }

            return groups.Where(g => g.Notes.Any()).ToList();
        }

        internal static TimeSignatureType TimeSignatureType(int dnd, int div)
        {


            if (dnd == 2 || dnd == 3 || dnd == 4)
            {
                return DataModel.TimeSignatureType.SIMPLE;
            }
            else if (dnd == 6 || dnd == 8 || dnd == 12)
            {
                return DataModel.TimeSignatureType.COMPOUND;
            }

            // sure.. we'll do simple time
            return DataModel.TimeSignatureType.SIMPLE;
        }

        internal static double GetCalcLength(NoteLength length)
        {
            switch (length)
            {
                case NoteLength.WHOLE:
                    return 16;
                case NoteLength.HALF:
                    return 8;
                case NoteLength.QUARTER:
                    return 4;
                case NoteLength.EIGHTH:
                    return 2;
                case NoteLength.SIXTEENTH:
                    return 1;
                default:
                    return 0;
            }
        }

    }
}
