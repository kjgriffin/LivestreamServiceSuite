using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SlideCreater.LayoutEngine
{
    public class BlockParagraphLayoutEngine
    {

        static public List<(List<string> words, float width, float height, bool linestart, bool fulltextonline)> LayoutParagraph(Rectangle textblock, Font font, List<string> words)
        {
            List<(List<string>, float, float, bool, bool)> res = new List<(List<string>, float, float, bool, bool)>();
            Bitmap layoutbmp = new Bitmap(textblock.Width, textblock.Height);
            Graphics gfx = Graphics.FromImage(layoutbmp);
            StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

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
                res.Add((linewords.Take(wordcount-1).ToList(), measure.Width, measure.Height, startwords == 0, wordcount - 1 == words.Count));

                startwords += wordcount - 1;
            }

            return res;

        }
    }
}
