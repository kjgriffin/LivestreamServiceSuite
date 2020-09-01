using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Xenon.LayoutEngine
{
    class BlockParagraphLayoutEngine
    {

        static public List<BlockParagraph> LayoutParagraph(Rectangle textblock, Font font, List<string> words)
        {
            List<BlockParagraph> res = new List<BlockParagraph>();
            Bitmap layoutbmp = new Bitmap(textblock.Width, textblock.Height);
            Graphics gfx = Graphics.FromImage(layoutbmp);
            StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

            List<string> illegalLinestarts = new List<string>() { "!", "?", ":", "-", ".", ",", ";" };

            int startwords = 0;

            while (startwords < words.Count)
            {
                int wordcount = 1;
                // split into as many layout lines as required. uses word level granularity
                var linewords = words.Skip(startwords).Take(wordcount);
                var measure = gfx.MeasureString(string.Join("", linewords), font, textblock.Width, sf);
                while (measure.Width < textblock.Width && startwords + wordcount <= words.Count)
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
