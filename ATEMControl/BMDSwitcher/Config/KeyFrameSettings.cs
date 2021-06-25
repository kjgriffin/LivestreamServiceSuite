using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher.Config
{
    public class KeyFrameSettings
    {
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double SizeX { get; set; }
        public double SizeY { get; set; }

        public KeyFrameSettings Copy()
        {
            return new KeyFrameSettings()
            {
                PositionX = this.PositionX,
                PositionY = this.PositionY,
                SizeX = this.SizeX,
                SizeY = this.SizeX,
            };
        }
    }
}
