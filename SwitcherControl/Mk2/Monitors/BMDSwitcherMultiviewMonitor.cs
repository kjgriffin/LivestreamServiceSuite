using BMDSwitcherAPI;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherMultiview(_BMDSwitcherMultiViewEventType eventType, int window);
    internal class BMDSwitcherMultiviewMonitor : IBMDSwitcherMultiViewCallback
    {
        public event Notify_IBMDSwitcherMultiview OnNotification;
        public void Notify(_BMDSwitcherMultiViewEventType eventType, int window)
        {
            OnNotification(eventType, window);
        }
    }
}