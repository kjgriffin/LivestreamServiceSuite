using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTStitchedHymn : IXenonASTCommand
    {

        public List<string> ImageAssets { get; set; } = new List<string>();
        public string Title { get; set; }
        public string HymnName { get; set; }
        public string Number { get; set; }
        public string CopyrightInfo { get; set; }
        public bool StitchAll { get; set; }
        public IXenonASTElement Parent { get; private set; }

        private string CopyrightTune
        {
            get
            {
                var split = CopyrightInfo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3)
                {
                    return "Tune: " + split[0];
                }
                else
                {
                    return CopyrightInfo;
                }
            }
        }

        private string CopyrightText
        {
            get
            {
                var split = CopyrightInfo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3)
                {
                    return "Text: " + split[2];
                }
                else
                {
                    return CopyrightInfo;
                }
            }
        }



        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTStitchedHymn hymn = new XenonASTStitchedHymn();

            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "name", "number", "copyright");

            hymn.Title = args["title"];
            hymn.HymnName = args["name"];
            hymn.Number = args["number"];
            hymn.CopyrightInfo = args["copyright"];

            Lexer.GobbleWhitespace();

            if (Lexer.Inspect("("))
            {
                var optionalparams = Lexer.ConsumeArgList(false, "stitchall");
                if (optionalparams["stitchall"] == "stitchall")
                {
                    hymn.StitchAll = true;
                }
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleandLog("{", "Expected opening '{'");
            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                Lexer.GobbleWhitespace();
                string assetline = Lexer.ConsumeUntil(";");
                hymn.ImageAssets.Add(assetline);
                Lexer.GobbleandLog(";", "Expected ';' at end of asset dependency");
                Lexer.GobbleWhitespace();
            }
            Lexer.GobbleandLog("}", "Expected closing '}'");

            hymn.Parent = Parent;
            return hymn;
        }
        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]);
            sb.Append($"(\"{Title}\", \"{HymnName}\", \"{Number}\", \"{CopyrightInfo}\")");

            if (StitchAll)
            {
                sb.Append("(stitchall)");
            }

            sb.AppendLine();
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("{");
            indentDepth++;

            foreach (var asset in ImageAssets)
            {
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.Append(asset);
                sb.AppendLine(";");
            }

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }


        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> slides = new List<Slide>();
            Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating StitchedHymn {HymnName}", ErrorName = "Generation Debug Log", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            // main steps
            // 1. Figure out how many lines/stanzas and how many stanzas there are
            // 2. Figure out if we can squish everything on one slide, or if we need to go stanza by stanza

            // 1. Go through every asset and check its size.
            //      if its height is less than 45px its text, if its more than 85 its music
            //      might need to be inefficient here and open the file to check the height. Don't think we've got that info yet

            Dictionary<string, SixLabors.ImageSharp.Size> ImageSizes = new Dictionary<string, SixLabors.ImageSharp.Size>();

            foreach (var item in ImageAssets)
            {
                string assetpath = project.Assets.Find(a => a.Name == item)?.CurrentPath ?? "";
                try
                {
                    if (!string.IsNullOrEmpty(assetpath))
                    {

                        SixLabors.ImageSharp.IImageInfo metadata = SixLabors.ImageSharp.Image.Identify(assetpath);
                        ImageSizes[item] = new SixLabors.ImageSharp.Size(metadata.Width, metadata.Height);
                    }
                    else
                    {
                        Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Generating StitchedHymn", ErrorName = "Failed to load image asset", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"{item}", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
                        Debug.WriteLine($"Error opening image to check size: {item}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Generating StitchedHymn", ErrorName = "Failed to load image asset", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"{ex}", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
                    Debug.WriteLine($"Error opening image to check size: {ex}");
                    // hmmm....
                }
            }

            // create stanzainfo for everything
            // loop over every image
            // when we find a music line we find all text related to it (everything after until next music line)
            // create a bunch of verse lines for everything
            // when we get to end build into stanzas
            // this doesn't account for refrains.... we can detect refrain by height as well (even taller for 'Refrain' text included)
            // not sure how to detect how long refrain is... might be able to assume refrains have only one line, and stanzas would have multiple...
            // also not sure how to handle optional refrains....
            // refrains at the end can just all be refrain??
            // refrains at the beginnnig will be harder
            // expecially hymns that repeat it again after last verse too...
            // ....
            // perhaps we'll do a best effort (focused on making the default [read: no-refrain] cases work)
            // we'll try sorting out refrains - but we'll flag it, and add a comment line for manual inspection

            if (StitchAll)
            {
                // just a bunch of image lines...
                // can stop right here and build a slide
                Slide slide = new Slide();
                slide.Name = $"stitchedhymn";
                slide.Number = project.NewSlideNumber;
                slide.Asset = "";
                slide.Lines = new List<SlideLine>();
                slide.Format = SlideFormat.StitchedImage;
                slide.MediaType = MediaType.Image;

                slide.Data[StitchedImageRenderer.DATAKEY_TITLE] = Title;
                slide.Data[StitchedImageRenderer.DATAKEY_NAME] = HymnName;
                slide.Data[StitchedImageRenderer.DATAKEY_NUMBER] = Number;
                slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT] = CopyrightInfo;
                if (CopyrightText != CopyrightTune)
                {
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TEXT] = CopyrightText;
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TUNE] = CopyrightTune;
                }

                slide.Data[StitchedImageRenderer.DATAKEY_IMAGES] = ImageSizes.Select(i => new LSBImageResource(i.Key, i.Value)).ToList();

                slide.AddPostset(_Parent, true, true);

                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.StitchedImage);
                //project.Slides.Add(slide);
                slides.Add(slide);
                WarnVerseOverheightStitchAll(Logger, ImageSizes);

                return slide.ToList();

            }

            bool unconfidentaboutlinetype = false;
            LSBImageResource linemusic = null;
            List<LSBImageResource> linetexts = new List<LSBImageResource>();
            List<(LSBImageResource music, List<LSBImageResource> words)> CollatedLines = new List<(LSBImageResource music, List<LSBImageResource> words)>();
            for (int i = 0; i < ImageAssets.Count; i++)
            {
                // check for new music line
                if (ImageSizes[ImageAssets[i]].Height > 95)
                {
                    if (ImageSizes[ImageAssets[i]].Height > 130 && i == 0)
                    {
                    }
                    // add previous lines
                    if (linemusic != null)
                    {
                        CollatedLines.Add((linemusic, linetexts));
                    }

                    // create new stuff
                    linemusic = new LSBImageResource(ImageAssets[i], ImageSizes[ImageAssets[i]]);
                    linetexts = new List<LSBImageResource>();
                }
                // is text. attach to previous line
                else if (ImageSizes[ImageAssets[i]].Height < 45)
                {
                    linetexts.Add(new LSBImageResource(ImageAssets[i], ImageSizes[ImageAssets[i]]));
                }
                else
                {
                    unconfidentaboutlinetype = true;
                }
            }
            if (linemusic != null)
            {
                CollatedLines.Add((linemusic, linetexts));
            }

            if (unconfidentaboutlinetype)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident about linetype for hymn {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            }
            //if (unconfidentaboutlinetype)
            //{
            //Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident about refrain for hymn {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", int.MaxValue) });
            //}

            // now we go through collated lines and try and understand if they're regular verses/ if we got into refrains
            // and then uncollate and turn into verse based format


            // check all collated lines. If some lines have more words than others, then we'd suspect it's a refrain - not part of verse
            // (NOTE) LSB has an option to output stz#'s if not all are selected. Might be able to assume all hymns have multiple verses. then could use this to indicate if not the case

            // TODO: do something about refrains

            // for now handle
            /* Case 1 Hymns: only verse
             * Case 2 Hymns: verse, refrain
             */

            // check for a case 2 hymn
            var firstcollated = CollatedLines.FirstOrDefault();
            int numverses = firstcollated.words?.Count ?? 0;

            if (numverses == 0)
            {
                // just a bunch of image lines...
                // can stop right here and build a slide
                Slide slide = new Slide();
                slide.Name = $"stitchedhymn";
                slide.Number = project.NewSlideNumber;
                slide.Asset = "";
                slide.Lines = new List<SlideLine>();
                slide.Format = SlideFormat.StitchedImage;
                slide.MediaType = MediaType.Image;

                slide.Data[StitchedImageRenderer.DATAKEY_TITLE] = Title;
                slide.Data[StitchedImageRenderer.DATAKEY_NAME] = HymnName;
                slide.Data[StitchedImageRenderer.DATAKEY_NUMBER] = Number;
                slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT] = CopyrightInfo;
                if (CopyrightText != CopyrightTune)
                {
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TEXT] = CopyrightText;
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TUNE] = CopyrightTune;
                }

                slide.Data[StitchedImageRenderer.DATAKEY_IMAGES] = ImageAssets.Select(i => new LSBImageResource(i, ImageSizes[i])).ToList();

                slide.AddPostset(_Parent, true, true);

                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.StitchedImage);
                //project.Slides.Add(slide);
                WarnVerseOverheight(Logger, ImageSizes);

                return slide.ToList();
            }

            List<(LSBImageResource music, List<LSBImageResource> words)> VerseCollatedLines = new List<(LSBImageResource music, List<LSBImageResource> words)>();
            List<(LSBImageResource music, List<LSBImageResource> words)> RefrainCollatedLines = new List<(LSBImageResource music, List<LSBImageResource> words)>();
            bool foundrefrain = false;
            // NOTE: approach fails if refrain comes first
            foreach (var cl in CollatedLines)
            {
                if (cl.words.Count != numverses && cl.words.Count == 1)
                {
                    // after this everything is refrain???
                    foundrefrain = true;
                }
                if (foundrefrain)
                {
                    RefrainCollatedLines.Add(cl);
                }
                else
                {
                    VerseCollatedLines.Add(cl);
                }
            }

            // build into a StitchedImageHymnVerses
            List<StitchedImageHymnStanza> stanzas = new List<StitchedImageHymnStanza>();
            for (int i = 0; i < numverses; i++)
            {
                List<LSBPairedHymnLine> verselines = new List<LSBPairedHymnLine>();
                for (int j = 0; j < VerseCollatedLines.Count; j++)
                {
                    if (VerseCollatedLines[j].words.Count > i)
                    {
                        LSBPairedHymnLine line = new LSBPairedHymnLine(VerseCollatedLines[j].music, VerseCollatedLines[j].words[i]);
                        verselines.Add(line);
                    }
                    else
                    {
                        Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident generating verses for {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"Found unpaired line. Expected to find matching words for music line {j}.", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
                    }
                }
                StitchedImageHymnStanza stanza = new StitchedImageHymnStanza(verselines);
                stanzas.Add(stanza);
            }

            List<LSBPairedHymnLine> refrainlines = new List<LSBPairedHymnLine>();
            for (int i = 0; i < RefrainCollatedLines.Count; i++)
            {
                refrainlines.Add(new LSBPairedHymnLine(RefrainCollatedLines[i].music, RefrainCollatedLines[i].words[0]));
            }
            StitchedImageHymnStanza refrain = new StitchedImageHymnStanza(refrainlines);

            StitchedImageHymnVerses hymn = new StitchedImageHymnVerses() { Refrain = refrain, RepeatingPostRefrain = foundrefrain, Verses = stanzas };

            // At this point we should probably double check that total lines of refrain and all verses is the same number of lines we started with
            // if not- then we probably split it into verses/refrains incorrectly
            if (!hymn.PerformSanityCheck(ImageAssets.Count()))
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Unconfident splitting verses/refrains for {HymnName}", ErrorName = "Autogen Unconfident", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"Provided {ImageAssets.Count()} source images. Generated Hymn only uses {hymn.ComputeSourceLinesUsed()}", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            }

            // 2. add the height of all the images. if height > 1200??? then we'll do it by stanza
            const int MaxHeightForImags = 1200;
            int height = 0;
            foreach (var lineitem in ImageAssets)
            {
                height += ImageSizes[lineitem].Height;
            }

            if (height < MaxHeightForImags)
            {
                // do it all on one slide
                Slide slide = new Slide();
                slide.Name = $"stitchedhymn";
                slide.Number = project.NewSlideNumber;
                slide.Asset = "";
                slide.Lines = new List<SlideLine>();
                slide.Format = SlideFormat.StitchedImage;
                slide.MediaType = MediaType.Image;

                slide.Data[StitchedImageRenderer.DATAKEY_TITLE] = Title;
                slide.Data[StitchedImageRenderer.DATAKEY_NAME] = HymnName;
                slide.Data[StitchedImageRenderer.DATAKEY_NUMBER] = Number;
                slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT] = CopyrightInfo;
                if (CopyrightText != CopyrightTune)
                {
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TEXT] = CopyrightText;
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TUNE] = CopyrightTune;
                }

                slide.Data[StitchedImageRenderer.DATAKEY_IMAGES] = hymn.OrderAllAsOne();
                slide.AddPostset(_Parent, true, true);
                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.StitchedImage);
                slides.Add(slide);
            }
            else
            {
                // go verse by verse
                int versenum = 0;

                // if we want to lock all slides to same size, we need to find max size and then specify it for all generated slides


                foreach (var verse in hymn.Verses)
                {
                    Slide slide = new Slide();
                    slide.Name = $"stitchedhymn";
                    slide.Number = project.NewSlideNumber;
                    slide.Asset = "";
                    slide.Lines = new List<SlideLine>();
                    slide.Format = SlideFormat.StitchedImage;
                    slide.MediaType = MediaType.Image;

                    slide.Data[StitchedImageRenderer.DATAKEY_TITLE] = Title;
                    slide.Data[StitchedImageRenderer.DATAKEY_NAME] = HymnName;
                    slide.Data[StitchedImageRenderer.DATAKEY_NUMBER] = Number;
                    slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT] = CopyrightInfo;
                    if (CopyrightText != CopyrightTune)
                    {
                        slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TEXT] = CopyrightText;
                        slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TUNE] = CopyrightTune;
                    }

                    slide.Data[StitchedImageRenderer.DATAKEY_IMAGES] = hymn.OrderVerse(versenum++);

                    slide.AddPostset(_Parent, versenum == 0, hymn.Verses.Count == versenum && !hymn.RepeatingPostRefrain);
                    (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.StitchedImage);
                    slides.Add(slide);

                    if (hymn.RepeatingPostRefrain)
                    {
                        Slide refrainslide = new Slide();
                        refrainslide.Name = $"stitchedhymn";
                        refrainslide.Number = project.NewSlideNumber;
                        refrainslide.Asset = "";
                        refrainslide.Lines = new List<SlideLine>();
                        refrainslide.Format = SlideFormat.StitchedImage;
                        refrainslide.MediaType = MediaType.Image;

                        refrainslide.Data[StitchedImageRenderer.DATAKEY_TITLE] = Title;
                        refrainslide.Data[StitchedImageRenderer.DATAKEY_NAME] = HymnName;
                        refrainslide.Data[StitchedImageRenderer.DATAKEY_NUMBER] = Number;
                        refrainslide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT] = CopyrightInfo;
                        if (CopyrightText != CopyrightTune)
                        {
                            refrainslide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TEXT] = CopyrightText;
                            refrainslide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TUNE] = CopyrightTune;
                        }

                        refrainslide.Data[StitchedImageRenderer.DATAKEY_IMAGES] = hymn.OrderRefrain();


                        refrainslide.AddPostset(_Parent, false, hymn.Verses.Count == versenum);

                        (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, refrainslide, LanguageKeywordCommand.StitchedImage);
                        slides.Add(refrainslide);
                    }
                }

            }
            return slides;
        }

        private void WarnVerseOverheight(XenonErrorLogger Logger, Dictionary<string, SixLabors.ImageSharp.Size> ImageSizes)
        {
            var heightaprox = 200 + ImageAssets.Select(i => ImageSizes[i].Height).Aggregate((item, sum) => sum + item);
            var mwidth = ImageAssets.Select(i => ImageSizes[i].Width).Max();
            if (heightaprox > 1200)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Hymn over height: {HymnName}. Hymn interpreted to have only one verse,w with all lines on the same slide. Expected to have {heightaprox} height.", ErrorName = "Verse Overheight", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            }
            WarnAspectRatio(mwidth, heightaprox, Logger);
        }

        private void WarnVerseOverheightStitchAll(XenonErrorLogger Logger, Dictionary<string, SixLabors.ImageSharp.Size> ImageSizes)
        {
            var heightaprox = 200 + ImageSizes.Select(i => i.Value.Height).Aggregate((item, sum) => sum + item);
            var mwidth = ImageSizes.Values.Select(s => s.Width).Max();
            if (heightaprox > 1200)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Hymn over height: {HymnName}. Hymn requested with 'stichall' flag, but hymn is expected to have {heightaprox} height.", ErrorName = "StitchAll Overheight", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            }
            WarnAspectRatio(mwidth, heightaprox, Logger);
        }

        private void WarnAspectRatio(int maxwidth, int height, XenonErrorLogger Logger)
        {
            double ratio = (double)maxwidth / (double)height;
            if (ratio < 0.6) // maybe this is ok??
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Hymn Incorrect Aspect Ratio: {HymnName}. Hymn is expected to have an aspect ratio of {maxwidth}:{height}={ratio:0.00}.", ErrorName = "Verse Incorrect Aspect Ratio", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            }
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTStitchedHymn>");
            Debug.WriteLine($"Title={Title}");
            Debug.WriteLine($"HymnName={HymnName}");
            Debug.WriteLine($"Number={Number}");
            Debug.WriteLine($"Copyright={CopyrightInfo}");
            foreach (var asset in ImageAssets)
            {
                Debug.WriteLine($"ImageAsset={asset}");
            }
            Debug.WriteLine("</XenonASTStitchedHymn>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
