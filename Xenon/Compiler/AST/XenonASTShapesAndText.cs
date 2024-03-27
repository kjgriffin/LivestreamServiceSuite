using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Compiler.SubParsers;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTShapesAndText : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public List<string> Texts { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTShapesAndText shapeAndTexts = new XenonASTShapesAndText();
            shapeAndTexts._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();

            shapeAndTexts.Texts = TextBlockParser.ParseTextBlockLines(Lexer);

            shapeAndTexts.Parent = Parent;

            return shapeAndTexts;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.CustomText]);
            sb.AppendLine("(pretrim,trimat:`)");
            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;

            foreach (var text in Texts)
            {
                sb.AppendLine($"`{text}".PadLeft(indentDepth * indentSize));
            }

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide
            {
                Name = "UNNAMED_customtext",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ShapesAndTexts,
                MediaType = MediaType.Image,
            };

            slide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = Texts;
            slide.Data[ShapeAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.CustomText].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.CustomText);

            slide.AddPostset(_Parent, true, true);

            return slide.ToList();
        }

        public void GenerateDebug(Project project)
        {
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
