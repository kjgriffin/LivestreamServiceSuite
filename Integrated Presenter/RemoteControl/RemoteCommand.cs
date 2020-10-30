using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.RemoteControl
{
    class RemoteCommand
    {
        public string CmdName { get; set; }

        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();



    }
}
