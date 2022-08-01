using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcherControl.BMDSwitcher
{
    public delegate void SwitcherEventHandler(object sender, object args);
    public class SwitcherMonitor : IBMDSwitcherCallback
    {

        public event SwitcherEventHandler SwitcherDisconnected;

        public SwitcherMonitor()
        {

        }

        void IBMDSwitcherCallback.Notify(_BMDSwitcherEventType eventType, _BMDSwitcherVideoMode coreVideoMode)
        {
            if (eventType == _BMDSwitcherEventType.bmdSwitcherEventTypeDisconnected)
            {
                SwitcherDisconnected?.Invoke(this, null);
            }
        }
    }
}
