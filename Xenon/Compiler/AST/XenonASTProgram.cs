using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xenon.Compiler.Meta;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTProgram : IXenonASTElement
    {

        public List<XenonASTExpression> Expressions { get; set; } = new List<XenonASTExpression>();
        public IXenonASTElement Parent { get; private set; }

        public string SrcFile { get; set; } = "";

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger, IProgress<int> progress = null, int cprog = 0)
        {
            List<Slide> slides = new List<Slide>();
            int prog = 0;
            int total = Expressions.Count;
            foreach (var item in Expressions)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating Expression {item.Command.GetType()}", ErrorName = "Project Generation Debug", Generator = "XenonASTProgram:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, Token = ("", int.MaxValue) });
                var generated = item.Generate(project, this, Logger);
                if (generated?.Any() == true)
                {
                    slides.AddRange(generated);
                }
                prog++;
                progress?.Report(cprog + prog * 100 / total * ((100 - cprog) / 100));
            }

            // TO support multifile compilation, we can't assume files aren't interdependant upon attribute tagging...
            // so: we'll do the right thing and like any AST element, just return a bunch of slides (not modifying the project obj)
            // and let the compiler figure out what to do with them
            /*
            // Attribute sit outside the project right...
            // so here we can easily just 'update' the slides as required
            SlideVariableSubstituter subengine = new SlideVariableSubstituter(slides, project.BMDSwitcherConfig);

            // mark source on slides
            slides.ForEach(s => s.NonRenderedMetadata[XenonASTExpression.DATAKEY_CMD_SOURCEFILE_LOOKUP] = Logger.File);

            project.Slides.AddRange(subengine.ApplyNesscarySubstitutions());
            */

            // mark source on slides
            slides.ForEach(s => s.NonRenderedMetadata[XenonASTExpression.DATAKEY_CMD_SOURCEFILE_LOOKUP] = Logger.File);

            // at this point here we'll allow slides to be put onto the project
            //project.Slides.AddRange(slides);

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
            // pull the file from the logger
            p.SrcFile = Logger.File;
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

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            foreach (var expr in Expressions)
            {
                expr.DecompileFormatted(sb, ref indentDepth, indentSize);
            }
        }
    }
}
