using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher.Config
{
    public class BMDDSKSettings
    {
        public int InputFill { get; set; }
        public int InputCut { get; set; }
        public int Rate { get; set; }
        public int IsPremultipled { get; set; }
        public double Clip { get; set; }
        public double Gain { get; set; }
        public int Invert { get; set; }
        public int IsMasked { get; set; }
        public float MaskTop { get; set; }
        public float MaskBottom { get; set; }
        public float MaskLeft { get; set; }
        public float MaskRight { get; set; }
    }
}
