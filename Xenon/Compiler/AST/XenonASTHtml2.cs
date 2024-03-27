using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTHtml2 : IXenonASTCommand
    {
        public int _SourceLine { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public XenonASTHTML TemplateHTML { get; private set; }
        public string SourceText { get; private set; } = "";
        public string SourceTag { get; private set; } = "";
        public string SplitMode { get; private set; } = "";
        public string SplitData { get; private set; } = "";
        public bool TrimStart { get; private set; } = false;
        public bool TrimEnd { get; private set; } = false;
        public string NewLineReplace { get; private set; } = "";
        public bool NewLinePolicy { get; private set; } = false;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTHtml2 html2 = new XenonASTHtml2();
            html2._SourceLine = _SourceLine;
            html2.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                if (Lexer.Inspect("template"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#");
                    Lexer.GobbleandLog("html");
                    // template is a regular #html command
                    html2.TemplateHTML = (new XenonASTHTML()).Compile(Lexer, Logger, html2) as XenonASTHTML;
                }
                else if (Lexer.Inspect("content"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("[");
                    var key = Lexer.ConsumeUntil("]");
                    Lexer.GobbleandLog("]");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("{");
                    var data = Lexer.ConsumeUntil("}");
                    Lexer.GobbleandLog("}");
                    html2.SourceText = data;
                    html2.SourceTag = key;
                }
                else if (Lexer.Inspect("splitmode"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    var args = Lexer.ConsumeArgList(true, "mode", "data");
                    html2.SplitMode = args["mode"];
                    html2.SplitData = args["data"];
                }
                else if (Lexer.Inspect("trims"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    var args = Lexer.ConsumeArgList(false, "start", "end");
                    bool.TryParse(args["start"], out var start);
                    bool.TryParse(args["end"], out var end);
                    html2.TrimEnd = start;
                    html2.TrimStart = end;
                }
                else if (Lexer.Inspect("newline-policy"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    var args = Lexer.ConsumeArgList(true, "replacement");
                    html2.NewLineReplace = args["replacement"];
                    html2.NewLinePolicy = true;
                }


                Lexer.GobbleWhitespace();
            }
            Lexer.GobbleandLog("}");

            return html2;
        }

        void IXenonASTElement.DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            throw new NotImplementedException();
        }

        List<Slide> IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> slides = new List<Slide>();

            List<string> datas = new List<string>();

            string preproc = SourceText;
            if (TrimStart)
            {
                preproc = preproc.TrimStart();
            }
            if (TrimEnd)
            {
                preproc = preproc.TrimEnd();
            }
            if (NewLinePolicy)
            {
                preproc = Regex.Replace(preproc, Environment.NewLine, NewLineReplace);
            }

            if (SplitMode == "fixed-token")
            {
                datas = SplitText_FixedToken(preproc, SplitData);
            }

            foreach (var data in datas)
            {
                // makes multiple slides
                Slide slide = new Slide
                {
                    Name = "UNNAMED_html2",
                    Number = project.NewSlideNumber,
                    Lines = new List<SlideLine>(),
                    Asset = "",
                    Format = SlideFormat.HTML,
                    MediaType = MediaType.Image,
                };

                // inject on template
                var texts = new Dictionary<string, string>(TemplateHTML.Texts);
                texts[SourceTag] = data;

                slide.Data[HTMLSlideRenderer.DATAKEY_TEXTS] = texts;
                slide.Data[HTMLSlideRenderer.DATAKEY_IMGS] = TemplateHTML.Images;
                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.HTML2);

                slide.AddPostset(_Parent, datas.First() == data, datas.Last() == data);

                slides.Add(slide);
            }

            return slides;
        }

        private List<string> SplitText_FixedToken(string text, string splitstr)
        {
            var matches = text.Split(splitstr, StringSplitOptions.RemoveEmptyEntries);
            return matches.ToList();
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
