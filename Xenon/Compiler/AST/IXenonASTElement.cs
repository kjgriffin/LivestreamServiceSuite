using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    internal interface IXenonASTElement
    {

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer);

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages);

        public void Generate(Project project, IXenonASTElement _Parent);

        public void GenerateDebug(Project project);
        
    }
}
