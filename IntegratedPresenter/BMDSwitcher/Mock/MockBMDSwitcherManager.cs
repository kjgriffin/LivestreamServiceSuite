using BMDSwitcherAPI;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using log4net;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedPresenter.BMDSwitcher.Mock
{
    class MockBMDSwitcherManager : IBMDSwitcherManager
    {
        public bool GoodConnection { get; set; } = false;

        BMDSwitcherState _state;

        MockMultiviewer mockMultiviewer;

        public event SwitcherStateChange SwitcherStateChanged;
        public event SwitcherDisconnectedEvent OnSwitcherDisconnected;

        private ILog _logger;

        public MockBMDSwitcherManager(MainWindow parent)
        {
            // initialize logger
            // enable logging
            using (Stream cstream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Integrated_Presenter.Log4Net_switcher.config"))
            {
                log4net.Config.XmlConfigurator.Configure(cstream);
            }

            _logger = LogManager.GetLogger("SwitcherLogger");
            _logger.Info($"[Mock SW] Switcher Logger for {parent?.Title} Started.");


            _state = new BMDSwitcherState();
            _state.SetDefault();
            Dictionary<int, string> mapping = new Dictionary<int, string>()
            {
                [1] = "back",
                [2] = "pres",
                [3] = "key",
                [4] = "slide",
                [5] = "organ",
                [6] = "right",
                [7] = "center",
                [8] = "left"
            };
            mockMultiviewer = new MockMultiviewer(mapping, parent.Config);
            mockMultiviewer.OnMockWindowClosed += MockMultiviewer_OnMockWindowClosed;
            parent.PresentationStateUpdated += Parent_PresentationStateUpdated;
        }

        private void MockMultiviewer_OnMockWindowClosed()
        {
            _logger.Info($"[Mock SW] USER requested close");
            OnSwitcherDisconnected?.Invoke();
        }

        private void Parent_PresentationStateUpdated(Slide currentslide)
        {
            _logger.Info($"[Mock SW] Presentation State was updated");
            mockMultiviewer.UpdateSlideInput(currentslide);
        }

        public BMDSwitcherState ForceStateUpdate()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            return _state;
        }

        public BMDSwitcherState GetCurrentState()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            return _state;
        }

        public void PerformAutoTransition()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state = mockMultiviewer.PerformAutoTransition(_state);
            if (_state.TransNextKey1)
            {
                _state.USK1OnAir = !_state.USK1OnAir;
            }
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformCutTransition()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
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
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()} {sourceID}");
            _state.PresetID = sourceID;
            mockMultiviewer.SetPresetInput(sourceID);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformProgramSelect(int sourceID)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()} {sourceID}");
            _state.ProgramID = sourceID;
            mockMultiviewer.SetProgramInput(sourceID);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleDSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK1OnAir = !_state.DSK1OnAir;
            mockMultiviewer.SetDSK1(_state.DSK1OnAir);
            mockMultiviewer.SetTieDSK1(_state.DSK1Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public bool TryConnect(string address)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            // we can connect to anything (since its a mock)
            GoodConnection = true;
            ForceStateUpdate();
            SwitcherStateChanged?.Invoke(_state);
            return true;
        }

        public void Disconnect()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            OnSwitcherDisconnected?.Invoke();
            mockMultiviewer.Close();
        }

        public void PerformAutoOffAirDSK2()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK2OnAir = false;
            mockMultiviewer.FadeDSK2(false);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOnAirDSK2()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK2OnAir = true;
            mockMultiviewer.FadeDSK2(true);
            SwitcherStateChanged?.Invoke(_state);
        }

        public async void PerformTakeAutoDSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK1OnAir = !_state.DSK1OnAir;
            mockMultiviewer.FadeDSK1(_state.DSK1OnAir);
            await Task.Delay(1000);
            SwitcherStateChanged?.Invoke(_state);
        }

        public async void PerformTakeAutoDSK2()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK2OnAir = !_state.DSK2OnAir;
            mockMultiviewer.FadeDSK2(_state.DSK2OnAir);
            await Task.Delay(1000);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformTieDSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK1Tie = !_state.DSK1Tie;
            mockMultiviewer.SetTieDSK1(_state.DSK1Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformSetTieDSK1(bool set)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK1Tie = set;
            mockMultiviewer.SetTieDSK1(_state.DSK1Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformTieDSK2()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK2Tie = !_state.DSK2Tie;
            mockMultiviewer.SetTieDSK2(_state.DSK2Tie);
            SwitcherStateChanged?.Invoke(_state);
        }
        public void PerformSetTieDSK2(bool set)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK2Tie = set;
            mockMultiviewer.SetTieDSK2(_state.DSK2Tie);
            SwitcherStateChanged?.Invoke(_state);
        }


        public void PerformToggleDSK2()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK2OnAir = !_state.DSK2OnAir;
            mockMultiviewer.SetDSK2(_state.DSK2OnAir);
            mockMultiviewer.SetTieDSK2(_state.DSK2Tie);
            SwitcherStateChanged?.Invoke(_state);
        }

        public async void PerformToggleFTB()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.FTB = !_state.FTB;
            mockMultiviewer.SetFTB(_state.FTB);
            await Task.Delay(1000);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOffAirDSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK1OnAir = false;
            mockMultiviewer.FadeDSK1(false);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAutoOnAirDSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DSK1OnAir = true;
            mockMultiviewer.FadeDSK1(true);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformToggleUSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
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
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            mockMultiviewer.Close();
        }

        public void ConfigureSwitcher(BMDSwitcherConfigSettings config)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
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
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyFrame = 1;
            _state.DVESettings.Current = _state.DVESettings.KeyFrameA;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformUSK1RunToKeyFrameB()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyFrame = 2;
            _state.DVESettings.Current = _state.DVESettings.KeyFrameB;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformUSK1RunToKeyFrameFull()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyFrame = 0;
            _state.DVESettings.Current = new KeyFrameSettings() { PositionX = 0, PositionY = 0, SizeX = 1, SizeY = 1 };
            SwitcherStateChanged?.Invoke(_state);
        }

        void IBMDSwitcherManager.PerformUSK1FillSourceSelect(int sourceID)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1FillSource = sourceID;
            mockMultiviewer.SetUSK1FillSource(sourceID);
            SwitcherStateChanged?.Invoke(_state);
        }

        void IBMDSwitcherManager.PerformToggleBackgroundForNextTrans()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");

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

            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
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
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            //_state.DVESettings.Current = settings.Current;
            //_state.DVESettings.MaskTop = settings.MaskTop;
            //_state.DVESettings.MaskBottom = settings.MaskBottom;
            //_state.DVESettings.MaskLeft = settings.MaskLeft;
            //_state.DVESettings.MaskRight = settings.MaskRight;
            //_state.DVESettings.IsMasked = settings.IsMasked;

            _state.DVESettings = settings?.Copy();
            mockMultiviewer.SetPIPPosition(settings);



            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetPIPKeyFrameA(BMDUSKDVESettings settings)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DVESettings.IsMasked = settings.IsMasked;
            _state.DVESettings.MaskTop = settings.MaskTop;
            _state.DVESettings.MaskBottom = settings.MaskBottom;
            _state.DVESettings.MaskLeft = settings.MaskLeft;
            _state.DVESettings.MaskRight = settings.MaskRight;
            _state.DVESettings.IsBordered = settings.IsBordered;
            _state.DVESettings.KeyFrameA = settings.KeyFrameA;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetPIPKeyFrameB(BMDUSKDVESettings settings)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.DVESettings.IsMasked = settings.IsMasked;
            _state.DVESettings.MaskTop = settings.MaskTop;
            _state.DVESettings.MaskBottom = settings.MaskBottom;
            _state.DVESettings.MaskLeft = settings.MaskLeft;
            _state.DVESettings.MaskRight = settings.MaskRight;
            _state.DVESettings.IsBordered = settings.IsBordered;
            _state.DVESettings.KeyFrameB = settings.KeyFrameB;
            SwitcherStateChanged?.Invoke(_state);
        }


        public void ConfigureUSK1PIP(BMDUSKDVESettings settings)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyType = 1;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void ConfigureUSK1Chroma(BMDUSKChromaSettings settings)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyType = 2;
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformOnAirUSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1OnAir = true;
            mockMultiviewer.SetUSK1OnAir(_state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformOffAirUSK1()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1OnAir = false;
            mockMultiviewer.SetUSK1OffAir(_state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetUSK1TypeDVE()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyType = 1;
            mockMultiviewer.setUSK1KeyType(1);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void SetUSK1TypeChroma()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.USK1KeyType = 2;
            mockMultiviewer.setUSK1KeyType(2);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformSetKey1OnForNextTrans()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.TransNextKey1 = true;
            mockMultiviewer.SetUSK1ForNextTrans(true, _state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformSetKey1OffForNextTrans()
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()}");
            _state.TransNextKey1 = false;
            mockMultiviewer.SetUSK1ForNextTrans(false, _state);
            SwitcherStateChanged?.Invoke(_state);
        }

        public void PerformAuxSelect(int sourceID)
        {
            _logger.Info($"[Mock SW] {System.Reflection.MethodBase.GetCurrentMethod()} {sourceID}");
            _state.AuxID = sourceID;
            SwitcherStateChanged?.Invoke(_state);
        }
    }
}
