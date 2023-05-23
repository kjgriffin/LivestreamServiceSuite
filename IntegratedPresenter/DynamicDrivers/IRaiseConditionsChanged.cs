using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;

namespace Integrated_Presenter.DynamicDrivers
{
    internal interface IRaiseConditionsChanged
    {
        event EventHandler OnConditionalsChanged;
        Dictionary<string, bool> GetConditionals(Dictionary<string, WatchVariable> watches);
    }

}
