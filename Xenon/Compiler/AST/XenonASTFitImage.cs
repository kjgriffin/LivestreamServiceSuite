using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTFitImage : IXenonASTCommand
    {
        public string AssetName { get; set; }
        public string KeyType { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTFitImage fullimage = new XenonASTFitImage();
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "asset");
            fullimage.AssetName = args["asset"];
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("["))
                {
                    Lexer.Consume();
                    fullimage.KeyType = Lexer.ConsumeUntil("]");
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                }
            }

            return fullimage;

        }

        public void Generate(Project project, IXenonASTElement _Project, XenonErrorLogger Logger)
        {
            // create a full image slide
            Slide imageslide = new Slide();
            imageslide.Name = "UNNAMED_image";
            imageslide.Number = project.NewSlideNumber;
            imageslide.Lines = new List<SlideLine>();
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.CurrentPath;
            }
            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            imageslide.Lines.Add(sl);
            imageslide.Format = SlideFormat.ScaledImage;
            imageslide.Asset = assetpath;
            imageslide.MediaType = MediaType.Image;
            imageslide.Data["key-type"] = KeyType;

            project.Slides.Add(imageslide);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTFullImage>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTFullImage>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
