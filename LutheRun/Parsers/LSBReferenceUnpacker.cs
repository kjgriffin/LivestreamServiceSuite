using Concord;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun.Parsers
{

    public class LSBReferenceUnpacker
    {
        public class VRef
        {
            /// <summary>
            /// Name of the Book
            /// </summary>
            public string Book { get; set; }
            /// <summary>
            /// Chapter in the Book (-1) means entire book
            /// </summary>
            public int Chapter { get; set; }
            /// <summary>
            /// Verse (-1) means entire chapter
            /// </summary>
            public int Verse { get; set; }
        }


        public class SectionReference
        {
            public string Book { get; set; }
            public int StartChapter { get; set; }
            /// <summary>
            /// default to 1 for entire chapter ref
            /// </summary>
            public int StartVerse { get; set; }
            public int EndChapter { get; set; }
            /// <summary>
            /// -1 indicates entire chapter ref
            /// </summary>
            public int EndVerse { get; set; }
        }

        public List<VRef> EnumerateVerses(SectionReference section, IBibleInfoProvider info)
        {
            List<VRef> vRefs = new List<VRef>();

            // check if whole chapter
            if (section.StartChapter == section.EndChapter && section.EndVerse == -1)
            {
                return EnumerateUntilEndOfChapter(info, section.Book, section.EndChapter, section.StartVerse);
            }
            else if (section.StartChapter == section.EndChapter)
            {
                return EnumerateInsideChapter(section.Book, section.StartChapter, section.StartVerse, section.EndVerse);
            }
            else
            {
                // spans multiple chapters
                int chapter = section.StartChapter;
                while (chapter != section.EndChapter)
                {
                    vRefs.AddRange(EnumerateUntilEndOfChapter(info, section.Book, chapter, chapter == section.StartChapter ? section.StartVerse : 1));
                }
                vRefs.AddRange(EnumerateInsideChapter(section.Book, section.EndChapter, 1, section.EndVerse));
            }

            return vRefs;
        }

        private List<VRef> EnumerateUntilEndOfChapter(IBibleInfoProvider info, string Book, int chapter, int startverse = 1)
        {
            return EnumerateInsideChapter(Book, chapter, startverse, info.GetVerseCount(Book, chapter));
        }

        private List<VRef> EnumerateInsideChapter(string Book, int chapter, int startVerse, int endverse)
        {
            List<VRef> vrefs = new List<VRef>();
            for (int i = startVerse; i <= endverse; i++)
            {
                vrefs.Add(new VRef
                {
                    Book = Book,
                    Chapter = chapter,
                    Verse = i,
                });
            }
            return vrefs;
        }

        public List<SectionReference> ParseSections(string reference)
        {
            // Step 1. pull out book
            // references always start with <#? book>
            var book = Regex.Match(reference, @"^(?<book>(\d\s)?\w+)").Groups["book"].Value;

            var refnobook = reference.Remove(0, book.Length).Trim();

            // Step 2. find all sections
            // a section has either an explicit or implicit chapter
            // it may also contain a start or end verse

            var sections = refnobook.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

            List<SectionReference> rsections = new List<SectionReference>();

            var lastchapter = -1;
            foreach (var section in sections)
            {
                SectionReference sec;
                if (section == sections.First())
                {
                    sec = ParseSectionRef(book, section);
                }
                else
                {
                    if (section.Contains(":"))
                    {
                        sec = ParseSectionRef(book, section);
                    }
                    else
                    {
                        // continues from previous chapter
                        sec = new SectionReference()
                        {
                            Book = book,
                            StartChapter = lastchapter,
                        };

                        var range = section.Trim().Split(new char[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries);
                        var start = range.First();
                        var end = range.Last();

                        sec.StartVerse = int.Parse(Regex.Match(start, @"\d+").Value);

                        // parse end checking for a different chapter
                        if (end.Contains(":"))
                        {
                            sec.EndChapter = int.Parse(Regex.Match(end, @"^\d+").Value);
                            sec.EndVerse = int.Parse(Regex.Match(end, @"\d+$").Value);
                        }
                        else
                        {
                            sec.EndChapter = sec.StartChapter;
                            sec.EndVerse = int.Parse(Regex.Match(end, @"^\d+").Value);
                        }
                    }
                }

                lastchapter = sec.EndChapter;
                rsections.Add(sec);
            }

            return rsections;
        }

        private static SectionReference ParseSectionRef(string book, string section)
        {
            SectionReference sec;
            // required to start with a chapter
            var i = section.IndexOf(":");
            if (i != -1)
            {
                sec = new SectionReference()
                {
                    Book = book,
                    StartChapter = int.Parse(section.Substring(0, i)),
                };

                var vsel = section.Substring(i + 1);

                var range = section.Trim().Split(new char[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries);
                var start = range.First();
                var end = range.Last();

                sec.StartVerse = int.Parse(Regex.Match(start, @"\d+$").Value);

                // parse end checking for a different chapter
                if (end.Contains(":"))
                {
                    sec.EndChapter = int.Parse(Regex.Match(end, @"^\d+").Value);
                    sec.EndVerse = int.Parse(Regex.Match(end, @"\d+$").Value);
                }
                else
                {
                    sec.EndChapter = sec.StartChapter;
                    sec.EndVerse = int.Parse(Regex.Match(end, @"^\d+").Value);
                }
            }
            else
            {
                // ref is entire chapter
                sec = new SectionReference
                {
                    Book = book,
                    StartChapter = int.Parse(section),
                    EndChapter = int.Parse(section),
                    StartVerse = 1,
                    EndVerse = -1,
                };
            }

            return sec;
        }
    }
}
