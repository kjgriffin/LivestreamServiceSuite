using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Helpers;
using System.Linq;

namespace Xenon.Compiler
{
    class XenonASTLiturgyImage : IXenonASTCommand
    {
        public string AssetName { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTLiturgyImage litimage = new XenonASTLiturgyImage();
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "asset");
            litimage.AssetName = args["asset"];
            return litimage;

        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // create a liturgy image slide
            Slide imageslide = new Slide
            {
                Name = "UNNAMED_image",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>()
            };
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.CurrentPath;
            }
            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            imageslide.Lines.Add(sl);
            imageslide.Format = SlideFormat.LiturgyImage;
            imageslide.Asset = assetpath;
            imageslide.MediaType = MediaType.Image;

            if (project.GetAttribute("alphatranscol").Count > 0)
            {
                imageslide.Colors.Add("keytrans", GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault()));
            }

            project.Slides.Add(imageslide);

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTLiturgyImage>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTLiturgyImage>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
