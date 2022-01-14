using BMDSwitcherAPI;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherDownstreamKey(_BMDSwitcherDownstreamKeyEventType eventType);
    internal class BMDSwitcherDownstreamKeyMonitor : IBMDSwitcherDownstreamKeyCallback
    {
        public event Notify_IBMDSwitcherDownstreamKey OnNotification;
        public void Notify(_BMDSwitcherDownstreamKeyEventType eventType)
        {
            OnNotification?.Invoke(eventType);
        }
    }
}