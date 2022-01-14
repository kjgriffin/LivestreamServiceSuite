using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitcherControl.Mk2.API
{
    internal class APIPerform_ME1PresetSelect
    {
        long presetValue;

        public APIPerform_ME1PresetSelect(long phySourceID)
        {
            presetValue = phySourceID;
        }

        public QueuedSwitcherWorkItem Command { get => new QueuedSwitcherWorkItem() { APIInvokeAction = PerformSelect }; }

        private void PerformSelect(BMDSwitcherAPIInterface api)
        {
            api.mixEffect1Monitor.OnNotification += MixEffect1Monitor_OnNotification;
            api?.mixEffect1?.SetPreviewInput(presetValue);
        }

        private void MixEffect1Monitor_OnNotification(_BMDSwitcherMixEffectBlockEventType eventType)
        {
            if (eventType == _BMDSwitcherMixEffectBlockEventType.bmdSwitcherMixEffectBlockEventTypePreviewInputChanged)
            {
            }
        }
    }
}
