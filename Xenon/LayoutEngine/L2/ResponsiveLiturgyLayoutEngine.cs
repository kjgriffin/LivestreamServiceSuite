using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<List<SizedTextBlurb>> GenerateSlides(List<ResponsiveStatement> statements, ResponsiveLiturgySlideLayoutInfo layout)
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

        private List<List<SizedTextBlurb>> PlaceBlurbsForSingleSpeaker(SizedCandidateBlock block, ResponsiveLiturgySlideLayoutInfo layout)
        {
            List<List<SizedTextBlurb>> slides = new List<List<SizedTextBlurb>>();
            // at this point we know:

            // 1. the size of every word
            // 2. we have only the one speaker
            // 3. we'll need more than 1 slide to do the job




            return slides;
        }

        private List<SizedTextBlurb> PlaceCandidateBlock(SizedCandidateBlock block, ResponsiveLiturgySlideLayoutInfo layout)
        {
            List<SizedTextBlurb> blurbs = new List<SizedTextBlurb>();
            TextboxLayout TEXTBOX = layout.Textboxes.FirstOrDefault() ?? ResponsiveLiturgyLayoutInfoPrototyptes.TEXTBOX;

            // determine the available width of the textbox
            float MAXWIDTH = layout.Textboxes.FirstOrDefault()?.Textbox.Size.Width ?? 0;
            // determine the available height
            float MAXHEIGHT = layout.Textboxes.FirstOrDefault()?.Textbox.Size.Height ?? 0;
            // all lines same height- use tallest height
            double lheight = block.MaxHeight;


            // for now just stuff 'em in
            // we should be gauranteed that there is 'a' fit...
            // TODO: ability to apply other spacing modes

            // for now use equidistant line spacing
            // TODO: add spacing options to proto
            int spacinglines = layout.Textboxes.First().VPaddingEnabled ? block.NumLines + 1 : Math.Max(block.NumLines - 1, 1);
            double interspacing = (TEXTBOX.Textbox.Size.Height - (lheight * block.NumLines)) / spacinglines;
            interspacing = Math.Max(interspacing, layout.Textboxes.First().MinInterLineSpace);

            double Yoff = layout.Textboxes.First().VPaddingEnabled ? interspacing : 0;
            double Xoff = 0;

            foreach (var line in block.Lines)
            {
                // Start a new line
                Xoff = 0;
                // place speaker
                line.Speaker.Place(new Point(TEXTBOX.Textbox.Origin.X, (int)(TEXTBOX.Textbox.Origin.Y + Yoff)));
                blurbs.Add(line.Speaker);

                Xoff = layout.Textboxes.First().SpeakerColumnWidth;

                // place every piece of text, wrap onto new lines a necessary
                foreach (var word in line.Content)
                {
                    if (word.Size.Width + Xoff < MAXWIDTH)
                    {
                        word.Place(new Point((int)(TEXTBOX.Textbox.Origin.X + Xoff), (int)(TEXTBOX.Textbox.Origin.Y + Yoff)));
                        Xoff += word.Size.Width;
                    }
                    else
                    {
                        // new line
                        Xoff = layout.Textboxes.First().SpeakerColumnWidth;
                        Yoff += interspacing + lheight;
                        word.Place(new Point((int)(TEXTBOX.Textbox.Origin.X + Xoff), (int)(TEXTBOX.Textbox.Origin.Y + Yoff)));
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
        private List<SizedCandidateBlock> Rule2(CandidateBlock block, ResponsiveLiturgySlideLayoutInfo layout)
        {
            List<SizedCandidateBlock> sblocks = new List<SizedCandidateBlock>();

            TextboxLayout TEXTBOX = layout.Textboxes.FirstOrDefault() ?? ResponsiveLiturgyLayoutInfoPrototyptes.TEXTBOX;

            // determine the available width of the textbox
            float MAXWIDTH = layout.Textboxes.FirstOrDefault()?.Textbox.Size.Width ?? 0;
            // determine the available height
            float MAXHEIGHT = layout.Textboxes.FirstOrDefault()?.Textbox.Size.Height ?? 0;

            // now compute the height of every word in the the block's lines
            SizedCandidateBlock sblock;
            using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero))
            {
                sblock = SizedCandidateBlock.CreateSized(block, gfx, layout);
            }

            // use a greed stuffing check to see what the 'maximum' quantity of words we can stuff in is

            // all we're checking here is if we can already detect if we need to kick a 'whole' statement- ie. it would go over anyways
            // compute how many physical lines (ie. height) is minimally required for each line
            int linesused = 0;
            foreach (SizedResponsiveStatement line in sblock.Lines)
            {
                linesused++;
                double widthused = line.Speaker.Size.Width + layout.Textboxes.First().SpeakerColumnWidth;

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
                        widthused = line.Speaker.Size.Width + layout.Textboxes.First().SpeakerColumnWidth + w.Size.Width;
                    }
                }
            }

            // if we discover that all the lines's total heights don't fit then we'd put the lines in seperate blocks
            double lheight = sblock.MaxHeight;
            double minheightreq = linesused * lheight
                + Math.Max(linesused - 1, 1) * layout.Textboxes.First().MinInterLineSpace
                + (layout.Textboxes.First().VPaddingEnabled ? layout.Textboxes.First().MinInterLineSpace * 2 : 0);

            if (minheightreq > MAXHEIGHT)
            {
                // split block
                // at this point we're gauranteed 1 speaker per slide
                // so hand it off to something that can more intelligenetly handle that case
                sblocks.AddRange(sblock.Lines.Select(x => new SizedCandidateBlock()
                {
                    Lines = new List<SizedResponsiveStatement>() { x },
                    NumLines = -1, // TODO: need to recalculate how many lines this actually uses (this will be done by something that actually understands how to solve that part of the problem)
                }));
            }
            else
            {
                // one block
                sblock.NumLines = linesused;
                sblocks.Add(sblock);
            }

            return sblocks;
        }



        /// <summary>
        /// Rule 1: Call/Response. Max 1 Switches of speaker per slide. If first speaker is not caller then no other speakers allowed.
        /// </summary>
        private List<CandidateBlock> Rule1(List<ResponsiveStatement> lines, ResponsiveLiturgySlideLayoutInfo layout)
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
                if (speakers > layout.Textboxes.First().MaxSpeakers || (!CALLERS.Contains(lastspeaker)).Optional(layout.Textboxes.First().EnforceCallResponse))
                {
                    // NEW BLOCK
                    lastspeaker = "";
                    speakers = 0;
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
