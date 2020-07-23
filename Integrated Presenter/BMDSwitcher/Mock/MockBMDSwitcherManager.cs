using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    class MockBMDSwitcherManager : IBMDSwitcherManager
    {
        public bool GoodConnection { get; set; } = false;

        BMDSwitcherState _state;

        MockMultiviewer mockMultiviewer;

        public event SwitcherStateChange SwitcherStateChanged;

        public MockBMDSwitcherManager(MainWindow parent)
        {
            _state = new BMDSwitcherState();
            _state.SetDefault();
            mockMultiviewer = new MockMultiviewer();
            parent.PresentationStateUpdated += Parent_PresentationStateUpdated;
        }

        private void Parent_PresentationStateUpdated(Slide currentslide)
        {
            mockMultiviewer.UpdateSlideInput(currentslide);
        }

        public BMDSwitcherState ForceStateUpdate()
        {
            return _state;
        }

        public BMDSwitcherState GetCurrentState()
        {
            return _state;
        }

        public void PerformAutoTransition()
        {
            // for now just do a cut transition
            PerformCutTransition();
        }

        public void PerformCutTransition()
        {
            long presetid = _state.PresetID;
            long programid = _state.ProgramID;

            _state.PresetID = programid;
            _state.ProgramID = presetid;
            mockMultiviewer.SetPresetInput((int)_state.PresetID);
            mockMultiviewer.SetProgramInput((int)_state.ProgramID);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformPresetSelect(int sourceID)
        {
            _state.PresetID = (long)sourceID;
            mockMultiviewer.SetPresetInput(sourceID);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformProgramSelect(int sourceID)
        {
            _state.ProgramID = (long)sourceID;
            mockMultiviewer.SetProgramInput(sourceID);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleDSK1()
        {
            _state.DSK1OnAir = !_state.DSK1OnAir;
            mockMultiviewer.SetDSK1(_state.DSK1OnAir);
            SwitcherStateChanged?.Invoke(_state);
        }

        public bool TryConnect(string address)
        {
            // we can connect to anything (since its a mock)
            GoodConnection = true;
            ForceStateUpdate();
            return true;
        }

        public void PerformAutoOffAirDSK2()
        {
            _state.DSK2OnAir = false;
            mockMultiviewer.SetDSK2(false);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOnAirDSK2()
        {
            _state.DSK2OnAir = false;
            mockMultiviewer.SetDSK2(false);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformTakeAutoDSK1()
        {
            PerformToggleDSK1();
        }

        public void PerformTakeAutoDSK2()
        {
            PerformToggleDSK2();
        }

        public void PerformTieDSK1()
        {
            _state.DSK1Tie = !_state.DSK1Tie;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformTieDSK2()
        {
            _state.DSK2Tie = !_state.DSK2Tie;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleDSK2()
        {
            _state.DSK2OnAir = !_state.DSK2OnAir;
            mockMultiviewer.SetDSK2(_state.DSK2OnAir);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleFTB()
        {
            _state.FTB = !_state.FTB;
            mockMultiviewer.SetFTB(_state.FTB);
            SwitcherStateChanged?.Invoke(_state);
        }
    }
}
