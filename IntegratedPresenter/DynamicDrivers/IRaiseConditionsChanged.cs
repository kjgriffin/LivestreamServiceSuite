using Integrated_Presenter.Automation;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.Windows.Markup;

using VariableMarkupAttributes;

namespace Integrated_Presenter.DynamicDrivers
{
    internal interface IRaiseConditionsChanged : IAutomationConditionProvider
    {
        event EventHandler OnConditionalsChanged;
        //Dictionary<string, bool> GetConditionals(Dictionary<string, WatchVariable> watches);
    }

    

}
