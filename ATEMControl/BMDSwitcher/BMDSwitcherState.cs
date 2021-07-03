using BMDSwitcherAPI;
using IntegratedPresenter.BMDSwitcher.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntegratedPresenter.BMDSwitcher
{
    public class BMDSwitcherState
    {

        public BMDSwitcherState()
        {
            SetDefault();
        }

        public bool InTransition { get; set; }
        public int TransitionFramesRemaining { get; set; }
        public double TransitionPosition { get; set; }

        public long PresetID { get; set; }
        public long ProgramID { get; set; }
        public long AuxID { get; set; }

        public bool USK1OnAir { get; set; }
        public long USK1FillSource { get; set; }

        public bool TransNextBackground { get; set; }
        public bool TransNextKey1 { get; set; }

        /// <summary>
        /// 1 = A, 2 = B, 0 = Full, -1 = other
        /// </summary>
        public int USK1KeyFrame { get; set; }

        /// <summary>
        /// 1 = DVE, 2 = Chroma
        /// </summary>
        public int USK1KeyType { get; set; }

        public bool DSK1OnAir { get; set; }
        public bool DSK1Tie { get; set; }
        public bool DSK2OnAir { get; set; }
        public bool DSK2Tie { get; set; }

        public bool FTB { get; set; }

        public BMDUSKChromaSettings ChromaSettings { get; set; } = new BMDUSKChromaSettings();
        public BMDUSKDVESettings DVESettings { get; set; } = new BMDUSKDVESettings();



        public void SetDefault()
        {
            InTransition = false;
            TransitionPosition = 0;
            TransitionFramesRemaining = 0;
            PresetID = -1;
            ProgramID = -1;
            AuxID = -1;
            DSK1OnAir = false;
            USK1OnAir = false;
            USK1KeyType = 1;
            DSK1Tie = false;
            DSK2OnAir = false;
            DSK2Tie = false;
            FTB = false;
            USK1FillSource = -1;
            USK1KeyFrame = -1;
            TransNextBackground = true;
            TransNextKey1 = false;
            ChromaSettings = new BMDUSKChromaSettings()
            {
                FillSource = 0,
                Gain = 0,
                Hue = 0,
                Lift = 0,
                Narrow = 0,
                YSuppress = 0
            };
            DVESettings = new BMDUSKDVESettings()
            {
                Current = new KeyFrameSettings()
                {
                    PositionX = 0,
                    PositionY = 0,
                    SizeX = 0,
                    SizeY = 0
                },
                DefaultFillSource = 0,
                IsBordered = 0,
                IsMasked = 0,
                MaskBottom = 0,
                MaskTop = 0,
                MaskLeft = 0,
                MaskRight = 0,
                KeyFrameA = new KeyFrameSettings()
                {
                    PositionX = 0,
                    PositionY = 0,
                    SizeX = 0,
                    SizeY = 0
                },
                KeyFrameB = new KeyFrameSettings()
                {
                    PositionX = 0,
                    PositionY = 0,
                    SizeX = 0,
                    SizeY = 0
                }
            };


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
            return new BMDSwitcherState()
            {
                InTransition = this.InTransition,
                TransitionFramesRemaining = this.TransitionFramesRemaining,
                TransitionPosition = this.TransitionPosition,
                PresetID = this.PresetID,
                ProgramID = this.ProgramID,
                AuxID = this.AuxID,
                USK1OnAir = this.USK1OnAir,
                USK1KeyType = this.USK1KeyType,
                USK1FillSource = this.USK1FillSource,
                USK1KeyFrame = this.USK1KeyFrame,
                DSK1OnAir = this.DSK1OnAir,
                DSK1Tie = this.DSK1Tie,
                DSK2OnAir = this.DSK2OnAir,
                DSK2Tie = this.DSK2Tie,
                FTB = this.FTB,
                TransNextBackground = this.TransNextBackground,
                TransNextKey1 = this.TransNextKey1,
                ChromaSettings = this.ChromaSettings.Copy(),
                DVESettings = this.DVESettings.Copy(),
            };

        }

    }
}
