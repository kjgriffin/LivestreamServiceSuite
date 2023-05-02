using Integrated_Presenter.Automation;
using Integrated_Presenter.ViewModels.MatrixControls;

using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using VariableMarkupAttributes;

namespace Integrated_Presenter.DynamicDrivers
{

    internal interface IDynamicDriver
    {
        static string ParseConfigID(string rawText)
        {
            var lines = rawText.Split(Environment.NewLine);
            if (lines.FirstOrDefault().StartsWith("dynamic"))
            {
                var match = Regex.Match(lines.FirstOrDefault(), "dynamic:(?<module>.*);");
                if (match.Success)
                {
                    return match.Groups["module"].Value;
                }
            }
            return string.Empty;
        }

        bool SupportsConfig(string configID);
        void ConfigureControls(string rawText, string resourceFolder);
        void ClearControls();
    }

    internal interface IRaiseConditionsChanged
    {
        event EventHandler OnConditionalsChanged;
        Dictionary<string, bool> GetConditionals(Dictionary<string, WatchVariable> watches);
    }

    internal class DynamicMatrixDriver : IDynamicDriver
    {
        IActionAutomater _actionEngine;
        IRaiseConditionsChanged _conditions;
        _4x3Matrix _ui;

        Dictionary<string, DynamicControlTButtonDefinition> _buttons = new Dictionary<string, DynamicControlTButtonDefinition>();
        Dictionary<string, WatchVariable> _watches = new Dictionary<string, WatchVariable>();

        public DynamicMatrixDriver(_4x3Matrix ui, IActionAutomater automater, IRaiseConditionsChanged conditions)
        {
            _ui = ui;
            _actionEngine = automater;
            _conditions = conditions;

            _conditions.OnConditionalsChanged += _conditions_OnConditionalsChanged;
            _ui.OnButtonClick += _ui_OnButtonClick;
        }

        private async void _ui_OnButtonClick(object sender, (int x, int y) e)
        {
            // execute the actions as required
            if (_buttons.TryGetValue($"{e.x},{e.y}", out var btn))
            {
                foreach (var action in btn.Actions)
                {
                    await _actionEngine.PerformAutomationAction(action);
                }
            }
        }

        private void _conditions_OnConditionalsChanged(object sender, EventArgs e)
        {
            // blanket update everything?
            DrawButtons();
        }

        private void DrawButtons()
        {
            var cstate = _conditions.GetConditionals(_watches);
            // process the draw calls for each button
            foreach (var btn in _buttons.Values)
            {
                foreach (var dv in btn.DrawValues)
                {
                    // check if conditional
                    bool run = SumOfProductExpression.EvaluateExpression(dv.expr, cstate);
                    if (run)
                    {
                        _ui.UpdateButton(btn.X, btn.Y, dv.pkey, dv.value);
                    }
                }
            }
        }

        public void ClearControls()
        {

        }

        public void ConfigureControls(string rawText, string resourcefolder)
        {
            // parse it
            var entries = DynamicControlTemplateParser.FindButtonEntries(rawText);
            var buttons = entries.Select(e => DynamicControlTemplateParser.ParseButtonEntry(e, resourcefolder)).ToList();

            List<AutomationAction> globalActions = new List<AutomationAction>();
            var globals = DynamicControlTemplateParser.FindGlobalConditions(rawText);

            // parse globals (will ultimately only extract/use watches)
            if (ActionLoader.TryLoadActions(globals, resourcefolder, out var loadedActions))
            {
                globalActions = loadedActions.Actions.Select(x => x.Action).ToList();
            }

            _watches = DynamicControlTemplateParser.ComputeAggregateWatchVariables(buttons, globalActions);

            // inject watches into automater
            _actionEngine.ProvideWatchInfo(() => _watches);

            _buttons.Clear();

            foreach (var button in buttons)
            {
                _buttons[$"{button.X},{button.Y}"] = button;
                string top = "";
                string bottom = "";
                string backcolor = "#eaeaea";
                string hovercolor = "#ffa500";
                bool enabled = false;

                // dump in defaults for now
                _ui.InstallButton(button.X, button.Y, top, bottom, backcolor, hovercolor, enabled);
            }
            // perform update of all
            DrawButtons();
        }

        public bool SupportsConfig(string configID)
        {
            return configID == "matrix(4x3)";
        }
    }

    public class DynamicControlTButtonDefinition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public List<(string pkey, string value, SumOfProductExpression expr)> DrawValues { get; set; } = new List<(string pkey, string value, SumOfProductExpression expr)>();
        public List<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
    }

    internal static class DynamicControlTemplateParser
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
                    int x = int.Parse(config.Substring(index++, 1));
                    index++;
                    int y = int.Parse(config.Substring(index++, 1));
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
                    string vname = (string)action.RawParams[2];
                    string wpath = (string)action.RawParams[0];
                    object expectation = action.RawParams[1];
                    variables[vname] = new WatchVariable(wpath, expectation, AutomationActionArgType.Boolean);
                }
                foreach (var action in allSlideActions.Where(x => x.Action == AutomationActions.WatchSwitcherStateIntVal || x.Action == AutomationActions.WatchStateIntVal))
                {
                    string vname = (string)action.RawParams[2];
                    string wpath = (string)action.RawParams[0];
                    object expectation = action.RawParams[1];
                    variables[vname] = new WatchVariable(wpath, expectation, AutomationActionArgType.Integer);
                }
            }


            return variables;
        }

    }

}
