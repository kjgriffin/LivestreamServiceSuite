using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{

    public delegate void SwitcherNextTransitionEventHandler(object sender);

    class SwitcherTransitionMonitor : IBMDSwitcherTransitionParametersCallback
    {

        public event SwitcherNextTransitionEventHandler OnNextTransitionSelectionChanged;

        void IBMDSwitcherTransitionParametersCallback.Notify(_BMDSwitcherTransitionParametersEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionStyleChanged:
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionStyleChanged:
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeTransitionSelectionChanged:
                    break;
                case _BMDSwitcherTransitionParametersEventType.bmdSwitcherTransitionParametersEventTypeNextTransitionSelectionChanged:
                    OnNextTransitionSelectionChanged?.Invoke(this);
                    break;
                default:
                    break;
            }
        }
    }
}
