
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop.DynamicDrivers
{
    public static class DynamicControlTemplateParser
    {
        private static bool TryFind(this string s, ref int startIndex, string match)
        {
            if (s.Length - startIndex - match.Length > 0)
            {
                var sub = s.Substring(startIndex, match.Length);
                if (sub == match)
                {
                    startIndex += match.Length;
                    return true;
                }
            }
            return false;
        }

        private static int ExtractMatchedBraces(string input, int sindex, out string inside)
        {
            // starts at sindex within input
            // searches for initial brace
            // searches for matching closing brace

            bool open = false;
            bool close = false;
            int depth = 0;
            StringBuilder sb = new StringBuilder();

            while (sindex < input.Length)
            {
                if (input[sindex] == '{')
                {
                    if (depth == 0)
                    {
                        open = true;
                    }
                    depth++;
                }
                if (input[sindex] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        close = true;
                    }
                }

                if (open)
                {
                    sb.Append(input[sindex]);
                }

                if (open && close)
                {
                    inside = sb.ToString();
                    return sindex;
                }

                sindex++;
            }
            inside = sb.ToString();
            return sindex;
        }

        private static int ParseInt(string input, ref int index)
        {
            IEnumerable<char> num = new List<char>();
            while (Regex.Match(input[index].ToString(), @"\d").Success)
            {
                num = num.Append(input[index]);
                index++;
            }
            if (int.TryParse(string.Join("", num), out var result))
            {
                return result;
            }
            return 0;
        }

        public static List<(int, int, string)> FindButtonEntries(string config)
        {
            List<(int, int, string)> result = new List<(int, int, string)>();
            int index = 0;
            while (index < config.Length)
            {
                // find definition of button
                if (config.TryFind(ref index, "[TButton]"))
                {
                    // expect
                    //int x = int.Parse(config.Substring(index++, 1));
                    int x = ParseInt(config, ref index);
                    index++;
                    //int y = int.Parse(config.Substring(index++, 1));
                    int y = ParseInt(config, ref index);
                    string draw;
                    index = ExtractMatchedBraces(config, index, out var btntxt);
                    result.Add((x, y, btntxt));
                }
                else
                {
                    index++;
                }
            }
            return result;
        }

        public static string FindGlobalConditions(string config)
        {
            string globals = "";

            int index = 0;
            while (index < config.Length)
            {
                // find definition of button
                if (config.TryFind(ref index, "[Globals]"))
                {
                    index = ExtractMatchedBraces(config, index, out globals);
                    if (globals.Length > 1)
                    {
                        globals = globals.Remove(0, 1);
                        globals = globals.Remove(globals.Length - 1, 1);
                        globals = globals.Trim();
                    }
                    return globals;
                }
                else
                {
                    index++;
                }
            }


            return globals;
        }

        public static DynamicControlTButtonDefinition ParseButtonEntry((int x, int y, string body) entry, string resourcefolder)
        {
            string draw = "";
            string actions = "";
            int i = 0;
            while (i < entry.body.Length)
            {
                if (entry.body.TryFind(ref i, "draw="))
                {
                    ExtractMatchedBraces(entry.body, i, out draw);
                    if (draw.Length > 1)
                    {
                        draw = draw.Remove(0, 1);
                        draw = draw.Remove(draw.Length - 1, 1);
                        draw = draw.Trim();
                    }
                }
                if (entry.body.TryFind(ref i, "fire="))
                {
                    ExtractMatchedBraces(entry.body, i, out actions);
                    if (actions.Length > 1)
                    {
                        actions = actions.Remove(0, 1);
                        actions = actions.Remove(actions.Length - 1, 1);
                        actions = actions.Trim();
                    }
                }
                i++;
            }

            DynamicControlTButtonDefinition def = new DynamicControlTButtonDefinition();

            def.X = entry.x;
            def.Y = entry.y;

            // parse script
            if (ActionLoader.TryLoadActions(actions, resourcefolder, out var loadedActions))
            {
                def.Actions = loadedActions.Actions.Select(x => x.Action).ToList();
            }

            // parse draw
            var draws = draw.Split(Environment.NewLine);
            foreach (var d in draws)
            {
                // expected format is: <cond>VAR=val;
                var match = Regex.Match(d, "(<(?<cond>.*)>)?(?<var>\\w+)=(?<val>.*);");
                if (match.Success)
                {
                    SumOfProductExpression expr = new SumOfProductExpression();
                    if (match.Groups["cond"].Value.Length > 0)
                    {
                        expr = SumOfProductExpression.Parse(match.Groups["cond"].Value);
                    }
                    def.DrawValues.Add((match.Groups["var"].Value, match.Groups["val"].Value, expr));
                }
            }


            return def;
        }

        public static Dictionary<string, WatchVariable> ComputeAggregateWatchVariables(List<DynamicControlTButtonDefinition> buttons, List<AutomationAction> globalActions)
        {
            // find every slide with actions the require watches
            Dictionary<string, WatchVariable> variables = new Dictionary<string, WatchVariable>();

            foreach (var button in buttons)
            {
                var allSlideActions = button.Actions.Concat(globalActions);

                foreach (var action in allSlideActions.Where(x => x.Action == AutomationActions.WatchSwitcherStateBoolVal || x.Action == AutomationActions.WatchStateBoolVal))
                {
                    string vname = (string)action.Parameters[2].LiteralValue;
                    string wpath = (string)action.Parameters[0].LiteralValue;
                    object expectation = action.Parameters[1].LiteralValue;
                    variables[vname] = new WatchVariable(wpath, expectation, AutomationActionArgType.Boolean);
                }
                foreach (var action in allSlideActions.Where(x => x.Action == AutomationActions.WatchSwitcherStateIntVal || x.Action == AutomationActions.WatchStateIntVal))
                {
                    string vname = (string)action.Parameters[2].LiteralValue;
                    string wpath = (string)action.Parameters[0].LiteralValue;
                    object expectation = action.Parameters[1].LiteralValue;
                    variables[vname] = new WatchVariable(wpath, expectation, AutomationActionArgType.Integer);
                }
            }
            return variables;
        }

    }

}
