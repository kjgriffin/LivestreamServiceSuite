namespace Xenon.Engraver.DataModel
{
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
