using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.BMDSwitcher.Config
{
    public class BMDMultiviewerSettings
    {
        public int Layout { get; set; }
        public int Window2 { get; set; }
        public int Window3 { get; set; }
        public int Window4 { get; set; }
        public int Window5 { get; set; }
        public int Window6 { get; set; }
        public int Window7 { get; set; }
        public int Window8 { get; set; }
        public int Window9 { get; set; }

        public List<int> ShowVUMetersOnWindows { get; set; } = new List<int>();

        public static BMDMultiviewerSettings Default()
        {
            return new BMDMultiviewerSettings()
            {
                Layout = (int)_BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop, // 12
                Window2 = 8,
                Window3 = 7,
                Window4 = 6,
                Window5 = 5,
                Window6 = 4,
                Window7 = 3,
                Window8 = 2,
                Window9 = 1,
                ShowVUMetersOnWindows = new List<int>() // by defaul don't show vu meters
            };
        }

    }
}
