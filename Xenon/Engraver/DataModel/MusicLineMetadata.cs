namespace Xenon.Engraver.DataModel
{
    internal class MusicLineMetadata
    {
        public Clef Clef { get; set; } = Clef.Trebble;
        public KeySignature KeySignature { get; set; } = KeySignature.NONE;
        public NoteLength Nominal { get; set; } = NoteLength.QUARTER;
        public TimeSignatureMetadata TimeSignature { get; set; } = new TimeSignatureMetadata();
        public int StaffLines { get; set; } = 5;

    }



}
