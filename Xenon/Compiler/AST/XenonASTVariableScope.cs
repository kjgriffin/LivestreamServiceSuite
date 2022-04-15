using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Xenon.Compiler.Suggestions;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTVariableScope : IXenonASTCommand, IXenonASTScope, IXenonCommandSuggestionCallback
    {
        public IXenonASTElement Parent { get; private set; }

        public XenonASTElementCollection children;

        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
        public string ScopeName { get; private set; }
        public List<RegexMatchedContextualSuggestions> contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>()
        {
            new RegexMatchedContextualSuggestions("#var", false, "", new List<(string, string)>{ ("#var", "") }, null),
            new RegexMatchedContextualSuggestions("\\(", false, "", new List<(string, string)>{ ("(", "Begin Parameters") }, null),
            new RegexMatchedContextualSuggestions("\"", false, "", new List<(string, string)>{ ("\"", "Begin Variable Name") }, null),
            new RegexMatchedContextualSuggestions("[^\"]+", false, "varname", null, GetContextualSuggestionsForVariableName),
            new RegexMatchedContextualSuggestions("\"", false, "", new List<(string, string)>{("\"", "End Variable Name")}, null),
            new RegexMatchedContextualSuggestions(",", false, "", new List<(string, string)>{ (",", "") }, null),
            new RegexMatchedContextualSuggestions("(```)|(\")", false, "septype", new List<(string, string)>{ ("```", "Enclose Value"), ("\"", "Enclose Value") }, null),
            new RegexMatchedContextualSuggestions(".+", false, "", null, GetContextualSuggestsionForEndOfCommand),
        };

        static IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand GetContextualSuggestionsForVariableName = (priorcaptures, sourcesnippet, remainingsnippet, knownassets, knownlayouts) =>
        {
            return (false, new List<(string, string)>() { ("\"", "End Variable Name") }.Concat(LanguageKeywords.LayoutForType.Select(x => ($"{LanguageKeywords.Commands[x.Key]}.Layout", $"Set layout override for layout type: {LanguageKeywords.Commands[x.Key]}"))).ToList());
        };

        static IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand GetContextualSuggestsionForEndOfCommand = (priorcaptures, sourcesnippet, remainingsnippet, knownassets, knownlayouts) =>
        {
            if (Regex.Match(remainingsnippet, $"${priorcaptures["septype"]}").Success)
            {
                if (Regex.Match(remainingsnippet, "$\\)").Success)
                {
                    return (true, new List<(string, string)>());
                }
                return (false, new List<(string suggestion, string description)> { (")", "End Parameters") });
            }
            else
            {
                return (false, new List<(string suggestion, string description)> { (priorcaptures["septype"], "Enclose Value") }.Concat(GetContextualSuggestionsForValueOfVariableName(priorcaptures["varname"], knownlayouts)).ToList());
            }
        };

        static List<(string, string)> GetContextualSuggestionsForValueOfVariableName(string varname, List<(string lib, LanguageKeywordCommand grp, string lname)> knownlayouts)
        {
            var vname = Regex.Match(varname, "(?<name>.*)\\.Layout");
            if (vname.Success)
            {
                return knownlayouts.Where(x => x.grp == LanguageKeywords.Commands.First(c => c.Value == vname.Groups["name"].Value).Key).Select(x => ($"{x.lib}::{x.lname}", $"Use Layout {{{x.lname}}} from Library {{{x.lib}}}")).ToList();
            }
            return new List<(string, string)>();
        }

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

            do
            {
                if (Lexer.InspectEOF())
                {
                    Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Info, ErrorName = "Scope not closed", ErrorMessage = "Incomplete Scope", Token = Lexer.EOFText, Generator = "XenonASTVariableScope::Compile" });
                    return scope;
                }

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

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.VariableScope]);
            sb.AppendLine($"({ScopeName})");
            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;

            children.DecompileFormatted(sb, ref indentDepth, indentSize);

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            return (children as IXenonASTElement).Generate(project, _Parent, Logger);
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
            Variables[vname] = value;
            if (Variables.ContainsKey(vname) || (this as IXenonASTElement).CheckAnsestorScopeFornameConflict(vname).found)
            {
                return true;
            }
            return false;
        }

    }
}
