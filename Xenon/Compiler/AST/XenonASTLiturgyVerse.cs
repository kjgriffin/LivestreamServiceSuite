using System;
using System.Collections.Generic;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTLiturgyVerse : IXenonASTCommand
    {
        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            throw new NotImplementedException();
        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            throw new NotImplementedException();
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
