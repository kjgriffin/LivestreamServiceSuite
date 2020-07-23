using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter
{
    public class MixEffectBlockMonitor : IBMDSwitcherMixEffectBlockCallback
    {
        public event SwitcherEventHandler ProgramInputChanged;
        public event SwitcherEventHandler PreviewInputChanged;
        public event SwitcherEventHandler FateToBlackFullyChanged;
        

        public MixEffectBlockMonitor()
        {

        }

        void IBMDSwitcherMixEffectBlockCallback.Notify(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            switch(eventType)
            {
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeProgramInputChanged:
                    ProgramInputChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewInputChanged:
                    PreviewInputChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeFadeToBlackFullyBlackChanged:
                    FateToBlackFullyChanged?.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }

    }
}
