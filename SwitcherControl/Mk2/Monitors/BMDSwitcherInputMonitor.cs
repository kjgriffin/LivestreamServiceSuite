using BMDSwitcherAPI;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherInput(_BMDSwitcherInputEventType eventType);
    internal class BMDSwitcherInputMonitor : IBMDSwitcherInputCallback
    {
        public event Notify_IBMDSwitcherInput OnNotification;
        public void Notify(_BMDSwitcherInputEventType eventType)
        {
            OnNotification?.Invoke(eventType);
        }
    }
}