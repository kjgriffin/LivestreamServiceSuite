using Concord.BibleData;

using System;
using System.Text.Json;

namespace Concord
{
    public class HardCopyAPIv2 : IBibleAPI
    {
        Bible _Source;

        internal HardCopyAPIv2(string sourceBlob)
        {
            _Source = JsonSerializer.Deserialize<Bible>(sourceBlob);

            // normalize + metadata
            foreach (var book in _Source.Books)
            {
                string camelName = book.Key;
                var meta = _Source.BibleBooks[camelName];

                // add extra metadata
                _Source.ExtendedMetadata[camelName.ToLower()] = new ExtendedBibleBook
                {
                    name = meta.name,
                    genre = meta.genre,
                    id = meta.id,
                    testament = meta.testament,
                    indexedName = camelName,
                    lowerName = camelName.ToLower(),
                    chapterCount = book.Value.Chapters.Count,
                };
            }
        }

        public IBibleVerse GetVerse(string Book, int chapter, int verse)
        {
            if (_Source.ExtendedMetadata.TryGetValue(Book.ToLower(), out var bookinfo))
            {
                if (_Source.Books.TryGetValue(bookinfo.indexedName, out var book))
                {
                    if (book.Chapters.TryGetValue(chapter, out var c))
                    {
                        if (c.Verses.TryGetValue(verse, out var v))
                        {
                            return v;
                        }
                    }
                }
            }
            throw new ArgumentOutOfRangeException($"No definition for {Book} {chapter}:{verse}");
        }

        public int GetChapterCount(string Book)
        {
            if (_Source.ExtendedMetadata.TryGetValue(Book.ToLower(), out var bookinfo))
            {
                return bookinfo.chapterCount;
            }
            throw new ArgumentOutOfRangeException($"No definition for {Book}");
        }

        public int GetVerseCount(string Book, int Chapter)
        {
            if (_Source.ExtendedMetadata.TryGetValue(Book.ToLower(), out var bookinfo))
            {
                if (_Source.Books.TryGetValue(bookinfo.indexedName, out var book))
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