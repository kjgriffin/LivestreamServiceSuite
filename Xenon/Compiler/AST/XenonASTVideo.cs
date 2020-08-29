using Xenon.SlideAssembly;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTVideo : IXenonASTCommand
    {

        public string AssetName { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            XenonASTVideo video = new XenonASTVideo();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect("\\)"))
            {
                sb.Append(Lexer.Consume());
            }
            video.AssetName = sb.ToString().Trim();
            Lexer.Gobble(")");
            return video;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            // create a video slide
            Slide videoslide = new Slide();
            videoslide.Name = "UNNAMED_video";
            videoslide.Number = project.NewSlideNumber;
            videoslide.Lines = new List<SlideLine>();
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.RelativePath;
            }
            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            videoslide.Lines.Add(sl);
            videoslide.Format = SlideFormat.Video;
            videoslide.Asset = AssetName;
            videoslide.MediaType = MediaType.Video;

            project.Slides.Add(videoslide);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTVideo>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTVideo>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}