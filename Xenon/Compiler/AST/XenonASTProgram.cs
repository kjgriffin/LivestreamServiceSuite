using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler.AST
{
    class XenonASTProgram : IXenonASTElement
    {

        public List<XenonASTExpression> Expressions { get; set; } = new List<XenonASTExpression>();
        public IXenonASTElement Parent { get; private set; }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger, IProgress<int> progress = null, int cprog = 0)
        {
            List<Slide> slides = new List<Slide>();
            int prog = 0;
            int total = Expressions.Count;
            foreach (var item in Expressions)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating Expression {item.Command.GetType()}", ErrorName = "Project Generation Debug", Generator = "XenonASTProgram:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, Token = ("", int.MaxValue) });
                slides.AddRange(item.Generate(project, this, Logger));
                prog++;
                progress?.Report(cprog + prog * 100 / total * ((100 - cprog) / 100));
            }

            // at this point here we'll allow slides to be put onto the project
            project.Slides.AddRange(slides);

            return slides;
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            return Generate(project, _Parent, Logger, null);
        }


        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTProgram>");
            foreach (var item in Expressions)
            {
                item.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTProgram>");
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTProgram p = new XenonASTProgram();
            // gaurd against empty file
            if (Lexer.InspectEOF())
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Info, ErrorName = "Empty Project", ErrorMessage = "Program contains no symbols.", Token = Lexer.EOFText, Generator = "Compile() XenonASTProgram" });
                return p;
            }
            do
            {
                XenonASTExpression expr = new XenonASTExpression();
                Lexer.GobbleWhitespace();
                expr = (XenonASTExpression)expr.Compile(Lexer, Logger, this);
                if (expr != null)
                {
                    p.Expressions.Add(expr);
                }
                Lexer.GobbleWhitespace();
            } while (!Lexer.InspectEOF());

            p.Parent = Parent;
            return p;
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
