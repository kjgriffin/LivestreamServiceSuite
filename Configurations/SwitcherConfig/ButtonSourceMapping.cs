using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher.Config
{
    public class ButtonSourceMapping
    {
        public string KeyName { get; set; }
        public int ButtonId { get; set; }
        public int PhysicalInputId { get; set; }
        public string ButtonName { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
    }
}
