using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTContent : IXenonASTElement
    {
        public string TextContent { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            throw new NotImplementedException();
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            throw new NotImplementedException();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTContent>");
            Debug.WriteLine(TextContent);
            Debug.WriteLine("</XenonASTContent>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
