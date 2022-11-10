using CCU.Config;

using CCUI_UI;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using SwitcherControl.BMDSwitcher;
using SwitcherControl.BMDSwitcher.State;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IntegratedPresenter.BMDSwitcher.Mock
{
    public class MockMultiviewer : IMockMultiviewerDriver
    {

        MockMultiviewerWindow multiviewerWindow;
        BMDSwitcherConfigSettings _config;

        public event SwitcherDisconnectedEvent OnMockWindowClosed;

        public MockMultiviewer(Dictionary<int, string> sourcemap, BMDSwitcherConfigSettings config)
        {
            multiviewerWindow = new MockMultiviewerWindow(sourcemap);
            _config = config;
            multiviewerWindow.Show();
            multiviewerWindow.Closed += MultiviewerWindow_Closed;
        }

        private void MultiviewerWindow_Closed(object sender, EventArgs e)
        {
            OnMockWindowClosed?.Invoke();
        }

        public void UpdateSlideInput(ISlide s)
        {
            multiviewerWindow.UpdateAuxSource(s);
        }

        public void SetProgramInput(int inputID)
        {
            multiviewerWindow.SetProgramSource(inputID);
        }

        public void SetPresetInput(int inputID)
        {
            multiviewerWindow.SetPreviewSource(inputID);
        }

        public void SetTieDSK1(bool tie)
        {
            multiviewerWindow.ShowPresetTieDSK1(tie);
        }

        public void SetTieDSK2(bool tie)
        {
            multiviewerWindow.ShowPresetTieDSK2(tie);
        }

        public void SetDSK1(bool onair)
        {
            if (onair)
            {
                multiviewerWindow.ShowProgramDSK1();
            }
            else
            {
                multiviewerWindow.HideProgramDSK1();
            }
        }

        public void FadeDSK1(bool onair)
        {
            if (onair)
            {
                multiviewerWindow.FadeInProgramDSK1();
            }
            else
            {
                multiviewerWindow.FadeOutProgramDSK1();
            }
        }

        public void SetDSK2(bool onair)
        {
            if (onair)
            {
                multiviewerWindow.ShowProgramDSK2();
            }
            else
            {
                multiviewerWindow.HideProgramDSK2();
            }
        }

        public void FadeDSK2(bool onair)
        {
            if (onair)
            {
                multiviewerWindow.FadeInProgramDSK2();
            }
            else
            {
                multiviewerWindow.FadeOutProgramDSK2();
            }
        }

        public BMDSwitcherState PerformAutoTransition(BMDSwitcherState state)
        {

            // take selected layers

            if (state.TransNextKey1)
            {
                if (state.USK1OnAir)
                {
                    multiviewerWindow.SetUSK1ProgramOff();
                    multiviewerWindow.SetUSK1PreviewOn();
                }
                else
                {
                    multiviewerWindow.SetUSK1ProgramOn();
                    multiviewerWindow.SetUSK1PreviewOff();
                }
            }

            // take all tied keyers
            if (state.DSK1Tie)
            {
                state.DSK1OnAir = !state.DSK1OnAir;
                FadeDSK1(state.DSK1OnAir);
                multiviewerWindow.ForcePresetTieDSK1(state.DSK1Tie && !state.DSK1OnAir);
            }
            if (state.DSK2Tie)
            {
                state.DSK2OnAir = !state.DSK2OnAir;
                FadeDSK2(state.DSK2OnAir);
                multiviewerWindow.ForcePresetTieDSK2(state.DSK2Tie && !state.DSK2OnAir);
            }

            // swap sources
            if (state.TransNextBackground)
            {
                long programID = state.ProgramID;
                long presetID = state.PresetID;

                state.ProgramID = presetID;
                state.PresetID = programID;

                multiviewerWindow.CrossFadeTransition(state);
            }

            return state;
        }

        public void SetFTB(bool onair)
        {
            multiviewerWindow.SetFTB(onair);
        }

        public void Close()
        {
            multiviewerWindow?.Close();
        }

        public void SetUSK1ForNextTrans(bool v, BMDSwitcherState state)
        {
            if (v)
            {
                if (state.USK1OnAir)
                {
                    multiviewerWindow?.SetUSK1PreviewOff();
                }
                else
                {
                    multiviewerWindow?.SetUSK1PreviewOn();
                }
            }
            else
            {
                if (state.USK1OnAir)
                {
                    multiviewerWindow?.SetUSK1PreviewOn();
                }
                else
                {
                    multiviewerWindow?.SetUSK1PreviewOff();
                }
            }
        }

        public void setUSK1KeyType(int v)
        {
            multiviewerWindow?.SetUSK1Type(v);
        }

        public void SetUSK1OffAir(BMDSwitcherState state)
        {
            if (state.TransNextKey1)
            {
                multiviewerWindow.SetUSK1PreviewOn();
            }
            else
            {
                multiviewerWindow.SetUSK1PreviewOff();
            }
            multiviewerWindow?.SetUSK1ProgramOff();
        }

        public void SetUSK1OnAir(BMDSwitcherState state)
        {
            if (state.TransNextKey1)
            {
                multiviewerWindow.SetUSK1PreviewOff();
            }
            else
            {
                multiviewerWindow.SetUSK1PreviewOn();
            }
            multiviewerWindow?.SetUSK1ProgramOn();
        }


        public void SetPIPPosition(BMDUSKDVESettings state)
        {
            multiviewerWindow.SetPIPPosition(state);
        }

        public void SetUSK1FillSource(int sourceID)
        {
            multiviewerWindow.SetPIPFillSource(sourceID);
        }

        public void UpdateMockCameraMovement(CameraUpdateEventArgs e)
        {
            //multiviewerWindow.UpdateMockCameraMovement(e);
        }

        public BMDSwitcherState PerformCutTransition(BMDSwitcherState state)
        {
            // NOTE: here we won't really do much for now...
            throw new NotImplementedException();
        }

        public void UpdateCCUConfig(CCPUConfig_Extended cfg)
        {
        }
    }
}
