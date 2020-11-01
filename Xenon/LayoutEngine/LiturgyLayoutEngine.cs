using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls.Ribbon.Primitives;
using Xenon.LiturgyLayout;

namespace Xenon.LayoutEngine
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
    class LiturgyLayoutEngine
    {

        public List<((string speaker, string speakertext) speaker, List<string> words)> Lines { get; set; } = new List<((string, string), List<string>)>();
        public List<LiturgyLine> LiturgyLines { get; set; } = new List<LiturgyLine>();
        public List<LiturgyLayoutLine> LayoutLines { get; set; } = new List<LiturgyLayoutLine>();
        public List<LiturgyTextLine> LiturgyTextLines { get; set; } = new List<LiturgyTextLine>();


        private List<string> SentenceEnds = new List<string>()
        {
            ".",
            "!",
            "?"
        };

        private Dictionary<string, string> Speakers = new Dictionary<string, string>()
        {
            ["P"] = "P",
            ["A"] = "A",
            ["L"] = "L",
            ["C"] = "C",
            ["R"] = "R",
            ["$"] = "",
        };

        private List<string> Escape = new List<string>()
        {
            "\\",
        };

        public void BuildLines(List<string> words, Dictionary<string, string> additionalspeakers, bool onlystartspeakersonnewline = false)
        {
            // go through all the words and assemble into sentences.

            StringBuilder sb = new StringBuilder();

            Dictionary<string, string> speakers = Speakers;

            if (additionalspeakers != null)
            {
                foreach (var kvp in additionalspeakers)
                {
                    speakers[kvp.Key] = kvp.Value;
                }
            }

            string speaker = "";

            List<string> line = new List<string>();

            bool skipnext = false;

            bool startofline = true;

            foreach (var word in words)
            {
                if (Escape.Contains(word))
                {
                    skipnext = true;
                    continue;
                }
                if (speakers.Keys.Contains(word) && !skipnext)
                {
                    if (speaker != string.Empty)
                    {
                        Lines.Add(((speaker, speakers[speaker]), finalizeline(line)));
                        LiturgyLines.Add(createliturgyline(speakers[speaker], finalizeline(line)));
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
                        startofline = true;
                    }
                }
                skipnext = false;
                if (startofline)
                {
                    startofline = false;
                }
            }
            Lines.Add(((speaker, speakers[speaker]), finalizeline(line)));
            LiturgyLines.Add(createliturgyline(speakers[speaker], finalizeline(line)));

            Lines.ForEach(s => Debug.WriteLine($"<{s.speaker}> \"{string.Join("", s.words)}\""));
        }

        private LiturgyLine createliturgyline(string speaker, List<string> words)
        {
            LiturgyLine liturgy = new LiturgyLine();
            liturgy.SpeakerText = speaker;
            List<string> trimmedwords = new List<string>();
            foreach (var word in words)
            {
                trimmedwords.Add(word);
            }
            // remove whitespace
            while (trimmedwords.FindIndex(r => Regex.Match(r, "\\s").Success) == 0)
            {
                trimmedwords.RemoveAt(0);
            }
            while (trimmedwords.Count > 0 && trimmedwords.FindLastIndex(r => Regex.Match(r, "\\s").Success) == trimmedwords.Count - 1)
            {
                trimmedwords.RemoveAt(trimmedwords.Count - 1);
            }
            foreach (var word in trimmedwords)
            {
                LiturgyWord w = new LiturgyWord()
                {
                    IsBold = (speaker == "C" || speaker == "R"),
                    IsLSBSymbol = word == "T",
                    Value = word,
                    Size = new Size(),
                    IsSized = false,
                };
                liturgy.Words.Add(w);
            }
            return liturgy;
        }

        private List<string> finalizeline(List<string> words)
        {
            List<string> result = new List<string>();
            // copy words
            words.ForEach(w => result.Add(w));
            // remove whitespace
            while (result.FindIndex(r => Regex.Match(r, "\\s").Success) == 0)
            {
                result.RemoveAt(0);
            }
            while (result.Count > 0 && result.FindLastIndex(r => Regex.Match(r, "\\s").Success) == result.Count - 1)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result;
        }

        public void BuildTextLines(Rectangle textbox)
        {
            foreach (var speakerline in LiturgyLines)
            {
                var tlines = BlockParagraphLayoutEngine.LayoutLiturgyIntoTextLines(textbox, speakerline);
                LiturgyTextLines.AddRange(tlines);
            }
        }

        public void BuildSlideLines(LiturgyLayoutRenderInfo renderInfo)
        {
            /*
            Basic algorithm:

            Every speaker line is rendered in paragraphs (that fit to the line size)

            Thus:

            p aasdfasdfas. asdfasdfa. asdf. asdf.
            P short.
            P more stuff here.

            should be rendered where the first 'line' is recognized as a paragraph and renders as many words as fit per line
            the next line starts on a new line (repeated speaker forces new line)
            
            etc.
            */

            foreach (var line in Lines)
            {
                Font f;
                switch (line.speaker.speakertext)
                {
                    case "C":
                        f = renderInfo.BoldFont;
                        break;
                    case "R":
                        f = renderInfo.BoldFont;
                        break;
                    default:
                        f = renderInfo.RegularFont;
                        break;
                }
                var par = BlockParagraphLayoutEngine.LayoutParagraph(renderInfo.TextBox, f, line.words);

                foreach (var l in par)
                {
                    LayoutLines.Add((line.speaker.speakertext, l.words, l.width, l.height, l.linestart, l.fulltextonline));
                }

            }

        }



    }
}
