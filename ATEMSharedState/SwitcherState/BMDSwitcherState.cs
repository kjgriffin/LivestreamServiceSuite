using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher;
using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections.Generic;
using System.Text;

using VariableMarkupAttributes.Attributes;

namespace ATEMSharedState.SwitcherState
{
    public static class DummyLoader
    {
        public static void Load()
        {

        }
    }



    [ExposesWatchableVariables]
    public class BMDSwitcherState
    {

        public BMDSwitcherState()
        {
            SetDefault();
        }

        public bool InTransition { get; set; }
        public int TransitionFramesRemaining { get; set; }
        public double TransitionPosition { get; set; }

        [ExposedAsVariable(nameof(PresetID))]
        public long PresetID { get; set; }

        [ExposedAsVariable(nameof(ProgramID))]
        public long ProgramID { get; set; }

        [ExposedAsVariable(nameof(AuxID))]
        public long AuxID { get; set; }

        [ExposedAsVariable(nameof(USK1OnAir))]
        public bool USK1OnAir { get; set; }
        [ExposedAsVariable(nameof(USK1FillSource))]
        public long USK1FillSource { get; set; }

        [ExposedAsVariable(nameof(TransNextBackground))]
        public bool TransNextBackground { get; set; }
        [ExposedAsVariable(nameof(TransNextKey1))]
        public bool TransNextKey1 { get; set; }

        /// <summary>
        /// 1 = A, 2 = B, 0 = Full, -1 = other
        /// </summary>
        public int USK1KeyFrame { get; set; }

        /// <summary>
        /// 1 = DVE, 2 = Chroma, 3 = Pattern
        /// </summary>
        public int USK1KeyType { get; set; }

        [ExposedAsVariable(nameof(DSK1OnAir))]
        public bool DSK1OnAir { get; set; }
        [ExposedAsVariable(nameof(DSK1Tie))]
        public bool DSK1Tie { get; set; }
        [ExposedAsVariable(nameof(DSK2OnAir))]
        public bool DSK2OnAir { get; set; }
        [ExposedAsVariable(nameof(DSK2Tie))]
        public bool DSK2Tie { get; set; }

        [ExposedAsVariable(nameof(FTB))]
        public bool FTB { get; set; }

        public BMDUSKChromaSettings ChromaSettings { get; set; } = new BMDUSKChromaSettings();


        [ExposedAsVariable(nameof(DVESettings))]
        public BMDUSKDVESettings DVESettings { get; set; } = new BMDUSKDVESettings();

        [ExposedAsVariable(nameof(PATTERNSettings))]
        public BMDUSKPATTERNSettings PATTERNSettings { get; set; } = new BMDUSKPATTERNSettings();

        public void SetDefault()
        {
            InTransition = false;
            TransitionPosition = 0;
            TransitionFramesRemaining = 0;
            PresetID = (long)BMDSwitcherVideoSources.Input2;
            ProgramID = (long)BMDSwitcherVideoSources.Input1;
            AuxID = (long)BMDSwitcherVideoSources.ME1Prog;
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
            PATTERNSettings = new BMDUSKPATTERNSettings()
            {
                PatternType = "",
                Inverted = false,
                Size = 1,
                Softness = 0,
                Symmetry = 1,
                XOffset = 0,
                YOffset = 0,
            };
        }

        public bool IsDifferentShot(BMDSwitcherState oldstate)
        {
            // for now only look at program output
            if (ProgramID != oldstate.ProgramID)
            {
                return true;
            }
            return false;
        }

        public BMDSwitcherState Copy()
        {
            return new BMDSwitcherState()
            {
                InTransition = InTransition,
                TransitionFramesRemaining = TransitionFramesRemaining,
                TransitionPosition = TransitionPosition,
                PresetID = PresetID,
                ProgramID = ProgramID,
                AuxID = AuxID,
                USK1OnAir = USK1OnAir,
                USK1KeyType = USK1KeyType,
                USK1FillSource = USK1FillSource,
                USK1KeyFrame = USK1KeyFrame,
                DSK1OnAir = DSK1OnAir,
                DSK1Tie = DSK1Tie,
                DSK2OnAir = DSK2OnAir,
                DSK2Tie = DSK2Tie,
                FTB = FTB,
                TransNextBackground = TransNextBackground,
                TransNextKey1 = TransNextKey1,
                ChromaSettings = ChromaSettings.Copy(),
                DVESettings = DVESettings.Copy(),
                PATTERNSettings = PATTERNSettings.Copy(),
            };

        }

    }
}
