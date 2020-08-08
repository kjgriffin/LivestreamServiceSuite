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

        public bool IsDifferentShot(BMDSwitcherState oldstate)
        {
            // for now only look at program output
            if (this.ProgramID != oldstate.ProgramID)
            {
                return true;
            }
            return false;
        }

        public BMDSwitcherState Copy()
        {
            return new BMDSwitcherState() { 
                PresetID = this.PresetID,
                ProgramID = this.ProgramID,
                USK1OnAir = this.USK1OnAir,
                DSK1OnAir = this.DSK1OnAir,
                DSK1Tie = this.DSK1Tie,
                DSK2OnAir = this.DSK2OnAir,
                DSK2Tie = this.DSK2Tie,
                FTB = this.FTB
            };

        }

    }
}
