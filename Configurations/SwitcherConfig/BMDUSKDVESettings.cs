using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.BMDSwitcher.Config
{

    public class PIPPlaceSettings
    {
        public double MaskTop { get; set; }
        public double MaskBottom { get; set; }
        public double MaskLeft { get; set; }
        public double MaskRight { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }

        public override string ToString()
        {
            return $"X{PosX};Y{PosY};SX{ScaleX};SY{ScaleY};ML{MaskLeft};MR{MaskRight};MT{MaskTop};MB{MaskBottom};";
        }

        public BMDUSKDVESettings PlaceOverride(BMDUSKDVESettings current)
        {
            current.MaskTop = (float)MaskTop;
            current.MaskBottom = (float)MaskBottom;
            current.MaskLeft = (float)MaskLeft;
            current.MaskRight = (float)MaskRight;
            current.IsMasked = (MaskTop != 0 || MaskBottom != 0 || MaskLeft != 0 || MaskRight != 0) ? 1 : 0;
            current.Current.PositionX = PosX;
            current.Current.PositionY = PosY;
            current.Current.SizeX = ScaleX;
            current.Current.SizeY = ScaleY;
            return current;
        }
    }

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
