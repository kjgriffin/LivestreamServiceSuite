﻿using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    class SwitcherMediaPoolLockMonitor : IBMDSwitcherLockCallback
    {

        public Action OnLockObtained { get; set; }

        void IBMDSwitcherLockCallback.Obtained()
        {
            OnLockObtained?.Invoke();
        }
    }
}
