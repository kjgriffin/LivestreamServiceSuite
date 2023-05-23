using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTHTML : IXenonASTCommand
    {
        public int _SourceLine { get; set; }
        public IXenonASTElement Parent { get; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            throw new NotImplementedException();
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            throw new NotImplementedException();
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
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
