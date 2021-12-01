using System;
using System.Collections.Generic;
using System.Text;

using Xenon.Compiler.Suggestions;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTScopedVariable : IXenonASTCommand, IXenonCommandSuggestionCallback
    {
        public IXenonASTElement Parent { get; private set; }

        public string VName;
        public string VValue;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTScopedVariable variable = new XenonASTScopedVariable();
            variable.Parent = Parent;
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("(", "Expected opening ( before params");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("\"", "Expected opening \" before variable name.");
            variable.VName = Lexer.ConsumeUntil("\"", errormessage: "Expected closing \" at end of variable name");
            Lexer.GobbleandLog("\"", "Expected closing \" at end of variable name");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog(",", "Expected , before variable value");
            Lexer.GobbleWhitespace();
            if (Lexer.Peek() == "```")
            {
                Lexer.GobbleandLog("```", "Expected ``` before variable value");
                variable.VValue = Lexer.ConsumeUntil("```", errormessage: "Expected closing ``` at end of variable value");
                Lexer.GobbleandLog("```", "Expected closing ``` at end of variable value");
                Lexer.GobbleWhitespace();
            }
            else if (Lexer.Peek() == "\"")
            {
                Lexer.GobbleandLog("\"", "Expected \" before variable value");
                variable.VValue = Lexer.ConsumeUntil("\"", errormessage: "Expected closing \" at end of variable value");
                Lexer.GobbleandLog("\"", "Expected closing \" at end of variable value");
                Lexer.GobbleWhitespace();
            }
            Lexer.GobbleandLog(")", "Expected closing ) at end of params");
            Lexer.GobbleWhitespace();
            return variable;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            IXenonASTElement p = Parent;

            var sc = (this as IXenonASTElement).CheckAnsestorScopeFornameConflict(VName);
            if (sc.found == true)
            {
                // warn for name conflict...
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Variable name {{{VName}}} was already defined in a parent scope {{{sc.scopename}}}. Will be overridden.", ErrorName = "Variable name masked", Generator = "XenonASTScopedVariable::Generate", Level = XenonCompilerMessageType.Warning });
            }

            bool set = false;
            while (p != null)
            {
                if (p is IXenonASTScope)
                {

                    (p as IXenonASTScope).SetScopedVariableValue(VName, VValue);
                    set = true;
                    break;
                }
                p = p.Parent;
            }

            // error if not set (do we fail to compile, or compile- just error)??
            if (!set)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Variable named {{{VName}}} was defined outside any valid scope. Will not be set.", ErrorName = "Invalid variable declaration", Generator = "XenonASTScopedVariable::Generate", Level = XenonCompilerMessageType.Error });
            }
        }

        public void GenerateDebug(Project project)
        {

        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        List<RegexMatchedContextualSuggestions> IXenonCommandSuggestionCallback.contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>()
        {
            ("#set", false, "", new List<(string, string)> { ("#set", "")}, null),
            ("\\(\"", false, "", new List<(string, string)> { ("(\"", "insert variable name")}, null),
            /*
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end variable name")},  ),
            (",", false, "", new List<(string, string)> {(",", "") }, null),
            ("\"", false, "", new List<(string, string)> { ("\"", "insert muscian name")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end musician name")}, null ),
            (",", false, "", new List<(string, string)> {(",", "") }, null),
            ("\"", false, "", new List<(string, string)> { ("\"", "insert accompanist")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end accompanist")}, null ),
            (",", false, "", new List<(string, string)> {(",", "") }, null),
            ("\"", false, "", new List<(string, string)> { ("\"", "insert credits")}, null),
            ("[^\"](?=\")", false, "", new List<(string, string)> { ("\"", "end credits")}, null ),
            ("\\)", false, "", new List<(string, string)> {(")", "") }, null),
            */
        };

      }
}
