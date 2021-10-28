using System;
using System.Collections.Generic;
using System.Text;
using BMDSwitcherAPI;

namespace IntegratedPresenter.BMDSwitcher
{
    class BMDSwitcherAuxMonitor : IBMDSwitcherInputAuxCallback
    {
        public event SwitcherEventHandler OnAuxInputChanged; 
        public void Notify(_BMDSwitcherInputAuxEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherInputAuxEventType.bmdSwitcherInputAuxEventTypeInputSourceChanged:
                    OnAuxInputChanged?.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }
    }
}
