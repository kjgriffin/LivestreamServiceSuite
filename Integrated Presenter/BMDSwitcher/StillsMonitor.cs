using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    class StillsMonitor: IBMDSwitcherStillsCallback
    {

        public Action OnTransferCompleted { get; set; }
        public Action OnTransferFailled { get; set; }
        public Action OnTransferCancelled { get; set; }

        public Action OnLockIdle { get; set; }

        void IBMDSwitcherStillsCallback.Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int index)
        {
            switch (eventType)
            {
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeValidChanged:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeNameChanged:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeHashChanged:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioValidChanged:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioNameChanged:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeAudioHashChanged:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeLockBusy:
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeLockIdle:
                    OnLockIdle?.Invoke();
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted:
                    OnTransferCompleted?.Invoke();
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCancelled:
                    OnTransferCancelled?.Invoke();
                    break;
                case _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferFailed:
                    OnTransferFailled?.Invoke();
                    break;
                default:
                    break;
            }
        }
    }
}
