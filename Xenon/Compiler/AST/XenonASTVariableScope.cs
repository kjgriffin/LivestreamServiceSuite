using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTVariableScope : IXenonASTCommand, IXenonASTScope
    {
        public IXenonASTElement Parent { get; private set; }

        public XenonASTElementCollection children;

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        public string ScopeName { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTVariableScope scope = new XenonASTVariableScope();
            scope.children = new XenonASTElementCollection(scope);
            scope.children.Elements = new List<IXenonASTElement>();
            scope.Parent = Parent;

            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "scopename");

            scope.ScopeName = args["scopename"];

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace { to mark start of scope");

            if (Lexer.InspectEOF())
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Info, ErrorName = "Scope not closed", ErrorMessage = "Incomplete Scope", Token = Lexer.EOFText, Generator = "XenonASTVariableScope::Compile" });
                return scope;
            }
            do
            {
                XenonASTExpression expr = new XenonASTExpression();
                Lexer.GobbleWhitespace();
                expr = (XenonASTExpression)expr.Compile(Lexer, Logger, scope);
                if (expr != null)
                {
                    scope.children.Elements.Add(expr);
                }
                Lexer.GobbleWhitespace();
            } while (!Lexer.Inspect("}"));
            Lexer.Consume();
            return scope;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            (children as IXenonASTElement).Generate(project, _Parent, Logger);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTVariableScope>");
            Debug.WriteLine("<children>");
            (children as IXenonASTElement).GenerateDebug(project);
            Debug.WriteLine("</children>");
            Debug.WriteLine("<variables>");
            foreach (var kvp in Variables)
            {
                Debug.WriteLine($"<var Key=\"{kvp.Key}\" Value=\"{kvp.Value}\"");
            }
            Debug.WriteLine("</variables>");
            Debug.WriteLine("</XenonASTVariableScope>");

        }

        public (bool found, string scopename) GetScopedVariableValue(string vname, out string value)
        {
            if (Variables.TryGetValue(vname, out value))
            {
                return (true, ScopeName);
            }
            if (Parent != null)
            {
                return (Parent as IXenonASTElement).TryGetScopedVariable(vname, out value);
            }
            value = "";
            return (false, "");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        public bool SetScopedVariableValue(string vname, string value)
        {
            if (Variables.ContainsKey(vname) || (this as IXenonASTElement).CheckAnsestorScopeFornameConflict(vname).found)
            {
                return false;
            }
            Variables[vname] = value;
            return true;
        }

    }
}
