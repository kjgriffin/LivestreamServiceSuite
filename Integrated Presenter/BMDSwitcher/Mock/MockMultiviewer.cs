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

        public MockMultiviewer(Dictionary<int, string> sourcemap, BMDSwitcherConfigSettings config)
        {
            multiviewerWindow = new MockMultiviewerWindow(sourcemap);
            _config = config;
            multiviewerWindow.Show();
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
            long programID = state.ProgramID;
            long presetID = state.PresetID;

            state.ProgramID = presetID;
            state.PresetID = programID;

            multiviewerWindow.CrossFadeTransition(state);

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
    }
}
