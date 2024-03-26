namespace IntegratedPresenterAPIInterop
{
    public class AutomationActionMetadata
    {
        public int NumArgs { get => OrderedParameters.Count; }
        public AutomationActions Action { get; set; }
        public string ActionName { get; set; }
        public List<AutomationActionParameterMetadata> OrderedParameters { get; set; } = new List<AutomationActionParameterMetadata>();

        public AutomationActionMetadata((AutomationActions action, string name) stuff)
        {
            Action = stuff.action;
            ActionName = stuff.name;
            OrderedParameters = new List<AutomationActionParameterMetadata>();
        }

        public AutomationActionMetadata(AutomationActions action, string actionName, List<AutomationActionParameterMetadata> paramdefs)
        {
            Action = action;
            ActionName = actionName;
            OrderedParameters = paramdefs ?? new List<AutomationActionParameterMetadata>();
        }

        public static implicit operator AutomationActionMetadata((AutomationActions action, string name) stuff)
        {
            return new AutomationActionMetadata(stuff);
        }
    }

}
