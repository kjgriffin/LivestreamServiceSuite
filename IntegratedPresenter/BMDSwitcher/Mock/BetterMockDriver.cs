using BMDSwitcherAPI;

using CCUI_UI;

using Integrated_Presenter.BMDSwitcher.Mock;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using SwitcherControl.BMDSwitcher;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IntegratedPresenter.BMDSwitcher.Mock
{

    public interface ICameraSourceProvider
    {
        //public bool TryGetSource(int PhysicalInputID, out string source);
        public bool TryGetSourceImage(int PhysicalInputID, out BitmapImage image);
        bool TryGetSourceVideo(int PhysicalInputID, out string source);
    }

    public interface ISwitcherStateProvider
    {
        public BMDSwitcherState GetState();
        /// <summary>
        /// Reports when a animation has completed
        /// </summary>
        /// <param name="keyerID">Id of keyer: 1 = DSK1, 2 = DSK2</param>
        /// <param name="endState">State of keyer after animation. 1 = OnAir, 0 = OffAir</param>
        void ReportDSKFadeComplete(int keyerID, int endState);
        void ReportMETransitionComplete(int activeProgram, int activePreset, bool usk1State);
        void ReportFTBComplete(bool endState);
    }


    class CameraSourceDriver : ICameraSourceProvider
    {

        const string DEFAULT_SOURCE = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png";
        Dictionary<int, string> m_sourceMap = new Dictionary<int, string>()
        {
            [0] = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png", // always black
            [1] = "pack://application:,,,/BMDSwitcher/Mock/Images/backcam.PNG",
            [2] = "pack://application:,,,/BMDSwitcher/Mock/Images/powerpoint.png",
            [3] = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png",
            [4] = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png",
            [5] = "pack://application:,,,/BMDSwitcher/Mock/Images/organshot1.PNG",
            [6] = "pack://application:,,,/BMDSwitcher/Mock/Images/rightshot.PNG",
            [7] = "pack://application:,,,/BMDSwitcher/Mock/Images/centershot.PNG",
            [8] = "pack://application:,,,/BMDSwitcher/Mock/Images/leftshot1.PNG",
            [(int)BMDSwitcherVideoSources.ColorBars] = "pack://application:,,,/BMDSwitcher/Mock/Images/cbars.png",
        };


        public int SlideID { get; set; } = 4;
        public int AKeyID { get; set; } = 3;

        public void UpdateLiveCameraSource(int PhysicalInputID, string source)
        {
            m_sourceMap[PhysicalInputID] = source;
        }

        ISlide m_slideSource;
        public void UpdateSlideSource(ISlide slide)
        {
            m_slideSource = slide;
        }

        public bool TryGetSourceImage(int PhysicalInputID, out BitmapImage source)
        {
            source = null;

            if (PhysicalInputID == SlideID)
            {
                return m_slideSource?.TryGetPrimaryImage(out source) ?? false;
            }
            else if (PhysicalInputID == AKeyID)
            {
                return m_slideSource?.TryGetKeyImage(out source) ?? false;
            }
            else if (m_sourceMap.TryGetValue(PhysicalInputID, out string spath))
            {
                source = new BitmapImage(new Uri(spath));
                return true;
            }

            return false;
        }

        public bool TryGetSourceVideo(int PhysicalInputID, out string source)
        {
            source = null;

            if (PhysicalInputID == SlideID)
            {
                return m_slideSource?.TryGetPrimaryVideoPath(out source) ?? false;
            }
            else if (PhysicalInputID == AKeyID)
            {
                return m_slideSource?.TryGetKeyVideoPath(out source) ?? false;
            }

            return false;
        }
    }


    public class BetterMockDriver : IMockMultiviewerDriver, ISwitcherStateProvider
    {

        BetterMockMVUI multiviewerWindow;
        BMDSwitcherConfigSettings _config;

        CameraSourceDriver _camDriver;

        BMDSwitcherState _state;

        // TODO: perhaps this isn't best
        bool _stateDSK1InFade = false;
        bool _stateDSK2InFade = false;

        bool _stateFTBInFade = false;

        public event SwitcherDisconnectedEvent OnMockWindowClosed;

        public event EventHandler<BMDSwitcherState> OnSwitcherStateUpdated;

        public BetterMockDriver(Dictionary<int, string> sourcemap, BMDSwitcherConfigSettings config, BMDSwitcherState startupState)
        {
            _camDriver = new CameraSourceDriver();
            multiviewerWindow = new BetterMockMVUI();
            _config = config;

            _state = startupState;

            multiviewerWindow.Show();
            multiviewerWindow.Closed += MultiviewerWindow_Closed;
            multiviewerWindow.ReConfigure(config);

            multiviewerWindow.RefreshUI(_camDriver, this);
        }

        private void MultiviewerWindow_Closed(object sender, EventArgs e)
        {
            OnMockWindowClosed?.Invoke();
        }

        public void UpdateSlideInput(ISlide s)
        {
            _camDriver.UpdateSlideSource(s);
            multiviewerWindow.RefreshUI(_camDriver, this);
        }

        #region AtomicStateUpdate

        // all these are handled elsewhere and we simply rely upon refreshing the UI in response to the generated state update event

        public void SetProgramInput(int inputID) { }
        public void SetPresetInput(int inputID) { }
        public void SetTieDSK1(bool tie) { }
        public void SetTieDSK2(bool tie) { }
        public void SetDSK1(bool onair) { }
        public void SetDSK2(bool onair) { }
        public void FadeDSK2(bool onair) { }
        public void SetUSK1ForNextTrans(bool v, BMDSwitcherState state) { }
        public void setUSK1KeyType(int v) { }
        public void SetUSK1OffAir(BMDSwitcherState state) { }
        public void SetUSK1OnAir(BMDSwitcherState state) { }
        public void SetPIPPosition(BMDUSKDVESettings state) { }
        public void SetUSK1FillSource(int sourceID) { }


        #endregion

        public void SetFTB(bool onair)
        {
            // assume that we're always toggling


            if (!_stateFTBInFade)
            {
                _stateFTBInFade = true;
                if (_state.FTB)
                {
                    // ftb always on air until done
                    //OnSwitcherStateUpdated?.Invoke(this, _state);
                    multiviewerWindow.StartFTB(this, onair);
                }
                else
                {
                    // ftb immediately on air
                    _state.FTB = true;
                    OnSwitcherStateUpdated?.Invoke(this, _state);
                    multiviewerWindow.StartFTB(this, onair);
                }
            }
        }


        public void FadeDSK1(bool onair)
        {
            // assumption here is that we are in control of everything related to the fade
            // the current state we have now is accurate
            // we'll be responsible for updating the mockswitcher as the state changes throughout the fade

            if (_stateDSK1InFade)
            {
                // we're busy fading- for now we'll ignore the request
                return;
            }

            if (onair && _state.DSK1OnAir != onair)
            {
                // fade it on

                // key technically goes on air immediately
                _state.DSK1OnAir = true;
                _stateDSK1InFade = true;

                OnSwitcherStateUpdated?.Invoke(this, _state);

                // start the transition
                multiviewerWindow.StartDSK1Fade(1, this);
            }
            else if (!onair && _state.DSK1OnAir != onair)
            {
                // fade 'er off
                _stateDSK1InFade = true;

                //OnSwitcherStateUpdated?.Invoke(this, _state);

                multiviewerWindow.StartDSK1Fade(-1, this);
            }

        }

        public BMDSwitcherState PerformAutoTransition(BMDSwitcherState state)
        {
            _state = state;

            int key1Dir = 0; // 0- no change, 1 -> on, -1 -> off
            // take all active layers
            if (_state.TransNextKey1)
            {
                // the ME animation will actually take care of animating and it can pick that up via the transnextkey1 that's still true
                if (!_state.USK1OnAir)
                {
                    // technically at this point we've started a transition with USK1 tied to it...
                    // so it's now 'on air'
                    _state.USK1OnAir = true;
                    key1Dir = 1;
                }
                else
                {
                    key1Dir = -1;
                }
            }
            if (_state.DSK1Tie)
            {
                // hmmmm this ain't quite right... :(
                FadeDSK1(!_state.DSK1OnAir);
            }
            if (_state.DSK2Tie)
            {
                // copy whatever works for DSK!
            }
            if (_state.TransNextBackground | _state.TransNextKey1)
            {
                // atem switcher causes preset immediately on air
                // keeps original onair
                // new programid is technically the new one immediately ??

                // start a fade transition for BKGD layer

                // let animation complete it and clean up the state
                _state.InTransition = true;
                OnSwitcherStateUpdated?.Invoke(this, _state);

                // fade it!
                multiviewerWindow.StartMEFade(this, _camDriver, key1Dir);
            }

            return _state;
        }

        public BMDSwitcherState PerformCutTransition(BMDSwitcherState state)
        {
            _state = state;

            // take all active layers
            if (_state.TransNextKey1)
            {
                _state.USK1OnAir = !_state.USK1OnAir;
            }
            if (_state.DSK1Tie)
            {
                _state.DSK1OnAir = !_state.DSK1OnAir;
            }
            if (_state.DSK2Tie)
            {
                _state.DSK2OnAir = !_state.DSK2OnAir;
            }
            if (_state.TransNextBackground)
            {
                long presetid = _state.PresetID;
                long programid = _state.ProgramID;
                _state.PresetID = programid;
                _state.ProgramID = presetid;
            }

            // hard cut transition
            // TODO: figure out if we need to do something fancy with transition state marking for fades

            return _state;
        }


        public void Close()
        {
            multiviewerWindow?.Close();
        }


        public void UpdateMockCameraMovement(CameraUpdateEventArgs e)
        {
            //multiviewerWindow.UpdateMockCameraMovement(e);
        }

        internal void HandleStateUpdate(BMDSwitcherState args)
        {
            _state = args;
            // probably need to refresh
            multiviewerWindow.RefreshUI(_camDriver, this);
        }

        BMDSwitcherState ISwitcherStateProvider.GetState()
        {
            return _state;
        }

        internal void HandlePlaybackStateUpdate(MainWindow.MediaPlaybackEventArgs e)
        {
            multiviewerWindow.SlideVideoPlaybackUpdate(e);
        }

        void ISwitcherStateProvider.ReportDSKFadeComplete(int keyerID, int endState)
        {
            if (keyerID == 1)
            {
                _stateDSK1InFade = false;
                _state.DSK1OnAir = endState == 1;
            }
            else if (keyerID == 2)
            {
                _stateDSK2InFade = false;
                _state.DSK2OnAir = endState == 1;
            }
            OnSwitcherStateUpdated?.Invoke(this, _state);
        }

        void ISwitcherStateProvider.ReportMETransitionComplete(int activeProgram, int activePreset, bool usk1State)
        {
            _state.InTransition = false;
            _state.ProgramID = activeProgram;
            _state.PresetID = activePreset;
            _state.USK1OnAir = usk1State;
            OnSwitcherStateUpdated?.Invoke(this, _state);
        }

        void ISwitcherStateProvider.ReportFTBComplete(bool endState)
        {
            _state.FTB = endState;
            _stateFTBInFade = false;
            OnSwitcherStateUpdated?.Invoke(this, _state);
        }
    }
}
