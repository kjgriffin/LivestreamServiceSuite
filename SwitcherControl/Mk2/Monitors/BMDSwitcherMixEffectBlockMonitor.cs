using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitcherControl.Mk2.Monitors
{
    public delegate void Notify_IBMDSwitcherMixEffectBlock(_BMDSwitcherMixEffectBlockEventType eventType);
    internal class BMDSwitcherMixEffectBlockMonitor : IBMDSwitcherMixEffectBlockCallback
    {
        public event Notify_IBMDSwitcherMixEffectBlock OnNotification;
        public void Notify(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            OnNotification?.Invoke(eventType);
        }
    }
}
