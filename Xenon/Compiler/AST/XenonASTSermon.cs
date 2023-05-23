using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTSermon : IXenonASTCommand
    {

        public string Title { get; set; }
        public string Reference { get; set; }
        public string Preacher { get; set; }
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTSermon sermon = new XenonASTSermon();
            sermon._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "reference", "preacher");
            sermon.Title = args["title"];
            sermon.Reference = args["reference"];
            sermon.Preacher = args["preacher"];
            sermon.Parent = Parent;
            return sermon;

        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]);
            sb.AppendLine($"(\"{Title}\", \"{Reference}\", \"{Preacher}\")");
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide sermonslide = new Slide
            {
                Name = "UNNAMED_sermon",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ShapesAndTexts,
                MediaType = MediaType.Image
            };

            List<string> strings = new List<string>
            {
                Title, Reference, Preacher,
            };

            sermonslide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = strings;
            sermonslide.Data[ShapeAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.Sermon].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, sermonslide, LanguageKeywordCommand.Sermon);


            sermonslide.AddPostset(_Parent, true, true);

            return sermonslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTSermon>");
            Debug.WriteLine($"Title='{Title}'");
            Debug.WriteLine($"Reference='{Reference}'");
            Debug.WriteLine($"Preacher='{Preacher}'");
            Debug.WriteLine("</XenonASTSermon>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
