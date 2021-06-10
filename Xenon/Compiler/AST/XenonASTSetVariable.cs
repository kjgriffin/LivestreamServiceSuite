using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTSetVariable : IXenonASTCommand
    {

        public string VariableName;
        public string Value;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(true, "name", "value");

            VariableName = args["name"];
            Value = args["value"];

            return this;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            project.AddAttribute(VariableName, Value);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTSetVariable>");
            Debug.WriteLine($"Name='{VariableName}'");
            Debug.WriteLine($"Value='{Value}'");
            Debug.WriteLine("</XenonASTSetVariable>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
