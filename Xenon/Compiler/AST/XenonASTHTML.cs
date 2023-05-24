using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTHTML : IXenonASTCommand
    {
        public int _SourceLine { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public Dictionary<string, string> Texts { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Images { get; private set; } = new Dictionary<string, string>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTHTML html = new XenonASTHTML();
            html._SourceLine = Lexer.Peek().linenum;
            html.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                if (Lexer.Inspect("text"))
                {
                    Lexer.Consume();
                    Lexer.GobbleandLog("[");
                    var key = Lexer.ConsumeUntil("]");
                    Lexer.GobbleandLog("]");
                    Lexer.GobbleandLog("{");
                    var data = Lexer.ConsumeUntil("}");
                    Lexer.GobbleandLog("}");
                    html.Texts[key] = data;
                }
                else if (Lexer.Inspect("img"))
                {
                    Lexer.Consume();
                    Lexer.GobbleandLog("[");
                    var key = Lexer.ConsumeUntil("]");
                    Lexer.GobbleandLog("]");
                    Lexer.GobbleandLog("(");
                    var data = Lexer.ConsumeUntil(")");
                    Lexer.GobbleandLog(")");
                    html.Images[key] = data;
                }
                Lexer.GobbleWhitespace();
            }
            Lexer.GobbleandLog("}");

            return html;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            throw new NotImplementedException();
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide
            {
                Name = "UNNAMED_html",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.HTML,
                MediaType = MediaType.Image,
            };

            slide.Data[HTMLSlideRenderer.DATAKEY_TEXTS] = Texts;
            slide.Data[HTMLSlideRenderer.DATAKEY_IMGS] = Images;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.HTML);

            slide.AddPostset(_Parent, true, true);

            return slide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            throw new NotImplementedException();
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
