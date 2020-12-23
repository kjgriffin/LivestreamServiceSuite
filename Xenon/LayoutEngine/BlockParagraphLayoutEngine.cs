using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Xenon.Helpers;
using Xenon.LiturgyLayout;

namespace Xenon.LayoutEngine
{
    class BlockParagraphLayoutEngine
    {

        static public List<BlockParagraph> LayoutVerseParagraph(Rectangle textblock, Font font, List<List<string>> lines)
        {
            List<BlockParagraph> res = new List<BlockParagraph>();

            Bitmap layoutbmp = new Bitmap(textblock.Width, textblock.Height);
            Graphics gfx = Graphics.FromImage(layoutbmp);
            StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

            float zoomx = (float)gfx.DpiX / 96f;
            float zoomy = (float)gfx.DpiY / 96f;

            /*
                Try to put one line / line. If needed wrap lines
             */

            foreach (var line in lines)
            {
                int startwords = 0;
                int wordcount = 1;
                var measure = gfx.MeasureString(string.Join("", line.Skip(startwords).Take(wordcount)), font, textblock.Width, sf);
                while (startwords < line.Count)
                {
                    wordcount++;

                }
            }

            return res;

        }

        static Font fregular = new Font("Arial", 36, FontStyle.Regular);
        static Font fbold = new Font("Arial", 36, FontStyle.Bold);
        static Font fitalic = new Font("Arial", 36, FontStyle.Italic);

        static Font flsbregular = new Font("LSBSymbol", 36, FontStyle.Regular);
        static Font flsbbold = new Font("LSBSymbol", 36, FontStyle.Bold);
        static Font flsbitalic = new Font("LSBSymbol", 36, FontStyle.Italic);

        static public List<LiturgyTextLine> LayoutLiturgyIntoTextLines(Rectangle textbox, LiturgyLine line)
        {
            List<LiturgyTextLine> textlines = new List<LiturgyTextLine>();

            Bitmap layoutbmp = new Bitmap(textbox.Width, textbox.Height);
            Graphics gfx = Graphics.FromImage(layoutbmp);

            StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

            List<string> illegalLineStarts = new List<string>() { "!", "?", ":", "-", ".", ",", ";", "'", "\"", "”" };

            float zoomx = (float)gfx.DpiX / 96f;
            float zoomy = (float)gfx.DpiY / 96f;


            // measure each word individually to generate list of 'words' that have a size
            foreach (var word in line.Words)
            {
                Font f = word.IsLSBSymbol ? (word.IsBold ? flsbbold : flsbregular) : (word.IsBold ? fbold : fregular);
                var size = gfx.MeasureStringCharacters(word.Value, f, textbox);
                // adjust for dpi scaling of text
                word.Size = new SizeF(size.Width * zoomx, size.Height * zoomy);
                word.IsSized = true;
            }

            // do linebreaking of words now that all have known measures

            /*
            Line breaking algorithm. Normalized fill.
            //Defines a minfilltospill eg 80%. ie to get text to split onto a second line we need to make sure that at least 80% of the first line's width is filled
            //Defines a minfillforspill eg 20% ie if text needs to be spilled we need to the second line to be the minimun of 20% the full width. (may need to steal from the prev line)
            perhaps though (to prevent edge cases) we can just have one value
            Defines a spillpercent eg 20% if we do spill we need to fill min 20% of spillover line. This means we can steal up to 20% of the fill from the previous line.

            Defines a list of illegal line starts.

            To do all this we'll just use a goodness factor on each word (calculated by how bad it would be to end the line on this word)
            goodness = (some function of how long the line is) * (some function of how bad this word is to end a line on) * (some function of how bad the next word is to start a line with) * (some function of how balanced all the lines are)

            To determine the target width of a line we need to know approx how the line should be split (purley on evenness)
            We will assume that the line should be balanced so that all lines (except the last spilled line) are approx equal size and the last line still meets the min length
             */

            double spillfactor = 0.3;
            double targetwidth = ComputeApproxLineTargetWidth(textbox.Width, spillfactor, line.Words);

            // compute goondess of breaking the line after this word
            // 1. check if the width of the line up to this word is close to target.
            // 2. check if this word is ok to end on
            // 3. check if the following word is ok to start on

            // do these steps on the whole line,
            // pick the best split
            // remove these words into a finalized line
            // keep doing this until the lines are split

            List<List<LiturgyWord>> splitlines = new List<List<LiturgyWord>>();
            List<LiturgyWord> remainingline = new List<LiturgyWord>();
            foreach (var word in line.Words)
            {
                remainingline.Add(word);
            }

            while (remainingline.Count > 0)
            {

                // prevent starting a line with whitespace
                if (Regex.Match(remainingline.First().Value, "\\s").Success)
                {
                    remainingline.RemoveAt(0);
                }

                List<(double score, LiturgyWord word)> scoredwords = new List<(double score, LiturgyWord word)>();
                float lwidth = 0;
                var wordstoscore = GetWordsThatFitOnLine(textbox.Width, remainingline);
                for (int i = 0; i < wordstoscore.Count; i++)
                {
                    LiturgyWord word = wordstoscore[i];
                    lwidth += word.Size.Width;
                    double widthdiff = (targetwidth - lwidth) / targetwidth;
                    double nextwordscore = 1;
                    double endbounus = 0;
                    if (i + 1 < remainingline.Count)
                    {
                        if (illegalLineStarts.Contains(remainingline[i + 1].Value))
                        {
                            // can't break line here
                            nextwordscore = 0;
                        }
                    }

                    // for now we'll add a bounus for ending a line with an illegal start
                    if (illegalLineStarts.Contains(word.Value))
                    {
                        endbounus = 1;
                    }

                    // compute score for word
                    // scoring is
                    // (1-worddiff) [0-1] * weight
                    // endbonus [0-1] * weight
                    // nextwordscore
                    double targetwidthweight = 1;
                    double endbonusweight = 0.2;

                    double score = (((1 - widthdiff) * targetwidthweight) + (endbounus * endbonusweight)) * nextwordscore;
                    scoredwords.Add((score, word));
                }


                // having now scored every word we can find the best end word
                int endwordindex = 0;
                double max = 0;
                int index = 0;
                foreach (var word in scoredwords)
                {
                    if (word.score > max)
                    {
                        max = word.score;
                        endwordindex = index;
                    }
                    index += 1;
                }

                // put every word including end word into a line
                // remove these words from remaining words
                List<LiturgyWord> linewords = new List<LiturgyWord>();
                for (int i = 0; i <= endwordindex; i++)
                {
                    linewords.Add(scoredwords[i].word);
                    remainingline.RemoveAt(0);
                }
                splitlines.Add(linewords);
            }


            for (int i = 0; i < splitlines.Count; i++)
            {
                textlines.Add(new LiturgyTextLine()
                {
                    Speaker = line.SpeakerText,
                    Width = ComputeWidth(splitlines[i]),
                    Height = ComputeHeight(splitlines[i]),
                    StartOfParagraph = i == 0,
                    EndOfParagraph = i == splitlines.Count - 1,
                    Words = splitlines[i]
                });
            }

            return textlines;
        }

        private static float ComputeApproxLineTargetWidth(float fullwidth, double spillpercent, List<LiturgyWord> linewords)
        {
            float linewidth = ComputeWidth(linewords);
            // compute the length of the entire 'line' of words
            // figure out max number of lines this will take
            int maxlines = (int)Math.Ceiling(linewidth / fullwidth);

            if (maxlines <= 1)
            {
                return linewidth;
            }
            // figure out average line width
            float targetwidth = fullwidth;

            // adjust target width to make spill line meet min target
            if (linewidth - (targetwidth * (maxlines - 1)) < targetwidth * spillpercent)
            {
                double buffwidth = (targetwidth * spillpercent) - (linewidth - (targetwidth * (maxlines - 1)));
                float stealwidth = (float)(buffwidth / (maxlines - 1));
                return targetwidth - stealwidth;
            }
            return targetwidth;
        }

        private static List<LiturgyWord> GetWordsThatFitOnLine(float maxwidth, List<LiturgyWord> words)
        {
            List<LiturgyWord> res = new List<LiturgyWord>();
            float width = 0;
            foreach (var word in words)
            {
                if (width + word.Size.Width <= maxwidth)
                {
                    width += word.Size.Width;
                    res.Add(word);
                }
                else
                {
                    return res;
                }
            }
            return res;
        }

        private static float ComputeWidth(List<LiturgyWord> words)
        {
            float width = 0;
            foreach (var word in words)
            {
                width += word.Size.Width;
            }
            return width;
        }

        public static float ComputeHeight(List<LiturgyWord> words)
        {
            float height = 0;
            foreach (var word in words)
            {
                if (word.Size.Height > height)
                {
                    height = word.Size.Height;
                }
            }
            return height;
        }

        static public List<BlockParagraph> LayoutParagraph(Rectangle textblock, Font font, List<string> words)
        {
            List<BlockParagraph> res = new List<BlockParagraph>();
            Bitmap layoutbmp = new Bitmap(textblock.Width, textblock.Height);
            Graphics gfx = Graphics.FromImage(layoutbmp);
            StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

            List<string> illegalLinestarts = new List<string>() { "!", "?", ":", "-", ".", ",", ";" };


            float zoomx = (float)gfx.DpiX / 96f;
            float zoomy = (float)gfx.DpiY / 96f;

            int startwords = 0;

            while (startwords < words.Count)
            {
                int wordcount = 1;
                // split into as many layout lines as required. uses word level granularity
                var linewords = words.Skip(startwords).Take(wordcount);
                var measure = gfx.MeasureString(string.Join("", linewords), font, textblock.Width, sf);
                while ((measure.Width * zoomx) < textblock.Width && startwords + wordcount <= words.Count)
                {
                    wordcount++;
                    linewords = words.Skip(startwords).Take(wordcount);
                    measure = gfx.MeasureString(string.Join("", linewords), font);
                }
                // this many words fit into the sentence

                // prevent starting new lines from 
                if (startwords + wordcount <= words.Count)
                {
                    if (illegalLinestarts.Contains(words[startwords + wordcount - 1]))
                    {
                        wordcount -= 1;
                    }
                }

                res.Add((linewords.Take(wordcount - 1).ToList(), measure.Width, measure.Height, startwords == 0, wordcount - 1 == words.Count));

                startwords += wordcount - 1;
            }

            Debug.WriteLine($"Foramtting\n{string.Join(System.Environment.NewLine, words)}\nwith: {font} in {textblock}\n[produced]");
            foreach (var b in res)
            {
                Debug.WriteLine(b);
            }

            return res;

        }
    }

    struct LiturgyTextLine
    {
        public List<LiturgyWord> Words { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Speaker { get; set; }
        public bool StartOfParagraph { get; set; }
        public bool EndOfParagraph { get; set; }
        public bool MultilineParagraph { get => !(StartOfParagraph && EndOfParagraph); }

        public override string ToString()
        {
            return string.Join("", Words.Select(p => p.Value));
        }


        public List<(string Text, bool IsBold, bool IsLSBSymbol, float Width)> GetTextChunks()
        {
            List<(string Text, bool IsBold, bool IsLSBSymbol, float Width)> Chunks = new List<(string Text, bool IsBold, bool IsLSBSymbol, float Width)>();

            StringBuilder sb = new StringBuilder();

            List<LiturgyWord> remainingwords = new List<LiturgyWord>();
            foreach (var word in Words)
            {
                remainingwords.Add(word);
            }

            while (remainingwords.Count > 0)
            {
                int wordsinchunk = 0;
                float width = 0;
                bool startwithbold = remainingwords.First().IsBold;
                bool startwithlsbsymbol = remainingwords.First().IsLSBSymbol;

                foreach (var word in remainingwords)
                {
                    if (word.IsLSBSymbol != startwithlsbsymbol || word.IsBold != startwithbold)
                    {
                        // create new chunk
                        Chunks.Add((sb.ToString(), startwithbold, startwithlsbsymbol, width));
                        sb.Clear();
                        break;
                    }
                    wordsinchunk += 1;
                    width += word.Size.Width;
                    sb.Append(word.Value);
                }

                for (int i = 0; i < wordsinchunk; i++)
                {
                    remainingwords.RemoveAt(0);
                }

                if (!string.IsNullOrWhiteSpace(sb.ToString()))
                {
                    Chunks.Add((sb.ToString(), startwithbold, startwithlsbsymbol, width));
                }

            }

            return Chunks;
        }

    }

    public struct BlockParagraph
    {
        public List<string> words;
        public float width;
        public float height;
        public bool linestart;
        public bool fulltextonline;

        public BlockParagraph(List<string> words, float width, float height, bool linestart, bool fulltextonline)
        {
            this.words = words;
            this.width = width;
            this.height = height;
            this.linestart = linestart;
            this.fulltextonline = fulltextonline;
        }

        public override string ToString()
        {
            return $"[block paragraph] <{string.Join(' ', words)}>";
        }

        public override bool Equals(object obj)
        {
            return obj is BlockParagraph other &&
                   EqualityComparer<List<string>>.Default.Equals(words, other.words) &&
                   width == other.width &&
                   height == other.height &&
                   linestart == other.linestart &&
                   fulltextonline == other.fulltextonline;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(words, width, height, linestart, fulltextonline);
        }

        public void Deconstruct(out List<string> words, out float width, out float height, out bool linestart, out bool fulltextonline)
        {
            words = this.words;
            width = this.width;
            height = this.height;
            linestart = this.linestart;
            fulltextonline = this.fulltextonline;
        }

        public static implicit operator (List<string> words, float width, float height, bool linestart, bool fulltextonline)(BlockParagraph value)
        {
            return (value.words, value.width, value.height, value.linestart, value.fulltextonline);
        }

        public static implicit operator BlockParagraph((List<string> words, float width, float height, bool linestart, bool fulltextonline) value)
        {
            return new BlockParagraph(value.words, value.width, value.height, value.linestart, value.fulltextonline);
        }
    }
}
