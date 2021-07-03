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
    }
}
