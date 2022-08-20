using System.Collections.Generic;

namespace Xenon.Engraver.DataModel
{
    internal class MusicBar
    {
        public BarType BeginBar { get; set; } = BarType.None;
        public BarType EndBar { get; set; } = BarType.Single;
        public List<NoteGroup> Notes { get; set; } = new List<NoteGroup>();
        public Clef Clef { get; set; } = Clef.Unkown;
        public bool ShowClef { get; set; } = false;
        public KeySignature KeySig { get; set; } = KeySignature.NONE;
        public bool ShowKeySig { get; set; } = false;
        public TimeSignatureMetadata Time { get; set; } = new TimeSignatureMetadata();
        public bool ShowTime { get; set; } = false;
    }



}
