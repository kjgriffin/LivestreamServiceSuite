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

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            XenonASTFitImage fullimage = new XenonASTFitImage();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect("\\)"))
            {
                sb.Append(Lexer.Consume());
            }
            fullimage.AssetName = sb.ToString().Trim();
            Lexer.Gobble(")");
            return fullimage;

        }

        public void Generate(Project project, IXenonASTElement _Project)
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
