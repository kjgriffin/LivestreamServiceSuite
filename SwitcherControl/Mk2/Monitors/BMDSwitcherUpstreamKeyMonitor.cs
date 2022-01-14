using BMDSwitcherAPI;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherUpstreamKey(_BMDSwitcherKeyEventType eventType);
    internal class BMDSwitcherUpstreamKeyMonitor : IBMDSwitcherKeyCallback
    {
        public event Notify_IBMDSwitcherUpstreamKey OnNotification;
        public void Notify(_BMDSwitcherKeyEventType eventType)
        {
            OnNotification?.Invoke(eventType);
        }
    }
}