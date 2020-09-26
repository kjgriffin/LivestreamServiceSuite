using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;
using Xenon.Compiler.AST;

namespace Xenon.Compiler
{
    internal interface IXenonASTElement
    {
        public void Generate(Project project, IXenonASTElement _Parent);
        public void GenerateDebug(Project project);
        public XenonCompilerSyntaxReport Recognize(Lexer Lexer);
        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger);

    }
}
