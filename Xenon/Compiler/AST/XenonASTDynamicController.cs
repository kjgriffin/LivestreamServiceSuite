using IntegratedPresenterAPIInterop.DynamicDrivers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            ("[^\\]]+(?=\\])", false, "tag-type", new List<(string, string)>{("[Globals]", "define the global watches"),("[TButton]", "declare a button for the panel")}, null),
            ("\\d+,\\d+", true, "", new List<(string, string)>{("0,0", "specify instrument location")}, null),
            ("{", false, "", new List<(string, string)>{("{", "begin declaration")}, null),
            ("^a^", false, "", null, GetContextualSuggestionsForPanelDeclarations),
            //("\\}", false, "", new List<(string, string)>{("}", "end controller definition")}, null),
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

            SlideVariableSubstituter.UnresolvedText unresolved = new SlideVariableSubstituter.UnresolvedText
            {
                DKEY = RawTextRenderer.DATAKEY_RAWTEXT_TARGET,
                Raw = Source,
            };

            res.Data[SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;

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

        #region Suggestions

        static IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand GetContextualSuggestionsForPanelDeclarations = (priorcaptures, sourcesnippet, remainingsnippet, knownAssets, knownLayouts) =>
                {
                    // its' not validation we're doing, so just get the start of the line and work from there.

                    List<(string suggestion, string description)> suggestions = new List<(string suggestion, string description)>();

                    string currentline = remainingsnippet.Split(Environment.NewLine, StringSplitOptions.None).LastOrDefault() ?? "";

                    // empty line... show commands, fullauto, title option
                    // fullauto line -> complete full auto
                    // command line -> complete command
                    // title line -> complete title

                    // based on prior capture, determine the tag type
                    var x = sourcesnippet;

                    if (RollBackToStackedBrace(sourcesnippet, "\\[Globals\\]\\s*", out string gsnippet, out _))
                    {
                        // globals
                        return XenonASTScript.GetContextualSuggestionsForScriptCommands.Invoke(priorcaptures, gsnippet, remainingsnippet, knownAssets, knownLayouts);
                    }
                    else if (RollBackToStackedBrace(sourcesnippet, "\\[TButton\\]\\s*\\d+,\\d+\\s*", out _, out _))
                    {
                        // in a button def
                        suggestions.Add(("draw={", "specify UI display info"));
                        suggestions.Add(("fire={", "specify on-click action"));
                        return (false, suggestions);
                    }
                    else if (RollBackToStackedBrace(sourcesnippet, "fire\\s*\\=\\s*", out string fsnippets, out _))
                    {
                        // in a button def -> script
                        return XenonASTScript.GetContextualSuggestionsForScriptCommands.Invoke(priorcaptures, fsnippets, remainingsnippet, knownAssets, knownLayouts);
                    }
                    else if (RollBackToStackedBrace(sourcesnippet, "draw\\s*\\=\\s*", out _, out _))
                    {
                        // in a button def -> display
                        suggestions.Add((";", "complete statement"));
                        suggestions.Add(("TopText=", "Specify the top line of text displayed (string)"));
                        suggestions.Add(("BottomText=", "Specify the bottom line of text displayed (string)"));
                        suggestions.Add(("BackColor=#", "Specify the background color (hex)"));
                        suggestions.Add(("TextColor=#", "Specify the text color (hex)"));
                        suggestions.Add(("Enabled=", "true/false if click fires script"));
                        return (false, suggestions);
                    }
                    else if (!RollBackToStackedBrace(sourcesnippet, "", out _, out bool unpaired) && !unpaired)
                    {
                        // allow us to exit
                        suggestions.Clear();
                        return (true, suggestions);
                    }

                    // otherwise allow [Globals] / [TButton] at this point
                    suggestions.Add(("[Globals]", "define global watches"));
                    suggestions.Add(("[TButton]", "define button"));
                    suggestions.Add(("}", "end controller"));


                    return (false, suggestions);
                };


        static bool RollBackToStackedBrace(string text, string regexTest, out string block, out bool unpaired)
        {
            unpaired = true;
            block = "";
            try
            {
                //var index = text.LastIndexOf("{");
                var index = FindFirstUnmatchedOpeningBrace(text);
                if (index == -1)
                {
                    unpaired = false;
                }
               
                var searchSpace = text.Substring(0, index + 1);

                // try and find if the last match index matches??

                var match = Regex.Match(searchSpace, regexTest + "{$");

                if (match.Success && (match.Index + match.Length) == searchSpace.Length)
                {
                    block = text.Substring(index);
                    return true;
                }
            }
            catch (Exception)
            {
                // I'm too lazy to validate everything!
            }
            return false;
        }

        public static int FindFirstUnmatchedOpeningBrace(string input)
        {
            int count = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                if (input[i] == '}')
                    count++;
                else if (input[i] == '{')
                {
                    if (count == 0)
                        return i;
                    count--;
                }
            }

            // If no unmatched opening brace is found, return -1
            return -1;
        }


        #endregion
    }
}
