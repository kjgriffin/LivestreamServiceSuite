using ATEMSharedState.SwitcherState;

using CCU.Config;

using CCUI_UI;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using SharedPresentationAPI.Presentation;

using System;

namespace IntegratedPresenter.BMDSwitcher.Mock
{
    public interface IMockMultiviewerDriver
    {
        event EventHandler OnMockWindowClosed;

        void Close();
        void FadeDSK1(bool onair);
        void FadeDSK2(bool onair);
        BMDSwitcherState PerformAutoTransition(BMDSwitcherState state);
        BMDSwitcherState PerformCutTransition(BMDSwitcherState state);
        void SetDSK1(bool onair);
        void SetDSK2(bool onair);
        void SetFTB(bool onair);
        void SetPIPPosition(BMDUSKDVESettings state);
        void SetPresetInput(int inputID);
        void SetProgramInput(int inputID);
        virtual void SetAuxInput(int inputID) { return; }
        void SetTieDSK1(bool tie);
        void SetTieDSK2(bool tie);
        void SetUSK1FillSource(int sourceID);
        void SetUSK1ForNextTrans(bool v, BMDSwitcherState state);
        void setUSK1KeyType(int v);
        void SetUSK1OffAir(BMDSwitcherState state);
        void SetUSK1OnAir(BMDSwitcherState state);
        void UpdateMockCameraMovement(CameraUpdateEventArgs e);
        void UpdateCCUConfig(CCPUConfig_Extended cfg);
        void UpdateSlideInput(ISlide s);
    }
}