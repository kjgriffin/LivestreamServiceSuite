using Configurations.SwitcherConfig;

using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.BMDSwitcher.Config
{
    public class BMDUSKSettings
    {
        //public int IsDVE { get; set; }
        //public int IsChroma { get; set; }

        public int DefaultKeyType { get; set; }

        public BMDUSKPATTERNSettings PATTERNSettings { get; set; } = new BMDUSKPATTERNSettings();
        public BMDUSKDVESettings PIPSettings { get; set; } = new BMDUSKDVESettings();
        public BMDUSKChromaSettings ChromaSettings { get; set; } = new BMDUSKChromaSettings();
    }
}
