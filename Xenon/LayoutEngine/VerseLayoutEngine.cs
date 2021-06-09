using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.LayoutEngine
{
    class VerseLayoutEngine
    {

        public List<VerseLayoutLine> LayoutLines { get; set; } = new List<VerseLayoutLine>();
        /// <summary>
        /// Uses newlines as line endings for each line
        /// </summary>
        /// <param name="words"></param>
        public void BuildLines(List<string> words)
        {
            LayoutLines.Clear();
            VerseLayoutLine line = new VerseLayoutLine();
            foreach (var word in words)
            {
                if (word == "\r\n" || word == "\n")
                {
                    if (line.Words.Count > 0)
                    {
                        LayoutLines.Add(line);
                        line = new VerseLayoutLine();
                    }
                }
                else
                {
                    line.Words.Add(word);
                }
            }
            if (line.Words.Count > 0)
            {
                LayoutLines.Add(line);
            }
        }
    }
}
