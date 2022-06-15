using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcherControl.BMDSwitcher
{
    class DownstreamKeyMonitor : IBMDSwitcherDownstreamKeyCallback
    {

        public event SwitcherEventHandler OnAirChanged;
        public event SwitcherEventHandler TieChanged;

        public void Notify(_BMDSwitcherDownstreamKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeTieChanged:
                    TieChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherDownstreamKeyEventType.bmdSwitcherDownstreamKeyEventTypeOnAirChanged:
                    OnAirChanged?.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }
    }
}
