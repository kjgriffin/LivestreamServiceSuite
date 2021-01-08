using BMDSwitcherAPI;
using Integrated_Presenter.BMDSwitcher.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Dictionary<int, string> mapping = new Dictionary<int, string>()
            {
                [1] = "cam1",
                [2] = "pres",
                [3] = "key",
                [4] = "slide",
                [5] = "organ",
                [6] = "right",
                [7] = "center",
                [8] = "left"
            };
            mockMultiviewer = new MockMultiviewer(mapping, parent.Config);
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
            _state = mockMultiviewer.PerformAutoTransition(_state);
            if (_state.TransNextKey1)
            {
                _state.USK1OnAir = !_state.USK1OnAir;
            }
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformCutTransition()
        {
            // take all layers
            if (_state.TransNextKey1)
            {
                if (_state.USK1OnAir)
                {
                    mockMultiviewer.SetUSK1OffAir(_state);
                }
                else
                {
                    mockMultiviewer.SetUSK1OnAir(_state);
                }
                _state.USK1OnAir = !_state.USK1OnAir;
            }
            // take all tied keyers
            if (_state.DSK1Tie)
            {
                _state.DSK1OnAir = !_state.DSK1OnAir;
                mockMultiviewer.SetDSK1(_state.DSK1OnAir);
                mockMultiviewer.SetTieDSK1(_state.DSK1Tie);
            }
            if (_state.DSK2Tie)
            {
                _state.DSK2OnAir = !_state.DSK2OnAir;
                mockMultiviewer.SetDSK2(_state.DSK2OnAir);
                mockMultiviewer.SetTieDSK2(_state.DSK2Tie);
            }
            if (_state.TransNextBackground)
            {
                long presetid = _state.PresetID;
                long programid = _state.ProgramID;
                _state.PresetID = programid;
                _state.ProgramID = presetid;
                mockMultiviewer.SetPresetInput((int)_state.PresetID);
                mockMultiviewer.SetProgramInput((int)_state.ProgramID);
            }
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
            mockMultiviewer.SetTieDSK1(_state.DSK1Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public bool TryConnect(string address)
        {
            // we can connect to anything (since its a mock)
            GoodConnection = true;
            ForceStateUpdate();
            SwitcherStateChanged?.Invoke(_state);
            return true;
        }

        public void PerformAutoOffAirDSK2()
        {
            _state.DSK2OnAir = false;
            mockMultiviewer.FadeDSK2(false);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOnAirDSK2()
        {
            _state.DSK2OnAir = true;
            mockMultiviewer.FadeDSK2(true);
            SwitcherStateChanged?.Invoke(_state);
        }

        public async void PerformTakeAutoDSK1()
        {
            _state.DSK1OnAir = !_state.DSK1OnAir;
            mockMultiviewer.FadeDSK1(_state.DSK1OnAir);
            await Task.Delay(1000);
            SwitcherStateChanged?.Invoke(_state);
        }

        public async void PerformTakeAutoDSK2()
        {
            _state.DSK2OnAir = !_state.DSK2OnAir;
            mockMultiviewer.FadeDSK2(_state.DSK2OnAir);
            await Task.Delay(1000);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformTieDSK1()
        {
            _state.DSK1Tie = !_state.DSK1Tie;
            mockMultiviewer.SetTieDSK1(_state.DSK1Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformTieDSK2()
        {
            _state.DSK2Tie = !_state.DSK2Tie;
            mockMultiviewer.SetTieDSK2(_state.DSK2Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleDSK2()
        {
            _state.DSK2OnAir = !_state.DSK2OnAir;
            mockMultiviewer.SetDSK2(_state.DSK2OnAir);
            mockMultiviewer.SetTieDSK2(_state.DSK2Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public async void PerformToggleFTB()
        {
            _state.FTB = !_state.FTB;
            mockMultiviewer.SetFTB(_state.FTB);
            await Task.Delay(1000);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOffAirDSK1()
        {
            _state.DSK1OnAir = false;
            mockMultiviewer.FadeDSK1(false);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOnAirDSK1()
        {
            _state.DSK1OnAir = true;
            mockMultiviewer.FadeDSK1(true);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleUSK1()
        {
            _state.USK1OnAir = !_state.USK1OnAir;
            // update multiviewer
            if (_state.USK1OnAir)
            {
                mockMultiviewer.SetUSK1OnAir(_state);
            }
            else
            {
                mockMultiviewer.SetUSK1OffAir(_state);
            }
            SwitcherStateChanged?.Invoke(_state);
        }

        public void Close()
        {
            mockMultiviewer.Close();
        }

        public void ConfigureSwitcher(BMDSwitcherConfigSettings config)
        {
            _state.USK1KeyType = config.USKSettings.IsChroma == 1 ? 2 : 1;
            if (config.USKSettings.IsChroma == 1)
            {
                _state.USK1FillSource = config.USKSettings.ChromaSettings.FillSource;
            }
            else
            {
                _state.USK1FillSource = config.USKSettings.PIPSettings.DefaultFillSource;
            }
            _state.DVESettings = config.USKSettings.PIPSettings;
            _state.ChromaSettings = config.USKSettings.ChromaSettings;

            ForceStateUpdate();
        }

        public void PerformUSK1RunToKeyFrameA()
        {
            _state.USK1KeyFrame = 1;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformUSK1RunToKeyFrameB()
        {
            _state.USK1KeyFrame = 2;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformUSK1RunToKeyFrameFull()
        {
            _state.USK1KeyFrame = 0;
            SwitcherStateChanged?.Invoke(_state);
        }

        void IBMDSwitcherManager.PerformUSK1FillSourceSelect(int sourceID)
        {
            _state.USK1FillSource = sourceID;
            SwitcherStateChanged?.Invoke(_state);
        }

        void IBMDSwitcherManager.PerformToggleBackgroundForNextTrans()
        {

            // only allow deselection if at least one layer is selected
            if (_state.TransNextBackground)
            {
                // to disable background key1 needs to be selected
                if (!_state.TransNextKey1)
                {
                    // dont' do anything
                    return;
                }
            }



            _state.TransNextBackground = !_state.TransNextBackground;
            SwitcherStateChanged?.Invoke(_state);
        }

        void IBMDSwitcherManager.PerformToggleKey1ForNextTrans()
        {

            // only allow deselection if at least one layer is selected
            if (_state.TransNextKey1)
            {
                // to disable key1 background needs to be selected
                if (!_state.TransNextBackground)
                {
                    // don't do anything
                    return;
                }
            }

            _state.TransNextKey1 = !_state.TransNextKey1;
            mockMultiviewer.SetUSK1ForNextTrans(_state.TransNextKey1, _state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetPIPPosition(BMDUSKDVESettings settings)
        {
            _state.DVESettings.Current = settings.Current;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetPIPKeyFrameA(BMDUSKDVESettings settings)
        {
            _state.DVESettings.KeyFrameA = settings.KeyFrameA;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void ConfigureUSK1PIP(BMDUSKDVESettings settings)
        {
            _state.USK1KeyType = 1;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void ConfigureUSK1Chroma(BMDUSKChromaSettings settings)
        {
            _state.USK1KeyType = 2;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformOnAirUSK1()
        {
            _state.USK1OnAir = true;
            mockMultiviewer.SetUSK1OnAir(_state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformOffAirUSK1()
        {
            _state.USK1OnAir = false;
            mockMultiviewer.SetUSK1OffAir(_state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetUSK1TypeDVE()
        {
            _state.USK1KeyType = 1;
            mockMultiviewer.setUSK1KeyType(1);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetUSK1TypeChroma()
        {
            _state.USK1KeyType = 2;
            mockMultiviewer.setUSK1KeyType(2);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformSetKey1OnForNextTrans()
        {
            _state.TransNextKey1 = true;
            mockMultiviewer.SetUSK1ForNextTrans(true, _state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformSetKey1OffForNextTrans()
        {
            _state.TransNextKey1 = false;
            mockMultiviewer.SetUSK1ForNextTrans(false, _state);
            SwitcherStateChanged?.Invoke(_state);
        }
    }
}
