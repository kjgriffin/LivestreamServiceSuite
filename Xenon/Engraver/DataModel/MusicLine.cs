using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Engraver.DataModel
{

    enum Clef
    {
        Unkown,
        Trebble,
        Base,
    }

    enum KeySignature
    {
        NONE,

        C_MAJOR,
        G_MAJOR,
        D_MAJOR,
        A_MAJOR,
        E_MAJOR,
        B_MAJOR,
        F_SHARP_MAJOR,
        C_SHARP_MAJOR,

        F_MAJOR,
        B_FLAT_MAJOR,
        E_FLAT_MAJOR,
        A_FLAT_MAJOR,
        D_FLAT_MAJOR,
        G_FLAT_MAJOR,
        C_FLAT_MAJOR,
    }

    enum NoteLength
    {
        WHOLE,
        HALF,
        QUARTER,
        EIGHTH,
        SIXTEENTH,
    }

    enum BarType
    {
        None,
        Single,
        Double,
        Repeat,
    }

    enum NoteName
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
    }

    enum Accidental
    {
        None,
        Sharp,
        Flat,
        Natural,
    }


    internal class MusicLineMetadata
    {
        public Clef Clef { get; set; } = Clef.Trebble;
        public KeySignature KeySignature { get; set; } = KeySignature.NONE;
        public string TimeDividend { get; set; } = "4";
        public string TimeDivisor { get; set; } = "4";
        public NoteLength Nominal { get; set; } = NoteLength.QUARTER;
        public int StaffLines { get; set; } = 5;


    }

    internal class MusicLine
    {
        public MusicLineMetadata Metadata { get; set; } = new MusicLineMetadata();
        public string SBegin { get; set; } = "";
        public string SEnd { get; set; } = "";
        public MusicSequence Notes { get; set; } = new MusicSequence();

    }

    internal class MusicSequence
    {
        public List<MusicBar> Bars { get; set; } = new List<MusicBar>();
    }

    internal class MusicBar
    {
        public BarType BeginBar { get; set; } = BarType.None;
        public BarType EndBar { get; set; } = BarType.Single;
        public List<Note> Notes { get; set; } = new List<Note>();
        public Clef Clef { get; set; } = Clef.Unkown;
        public bool ShowClef { get; set; } = false;
        public KeySignature KeySig { get; set; } = KeySignature.NONE;
        public bool ShowKeySig { get; set; } = false;
    }

    internal class Note
    {
        public int Register { get; set; }
        public NoteName Name { get; set; }
        public Accidental Accidental { get; set; } = Accidental.None;
        public NoteLength Length { get; set; }
        public int LengthDots { get; set; } = 0;
        public bool TieToNext { get; set; } = false;
    }



}
