using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SlideCreater.LayoutEngine
{
    /*
    This is where the magic of interpreting a natural language is.
    Will try to intelligently generate liturgy lines that fit onto a slide, and make sense

    The input is a project layout and a liturgyexpression

    will walk through the liturgy content and try to make a good decision about how to put it into lines.

    final lines will be tuple strings. the first string identifies the speaker, the second is the actual content text

    e.g. line:
    "P", "We begin in the Name of the LORD."

    each line will be tagged with the speaker, the render engine will be responsible for determining when to show speaker- ie. repeated lines by same speaker on same line should only show first instance
    but having extra info is good at this point so wrapping etc. won't require as complex state (at least not accross slides)


    The general methodology will be to use the liturgy slide's layout information to determine line size,
    and to interpret the content into discrete lines that are all single lineheight.

    The second pass will then group lines into slides based on the following rules:
    1. Slides starting with C can't transition to another speaker
    2. Slides can't Have a C speaker and not end with C.


     */
    public class LiturgyLayoutEngine
    {

        public List<(string speaker, List<string> words)> Lines { get; set; } = new List<(string, List<string>)>();


        private List<string> SentenceEnds = new List<string>()
        {
            ".",
            "!",
            "?"
        };

        private List<string> Speakers = new List<string>()
        {
            "P",
            "A",
            "L",
            "C"
        };

        public void BuildLines(List<string> words)
        {
            // go through all the words and assemble into sentences.

            StringBuilder sb = new StringBuilder();

            string speaker = "";

            List<string> line = new List<string>();

            foreach (var word in words)
            {
                if (Speakers.Contains(word))
                {
                    if (speaker != string.Empty)
                    {
                        Lines.Add((speaker, finalizeline(line)));
                    }
                    line.Clear();
                    // start a new sentence
                    speaker = word;
                }
                else
                {
                    // ignore newlines
                    if (word != "\r\n")
                    {
                        line.Add(word);
                    }
                    else
                    {
                        // use space instead
                        line.Add(" ");
                    }
                }
            }
            Lines.Add((speaker, finalizeline(line)));

            Lines.ForEach(s => Debug.WriteLine($"<{s.speaker}> \"{string.Join("", s.words)}\""));
        }

        private List<string> finalizeline(List<string> words)
        {
            List<string> result = new List<string>();
            // copy words
            words.ForEach(w => result.Add(w));
            // remove whitespace
            while(result.FindIndex(r => Regex.Match(r, "\\s").Success) == 0)
            {
                result.RemoveAt(0);
            }
            while(result.FindLastIndex(r => Regex.Match(r, "\\s").Success) == result.Count - 1)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }

        public void BuildSlideLines()
        {
            // start with lines and see if we can fit them


        }



    }
}
