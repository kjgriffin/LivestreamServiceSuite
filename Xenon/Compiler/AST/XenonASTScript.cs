using Xenon.SlideAssembly;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTScript : IXenonASTCommand
    {

        public string Source { get; set; } = "";

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTScript script = new XenonASTScript();
            Lexer.GobbleWhitespace();
            StringBuilder sb = new StringBuilder();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();
            while (!Lexer.InspectEOF())
            {
                sb.Append(Lexer.Consume());
                if (Lexer.Inspect("}"))
                {
                    script.Source = sb.ToString();
                    Lexer.Consume();
                    break;
                }
            }
            return script;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            // create a video slide
            Slide script = new Slide
            {
                Name = "UNNAMED_script",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>()
            };
            script.Format = SlideFormat.Script;
            script.Asset = "";
            script.MediaType = MediaType.Text;
            script.Data["source"] = Source;

            project.Slides.Add(script);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTScript>");
            Debug.WriteLine(Source);
            Debug.WriteLine("</XenonASTScript>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}