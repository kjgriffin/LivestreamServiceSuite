using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.BMDSwitcher.Config
{
    public class BMDUSKDVESettings
    {
        public int DefaultFillSource { get; set; }
        public int IsBordered { get; set; }
        public int IsMasked { get; set; }
        public float MaskTop { get; set; }
        public float MaskBottom { get; set; }
        public float MaskLeft { get; set; }
        public float MaskRight { get; set; }
        public KeyFrameSettings Current { get; set; }
        public KeyFrameSettings KeyFrameA { get; set; }
        public KeyFrameSettings KeyFrameB { get; set; }

        public BMDUSKDVESettings Copy()
        {
            return new BMDUSKDVESettings()
            {
                DefaultFillSource = this.DefaultFillSource,
                IsBordered = this.IsBordered,
                IsMasked = this.IsMasked,
                MaskTop = this.MaskTop,
                MaskBottom = this.MaskBottom,
                MaskLeft = this.MaskLeft,
                MaskRight = this.MaskRight,
                Current = this.Current?.Copy(),
                KeyFrameA = this.KeyFrameA?.Copy(),
                KeyFrameB = this.KeyFrameB?.Copy(),
            };
        }

    }
}
