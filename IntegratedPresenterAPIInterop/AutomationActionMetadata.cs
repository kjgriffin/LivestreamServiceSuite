
using System.Collections.Generic;

namespace IntegratedPresenterAPIInterop
{
    public struct AutomationActionMetadata
    {
        public int NumArgs;
        public AutomationActions Action;
        public string ActionName;
        public List<AutomationActionArgType>? OrderedArgTypes;

        public AutomationActionMetadata((int nargs, AutomationActions action, string name, List<AutomationActionArgType>? argtypes) stuff)
        {
            NumArgs = stuff.nargs;
            Action = stuff.action;
            ActionName = stuff.name;
            OrderedArgTypes = stuff.argtypes;
        }

        public static implicit operator AutomationActionMetadata((int nargs, AutomationActions action, string name, List<AutomationActionArgType>? argtypes) stuff)
        {
            return new AutomationActionMetadata(stuff);
        }
    }

}
