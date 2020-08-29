using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTSlideBreak : IXenonASTCommand
    {
        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
            Lexer.GobbleWhitespace();
            return slidebreak;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("</XenonASTSlideBreak>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
