using System;

namespace IntegratedPresenterAPIInterop
{

    public delegate void AutomationActionUpdateEventArgs(TrackedAutomationAction updatedAction);
    public class TrackedAutomationAction
    {
        public Guid ID { get; }
        public TrackedActionState State { get; set; }
        public TrackedActionRunType RunType { get; set; }
        public AutomationAction Action { get; set; }

        public TrackedAutomationAction(AutomationAction action, TrackedActionRunType runType)
        {
            Action = action;
            State = TrackedActionState.Ready;
            RunType = runType;
            ID = Guid.NewGuid();
        }

    }
}