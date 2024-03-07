using Integrated_Presenter.Automation;
using Integrated_Presenter.ViewModels.MatrixControls;
using Integrated_Presenter.Windows;

using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;
using IntegratedPresenterAPIInterop.DynamicDrivers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Integrated_Presenter.DynamicDrivers
{

    internal class SpareDynamicMatrixDriver : IExtraDynamicDriver
    {
        const string SPARE_ID = "spare";

        IActionAutomater _actionEngine;
        IRaiseConditionsChanged _conditions;
        ICalculatedVariableManager _calculator;

        Window _parent;
        SparePanel _window;
        GenericMatrix _ui;

        Dictionary<string, DynamicControlTButtonDefinition> _buttons = new Dictionary<string, DynamicControlTButtonDefinition>();
        Dictionary<string, WatchVariable> _watches = new Dictionary<string, WatchVariable>();

        string _rawText = "";
        string _folder = "";

        public SpareDynamicMatrixDriver(Window parent, IActionAutomater automater, IRaiseConditionsChanged conditions, ICalculatedVariableManager calculator)
        {
            _parent = parent;
            _actionEngine = automater;
            _conditions = conditions;
            _calculator = calculator;

            SetupUI();
            _conditions.OnConditionalsChanged += _conditions_OnConditionalsChanged;
        }

        private void SetupUI()
        {
            _window = new SparePanel();
            // if we keep closing and re-opening, this probably will eventually lead to a memory leak
            _window.OnReleaseFocus += _window_OnReleaseFocus;
            _ui = _window.matrix;
            _ui.OnButtonClick += _ui_OnButtonClick;

            ConfigureControls(_rawText, _folder, true, _calculator);
        }

        private void _window_OnReleaseFocus(object sender, EventArgs e)
        {
            _parent.Focus();
        }

        private async void _ui_OnButtonClick(object sender, (int x, int y) e)
        {
            _parent?.Focus();
            // execute the actions as required
            if (_buttons.TryGetValue($"{e.x},{e.y}", out var btn))
            {
                foreach (var action in btn.Actions)
                {
                    await _actionEngine.PerformAutomationAction(action, SPARE_ID);
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
                    bool run = SumOfProductExpression.EvaluateExpression(dv.CondExpr, cstate);
                    if (run)
                    {
                        if (!_ui.CheckAccess())
                        {
                            _ui.Dispatcher.Invoke(() =>
                            {
                                _ui.UpdateButton(btn.X, btn.Y, dv, _calculator);
                            });
                        }
                        else
                        {
                            _ui.UpdateButton(btn.X, btn.Y, dv, _calculator);
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
            _rawText = rawText;
            _folder = resourcefolder;
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
            calculator.ReleaseVariables(SPARE_ID);

            // run all global actions?
            Task.Run(async () =>
            {
                foreach (var initGlobalAction in globalActions)
                {
                    await _actionEngine.PerformAutomationAction(initGlobalAction, SPARE_ID);
                }
            }).Wait(); // ewwww!

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
            return true;
        }

        public void ShowUI()
        {
            if (_window == null)
            {
                SetupUI();
                _window.Show();
            }
            else if (_window?.IsVisible == false)
            {
                SetupUI();
                _ui = _window.matrix;
                _window.Show();
            }
            else
            {
                _window.Show();
            }
        }

        public void Focus()
        {
            if (_window?.IsVisible == true)
            {
                _window.Focus();
            }
            else
            {
                SetupUI();
                _window.Show();
                _window.Focus();
            }
        }

        public void Repaint()
        {
            DrawButtons();
        }
    }

}
