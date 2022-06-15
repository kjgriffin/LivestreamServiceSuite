using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcherControl.BMDSwitcher
{
    public class MultiViewMonitor : IBMDSwitcherMultiViewCallback
    {
        void IBMDSwitcherMultiViewCallback.Notify(_BMDSwitcherMultiViewEventType eventType, int window)
        {
            switch (eventType)
            {
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeLayoutChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeWindowChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsVuMeterChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterEnabledChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeVuMeterOpacityChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeCurrentInputSupportsSafeAreaChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeSafeAreaEnabledChanged:
                    break;
                case _BMDSwitcherMultiViewEventType.bmdSwitcherMultiViewEventTypeProgramPreviewSwappedChanged:
                    break;
                default:
                    break;
            }
        }
    }
}
