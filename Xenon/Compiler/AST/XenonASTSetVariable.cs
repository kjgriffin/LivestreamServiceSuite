using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Xenon.Helpers;
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


        static List<(string, List<(string, string)>)> contextualsuggestions = new List<(string, List<(string, string)>)>()
        {
            ("#set", new List<(string, string)>() { ("#set", "")}),
            ("\\(\"", new List<(string, string)>() { ("(\"", "insert variable name")}),
            ("[^\"]+", new List<(string, string)>() { ("otherspeakers", ""), ("global.rendermode.alpha", "")}),
            ("\"", new List<(string, string)>() { ("\"", "")}),
            (",", new List<(string, string)>() { (",", "")}),
            ("\"", new List<(string, string)>() { ("\"", "")}),
            (".+\"", new List<(string, string)>() { ("\"", "enclose variable value")}),
            ("\\)", new List<(string, string)>(){(")", "")}),
        };

        public (bool complete, List<(string suggestion, string description)> suggestions) GetContextualSuggestions(string sourcecode)
        {
            return XenonSuggestionService.GetDescriptionsForRegexMatchedSequence(contextualsuggestions, sourcecode);

            if (sourcecode.StartsWith("#set"))
            {
                if (sourcecode.TrySubstring("#set".Length, 1) == "(")
                {

                }
                return (true, new List<(string, string)>());
            }
            else
            {
                return (false, new List<(string suggestion, string description)>() { ("#set", "") });
            }
        }

    }
}
