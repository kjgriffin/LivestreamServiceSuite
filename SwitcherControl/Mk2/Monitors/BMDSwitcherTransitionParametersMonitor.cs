using BMDSwitcherAPI;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherTransitionParameters(_BMDSwitcherTransitionParametersEventType eventType);
    internal class BMDSwitcherTransitionParametersMonitor : IBMDSwitcherTransitionParametersCallback
    {
        public Notify_IBMDSwitcherTransitionParameters OnNotification;
        public void Notify(_BMDSwitcherTransitionParametersEventType eventType)
        {
            OnNotification?.Invoke(eventType);
        }
    }
}