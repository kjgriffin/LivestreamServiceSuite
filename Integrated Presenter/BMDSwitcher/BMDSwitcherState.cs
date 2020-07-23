using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    public class BMDSwitcherState
    {

        public long PresetID { get; set; }
        public long ProgramID { get; set; }

        public bool USK1OnAir { get; set; }

        public bool DSK1OnAir { get; set; }
        public bool DSK1Tie { get; set; }
        public bool DSK2OnAir { get; set; }
        public bool DSK2Tie { get; set; }

        public bool FTB { get; set; }


        public void SetDefault()
        {
            PresetID = -1;
            ProgramID = -1;
            DSK1OnAir = false;
            USK1OnAir = false;
            DSK1Tie = false;
            DSK2OnAir = false;
            DSK2Tie = false;
            FTB = false;
        }

    }
}
