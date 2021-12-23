using Xenon.SlideAssembly;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Collections.Generic;
using System.Text;
using Xenon.Compiler.AST;

namespace Xenon.Compiler
{
    class XenonASTVideo : IXenonASTCommand
    {

        public string AssetName { get; set; }
        public string KeyAssetName { get; set; }
        public string KeyType { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTVideo video = new XenonASTVideo();
            Lexer.GobbleWhitespace();
            StringBuilder sb = new StringBuilder();
            var args = Lexer.ConsumeArgList(false, "assetname");
            video.AssetName = args["assetname"];
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("["))
                {
                    Lexer.Consume();
                    video.KeyType = Lexer.ConsumeUntil("]");
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                }
                else if (Lexer.Inspect("<"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    video.KeyAssetName = Lexer.ConsumeUntil(">", "Expected closing '>' for key file.");
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                }
            }
            video.Parent = Parent;
            return video;

        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // create a video slide
            Slide videoslide = new Slide
            {
                Name = "UNNAMED_video",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>()
            };
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.CurrentPath;
            }
            if (!string.IsNullOrWhiteSpace(KeyAssetName))
            {
                var keyasset = project.Assets.Find(p => p.Name == KeyAssetName);
                if (keyasset != null)
                {
                    videoslide.Data["key-file"] = keyasset.CurrentPath;
                }
            }

            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            videoslide.Lines.Add(sl);
            videoslide.Format = SlideFormat.Video;
            videoslide.Asset = AssetName;
            videoslide.MediaType = MediaType.Video;
            videoslide.Data["key-type"] = KeyType;

            videoslide.AddPostset(_Parent, true, true);

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