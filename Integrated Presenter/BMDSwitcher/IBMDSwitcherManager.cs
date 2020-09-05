using Integrated_Presenter.BMDSwitcher;

namespace Integrated_Presenter
{
    public interface IBMDSwitcherManager
    {
        bool GoodConnection { get; set; }

        event SwitcherStateChange SwitcherStateChanged;

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
        void PerformTakeAutoDSK1();
        void PerformTakeAutoDSK2();
        void PerformTieDSK1();
        void PerformTieDSK2();
        void PerformToggleDSK1();
        void PerformToggleDSK2();
        void PerformToggleFTB();
        bool TryConnect(string address);
        void PerformToggleUSK1();

        void Close();
        void ConfigureSwitcher();
    }
}