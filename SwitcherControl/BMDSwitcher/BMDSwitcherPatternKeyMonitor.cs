using BMDSwitcherAPI;

using System;

namespace SwitcherControl.BMDSwitcher
{
    internal class BMDSwitcherPatternKeyMonitor : IBMDSwitcherKeyPatternParametersCallback
    {
        public event EventHandler OnAnyChange;

        public void Notify(_BMDSwitcherKeyPatternParametersEventType eventType)
        {
            OnAnyChange?.Invoke(this, null);
        }
    }
}
