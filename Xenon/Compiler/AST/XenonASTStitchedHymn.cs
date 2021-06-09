using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
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

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTStitchedHymn hymn = new XenonASTStitchedHymn();

            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "name", "number", "copyright");

            hymn.Title = args["title"];
            hymn.HymnName = args["name"];
            hymn.Number = args["number"];
            hymn.CopyrightInfo = args["copyright"];

            Lexer.GobbleWhitespace();

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

            return hymn;
        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            // main steps
            // 1. Figure out how many lines/stanzas and how many stanzas there are
            // 2. Figure out if we can squish everything on one slide, or if we need to go stanza by stanza

            // 1. Go through every asset and check its size.
            //      if its height is less than 45px its text, if its more than 85 its music
            //      might need to be inefficient here and open the file to check the height. Don't think we've got that info yet

            Dictionary<string, Size> ImageSizes = new Dictionary<string, Size>();

            foreach (var item in ImageAssets)
            {
                try
                {
                    using (Bitmap b = new Bitmap(project.Assets.Find(a => a.Name == item).CurrentPath))
                    {
                        ImageSizes[item] = b.Size;
                        Debug.WriteLine($"Image {item} has size {b.Size}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening image to check size: {ex}");
                    // hmmm....
                }
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
                slide.Format = SlideFormat.StichedImage;
                slide.MediaType = MediaType.Image;

                slide.Data["title"] = Title;
                slide.Data["hymnname"] = HymnName;
                slide.Data["number"] = Number;
                slide.Data["copyright"] = CopyrightInfo;

                slide.Data["assets"] = ImageAssets;
                slide.Data["imagesizes"] = ImageSizes;
            }
            else
            {
                
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
