using IntegratedPresenterAPIInterop.DynamicDrivers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler.Meta;
using Xenon.Compiler.Suggestions;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTDynamicController : IXenonASTCommand, IXenonCommandSuggestionCallback
    {
        public int _SourceLine { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public string Source { get; set; } = "";
        public string KeyName { get; set; } = "";
        List<RegexMatchedContextualSuggestions> IXenonCommandSuggestionCallback.contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>
        {
            ("#dynamiccontroller", false, "", new List<(string, string)>{("#dynamiccontroller", "")}, null),
            ("\\(", false, "", new List<(string, string)>{("(", "begin params")}, null),
            ("[^)]", false, "", new List<(string, string)>{(")", "panel name")}, null),
            ("\\)", false, "", new List<(string, string)>{(")", "end params")}, null),
            ("\\{", false, "", new List<(string, string)>{("{", "controller definition")}, null),
            ("dynamic:", false, "", new List<(string, string)>{("dynamic:", "controller definition")}, null),
            ("[^;]+(?=;)", false, "", new List<(string, string)>{("panel(4x3);", "panel style")}, null),
            ("[^\\}]+(?=\\})", false, "", new List<(string, string)>{}, null),
            ("\\}", false, "", new List<(string, string)>{("}", "end controller definition")}, null),
        };

        private Token _srcToken;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTDynamicController controller = new XenonASTDynamicController();
            controller._SourceLine = Lexer.Peek().linenum;
            controller._srcToken = Lexer.CurrentToken;

            Lexer.GobbleWhitespace();

            StringBuilder sb = new StringBuilder();

            var args = Lexer.ConsumeArgList(false, "keyname");

            controller.KeyName = args["keyname"];

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            int depth = 1;
            while (!Lexer.InspectEOF())
            {
                // use stackbased unbounded nesting traversal to find block
                if (Lexer.Inspect("{"))
                {
                    depth++;
                }
                else if (Lexer.Inspect("}"))
                {
                    depth--;
                }

                if (depth == 0)
                {
                    // we're done
                    break;
                }

                // part of source
                sb.Append(Lexer.Consume());
            }

            Lexer.GobbleandLog("}");

            controller.Source = sb.ToString();

            Lexer.GobbleWhitespace();

            controller.Parent = Parent;

            return controller;
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide res = new Slide
            {
                Name = "UNNAMED_CONTROL_resource",
                Number = -1, // Slide is not given a number since is not an ordered slide
                Lines = new List<SlideLine>(),
                Asset = "",
            };

            res.Format = SlideFormat.RawTextFile;
            res.MediaType = MediaType.Empty;

            res.Data[RawTextRenderer.DATAKEY_KEYNAME] = KeyName;

            SlideNumberVariableSubstituter.UnresolvedText unresolved = new SlideNumberVariableSubstituter.UnresolvedText
            {
                DKEY = RawTextRenderer.DATAKEY_RAWTEXT_TARGET,
                Raw = Source,
            };

            res.Data[SlideNumberVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;

            return res.ToList();
        }

        #region Unused IXenonASTCoommand
        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
        }

        public void GenerateDebug(Project project)
        {
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
