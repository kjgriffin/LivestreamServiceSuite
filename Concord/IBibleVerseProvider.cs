namespace Concord
{
    public interface IBibleVerseProvider
    {
        IBibleVerse GetVerse(string Book, int chapter, int verse);
    }



}