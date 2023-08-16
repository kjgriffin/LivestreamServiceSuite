using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Concord
{
    public interface IBibleVerse
    {
        int Number { get; }
        string Text { get; }
        int ChapterRef { get; }
        int BookRef { get; }
    }

    public class HardCopyAPI : IBibleInfoProvider, IBibleVerseProvider
    {
        Dictionary<int, Book> Books;
        Dictionary<string, BookMetadata> BookMeta;
        PericopeIndex pericopeIndex;

        public class BibleVerse
        {
            public int pk { get; set; }
            public string translation { get; set; }
            public int book { get; set; }
            public int chapter { get; set; }
            public int verse { get; set; }
            public string text { get; set; }
        }

        public class Verse : IBibleVerse
        {
            public int Number { get; set; }
            public string Text { get; set; }
            public string Pericope { get; set; } = "";
            public int ChapterRef { get; set; }
            public int BookRef { get; set; }
        }

        public class Chapter
        {
            public int Number { get; set; }
            public Dictionary<int, Verse> Verses { get; set; }
            public int BookRef { get; set; }
        }

        public class Book
        {
            public int Number { get; set; }
            public Dictionary<int, Chapter> Chapters { get; set; }
        }

        public class BookMetadata
        {
            public int bookid { get; set; }
            public string name { get; set; }
            public int chronorder { get; set; }
            public int chapters { get; set; }
        }

        private void Parse(string btext, string bmeta)
        {
            List<BibleVerse> Contents = JsonSerializer.Deserialize<List<BibleVerse>>(btext);

            Books = new Dictionary<int, Book>();

            foreach (var item in Contents)
            {
                Book book;
                if (!Books.TryGetValue(item.book, out book))
                {
                    book = new Book()
                    {
                        Chapters = new Dictionary<int, Chapter>(),
                        Number = item.book,
                    };
                    Books[book.Number] = book;
                }

                Chapter chapter;
                if (!book.Chapters.TryGetValue(item.chapter, out chapter))
                {
                    chapter = new Chapter()
                    {
                        Number = item.chapter,
                        BookRef = item.book,
                        Verses = new Dictionary<int, Verse>(),
                    };
                    book.Chapters[chapter.Number] = chapter;
                }

                chapter.Verses[item.verse] = new Verse()
                {
                    BookRef = item.book,
                    ChapterRef = item.chapter,
                    Number = item.verse,
                    Text = item.text,
                };

            }

            List<BookMetadata> Meta = JsonSerializer.Deserialize<List<BookMetadata>>(bmeta);

            BookMeta = new Dictionary<string, BookMetadata>();
            foreach (var book in Meta)
            {
                BookMeta[book.name.ToLower()] = book;
            }

        }

        internal HardCopyAPI(string bibleText, string bibleMetadata)
        {
            Parse(bibleText, bibleMetadata);
            pericopeIndex = new PericopeIndex(BibleBuilder.GetBlob("2450.Pericopes.csv"));
        }


        public IBibleVerse GetVerse(string Book, int chapter, int verse)
        {
            if (BookMeta.TryGetValue(Book.ToLower(), out var bookinfo))
            {
                if (Books.TryGetValue(bookinfo.bookid, out var book))
                {
                    if (book.Chapters.TryGetValue(chapter, out var c))
                    {
                        if (c.Verses.TryGetValue(verse, out var v))
                        {
                            // add pericope info
                            if (string.IsNullOrEmpty(v.Pericope))
                            {
                                var pericope_lookup = pericopeIndex.FindPericope(Book, chapter, verse);
                                if (pericope_lookup != null)
                                {
                                    v.Pericope = pericope_lookup.Title;
                                    v.Text = v.Text.Remove(0, pericope_lookup.Title.Length).Trim();
                                    if (v.Text.StartsWith("<br/>"))
                                    {
                                        v.Text = v.Text.Remove(0, 5).Trim();
                                    }
                                }
                            }
                            return v;
                        }
                    }
                }
            }
            throw new ArgumentOutOfRangeException($"No definition for {Book} {chapter}:{verse}");
        }

        public int GetChapterCount(string Book)
        {
            if (BookMeta.TryGetValue(Book.ToLower(), out var bookinfo))
            {
                return bookinfo.chapters;
            }
            throw new ArgumentOutOfRangeException($"No definition for {Book}");
        }

        public int GetVerseCount(string Book, int Chapter)
        {
            if (BookMeta.TryGetValue(Book.ToLower(), out var bookinfo))
            {
                if (Books.TryGetValue(bookinfo.bookid, out var book))
                {
                    if (book.Chapters.TryGetValue(Chapter, out var c))
                    {
                        return c.Verses.Count;
                    }
                }
            }
            throw new ArgumentOutOfRangeException($"No definition for {Book} {Chapter}");

        }
    }



}