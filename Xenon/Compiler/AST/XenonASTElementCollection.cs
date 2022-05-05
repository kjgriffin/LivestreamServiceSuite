using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTElementCollection : IXenonASTCommand
    {

        public List<IXenonASTElement> Elements { get; set; } = new List<IXenonASTElement>();
        public IXenonASTElement Parent { get; private set; }

        IXenonASTElement IXenonASTElement.Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this.Parent = Parent;
            throw new NotImplementedException();
        }
        void IXenonASTElement.PreGenerate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            foreach (var elem in Elements)
            {
                elem.PreGenerate(project, _Parent, Logger);
            }
        }


        List<Slide> IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> slides = new List<Slide>();
            foreach (var elem in Elements)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating Expression {elem.GetType()}", ErrorName = "Generation Debug Log", Generator = "XenonASTExlementCollection:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, });
                slides.AddRange(elem.Generate(project, _Parent, Logger));
            }
            return slides;
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTElementCollection>");
            foreach (var elem in Elements)
            {
                elem.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTElementCollection>");
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            foreach (var elem in Elements)
            {
                elem.DecompileFormatted(sb, ref indentDepth, indentSize);
            }
        }

        public XenonASTElementCollection(IXenonASTElement Parent)
        {
            this.Parent = Parent;
        }
    }
}
