using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    public class InputMonitor : IBMDSwitcherInputCallback
    {
        public event SwitcherEventHandler LongNameChanged;
        public event SwitcherEventHandler ShortNameChanged;

        public IBMDSwitcherInput Input { get; private set; }

        public InputMonitor(IBMDSwitcherInput input)
        {
            Input = input;
        }

        void IBMDSwitcherInputCallback.Notify(_BMDSwitcherInputEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeLongNameChanged:
                    LongNameChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherInputEventType.bmdSwitcherInputEventTypeShortNameChanged:
                    ShortNameChanged?.Invoke(this, null);
                    break;
            }
        }

    }
}
