using Integrated_Presenter.Automation;
using Integrated_Presenter.ViewModels.MatrixControls;

using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;
using IntegratedPresenterAPIInterop.DynamicDrivers;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Integrated_Presenter.DynamicDrivers
{

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
                    await _actionEngine.PerformAutomationAction(action, ICalculatedVariableManager.IP_PANEL);
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
                        if (!_ui.CheckAccess())
                        {
                            _ui.Dispatcher.Invoke(() =>
                            {
                                _ui.UpdateButton(btn.X, btn.Y, dv.pkey, dv.value);
                            });
                        }
                        else
                        {
                            _ui.UpdateButton(btn.X, btn.Y, dv.pkey, dv.value);
                        }
                    }
                }
            }
        }

        public void ClearControls()
        {

        }

        public void ConfigureControls(string rawText, string resourcefolder, bool overwriteAll, ICalculatedVariableManager calculator)
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

            // reset variables
            calculator.ReleaseVariables(ICalculatedVariableManager.IP_PANEL);

            if (overwriteAll)
            {
                _buttons.Clear();
                _ui.Dispatcher.Invoke(() =>
                {
                    _ui.ClearMatrix();
                });
            }

            foreach (var button in buttons)
            {
                string id = $"{button.X},{button.Y}";
                _buttons[id] = button;
                string top = "";
                string bottom = "";
                string backcolor = "#eaeaea";
                string hovercolor = "#ffa500";
                bool enabled = false;

                // dump in defaults for now
                if (!_ui.CheckAccess())
                {
                    _ui.Dispatcher.Invoke(() =>
                    {
                        _ui.InstallButton(button.X, button.Y, top, bottom, backcolor, hovercolor, enabled);
                    });
                }
                else
                {
                    _ui.InstallButton(button.X, button.Y, top, bottom, backcolor, hovercolor, enabled);
                }

            }
            // perform update of all
            DrawButtons();
        }

        public bool SupportsConfig(string configID)
        {
            return configID == "matrix(4x3)";
        }
    }

}
