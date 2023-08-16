using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Concord.BibleData
{
    class Bible
    {
        public Dictionary<string, BibleBook> BibleBooks { get; set; } = new Dictionary<string, BibleBook>();

        [JsonIgnore]
        public Dictionary<string, ExtendedBibleBook> ExtendedMetadata { get; set; } = new Dictionary<string, ExtendedBibleBook>();

        public Dictionary<string, BookContent> Books { get; set; } = new Dictionary<string, BookContent>();

        public string Translation { get; set; }
    }

    class BookContent
    {
        public Dictionary<int, ChapterContent> Chapters { get; set; } = new Dictionary<int, ChapterContent>();
    }

    class ChapterContent
    {
        public Dictionary<int, VerseContent> Verses { get; set; } = new Dictionary<int, VerseContent>();
    }

    class Genre
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    class ChapterInfo
    {
        public int id { get; set; }
    }

    class BibleBook
    {
        public int id { get; set; }
        public string name { get; set; }
        public string testament { get; set; }
        public Genre genre { get; set; } = new Genre();
    }

    class ExtendedBibleBook : BibleBook
    {
        public int chapterCount { get; set; }
        public string lowerName { get; set; }
        public string indexedName { get; set; }
    }

    class Book
    {
        public int id { get; set; }
        public string name { get; set; }
        public string testament { get; set; }
    }

    class VerseContent : IBibleVerse
    {
        public int id { get; set; }
        public Book book { get; set; } = new Book();
        public int chapterId { get; set; }
        public int verseId { get; set; }
        public string verse { get; set; }
        int IBibleVerse.Number { get => verseId; }
        string IBibleVerse.Text { get => verse; }
        int IBibleVerse.ChapterRef { get => chapterId; }
        int IBibleVerse.BookRef { get => book.id; }
    }
}
