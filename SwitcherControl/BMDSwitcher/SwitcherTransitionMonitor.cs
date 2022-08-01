using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcherControl.BMDSwitcher
{

    public delegate void SwitcherNextTransitionEventHandler(object sender);

    class SwitcherTransitionMonitor : IBMDSwitcherTransitionParametersCallback
    {

        public event SwitcherNextTransitionEventHandler OnTransitionSelectionChanged;

        void IBMDSwitcherTransitionParametersCallback.Notify(_BMDSwitcherTransitionParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionStyleChanged:
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionStyleChanged:
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionSelectionChanged:
                    OnTransitionSelectionChanged?.Invoke(this);
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionSelectionChanged:
                    break;
                default:
                    break;
            }
        }
    }
}
