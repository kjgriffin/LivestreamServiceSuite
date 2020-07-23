using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    class UpstreamKeyMonitor : IBMDSwitcherKeyCallback
    {
        public event SwitcherEventHandler UpstreamKeyOnAirChanged;

        void IBMDSwitcherKeyCallback.Notify(_BMDSwitcherKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeOnAirChanged:
                    UpstreamKeyOnAirChanged?.Invoke(this, null);
                    break;

            }
        }
    }
}
