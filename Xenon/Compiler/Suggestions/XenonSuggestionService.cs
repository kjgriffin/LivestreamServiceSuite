using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Xenon.AssetManagment;
using Xenon.Compiler.AST;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.Suggestions
{
    public class XenonSuggestionService : IXenonCommandExtraInfoProvider
    {


        public TopLevelCommandContextualSuggestions GetSuggestionsByRegexMatchedSequence(List<RegexMatchedContextualSuggestions> sequentialoptions, string sourcecode, IXenonCommandSuggestionCallback root)
        {
            /*
            1. start matching regexes in sequence
            2. provide the suggestions for the first failed match
             */

            StringBuilder strbuffer = new StringBuilder();
            strbuffer.Append(sourcecode);

            Dictionary<string, string> priormatches = new Dictionary<string, string>();

            List<(string, string, int)> lastsuggestions = new List<(string, string, int)>();

            foreach (var option in sequentialoptions)
            {
                // try matching regex against source code
                // upon success remove the matched part and continue on
                var match = Regex.Match(strbuffer.ToString(), option.Regex);
                if (match.Success)
                {
                    if (!string.IsNullOrEmpty(option.Captureas))
                    {
                        priormatches[option.Captureas] = match.Value;
                    }
                    strbuffer.Remove(0, match.Length + match.Index);
                    lastsuggestions.Clear();
                }
                else if (option.Optional)
                {
                    lastsuggestions.AddRange(option.Suggestions.Select(x => (x.Item1, x.Item2, strbuffer.ToString().Length)));
                }
                else
                {
                    if (option.ExternalFunc != null)
                    {
                        // call back to base type and have them use a contextual suggestion based on current option
                        //var extfunc = root.GetType().GetField(option.ExternalFunc, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        //if (extfunc.FieldType == typeof(IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand))
                        //{
                        //IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand dfunc = (IXenonCommandSuggestionCallback.GetContextualSuggestionsForCommand)extfunc.GetValue(root);
                        //var suggestions = dfunc.Invoke(priormatches, sourcecode, strbuffer.ToString()).Concat(lastsuggestions);
                        //return (false, suggestions.ToList());
                        //}
                        var suggestions = option.ExternalFunc?.Invoke(priormatches, sourcecode, strbuffer.ToString(), GetKnownAssets(), GetKnownLayouts(), this);
                        var suggested = suggestions?.suggestions.Select(x => (x.suggestion, x.description, /*strbuffer.ToString().Length*/ x.captureindex)).Concat(lastsuggestions).ToList();
                        if (suggestions.HasValue)
                        {
                            return (suggestions.Value.complete, suggested);
                        }
                        return (false, lastsuggestions); // really shouldn't ever return the default here
                    }

                    // order suggestions by partial completion, then alphabetically
                    string remainder = strbuffer.ToString();

                    return (false, option.Suggestions.Select(x => (x.Item1, x.Item2, strbuffer.ToString().Length)).Concat(lastsuggestions).OrderByClosestMatch(remainder).ToList());
                }
            }

            return (true, lastsuggestions);

        }


        public List<(string item, string description, int startIndex)> GetSuggestions(string sourcetext, int caretpos)
        {
            List<(string, string, int)> suggestions = new List<(string, string, int)>();

            if (!IsOnCommentLine(sourcetext.Substring(0, caretpos)))
            {
                // perform a lookbehind and dispatch based on that
                suggestions.AddRange(GetSuggestionsByLookBehind(sourcetext, caretpos));
            }

            return suggestions;
        }

        private bool IsOnCommentLine(string searchtext)
        {
            string line = searchtext.Split(System.Environment.NewLine, StringSplitOptions.None).Last();
            if (line.Contains("//"))
            {
                return true;
            }
            var open = Regex.Matches(searchtext, "\\/\\*").Count;
            var closed = Regex.Matches(searchtext, "\\*\\/").Count;
            if (open > closed)
            {
                return true;
            }
            return false;
        }


        private List<(string item, string description, int startPos)> GetSuggestionsByLookBehind(string sourcetext, int caretpos)
        {
            /*
            1. compute current context (if preformance allows) -> scan backwards and try to build an infered stack of where in the AST we are. Only need to go to the most curent top level element
            2. if we don't have a top level element -> provide list of valid commands
            3. if we have a top level element -> drill down into the appropriate help tree based on the stack we build for this command
             */

            var toplevelcmd = WalkBackToTopLevelCommand(sourcetext, caretpos);
            if (toplevelcmd.cmd == LanguageKeywordCommand.INVALIDUNKNOWN)
            {
                List<(string, string)> res = new List<(string, string)>();
                if (toplevelcmd.index == -1 || toplevelcmd.index + LanguageKeywords.Commands.GetValueOrDefault(toplevelcmd.cmd, "#").Length == caretpos)
                {
                    return GetExpressionSuggestion(sourcetext.Substring(Math.Max(toplevelcmd.index, 0)), caretpos, caretpos - 1);
                }
                else
                {
                    var partialmatch = Regex.Match(sourcetext.Substring(toplevelcmd.index), @"#(?<partial>\w*)").Groups["partial"];//.Value;
                    var partialsuggestions = GetPartialMatchedTopLevelCommands(partialmatch.Value).Select(m => ("#" + m.cmdstr, ""));
                    if (partialsuggestions.Any())
                    {
                        return partialsuggestions.Select(x => (x.Item1, x.Item2, toplevelcmd.index)).ToList();
                    }
                    // don't think we ever get here??
                    Debugger.Break();
                    return GetExpressionSuggestion(sourcetext, caretpos, toplevelcmd.index - 1);
                }
            }
            else
            {
                if (toplevelcmd.completed)
                {
                    return GetExpressionSuggestion(sourcetext.Substring(toplevelcmd.index + 1 + LanguageKeywords.Commands[toplevelcmd.cmd].Length), caretpos, caretpos);
                }

                return GetCommandContextualSuggestions(toplevelcmd.cmd, sourcetext.Substring(toplevelcmd.index, caretpos - toplevelcmd.index))
                    .Select(x => (x.suggestion, x.description, caretpos - x.captureIndex)) // indicies are relative to index behind carret
                    .ToList();
            }

        }

        private List<(string, string, int)> GetExpressionSuggestion(string sourcetext, int rootcaretpos, int indexcapture)
        {
            var cmdsug = AllCommandKeywords().Select(k => (k.Item1, k.Item2, indexcapture));

            var suggestsions = new List<(string, string, int)>();
            // expression
            // can be either command
            // or prefix annotation
            // or postfix postset
            // or postfix pilot

            var tsource = sourcetext;


            if (IsInDecorator(tsource, 0, out int offset))
            {
                suggestsions.Add(("[@label::<label-name>]", "Mark slide for used with script subsitutions of form '%slide.num.<label-name>.<index>%'", indexcapture - 1));
            }
            else if (tsource.Substring(offset).TrimStart().StartsWith("#") /* && tsource.TrimEnd().LastIndexOf("#") < offset*/)
            {
                suggestsions.AddRange(cmdsug);
            }
            else
            {
                suggestsions.Add(("[", "Decorate expression with attribute", indexcapture));
                suggestsions.AddRange(AllCommandKeywords().Select(k => (k.Item1, k.Item2, indexcapture)));
            }

            return suggestsions;
        }

        private bool IsInDecorator(string source, int startindex, out int captureindex)
        {
            string remainder = source.TrimStart();
            captureindex = startindex + source.Length - remainder.Length;

            var index = remainder.IndexOf("[");
            if (index == -1)
                return false;
            remainder = remainder.Remove(0, index + 1); // replace can re-replace the '['
            captureindex += index + 1;

            index = remainder.IndexOf("]");
            if (index != -1)
            {
                captureindex += index + 1;
                return IsInDecorator(remainder.Remove(0, index + 1), captureindex, out captureindex);
            }
            return true;
        }


        internal static List<(string, string)> AllCommandKeywords()
        {
            /*
            List<(string, string)> res = new List<(string, string)>();
            foreach (var kwd in LanguageKeywords.Commands.Values)
            {
                res.Add(("#" + kwd, ""));
            }
            return res;
            */
            return LanguageKeywords.Commands
                .Where(cmd => LanguageKeywords.LanguageKeywordMetadata[cmd.Key].toplevel == true)
                .Select(cmd => ($"#{cmd.Value}", ""))
                .ToList();
        }



        private (bool completed, LanguageKeywordCommand cmd, int index) WalkBackToTopLevelCommand(string sourcetext, int caretpos)
        {

            int index = caretpos - 1;
            int firstcmdindex = -1;
            while (index >= 0)
            {
                if (sourcetext[index] == '#')
                {
                    if (firstcmdindex == -1)
                    {
                        firstcmdindex = index;
                    }
                    // perform a relative (to index) lookahead to match the largest command string
                    var match = Regex.Match(sourcetext.Substring(index), @"#(?<command>\w*)");
                    if (match.Groups.ContainsKey("command"))
                    {
                        if (LanguageKeywords.Commands.Values.Contains(match.Groups["command"].Value))
                        {
                            // if we end up having significant nesting of commands may need to revisit idea of walking back to toplevel
                            // but for now we'll ignore non-toplevel commands (eg. #break, #verse)
                            LanguageKeywordCommand cmd = LanguageKeywords.Commands.First(k => k.Value == match.Groups["command"].Value).Key;
                            if (LanguageKeywords.LanguageKeywordMetadata[cmd].toplevel)
                            {
                                if (!IsCommandComplete(cmd, sourcetext.Substring(index, caretpos - index)))
                                {
                                    return (false, cmd, index);
                                }
                                else
                                {
                                    return (true, cmd, firstcmdindex);
                                }
                            }
                        }
                        else
                        {
                            // check if there's a prior unfinished command
                            var priorcmd = WalkBackToTopLevelCommand(sourcetext, index);
                            if (priorcmd.completed || priorcmd.index == -1)
                            {
                                return (false, LanguageKeywordCommand.INVALIDUNKNOWN, firstcmdindex);
                            }
                            else return priorcmd;
                        }
                    }
                }
                index--;
            }
            return (false, LanguageKeywordCommand.INVALIDUNKNOWN, firstcmdindex);
        }

        private (bool completed, LanguageKeywordCommand cmd, int index) WalkBackToNearestValidCommand(string sourcetext, int caretpos, LanguageKeywordCommand toplevelcmd)
        {
            int index = caretpos - 1;
            int firstcmdindex = -1;
            while (index >= 0)
            {
                if (sourcetext[index] == '#')
                {
                    if (firstcmdindex == -1)
                    {
                        firstcmdindex = index;
                    }
                    // perform a relative (to index) lookahead to match the largest command string
                    var match = Regex.Match(sourcetext.Substring(index), @"#(?<command>\w*)");
                    if (match.Groups.ContainsKey("command"))
                    {
                        if (LanguageKeywords.Commands.Values.Contains(match.Groups["command"].Value))
                        {
                            // here we will return the command, but need to validate that it belongs as nested under the 
                            LanguageKeywordCommand cmd = LanguageKeywords.Commands.First(k => k.Value == match.Groups["command"].Value).Key;
                            if (LanguageKeywords.LanguageKeywordMetadata[cmd].toplevel)
                            {
                                if (!IsCommandComplete(cmd, sourcetext.Substring(index, caretpos - index)))
                                {
                                    return (false, cmd, index);
                                }
                                else
                                {
                                    return (true, cmd, firstcmdindex);
                                }
                            }
                        }
                        else
                        {
                            // check if there's a prior unfinished command
                            var priorcmd = WalkBackToTopLevelCommand(sourcetext, index);
                            if (priorcmd.completed || priorcmd.index == -1)
                            {
                                return (false, LanguageKeywordCommand.INVALIDUNKNOWN, firstcmdindex);
                            }
                            else return priorcmd;
                        }
                    }
                }
                index--;
            }
            return (false, LanguageKeywordCommand.INVALIDUNKNOWN, firstcmdindex);

        }

        private List<(LanguageKeywordCommand cmd, string cmdstr)> GetPartialMatchedTopLevelCommands(string partialstr)
        {
            return LanguageKeywords.Commands
                .Where(cmd => LanguageKeywords.LanguageKeywordMetadata[cmd.Key].toplevel == true && cmd.Value.StartsWith(partialstr))
                .Select(cmd => (cmd.Value, cmd))
                .OrderByClosestMatch(partialstr)
                .Select(x => (x.Item2.Key, x.Item2.Value))
                .ToList();
        }

        private List<(string suggestion, string description, int captureIndex)> GetCommandContextualSuggestions(LanguageKeywordCommand cmd, string sourcecode)
        {
            if (CommandContextutalSuggestionDispatcher.ContainsKey(cmd))
            {
                return CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => (true, new List<(string, string, int)>())).Invoke(sourcecode).Suggestions;
            }
            else if (XenonAPIConstructor.APIMetadata.TryGetValue(cmd, out var xapi))
            {
                return XAPISuggestionHelper.GetCommandContextualSuggestions(xapi, sourcecode);
            }
            return CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => (true, new List<(string, string, int)>())).Invoke(sourcecode).Suggestions;
        }

        private bool IsCommandComplete(LanguageKeywordCommand cmd, string sourcecode)
        {
            if (CommandContextutalSuggestionDispatcher.ContainsKey(cmd))
            {
                return CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => (true, new List<(string, string, int)>())).Invoke(sourcecode).Complete;
            }
            else if (XenonAPIConstructor.APIMetadata.TryGetValue(cmd, out var xapi))
            {
                return XAPISuggestionHelper.IsCommandComplete(xapi, sourcecode);
            }
            return CommandContextutalSuggestionDispatcher.GetValueOrDefault(cmd, (_) => (true, new List<(string, string, int)>())).Invoke(sourcecode).Complete;
        }

        private Dictionary<LanguageKeywordCommand, Func<string, TopLevelCommandContextualSuggestions>> CommandContextutalSuggestionDispatcher = new Dictionary<LanguageKeywordCommand, Func<string, TopLevelCommandContextualSuggestions>>();

        public void Initialize()
        {
            CommandContextutalSuggestionDispatcher[LanguageKeywordCommand.SetVar] = (str) => (IXenonASTCommand.GetInstance<XenonASTSetVariable>() as IXenonCommandSuggestionCallback).GetContextualSuggestionsFromOption(this, str, IXenonASTCommand.GetInstance<XenonASTSetVariable>());
            CommandContextutalSuggestionDispatcher[LanguageKeywordCommand.Liturgy] = (str) => (IXenonASTCommand.GetInstance<XenonASTLiturgy>() as IXenonCommandSuggestionCallback).GetContextualSuggestionsFromOption(this, str, IXenonASTCommand.GetInstance<XenonASTLiturgy>());
            CommandContextutalSuggestionDispatcher[LanguageKeywordCommand.AnthemTitle] = (str) => (IXenonASTCommand.GetInstance<XenonASTAnthemTitle>() as IXenonCommandSuggestionCallback).GetContextualSuggestionsFromOption(this, str, IXenonASTCommand.GetInstance<XenonASTAnthemTitle>());
            CommandContextutalSuggestionDispatcher[LanguageKeywordCommand.Script] = (str) => (IXenonASTCommand.GetInstance<XenonASTScript>() as IXenonCommandSuggestionCallback).GetContextualSuggestionsFromOption(this, str, IXenonASTCommand.GetInstance<XenonASTScript>());
            CommandContextutalSuggestionDispatcher[LanguageKeywordCommand.ScopedVariable] = (str) => (IXenonASTCommand.GetInstance<XenonASTVariableScope>() as IXenonCommandSuggestionCallback).GetContextualSuggestionsFromOption(this, str, IXenonASTCommand.GetInstance<XenonASTVariableScope>());
            CommandContextutalSuggestionDispatcher[LanguageKeywordCommand.DynamicControllerDef] = (str) => (IXenonASTCommand.GetInstance<XenonASTDynamicController>() as IXenonCommandSuggestionCallback).GetContextualSuggestionsFromOption(this, str, IXenonASTCommand.GetInstance<XenonASTDynamicController>());
        }

        private Project proj;

        BMDSwitcherConfigSettings IXenonCommandExtraInfoProvider.ProjectConfigState { get => this.proj?.BMDSwitcherConfig ?? DefaultConfig.GetDefaultConfig(); }

        public XenonSuggestionService(Project proj)
        {
            Initialize();
            this.proj = proj;
        }

        private List<(string, LanguageKeywordCommand, string)> GetKnownLayouts()
        {
            List<(string, LanguageKeywordCommand, string)> knownLayouts = new List<(string, LanguageKeywordCommand, string)>();
            foreach (var lib in proj?.LayoutManager?.AllLibraries())
            {
                foreach (var grp in lib.Layouts.GroupBy(x => x.Group))
                {
                    foreach (var layout in grp)
                    {
                        knownLayouts.Add((lib.LibName, LanguageKeywords.Commands.First(x => x.Value == grp.Key).Key, layout.Name));
                    }
                }
            }
            return knownLayouts;
        }

        private List<(string, AssetType)> GetKnownAssets()
        {
            return proj?.Assets?.Select(a => (a.Name, a.Type)).ToList();
        }


    }
}
