using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTLiturgyImage : IXenonASTCommand
    {
        public string AssetName { get; set; }
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTLiturgyImage litimage = new XenonASTLiturgyImage();
            litimage._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "asset");
            litimage.AssetName = args["asset"];
            litimage.Parent = Parent;
            return litimage;

        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]);
            sb.AppendLine($"({AssetName})");
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
