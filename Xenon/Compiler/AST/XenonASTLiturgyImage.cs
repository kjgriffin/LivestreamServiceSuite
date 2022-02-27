using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Helpers;
using System.Linq;
using Xenon.Renderer;

namespace Xenon.Compiler.AST
{
    class XenonASTLiturgyImage : IXenonASTCommand
    {
        public string AssetName { get; set; }
        public IXenonASTElement Parent { get; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTLiturgyImage litimage = new XenonASTLiturgyImage();
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "asset");
            litimage.AssetName = args["asset"];
            return litimage;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // create a liturgy image slide
            Slide imageslide = new Slide
            {
                Name = "UNNAMED_image",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.AdvancedImages,
                MediaType = MediaType.Image,
            };

            List<string> images = new List<string> { AssetName };

            imageslide.Data[AdvancedImageSlideRenderer.DATAKEY_IMAGES] = images;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, imageslide, LanguageKeywordCommand.LiturgyImage);

            imageslide.AddPostset(_Parent, true, true);

            return imageslide.ToList();
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
