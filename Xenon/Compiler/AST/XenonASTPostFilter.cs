using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTPostFilter : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public List<IXenonASTElement> Children { get; set; } = new List<IXenonASTElement>();
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTPostFilter filter = new XenonASTPostFilter();
            filter._SourceLine = Lexer.Peek().linenum;




            filter.Parent = Parent;
            return filter;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            throw new NotImplementedException();
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            return new List<Slide>();
        }

        public void GenerateDebug(Project project)
        {
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
