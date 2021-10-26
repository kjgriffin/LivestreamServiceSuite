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

        IXenonASTElement IXenonASTElement.Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            throw new NotImplementedException();
        }

        void IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            foreach (var elem in Elements)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating Expression {elem.GetType()}", ErrorName = "Generation Debug Log", Generator = "XenonASTExlementCollection:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, });
                elem.Generate(project, _Parent, Logger);
            }
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
    }
}
