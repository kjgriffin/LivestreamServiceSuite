using Integrated_Presenter.BMDSwitcher.Config;
using Integrated_Presenter.BMDSwitcher.Mock;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integrated_Presenter.BMDSwitcher
{
    public class MockMultiviewer
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

        public void UpdateSlideInput(Slide s)
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

        internal void Close()
        {
            multiviewerWindow?.Close();
        }

        internal void SetUSK1ForNextTrans(bool v, BMDSwitcherState state)
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

        internal void setUSK1KeyType(int v)
        {
            multiviewerWindow?.SetUSK1Type(v);
        }

        internal void SetUSK1OffAir(BMDSwitcherState state)
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

        internal void SetUSK1OnAir(BMDSwitcherState state)
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


        internal void SetPIPPosition(BMDUSKDVESettings state)
        {
            multiviewerWindow.SetPIPPosition(state);
        }

        internal void SetUSK1FillSource(int sourceID)
        {
            multiviewerWindow.SetPIPFillSource(sourceID);
        }
    }
}
