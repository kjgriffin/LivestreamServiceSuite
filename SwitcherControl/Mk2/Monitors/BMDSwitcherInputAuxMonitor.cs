using BMDSwitcherAPI;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherInputAux(_BMDSwitcherInputAuxEventType eventType);
    internal class BMDSwitcherInputAuxMonitor :IBMDSwitcherInputAuxCallback
    {
        public event Notify_IBMDSwitcherInputAux OnNotification;
        public void Notify(_BMDSwitcherInputAuxEventType eventType)
        {
            OnNotification?.Invoke(eventType);
        }
    }
}