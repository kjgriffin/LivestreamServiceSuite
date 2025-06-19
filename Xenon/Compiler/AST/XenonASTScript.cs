using ATEMSharedState.SwitcherState;

using IntegratedPresenterAPIInterop;

using SharedPresentationAPI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using VariableMarkupAttributes;
using VariableMarkupAttributes.Attributes;

using Xenon.AssetManagment;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Compiler.Meta;
using Xenon.Compiler.Suggestions;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTScript : IXenonASTCommand, IXenonCommandSuggestionCallback
    {

        public string Source { get; set; } = "";
        public IXenonASTElement Parent { get; private set; }

        private Token _srcToken;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTScript script = new XenonASTScript();
            script._SourceLine = Lexer.Peek().linenum;
            script._srcToken = Lexer.CurrentToken;
            Lexer.GobbleWhitespace();
            StringBuilder sb = new StringBuilder();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();
            while (!Lexer.InspectEOF())
            {
                sb.Append(Lexer.Consume());
                if (Lexer.Inspect("}"))
                {
                    Lexer.Consume();
                    break;
                }
            }
            string src = sb.ToString();
            // handle whitespace
            var slines = src.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            script.Source = string.Join(Environment.NewLine, slines.Select(x => x.Trim()));
            script.Parent = Parent;
            return script;

        }
        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.AppendLine(LanguageKeywords.Commands[LanguageKeywordCommand.Script]);

            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("{");
            indentDepth++;

            foreach (var line in Source.Split(Environment.NewLine))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.Append("".PadLeft(indentDepth * indentSize));
                    sb.AppendLine(line);
                }
            }

            indentDepth--;
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("}");
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // create a video slide
            Slide script = new Slide
            {
                Name = "UNNAMED_script",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>()
            };
            script.Format = SlideFormat.Script;
            script.Asset = "";
            script.MediaType = MediaType.Text;

            string src = Source;

            // resolve variables
            // for now only check if a button in the config file
            /*
            foreach (var match in Regex.Matches(Source, "%(?<var>.*)%").AsEnumerable())
            {
                // pretty sure that this is now better handled with camera substitution
                // check if we have that one
                if (project.BMDSwitcherConfig.Routing.Any(x => x.ButtonName == match?.Groups["var"]?.Value))
                {
                    src = Regex.Replace(src, $"%{match.Groups["var"].Value}%", project.BMDSwitcherConfig.Routing.FirstOrDefault(x => x.ButtonName == match.Groups["var"].Value).PhysicalInputId.ToString());
                }
                else if (match?.Groups["var"].Value == "pres" || match?.Groups["var"].Value.StartsWith("slide.num") == true)
                {
                    // this is OK
                    // leave it
                }
                else if (match?.Groups["var"].Value.StartsWith("cam.") == true)
                {
                    // let us substitue later
                }
                else
                {
                    Logger.Log(new XenonCompilerMessage()
                    {
                        ErrorName = "Invalid Variable Substution",
                        ErrorMessage = $"Attempting to replace varible with name %{match?.Groups["var"]?.Value}%, but could not find it.",
                        Generator = "XenonASTScript::Generate",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = _srcToken,
                    });
                }
            }
            */


            SlideVariableSubstituter.UnresolvedText unresolved = new SlideVariableSubstituter.UnresolvedText
            {
                DKEY = ScriptRenderer.DATAKEY_SCRIPTSOURCE_TARGET,
                Raw = src,
            };
            script.Data[SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;

            return script.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTScript>");
            Debug.WriteLine(Source);
            Debug.WriteLine("</XenonASTScript>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        List<RegexMatchedContextualSuggestions> IXenonCommandSuggestionCallback.contextualsuggestions { get; } = new List<RegexMatchedContextualSuggestions>()
        {
            ("#script", false, "", new List<(string, string)>() { ("#script", "")}, null),
            ("\\{", false, "", new List<(string, string)>() { ("{", "begin script")}, null),
            ("[^\\}]+(?=\\})",  false,"", null, GetContextualSuggestionsForScriptCommands),
            ("\\}", false, "", new List<(string, string)>() { ("\"", "end script")}, null),
        };
        public int _SourceLine { get; set; }

        public static IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand GetContextualSuggestionsForScriptCommands = (priorcaptures, sourcesnippet, remainingsnippet, knownAssets, knownLayouts, extraInfo) =>
        {
            // its' not validation we're doing, so just get the start of the line and work from there.

            List<(string suggestion, string description, int index)> suggestions = new List<(string suggestion, string description, int index)>();

            string currentline = remainingsnippet.Split(Environment.NewLine, StringSplitOptions.None).LastOrDefault() ?? "";

            // empty line... show commands, fullauto, title option
            // fullauto line -> complete full auto
            // command line -> complete command
            // title line -> complete title

            // Title
            if (currentline.TrimStart().StartsWith("#"))
            {
                if (!currentline.EndsWith(";"))
                {
                    suggestions.Add((";", "end script title", 0));
                }
            }
            // Full auto line
            else if (currentline.TrimStart().StartsWith("!"))
            {
                if (!currentline.EndsWith(";"))
                {
                    suggestions.Add(("fullauto;", "mark slide as fully automated.", 0));
                    suggestions.Add(("displaysrc='<slide>';", "specify alt graphic for action slide", 0));
                    suggestions.Add(("keysrc='<slide>';", "specify alt key graphic for action slide", 0));
                }
            }
            // parse commands
            else if (currentline.TrimStart().Length > 0)
            {
                suggestions.AddRange(GetContextualSuggestionsForAction(currentline.TrimStart(), knownAssets, extraInfo));
            }
            // list all options
            else
            {
                suggestions.Add(("!fullauto;", "mark slide as fully automated.", 0));
                suggestions.Add(("!forcerunonload;", "mark slide to be taken automatically if presentation loads to this slide", 0));
                suggestions.Add(("#SCRIPT TITLE;", "add script title", 0));
                suggestions.Add(("@", "add setup action", 0));
                suggestions.Add(("arg0:", "add an action taking 0 arguments", 0));
                suggestions.Add(("arg1:", "add an action taking 1 arguments", 0));
                suggestions.Add(("argd8:", "add an action taking 8 arguments", 0));
                suggestions.Add(("cmd:", "add an action (arguments will be parsed dynamically)", 0));
                //suggestions.AddRange(LanguageKeywords.ScriptActionsMetadata.Select(c => (c.Value.ActionName, "")));
            }

            return (false, suggestions);
        };

        private static List<(string, string, int)> GetContextualSuggestionsForAction(string action, List<(string name, AssetType type)> knownAssets, IXenonCommandExtraInfoProvider extraInfo)
        {
            var conditionless = Regex.Match(action, "^\\s*@?(<.*>)?(?<line>.*)").Groups["line"]?.Value ?? action;
            action = conditionless;
            List<(string, string, int)> suggestions = new List<(string, string, int)>();
            if (action.StartsWith("@"))
            {
                // doesn't really matter for the purpose of suggestions, so just eat it and try again
                return GetContextualSuggestionsForAction(action.Remove(0, 1), knownAssets, extraInfo);
            }

            if (action.StartsWith("arg0:"))
            {
                return GetcontextualSuggestionsForArg0Action(action.Remove(0, 5)).Select(x => (x.Item1, x.Item2, 0)).ToList();
            }
            else if (action.StartsWith("arg1:"))
            {
                return GetcontextualSuggestionsForArg1Action(action.Remove(0, 5), knownAssets, extraInfo).Select(x => (x.Item1, x.Item2, 0)).ToList();
            }
            else if (action.StartsWith("argd8:"))
            {
                return GetcontextualSuggestionsForArgd8Action(action.Remove(0, 6)).Select(x => (x.Item1, x.Item2, 0)).ToList();
            }
            else if (action.StartsWith("cmd:"))
            {
                return GetcontextualSuggestiongsForCmdAction(action.Remove(0, 4), knownAssets, extraInfo);
            }
            else
            {
                suggestions.Add(("arg0:", "add an action taking 0 arguments", 0));
                suggestions.Add(("arg1:", "add an action taking 1 arguments", 0));
                suggestions.Add(("argd8:", "add an action taking 8 arguments", 0));
                suggestions.Add(("cmd:", "add an action (arguments will be parsed dynamically)", 0));
                return suggestions
                    .OrderByClosestStrictMatch(action)
                    .ToList();
            }
        }

        private static List<(string, string)> GetcontextualSuggestionsForArg0Action(string action)
        {
            List<(string, string)> suggestions = new List<(string, string)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string)>() { ("];", "end command annotation") };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string)>() { (";", "end command") };
            }

            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                .Where(c => c.Value.NumArgs == 0)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, ""))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                suggestions.Add(("[", "add command annotation"));
                suggestions.Add((";", ""));
            }

            return suggestions;
        }

        private static bool IsInsideAnnotation(string action)
        {
            int i = action.Length;
            while (--i >= 0 && action[i] != ']')
            {
                if (action[i] == '[')
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsInsideParamList(string action, out string cmdname, out string insidetext)
        {
            cmdname = "";
            insidetext = "";
            int i = action.Length;
            while (--i >= 0 && action[i] != ')')
            {
                if (action[i] == '(')
                {
                    cmdname = action.Substring(0, i);
                    insidetext = action.Substring(i + 1);
                    return true;
                }
            }
            return false;
        }

        private static List<(string, string)> GetcontextualSuggestionsForArg1Action(string action, List<(string name, AssetType type)> knownAssets, IXenonCommandExtraInfoProvider extraInfo)
        {

            List<(string, string)> suggestions = new List<(string, string)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string)>() { ("];", "end command annotation") };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string)>() { (";", "end command") };
            }

            if (IsInsideParamList(action, out string cmd, out string insidetext))
            {

                // TODO: confirm request wants a camera
                // for now assume it might and allow variable '%' substituion for any of the named variables in the config
                suggestions.AddRange(GetParameterSuggestions(cmd, knownAssets, insidetext, extraInfo).Select(x => (x.Item1, x.Item2)));
            }


            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                .Where(c => c.Value.NumArgs == 1)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, ""))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                suggestions.Add(("(", ""));
            }

            return suggestions;
        }
        private static List<(string, string)> GetcontextualSuggestionsForArgd8Action(string action)
        {

            List<(string, string)> suggestions = new List<(string, string)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string)>() { ("];", "end command annotation") };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string)>() { (";", "end command") };
            }

            if (IsInsideParamList(action, out _, out _))
            {
                // get command and look it up by string

            }


            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                .Where(c => c.Value.NumArgs == 8)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, ""))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                suggestions.Add(("(", ""));
            }

            return suggestions;
        }

        private static List<(string, string, int)> GetcontextualSuggestiongsForCmdAction(string action, List<(string name, AssetType type)> knownAssets, IXenonCommandExtraInfoProvider extraInfo)
        {

            List<(string, string, int)> suggestions = new List<(string, string, int)>();

            if (IsInsideAnnotation(action))
            {
                return new List<(string, string, int)>() { ("];", "end command annotation", 0) };
            }

            if (action.EndsWith(']'))
            {
                return new List<(string, string, int)>() { (";", "end command", 0) };
            }

            if (IsInsideParamList(action, out string cmd, out string insidetext))
            {

                // TODO: confirm request wants a camera
                // for now assume it might and allow variable '%' substituion for any of the named variables in the config
                suggestions.AddRange(GetParameterSuggestions(cmd, knownAssets, insidetext, extraInfo));
            }


            // check if we match command, else get closeset match. if matching option end or notes.
            suggestions.AddRange(IntegratedPresenterAPIInterop.MetadataProvider.ScriptActionsMetadata
                .Select(sm => (sm.Value.ActionName, sm.Value))
                //.Where(c => c.Value.NumArgs == 1)
                .OrderByClosestStrictMatch(action)
                .Select(x => (x.Item1, "", 0))
                .Where(x => x.Item1.Length >= action.Length));

            if (suggestions.Any(s => s.Item1.Length == action.Length))
            {
                if (MetadataProvider.TryGetScriptActionMetadataByCommandName(action, out var metadata) && metadata.NumArgs > 0)
                {
                    suggestions.Add(("(", "", 0));
                }
                else
                {
                    suggestions.Add((";", "", 0));
                }
            }

            return suggestions;
        }

        private static List<(string, string, int)> GetParameterSuggestions(string cmd, List<(string name, AssetType type)> knownAssets, string insidetext, IXenonCommandExtraInfoProvider extraInfo)
        {
            List<(string, string, int)> suggestions = new List<(string, string, int)>();
            if (MetadataProvider.TryGetScriptActionMetadataByCommandName(cmd, out var cmdMetadata))
            {
                // differentiate between parameters
                var splittybois = insidetext.Split(",", StringSplitOptions.None);
                var pnum = splittybois.Length;
                var index = 0;

                // build API helper
                Dictionary<AutomationActionArgType, string> argID = new Dictionary<AutomationActionArgType, string>()
                {
                    [AutomationActionArgType.Integer] = "int",
                    [AutomationActionArgType.Boolean] = "bool",
                    [AutomationActionArgType.String] = "string",
                    [AutomationActionArgType.Double] = "double",
                };

                if (pnum <= cmdMetadata.NumArgs && cmdMetadata.NumArgs > 0)
                {
                    var aid = Math.Max(pnum - 1, 0);
                    var arg = cmdMetadata.OrderedParameters[aid];
                    suggestions.Add(($"<{arg.ArgName}:{arg.ArgType}>", $"[API] parameter ({pnum}/{cmdMetadata.NumArgs})", index));
                    suggestions.AddRange(arg.StaticHints.Select(x => (x.item, x.description, index)));

                    switch (arg.ParamValueHints)
                    {
                        case ExpectedVariableContents.VIDEOSOURCE:
                            // TODO: map cameras
                            suggestions.AddRange(extraInfo.ProjectConfigState.Routing.Select(x => ($"%cam.{x.LongName}%", $"'{x.KeyName}' button: ({x.ButtonName},{x.ButtonId}) physical source: ({x.PhysicalInputId})", index)));
                            break;
                        case ExpectedVariableContents.PROJECTASSET:
                            suggestions.AddRange(knownAssets.Select(x => (x.name, $"({x.type})", index)));
                            break;
                        case ExpectedVariableContents.EXPOSEDSTATE:

                            // for now just bmd state exposed
                            // eventually find all assemblies loaded and extract
                            //DummyLoader
                            ATEMSharedState_AssemblyDummyLoader.Load();
                            SharedPresentationAPI_AssemblyDummyLoader.Load();

                            HashSet<AutomationActionArgType> validTypes = new HashSet<AutomationActionArgType>() { AutomationActionArgType.UNKNOWN_TYPE };

                            switch (cmdMetadata.Action)
                            {
                                case AutomationActions.WatchSwitcherStateBoolVal:
                                case AutomationActions.WatchStateBoolVal:
                                    validTypes.Add(AutomationActionArgType.Boolean);
                                    break;
                                case AutomationActions.WatchSwitcherStateIntVal:
                                case AutomationActions.WatchStateIntVal:
                                    validTypes.Add(AutomationActionArgType.Integer);
                                    break;
                                case AutomationActions.SetupComputedTrack:
                                    validTypes.Add(AutomationActionArgType.Integer);
                                    validTypes.Add(AutomationActionArgType.Boolean);
                                    validTypes.Add(AutomationActionArgType.String);
                                    validTypes.Add(AutomationActionArgType.Double);
                                    break;
                            }

                            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.FullName))
                            {
                                try
                                {
                                    foreach (Type t in ass.GetTypes())
                                    {
                                        if (t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ExposesWatchableVariablesAttribute>() != null)
                                        {
                                            var instance = Activator.CreateInstance(t);
                                            var props = VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(instance);
                                            suggestions.AddRange(props.Where(x => validTypes.Contains(x.Value.TypeInfo)).Select(x => (x.Key, $"Exposed State Type<{x.Value.TypeInfo}>", index)));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debugger.Break();
                                }
                            }
                            break;
                    }
                }

            }

            return suggestions;
        }


    }
}