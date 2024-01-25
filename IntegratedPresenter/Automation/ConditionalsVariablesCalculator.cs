using Integrated_Presenter.DynamicDrivers;

using IntegratedPresenter.BMDSwitcher.Mock;

using IntegratedPresenterAPIInterop;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VariableMarkupAttributes;
using VariableMarkupAttributes.Attributes;

namespace Integrated_Presenter.Automation
{
    internal class ConditionalsVariablesCalculator : IRaiseConditionsChanged, ICalculatedVariableManager
    {
        IPresentationProvider _pres;
        ISwitcherStateProvider _switcherState;
        IUserConditionProvider _userConditions;

        public event EventHandler OnConditionalsChanged;


        Dictionary<string, CalculatedVariable> calculatedVariables = new Dictionary<string, CalculatedVariable>();

        public ConditionalsVariablesCalculator(IPresentationProvider pres, ISwitcherStateProvider switcherState, IUserConditionProvider userConditions)
        {
            _pres = pres;
            _switcherState = switcherState;
            _userConditions = userConditions;
        }

        public void NotifyOfChange()
        {
            // update calculated variables??
            UpdateCalculatedVariables();

            OnConditionalsChanged?.Invoke(this, EventArgs.Empty);
        }

        private Dictionary<string, ExposedVariable> GetExposedVariables()
        {
            // currently get switcher state
            Dictionary<string, ExposedVariable> switcherVars = VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(_switcherState.switcherState);
            // report presentation state
            Dictionary<string, ExposedVariable> presVars = new Dictionary<string, ExposedVariable>();
            if (_pres != null)
            {
                presVars = VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(_pres);
            }
            // report pilot state
            Dictionary<string, ExposedVariable> pilotVars = new Dictionary<string, ExposedVariable>();

            return new Dictionary<string, ExposedVariable>(switcherVars.Concat(presVars).Concat(pilotVars));
        }

        public Dictionary<string, bool> GetConditionals(Dictionary<string, WatchVariable> externalWatches)
        {
            //var conditions = new Dictionary<string, bool>
            //{
            //    ["1"] = _Cond1.Value,
            //    ["2"] = _Cond2.Value,
            //    ["3"] = _Cond3.Value,
            //    ["4"] = _Cond4.Value,
            //};
            var conditions = _userConditions.GetActiveUserConditions();

            // Assumes that the curent state of the switcher is acurate
            // i.e. doesn't immediately re-poll... perhaps this is ok??
            // effectively just make sure to script delays sufficient to process anything we might depend on

            // extract required switcher state to satisfy requests
            var exposedVariables = GetExposedVariables(); // VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(switcherState);

            // try to find each requested value
            if (externalWatches != null)
            {
                foreach (var watch in externalWatches)
                {
                    bool evaluation = false;
                    // use reflection to find a matching value
                    if (exposedVariables.TryGetValue(watch.Value.VPath, out var eVal))
                    {
                        dynamic val = eVal.Value;
                        // process equation
                        switch (watch.Value.VType)
                        {
                            case AutomationActionArgType.Integer:
                                // yeah.... so int's are 32 bit and longs are 64 bit
                                // so just to be safe do it with longs (even if we describe it as an int)
                                // because some bmd switcher state uses longs
                                evaluation = (long)val == (long)watch.Value.ExpectedVal;
                                break;
                            case AutomationActionArgType.String:
                                evaluation = (string)val == (string)watch.Value.ExpectedVal;
                                break;
                            case AutomationActionArgType.Double:
                                evaluation = (double)val == (double)watch.Value.ExpectedVal;
                                break;
                            case AutomationActionArgType.Boolean:
                                evaluation = (bool)val == (bool)watch.Value.ExpectedVal;
                                break;
                        }
                    }

                    conditions[watch.Key] = evaluation;
                }
            }


            return conditions;

        }

        public void InitializeVariable(string owner, string name, AutomationActionArgType type, object initialValue)
        {
            // allow multi-init: probably
            if (!calculatedVariables.ContainsKey(name))
            {
                CalculatedVariable v = new CalculatedVariable
                {
                    InitVal = initialValue,
                    IsTracking = false,
                    LastVal = initialValue,
                    VarType = type,
                    VName = name,
                    Owner = owner,
                };
                calculatedVariables[name] = v;
            }
        }

        public void ReleaseVariables(string owner)
        {
            // find all vars
            var keys = calculatedVariables.Values.Where(x => x.Owner == owner).Select(x => x.VName).ToList();
            foreach (var item in keys)
            {
                calculatedVariables.Remove(item);
            }
        }

        private void UpdateCalculatedVariables()
        {
            // find calculated ones
            // get all ones in active tracking mode
            // update thier values
            var exposed = GetExposedVariables();
            foreach (var vCalc in calculatedVariables.Values.Where(v => v.IsTracking))
            {
                // try associate tracking
                if (exposed.TryGetValue(vCalc.VSourcePath, out var vSrc))
                {
                    // match types
                    if (vSrc.TypeInfo == vCalc.VarType)
                    {
                        // directly associate
                        vCalc.LastVal = vSrc.Value;
                    }
                }
            }
        }

        public void WriteVariableValue<T>(string name, T value)
        {
            if (calculatedVariables.TryGetValue(name, out var v))
            {
                switch (v.VarType)
                {
                    case AutomationActionArgType.Integer:
                        if (typeof(T) == typeof(int))
                        {
                            v.LastVal = value;
                        }
                        break;
                    case AutomationActionArgType.String:
                        if (typeof(T) == typeof(string))
                        {
                            v.LastVal = value;
                        }
                        break;
                    case AutomationActionArgType.Double:
                        if (typeof(T) == typeof(double))
                        {
                            v.LastVal = value;
                        }
                        break;
                    case AutomationActionArgType.Boolean:
                        if (typeof(T) == typeof(bool))
                        {
                            v.LastVal = value;
                        }
                        break;
                }
            }
        }

        public bool TryEvaluateVariableValue<T>(string name, out T value)
        {
            var exposedVariables = GetExposedVariables();
            value = default(T);

            if (calculatedVariables.TryGetValue(name, out var vCalc))
            {
                // process equation
                if (typeof(T) == typeof(int))
                {
                    // yeah.... so int's are 32 bit and longs are 64 bit
                    // so just to be safe do it with longs (even if we describe it as an int)
                    // because some bmd switcher state uses longs
                    dynamic x = (int)((long)vCalc.LastVal);
                    value = x;
                }
                if (typeof(T) == typeof(string))
                    value = (T)vCalc.LastVal;
                if (typeof(T) == typeof(double))
                    value = (T)vCalc.LastVal;
                if (typeof(T) == typeof(bool))
                    value = (T)vCalc.LastVal;

                return true;
            }
            return false;

        }

        public void SetupVariableTrack(string name, string trackingTarget)
        {
            if (calculatedVariables.TryGetValue(name, out var vCalc))
            {
                vCalc.IsTracking = true;
                vCalc.VSourcePath = trackingTarget;
            }
        }

        public void ReleaseVariableTrack(string name)
        {
            if (calculatedVariables.TryGetValue(name, out var vCalc))
            {
                vCalc.IsTracking = false;
            }
        }
    }

    class CalculatedVariable
    {
        public string VName { get; set; }
        public AutomationActionArgType VarType { get; set; }
        public object InitVal { get; set; }
        public object LastVal { get; set; }
        public bool IsTracking { get; set; }
        public string VSourcePath { get; set; }
        public string Owner { get; set; }
    }

}
