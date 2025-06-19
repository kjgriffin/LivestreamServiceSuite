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
    internal class StateObject
    {
        public object Value { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public Type Type { get; set; }
    }

    internal class ConditionalsVariablesCalculator : IRaiseConditionsChanged, ICalculatedVariableManager
    {
        IPresentationProvider _pres;
        ISwitcherStateProvider _switcherState;
        IUserConditionProvider _userConditions;

        public event EventHandler OnConditionalsChanged;


        Dictionary<string, CalculatedVariable> calculatedVariables = new Dictionary<string, CalculatedVariable>();
        Dictionary<string, StateObject> states = new Dictionary<string, StateObject>();

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
                //presVars = VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(_pres);
                presVars = _pres.GetExposedVariables();
            }
            // report pilot state
            Dictionary<string, ExposedVariable> pilotVars = new Dictionary<string, ExposedVariable>();

            // this is a bit wack... but if we've gone a head and allowed runtime-variables they ought be watchable
            // that probably exposes all sorts of dangers: conditional execution of writecomputedval that could loop endlessley??
            // but perhaps it's worth the gain...
            Dictionary<string, ExposedVariable> calculatedVars = new Dictionary<string, ExposedVariable>();
            // build them up
            foreach (var cVar in calculatedVariables)
            {
                // TODO: calculatedVariables are scoped by owner (panel driver)
                // is that implicit here??
                // perhaps we ought prefix these??
                var scopedName = cVar.Value.Owner + "." + cVar.Value.VName;
                calculatedVars.Add(scopedName, new ExposedVariable
                {
                    Metadata = null, // TODO: this may be dangerous
                    Path = scopedName,
                    TypeInfo = cVar.Value.VarType,
                    Value = cVar.Value.LastVal,
                });
            }

            return new Dictionary<string, ExposedVariable>(switcherVars.Concat(presVars).Concat(pilotVars).Concat(calculatedVars));
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


        public void InitializeVariable(string owner, string name, AutomationActionArgType type, string initialValue)
        {
            // allow multi-init: probably
            if (!calculatedVariables.ContainsKey(name))
            {

                CalculatedVariable v = new CalculatedVariable
                {
                    InitVal = CalculatedVariable.ParseVariableValue(type, initialValue),
                    IsTracking = false,
                    LastVal = CalculatedVariable.ParseVariableValue(type, initialValue),
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
            keys = states.Values.Where(x => x.Owner == owner).Select(x => x.Name).ToList();
            foreach (var item in keys)
            {
                states.Remove(item);
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
            value = default(T);

            if (calculatedVariables.TryGetValue(name, out var vCalc))
            {
                // process equation
                if (typeof(T) == typeof(int) && vCalc.VarType == AutomationActionArgType.Integer)
                {
                    // yeah.... so int's are 32 bit and longs are 64 bit
                    // so just to be safe do it with longs (even if we describe it as an int)
                    // because some bmd switcher state uses longs
                    if (vCalc.LastVal is long)
                    {
                        dynamic lconv = unchecked((int)((long)vCalc.LastVal));
                        value = lconv;
                    }
                    else if (vCalc.LastVal is int)
                    {
                        dynamic x = (int)(vCalc.LastVal);
                        value = x;
                    }
                    else
                    {
                        // not sure we sould really do this?
                        dynamic x = 0;
                        value = x;
                    }
                    return true;
                }
                if (typeof(T) == typeof(string) && vCalc.VarType == AutomationActionArgType.String)
                {
                    value = (T)vCalc.LastVal;
                    return true;
                }
                if (typeof(T) == typeof(double) && vCalc.VarType == AutomationActionArgType.Double)
                {
                    value = (T)vCalc.LastVal;
                    return true;
                }
                if (typeof(T) == typeof(bool) && vCalc.VarType == AutomationActionArgType.Boolean)
                {
                    value = (T)vCalc.LastVal;
                    return true;
                }
            }
            return false;

        }

        public void SetupVariableTrack(string name, string trackingTarget)
        {
            if (calculatedVariables.TryGetValue(name, out var vCalc))
            {
                vCalc.IsTracking = true;
                vCalc.VSourcePath = trackingTarget;
                NotifyOfChange();
            }
        }

        public void ReleaseVariableTrack(string name)
        {
            if (calculatedVariables.TryGetValue(name, out var vCalc))
            {
                vCalc.IsTracking = false;
            }
        }

        public bool TryGetVariableInfo(string wvalname, out CalculatedVariable vinfo)
        {
            return calculatedVariables.TryGetValue(wvalname, out vinfo);
        }

        public void PurgeVariable(string owner, string name)
        {
            if (calculatedVariables.TryGetValue(name, out var vCalc) && vCalc.Owner == owner)
            {
                calculatedVariables.Remove(name);
            }
        }

        public void StoreState<T>(string owner, string name, T state)
        {
            StateObject oState = new StateObject
            {
                Name = name,
                Owner = owner,
                Type = typeof(T),
                Value = state,
            };
            states[name] = oState;
        }

        public bool RecallState<T>(string owner, string name, out T state)
        {
            state = default(T);
            if (states.TryGetValue(name, out var sobj) && sobj?.Owner == owner && sobj.Type == typeof(T))
            {
                state = (T)sobj.Value;
                return true;
            }
            return false;
        }
    }

}
