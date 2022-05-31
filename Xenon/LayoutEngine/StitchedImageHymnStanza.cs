using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;

namespace Xenon.LayoutEngine
{

    class StitchedImageHymnVerses
    {

        public List<StitchedImageHymnStanza> Verses { get; set; }
        public StitchedImageHymnStanza Refrain { get; set; }
        public bool RepeatingPostRefrain { get; set; }


        /// <summary>
        /// Checks if the hymn's verses and refrains total the provided number of source lines.
        /// </summary>
        /// <param name="lines">Number of lines the hymn should total</param>
        /// <returns>True if hymn passes sanity check.</returns>
        public bool PerformSanityCheck(int lines)
        {
            int hlines = ComputeSourceLinesUsed();
            return hlines == lines;
        }

        public int ComputeSourceLinesUsed()
        {
            int hlines = 0;
            // assumes all verses have same number of lines (this should be true according to how it was generated anyways)
            var v1 = Verses.First();
            hlines += v1.Lines + v1.Lines * Verses.Count();
            if (RepeatingPostRefrain)
            {
                hlines += Refrain.Lines * 2;
            }

            return hlines;
        }

        public List<LSBImageResource> OrderAllAsOne()
        {
            List<LSBImageResource> res = new List<LSBImageResource>();

            for (int linenum = 0; linenum < Verses.First().Lines; linenum++)
            {
                for (int versenum = 0; versenum < Verses.Count; versenum++)
                {
                    if (versenum == 0)
                    {
                        res.Add(Verses[0].GetLine(linenum).Music);
                    }
                    res.Add(Verses[versenum].GetLine(linenum).Text);
                }
            }
            if (RepeatingPostRefrain)
            {
                for (int linenum = 0; linenum < Refrain.Lines; linenum++)
                {
                    res.Add(Refrain.GetLine(linenum).Music);
                    res.Add(Refrain.GetLine(linenum).Text);
                }
            }
            return res;
        }

        public List<LSBImageResource> OrderRefrain()
        {
            List<LSBImageResource> res = new List<LSBImageResource>();
            if (RepeatingPostRefrain)
            {
                for (int linenum = 0; linenum < Refrain.Lines; linenum++)
                {
                    res.Add(Refrain.GetLine(linenum).Music);
                    res.Add(Refrain.GetLine(linenum).Text);
                }
            }
            return res;
        }

        public List<LSBImageResource> OrderVerse(int versenum)
        {
            List<LSBImageResource> res = new List<LSBImageResource>();
            if (versenum > Verses.Count)
            {
                return res;
            }

            for (int i = 0; i < Verses[versenum].Lines; i++)
            {
                res.Add(Verses[versenum].GetLine(i).Music);
                res.Add(Verses[versenum].GetLine(i).Text);
            }
            return res;
        }

    }


    class StitchedImageHymnStanza
    {
        public int Lines { get => pairedlines.Count; }
        public List<LSBImageResource> AllAsssets { get; }

        private List<LSBPairedHymnLine> pairedlines = new List<LSBPairedHymnLine>();

        public LSBPairedHymnLine GetLine(int line)
        {
            return pairedlines[line];
        }

        public StitchedImageHymnStanza(IEnumerable<LSBPairedHymnLine> lines)
        {
            pairedlines = lines.ToList();
        }

    }

    class LSBPairedHymnLine
    {
        public LSBImageResource Music { get; private set; }
        public LSBImageResource Text { get; private set; }

        public LSBPairedHymnLine(LSBImageResource music, LSBImageResource text)
        {
            Music = music;
            Text = text;
        }
    }

    class LSBImageResource
    {
        public string AssetRef { get; private set; }
        public SixLabors.ImageSharp.Size Size { get; private set; }
        //public LSBImageResource(string assetref, Size size)
        //{
            //AssetRef = assetref;
            //Size = size;
        //}
        public LSBImageResource(string assetref, SixLabors.ImageSharp.Size size)
        {
            AssetRef = assetref;
            // prevent muation by copying this
            Size = new SixLabors.ImageSharp.Size(size.Width, size.Height);
        }
    }

}
