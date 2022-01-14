using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcher(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode corevideoMode);
    internal class BMDSwitcherMonitor : IBMDSwitcherCallback
    {
        public event Notify_IBMDSwitcher OnNotification;
        public void Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
        {
            OnNotification?.Invoke(eventType, coreVideoMode);
        }
    }
}
