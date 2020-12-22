using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    class UpstreamKeyMonitor : IBMDSwitcherKeyCallback
    {
        public event SwitcherEventHandler UpstreamKeyOnAirChanged;
        public event SwitcherEventHandler UpstreamKeyFillChanged;
        public event SwitcherEventHandler UpstreamKeyTypeChanged;

        void IBMDSwitcherKeyCallback.Notify(_BMDSwitcherKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputFillChanged:
                    UpstreamKeyFillChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeOnAirChanged:
                    UpstreamKeyOnAirChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeTypeChanged:
                    UpstreamKeyTypeChanged?.Invoke(this, null);
                    break;


            }
        }
    }
}
