using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using Xenon.Compiler.SubParsers;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.LayoutEngine.L2
{
    internal class ResponsiveLiturgyLayoutEngine
    {

        // TODO: define this somewhere better?
        static readonly string[] CALLERS = { "P", "A", "L", };
        static readonly string[] RESPONDERS = { "C", "R", };

        public List<List<SizedTextBlurb>> GenerateSlides(List<ResponsiveStatement> statements, LiturgyTextboxLayout layout)
        {

            List<List<SizedTextBlurb>> slides = new List<List<SizedTextBlurb>>();

            /*
                We're gauranteed this is the first rule we should do.
                This will always work- even if we can't fit the partitioned statements on 1 slide, they will need at least 1.
                We are also gauranteed that if we make further split the original split will still need to be preserved
             * /
            // Step 1. - Initial pass to split statements based on the following rule.
            /*
                    Call/Response
                        - Max 1 Switches between speakers
                        - If first speaker is not a caller then we can't have another speaker on the slide 
             */
            List<CandidateBlock> blocks = Rule1(statements, layout);

            // Step 2. - Split into multiple slides if it all doesn't fit
            /*
                    Greedy Fit
                        - if we can't git every word for every statement inside the textbox then we kick whole statements to a new slide
             */
            List<SizedCandidateBlock> sblocks = new List<SizedCandidateBlock>();
            foreach (var block in blocks)
            {
                sblocks.AddRange(Rule2(block, layout));
            }

            // Step 3. - Refinement
            /*
                    Weighted Fit
                        - perhaps its better to kick subsequent statements/ part of the current statement to new slides
             */

            // should be a couple of cases here...

            // 1. different speakers. In this case we need one type of algorithm that can also weight the cost/benefit of splitting taking into account
            //                          who's speaking at what point
            // 2. all one speaker. In this case we still don't actualy know the whole thing fits onto a slide. So we want a seperate algorithm that's tailored
            //                      to the specific case of same speaker. It will have to possible make multiple slides, but it will do so weighting soley on text/content

            // if we're going to allow for various horizontal spacing we need to do that here
            // (vertical spacing would be accounted for already)

            // TODO: implement here

            // OK- maybe I've decided that refining should place them as well.
            List<SizedCandidateBlock> refinedblocks = new List<SizedCandidateBlock>();
            foreach (var sblock in sblocks)
            {
                // refine it if necessary
                if (sblock.NumLines == -1)
                {
                    // it needed refinement, but it only has one line
                    slides.AddRange(PlaceBlurbsForSingleSpeaker(sblock, layout));
                }

                // for now don't mind bocks that already have a valid number of lines
                // (it probably wasn't optimal...) TODO: add refiner that handles that case
                // for now cheat and we'll use the greedy place for that
                else
                {
                    slides.Add(PlaceCandidateBlock(sblock, layout));
                }
            }

            // Finally - Slideify
            /*
                    Build a slide for each SizedCandidateBlock
                        - TODO: properly place each block
                            - for now they're just dumped in without thought to how they'll fit
             */
            //foreach (var block in refinedblocks)
            //{
            //    // at this point we should be gauranteed that it fit on the slide...
            //    // but perhaps there was a better way to do it????

            //    // create something that can be put into the slide
            //    // TODO: use alternate spacing if required (it will fit- but needs to be accounted for)
            //    slides.Add(PlaceCandidateBlock(block, layout));
            //}

            return slides;
        }

        static Dictionary<string, double> LineEndScores = new Dictionary<string, double>()
        {
            ["."] = 10,
            ["!"] = 10,
            ["?"] = 10,
            [","] = 5,
            [";"] = 8,
        };

        static Dictionary<string, double> LineStartScores = new Dictionary<string, double>()
        {

        };


        private List<List<SizedTextBlurb>> PlaceBlurbsForSingleSpeaker(SizedCandidateBlock block, LiturgyTextboxLayout layout)
        {
            List<List<SizedTextBlurb>> slides = new List<List<SizedTextBlurb>>();
            // determine the available width of the textbox
            float MAXWIDTH = layout.Textbox.Size.Width;
            // determine the available height
            float MAXHEIGHT = layout.Textbox.Size.Height;
            // all lines same height- use tallest height
            double lheight = block.MaxHeight;

            // at this point we know:

            // 1. the size of every word
            // 2. we have only the one speaker
            // 3. we'll need more than 1 slide to do the job

            // problem 1- don't know how many lines this will take
            // so we'll start building lines, adjusting for local optimization (word content)
            // then we can re-adjust
            // need some sort of algorith that operates on width's alone
            // do a greedy fill first, then shuffle to meet rules

            RollingLineArray lines = new RollingLineArray(MAXWIDTH - layout.SpeakerColumnWidth);

            foreach (var line in block.Lines)
            {
                lines.Append(line.Content);
                // do we want to force LSB's (or the users's) concept of lines to really == lines... ???
                // for now no. We'll just add a space and keep rolling
                //var newlinespace = new TextBlurb(Point.Empty, " ", TEXTBOX.Font.Name, (FontStyle)TEXTBOX.Font.Style, TEXTBOX.Font.Size, true, false); // perhaps mark as really new-line??
                //lines.Append(SizedTextBlurb.CreateMeasured(newlinespace, Graphics.FromHwnd(IntPtr.Zero), TEXTBOX.Font.Name, TEXTBOX.Font.Size, (FontStyle)TEXTBOX.Font.Style, TEXTBOX.Textbox.GetRectangleF()));
            }



            // optimize the actual layout of each line (may want to kick before end of line etc.)
            for (int i = 0; i < lines.Lines.Count; i++)
            {
                var line = lines.Lines[i];
                // go through the line and kick as necessary
                double lwidth = 0;
                double tminwidth = lines.LineWidth * 0.6;
                int j = 0;
                int bestj = -1;
                double bestjscore = 0;
                while (lwidth < tminwidth && j < line.Blurbs.Count)
                {
                    lwidth += line.Blurbs[j].Size.Width;
                    j++;
                }
                // see if there's a good candidate to kick onto a new line
                while (j < line.Blurbs.Count)
                {
                    var end = line.Blurbs[j].Text.LastOrDefault();
                    if (LineEndScores.ContainsKey(end.ToString()))
                    {
                        var score = LineEndScores[end.ToString()];
                        if (score > bestjscore) // prefer earliest match if scores equal (ie kick more)
                        {
                            bestj = j;
                            bestjscore = score;
                        }
                    }
                    j++;
                }
                // kick at best j
                if (bestj > 0)
                {
                    lines.KickandSpill(i, bestj + 1);
                }

            }


            // once we've optimized the lines we can trim whitespace
            foreach (var line in lines.Lines)
            {
                line.Trim();
            }


            // calculate max lines/slide
            double hpadding = layout.VPaddingEnabled ? layout.MinInterLineSpace * 2 : 0;
            int numlinesperslide = 0;
            double hrem = MAXHEIGHT - hpadding;
            while (hrem >= lheight)
            {
                numlinesperslide++;
                hrem -= lheight;
                hrem -= layout.MinInterLineSpace;
            }

            // compute number of slides required
            int slidesreq = (int)Math.Ceiling((double)lines.Lines.Count / numlinesperslide);

            // put lines on slides
            for (int i = 0; i < slidesreq; i++)
            {
                var lgroup = lines.Lines.Skip(i * numlinesperslide).Take((int)numlinesperslide).ToList();

                // compute interspacing
                int spacinglines = layout.VPaddingEnabled ? lgroup.Count + 1 : Math.Max(lgroup.Count - 1, 1);
                double interspacing = (layout.Textbox.Size.Height - (lheight * lgroup.Count)) / spacinglines;
                interspacing = Math.Max(interspacing, layout.MinInterLineSpace);

                // place every blurb
                double Yoff = layout.VPaddingEnabled ? interspacing : 0;
                double Xoff = 0;

                List<SizedTextBlurb> sblurbs = new List<SizedTextBlurb>();

                // place speaker only once per line (see assumption #2)
                if (layout.ShowSpeaker == true)
                {
                    var speaker = block.Lines.First().Speaker.Clone();
                    speaker.Place(new Point(layout.Textbox.Origin.X, (int)(layout.Textbox.Origin.Y + Yoff)));
                    sblurbs.Add(speaker);
                }

                foreach (var line in lgroup)
                {
                    // Start a new line
                    Xoff = 0;

                    Xoff = layout.SpeakerColumnWidth;

                    // place every piece of text on the line (should fit)
                    foreach (var word in line.Blurbs)
                    {
                        if (word.Size.Width + Xoff < MAXWIDTH)
                        {
                            word.Place(new Point((int)(layout.Textbox.Origin.X + Xoff), (int)(layout.Textbox.Origin.Y + Yoff)));
                            Xoff += word.Size.Width;
                        }
#if DEBUG
                        else
                        {
                            // really shouldn't get to here
                            Debugger.Break();
                        }
#endif
                        sblurbs.Add(word);
                    }

                    Yoff += interspacing + lheight;
                }

                slides.Add(sblurbs);

            }




            return slides;
        }

        private List<SizedTextBlurb> PlaceCandidateBlock(SizedCandidateBlock block, LiturgyTextboxLayout layout)
        {
            List<SizedTextBlurb> blurbs = new List<SizedTextBlurb>();

            // determine the available width of the textbox
            float MAXWIDTH = layout.Textbox.Size.Width;
            // determine the available height
            float MAXHEIGHT = layout.Textbox.Size.Height;
            // all lines same height- use tallest height
            double lheight = block.MaxHeight;


            // for now just stuff 'em in
            // we should be gauranteed that there is 'a' fit...
            // TODO: ability to apply other spacing modes

            // for now use equidistant line spacing
            // TODO: add spacing options to proto
            int spacinglines = layout.VPaddingEnabled ? block.NumLines + 1 : Math.Max(block.NumLines - 1, 1);
            double interspacing = (layout.Textbox.Size.Height - (lheight * block.NumLines)) / spacinglines;
            interspacing = Math.Max(interspacing, layout.MinInterLineSpace);

            double Yoff = layout.VPaddingEnabled ? interspacing : 0;
            double Xoff = 0;

            foreach (var line in block.Lines)
            {
                // Start a new line
                Xoff = 0;
                // place speaker
                if (layout.ShowSpeaker == true)
                {
                    line.Speaker.Place(new Point(layout.Textbox.Origin.X, (int)(layout.Textbox.Origin.Y + Yoff)));
                    blurbs.Add(line.Speaker);
                }

                Xoff = layout.SpeakerColumnWidth;

                // place every piece of text, wrap onto new lines a necessary
                foreach (var word in line.Content)
                {
                    if (word.Size.Width + Xoff < MAXWIDTH)
                    {
                        word.Place(new Point((int)(layout.Textbox.Origin.X + Xoff), (int)(layout.Textbox.Origin.Y + Yoff)));
                        Xoff += word.Size.Width;
                    }
                    else
                    {
                        // new line
                        Xoff = layout.SpeakerColumnWidth;
                        Yoff += interspacing + lheight;
                        word.Place(new Point((int)(layout.Textbox.Origin.X + Xoff), (int)(layout.Textbox.Origin.Y + Yoff)));
                        Xoff += word.Size.Width;
                    }
                    blurbs.Add(word);
                }

                Yoff += interspacing + lheight;
            }

            return blurbs;
        }


        /// <summary>
        /// Rule 2: First pass line layout. If a line can't fully fit, kick the whole line. 
        /// </summary>
        private List<SizedCandidateBlock> Rule2(CandidateBlock block, LiturgyTextboxLayout layout)
        {
            List<SizedCandidateBlock> sblocks = new List<SizedCandidateBlock>();

            // determine the available width of the textbox
            float MAXWIDTH = layout.Textbox.Size.Width;
            // determine the available height
            float MAXHEIGHT = layout.Textbox.Size.Height;

            // now compute the height of every word in the the block's lines
            SizedCandidateBlock sblock;
            sblock = SizedCandidateBlock.CreateSized(block, layout);

            // use a greed stuffing check to see what the 'maximum' quantity of words we can stuff in is

            // all we're checking here is if we can already detect if we need to kick a 'whole' statement- ie. it would go over anyways
            // compute how many physical lines (ie. height) is minimally required for each line
            int linesused = 0;
            foreach (SizedResponsiveStatement line in sblock.Lines)
            {
                linesused++;
                double widthused = line.Speaker.Size.Width + layout.SpeakerColumnWidth;

                foreach (var w in line.Content)
                {
                    if (widthused + w.Size.Width < MAXWIDTH)
                    {
                        widthused += w.Size.Width;
                    }
                    else
                    {
                        // we need a new line
                        linesused++;
                        widthused = line.Speaker.Size.Width + layout.SpeakerColumnWidth + w.Size.Width;
                    }
                }
            }

            // if we discover that all the lines's total heights don't fit then we'd put the lines in seperate blocks
            double lheight = sblock.MaxHeight;
            double minheightreq = linesused * lheight
                + Math.Max(linesused - 1, 1) * layout.MinInterLineSpace
                + (layout.VPaddingEnabled ? layout.MinInterLineSpace * 2 : 0);

            if (minheightreq > MAXHEIGHT)
            {
                // split block
                // at this point we're gauranteed 1 speaker per slide
                // so hand it off to something that can more intelligenetly handle that case

                // in the case we've got a bunch of singe lines...
                // may want to coalless lines by same speaker:
                // we'll do that here
                if (layout.SameSpeakerLineCoalescing)
                {
                    GroupBySpeaker(sblocks, sblock, layout);
                }
                else
                {
                    sblocks.AddRange(sblock.Lines.Select(x => new SizedCandidateBlock()
                    {
                        Lines = new List<SizedResponsiveStatement>() { x },
                        NumLines = -1, // TODO: need to recalculate how many lines this actually uses (this will be done by something that actually understands how to solve that part of the problem)
                    }));
                }
            }
            else
            {
                // one block
                sblock.NumLines = linesused;
                sblocks.Add(sblock);
            }

            return sblocks;
        }

        private static void GroupBySpeaker(List<SizedCandidateBlock> sblocks, SizedCandidateBlock sblock, LiturgyTextboxLayout layout)
        {
            List<List<SizedResponsiveStatement>> groupedBySpeaker = new List<List<SizedResponsiveStatement>>();
            List<SizedResponsiveStatement> linesBySpeaker = new List<SizedResponsiveStatement>();

            string lastspeaker = sblock.Lines.FirstOrDefault()?.Speaker.Text ?? "";
            int i = 0;
            foreach (var line in sblock.Lines)
            {
                if (line.Speaker.Text == lastspeaker)
                {
                    linesBySpeaker.Add(line);
                }
                else
                {
                    groupedBySpeaker.Add(linesBySpeaker);
                    linesBySpeaker = new List<SizedResponsiveStatement>();
                    linesBySpeaker.Add(line);
                }

                if (i < sblock.Lines.Count - 1)
                {
                    // add 2 spaces between each 'line'
                    LWJFont f = layout.LineFonts.GetOrDefault(line.Speaker.Text, layout.Font);
                    SizedTextBlurb space = SizedTextBlurb.CreateMeasured(new TextBlurb(Point.Empty, "  ", space: true), f.Name, f.Size, (System.Drawing.FontStyle)f.Style, Rectangle.Empty);
                    line.Content.Add(space);
                }


                lastspeaker = line.Speaker.Text;
                i++;
            }
            if (linesBySpeaker.Any())
            {
                groupedBySpeaker.Add(linesBySpeaker);
            }

            sblocks.AddRange(groupedBySpeaker.Select(x => new SizedCandidateBlock
            {
                Lines = x,
                NumLines = -1 // we don't really know how many 'layout lines this is'. So we'll mark it unknown (-1) and let further algorithms handle it
            }));
        }



        /// <summary>
        /// Rule 1: Call/Response. Max 1 Switches of speaker per slide. If first speaker is not caller then no other speakers allowed.
        /// </summary>
        private List<CandidateBlock> Rule1(List<ResponsiveStatement> lines, LiturgyTextboxLayout layout)
        {
            List<CandidateBlock> blocks = new List<CandidateBlock>();

            string lastspeaker = "";
            int speakers = 0;

            CandidateBlock block = new CandidateBlock();
            foreach (var line in lines)
            {
                if (lastspeaker != line.Speaker)
                {
                    speakers++;
                }
                if (speakers > layout.MaxSpeakers ||
                    (!CALLERS.Contains(lastspeaker) && CALLERS.Contains(line.Speaker)).OptionalFalse(layout.EnforceCallResponse)
                    )
                {
                    // NEW BLOCK
                    lastspeaker = " ";
                    speakers = 1;
                    if (block.Lines.Any())
                    {
                        blocks.Add(block);
                    }
                    block = new CandidateBlock();
                }
                // add to block
                lastspeaker = line.Speaker;
                block.Lines.Add(line);
            }
            if (block.Lines.Any() && blocks.LastOrDefault() != block)
            {
                blocks.Add(block);
            }

            return blocks;
        }


    }
}
