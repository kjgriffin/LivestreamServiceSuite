using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public struct AutomationActionMetadata
    {
        public int NumArgs;
        public AutomationActions Action;
        public string ActionName;
        public List<AutomationActionArgType>? OrderedArgTypes;
        public ExpectedVariableContents ParamaterContents;

        public AutomationActionMetadata((int nargs, AutomationActions action, string name, List<AutomationActionArgType>? argtypes, ExpectedVariableContents expectedContents) stuff)
        {
            NumArgs = stuff.nargs;
            Action = stuff.action;
            ActionName = stuff.name;
            OrderedArgTypes = stuff.argtypes;
            ParamaterContents = stuff.expectedContents;
        }

        public static implicit operator AutomationActionMetadata((int nargs, AutomationActions action, string name, List<AutomationActionArgType>? argtypes, ExpectedVariableContents expectedContents) stuff)
        {
            return new AutomationActionMetadata(stuff);
        }
    }

}
