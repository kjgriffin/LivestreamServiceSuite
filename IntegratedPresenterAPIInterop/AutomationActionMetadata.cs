using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public class AutomationActionMetadata
    {
        public int NumArgs { get; set; }
        public AutomationActions Action { get; set; }
        public string ActionName { get; set; }
        public List<(AutomationActionArgType type, string id)>? OrderedArgTypes { get; set; }
        public ExpectedVariableContents ParamaterContents { get; set; }

        public AutomationActionMetadata((int nargs, AutomationActions action, string name, List<(AutomationActionArgType, string)>? argtypes, ExpectedVariableContents expectedContents) stuff)
        {
            NumArgs = stuff.nargs;
            Action = stuff.action;
            ActionName = stuff.name;
            OrderedArgTypes = stuff.argtypes;
            ParamaterContents = stuff.expectedContents;
        }

        public AutomationActionMetadata(int numArgs, AutomationActions action, string actionName, List<(AutomationActionArgType, string)>? orderedArgTypes, ExpectedVariableContents paramaterContents)
        {
            NumArgs = numArgs;
            Action = action;
            ActionName = actionName;
            OrderedArgTypes = orderedArgTypes;
            ParamaterContents = paramaterContents;
        }

        public static implicit operator AutomationActionMetadata((int nargs, AutomationActions action, string name, List<(AutomationActionArgType, string)>? argtypes, ExpectedVariableContents expectedContents) stuff)
        {
            return new AutomationActionMetadata(stuff);
        }
    }

}
