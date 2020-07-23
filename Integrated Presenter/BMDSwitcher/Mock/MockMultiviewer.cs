using Integrated_Presenter.BMDSwitcher.Mock;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDSwitcher
{
    public class MockMultiviewer
    {

        MockMultiviewerWindow multiviewerWindow;

        public MockMultiviewer()
        {
            multiviewerWindow = new MockMultiviewerWindow();
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

        public void SetFTB(bool onair)
        {
            multiviewerWindow.SetFTB(onair);
        }

    }
}
