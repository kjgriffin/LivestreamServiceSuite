using CCUI_UI;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using SwitcherControl.BMDSwitcher;

namespace IntegratedPresenter.BMDSwitcher.Mock
{
    public interface IMockMultiviewerDriver
    {
        event SwitcherDisconnectedEvent OnMockWindowClosed;

        void Close();
        void FadeDSK1(bool onair);
        void FadeDSK2(bool onair);
        BMDSwitcherState PerformAutoTransition(BMDSwitcherState state);
        void SetDSK1(bool onair);
        void SetDSK2(bool onair);
        void SetFTB(bool onair);
        void SetPIPPosition(BMDUSKDVESettings state);
        void SetPresetInput(int inputID);
        void SetProgramInput(int inputID);
        void SetTieDSK1(bool tie);
        void SetTieDSK2(bool tie);
        void SetUSK1FillSource(int sourceID);
        void SetUSK1ForNextTrans(bool v, BMDSwitcherState state);
        void setUSK1KeyType(int v);
        void SetUSK1OffAir(BMDSwitcherState state);
        void SetUSK1OnAir(BMDSwitcherState state);
        void UpdateMockCameraMovement(CameraMotionEventArgs e);
        void UpdateSlideInput(ISlide s);
    }
}