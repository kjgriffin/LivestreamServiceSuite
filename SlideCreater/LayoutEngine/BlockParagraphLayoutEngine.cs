using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SlideCreater.LayoutEngine
{
    public class BlockParagraphLayoutEngine
    {

        static public List<(List<string> words, float width, float height)> LayoutParagraph(Rectangle textblock, Font font, List<string> words)
        {
            List<(List<string>, float, float)> res = new List<(List<string>, float, float)>();
            Bitmap layoutbmp = new Bitmap(textblock.Width, textblock.Height);
            Graphics gfx = Graphics.FromImage(layoutbmp);

            int startwords = 0;

            while (startwords < words.Count)
            {
                int wordcount = 1;
                // split into as many layout lines as required. uses word level granularity
                var linewords = words.Skip(startwords).Take(wordcount);
                var measure = gfx.MeasureString(string.Join("", linewords), font);
                while (measure.Width < textblock.Width && wordcount <= words.Count)
                {
                    wordcount++;
                    linewords = words.Skip(startwords).Take(wordcount);
                    measure = gfx.MeasureString(string.Join("", linewords), font);
                }
                // this many words fit into the sentence
                res.Add((linewords.ToList(), measure.Width, measure.Height));

                startwords += wordcount;
            }

            return res;

        }
    }
}
