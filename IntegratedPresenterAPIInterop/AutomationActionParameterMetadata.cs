using VariableMarkupAttributes;

namespace IntegratedPresenterAPIInterop
{
    public class AutomationActionParameterMetadata
    {
        public AutomationActionParameterMetadata(string argName, AutomationActionArgType argType, ExpectedVariableContents paramValueHints = ExpectedVariableContents.NONE, List<(string item, string description)> staticHints = null)
        {
            ArgName = argName;
            ArgType = argType;
            ParamValueHints = paramValueHints;
            StaticHints = staticHints ?? new List<(string item, string description)>();
        }

        public AutomationActionArgType ArgType { get; set; }
        public string ArgName { get; set; }
        public ExpectedVariableContents ParamValueHints { get; set; }
        public List<(string item, string description)> StaticHints { get; set; } = new List<(string item, string description)>();

    }

}
