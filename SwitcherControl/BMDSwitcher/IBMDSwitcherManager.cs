﻿using ATEMSharedState.SwitcherState;

using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using System;

namespace SwitcherControl.BMDSwitcher
{


    public interface IBMDSwitcherManager
    {
        bool GoodConnection { get; set; }

        event SwitcherStateChange SwitcherStateChanged;
        event EventHandler<bool> OnSwitcherConnectionChanged;

        BMDSwitcherState ForceStateUpdate();
        BMDSwitcherState GetCurrentState();
        void PerformAutoOffAirDSK2();
        void PerformAutoOnAirDSK2();
        void PerformAutoOffAirDSK1();
        void PerformAutoOnAirDSK1();
        void PerformAutoTransition();
        void PerformCutTransition();
        void PerformPresetSelect(int sourceID);
        void PerformProgramSelect(int sourceID);
        void PerformAuxSelect(int sourceID);
        void PerformTakeAutoDSK1();
        void PerformTakeAutoDSK2();
        void PerformTieDSK1();
        void PerformTieDSK2();
        void PerformSetTieDSK1(bool set);
        void PerformSetTieDSK2(bool set);
        void PerformToggleDSK1();
        void PerformToggleDSK2();
        void PerformToggleFTB();
        void TryConnect(string address);
        void Disconnect();
        void PerformToggleUSK1();
        void PerformOnAirUSK1();
        void PerformOffAirUSK1();
        void PerformUSK1RunToKeyFrameA();
        void PerformUSK1RunToKeyFrameB();
        void PerformUSK1RunToKeyFrameFull();
        void PerformUSK1FillSourceSelect(int sourceID);
        void PerformToggleBackgroundForNextTrans();
        void PerformSetBKDGOnForNextTrans();
        void PerformSetBKDGOffForNextTrans();
        void PerformToggleKey1ForNextTrans();
        void PerformSetKey1OnForNextTrans();
        void PerformSetKey1OffForNextTrans();

        void SetUSK1TypeDVE();
        void SetUSK1TypePATTERN();
        void SetUSK1TypeChroma();
        void SetPIPPosition(BMDUSKDVESettings settings);
        void SetPIPKeyFrameA(BMDUSKDVESettings settings);
        void SetPIPKeyFrameB(BMDUSKDVESettings settings);
        void ConfigureUSK1PIP(BMDUSKDVESettings settings);
        void ConfigureUSK1Chroma(BMDUSKChromaSettings settings);
        void ConfigureUSK1PATTERN(BMDUSKPATTERNSettings settings);

        void Close();
        void ConfigureSwitcher(BMDSwitcherConfigSettings config, bool hardUpdate = true);
        void ApplyState(BMDSwitcherState state);
    }
}