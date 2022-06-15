using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Text;

namespace SwitcherControl.BMDSwitcher
{
    public class MixEffectBlockMonitor : IBMDSwitcherMixEffectBlockCallback
    {
        public event SwitcherEventHandler ProgramInputChanged;
        public event SwitcherEventHandler PreviewInputChanged;
        public event SwitcherEventHandler FateToBlackFullyChanged;
        public event SwitcherEventHandler InTransitionChanged;
        public event SwitcherEventHandler TransitionPositionChanged;
        public event SwitcherEventHandler TransitionFramesRemainingChanged;


        public MixEffectBlockMonitor()
        {

        }

        void IBMDSwitcherMixEffectBlockCallback.Notify(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            switch (eventType)
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
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionFramesRemainingChanged:
                    TransitionFramesRemainingChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeTransitionPositionChanged:
                    TransitionPositionChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypeInTransitionChanged:
                    InTransitionChanged?.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }

    }
}
