using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using static Concord.HardCopyAPI;

namespace Concord
{
    public class PericopeIndex
    {
        public List<Pericope> Pericopes;

        public PericopeIndex(string csvText)
        {
            BuildIndex(csvText);
        }

        void BuildIndex(string csvText)
        {
            Pericopes = new List<Pericope>();
            var lines = csvText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith(","))
                {
                    var columns = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (columns.Length == 3)
                    {
                        var reference = Regex.Match(columns[2], @"(?<book>(\d\s)?\w+)\s(?<chapter>\d+):(?<verse>\d+)");
                        Pericopes.Add(new Pericope
                        {
                            Book = reference.Groups["book"].Value,
                            StartChapter = int.Parse(reference.Groups["chapter"].Value),
                            StartVerse = int.Parse(reference.Groups["verse"].Value),
                            Title = columns[0],
                        });
                    }
                }
            }
        }

        public Pericope FindPericope(string book, int chapter, int verse)
        {
            var match = Pericopes.FirstOrDefault(x => x.Book.ToLower() == book.ToLower() && x.StartChapter == chapter && x.StartVerse == verse);
            return match;
        }
    }
}
