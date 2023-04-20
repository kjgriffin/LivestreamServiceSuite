using BMDSwitcherAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
