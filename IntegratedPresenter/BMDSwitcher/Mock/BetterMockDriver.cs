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
            [5] = "pack://application:,,,/BMDSwitcher/Mock/Images/organcam.PNG",
            [6] = "pack://application:,,,/BMDSwitcher/Mock/Images/rightcam.PNG",
            [7] = "pack://application:,,,/BMDSwitcher/Mock/Images/centercam.PNG",
            [8] = "pack://application:,,,/BMDSwitcher/Mock/Images/leftcam.PNG",
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
    }


    public class BetterMockDriver : IMockMultiviewerDriver
    {

        BetterMockMVUI multiviewerWindow;
        BMDSwitcherConfigSettings _config;

        CameraSourceDriver _camDriver;

        public event SwitcherDisconnectedEvent OnMockWindowClosed;

        public BetterMockDriver(Dictionary<int, string> sourcemap, BMDSwitcherConfigSettings config)
        {
            _camDriver = new CameraSourceDriver();
            multiviewerWindow = new BetterMockMVUI();
            _config = config;
            multiviewerWindow.Show();
            multiviewerWindow.Closed += MultiviewerWindow_Closed;
            multiviewerWindow.ReConfigure(config);

            multiviewerWindow.RefreshUI(_camDriver);
        }

        private void MultiviewerWindow_Closed(object sender, EventArgs e)
        {
            OnMockWindowClosed?.Invoke();
        }

        public void UpdateSlideInput(ISlide s)
        {
            //var src = s.tryget
            //_camDriver.UpdateSource(_config.Routing.FirstOrDefault(x => x.KeyName == "slide").PhysicalInputId,
            _camDriver.UpdateSlideSource(s);
            multiviewerWindow.RefreshUI(_camDriver);
        }

        public void SetProgramInput(int inputID)
        {
            //multiviewerWindow.SetProgramSource(inputID);
        }

        public void SetPresetInput(int inputID)
        {
            //multiviewerWindow.SetPreviewSource(inputID);
        }

        public void SetTieDSK1(bool tie)
        {
            //multiviewerWindow.ShowPresetTieDSK1(tie);
        }

        public void SetTieDSK2(bool tie)
        {
            //multiviewerWindow.ShowPresetTieDSK2(tie);
        }

        public void SetDSK1(bool onair)
        {
            if (onair)
            {
                //multiviewerWindow.ShowProgramDSK1();
            }
            else
            {
                //multiviewerWindow.HideProgramDSK1();
            }
        }

        public void FadeDSK1(bool onair)
        {
            if (onair)
            {
                //multiviewerWindow.FadeInProgramDSK1();
            }
            else
            {
                //multiviewerWindow.FadeOutProgramDSK1();
            }
        }

        public void SetDSK2(bool onair)
        {
            if (onair)
            {
                //multiviewerWindow.ShowProgramDSK2();
            }
            else
            {
                //multiviewerWindow.HideProgramDSK2();
            }
        }

        public void FadeDSK2(bool onair)
        {
            if (onair)
            {
                //multiviewerWindow.FadeInProgramDSK2();
            }
            else
            {
                //multiviewerWindow.FadeOutProgramDSK2();
            }
        }

        public BMDSwitcherState PerformAutoTransition(BMDSwitcherState state)
        {
            return null;

            // take selected layers

            /*
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
            */
        }

        public void SetFTB(bool onair)
        {
            //multiviewerWindow.SetFTB(onair);
        }

        public void Close()
        {
            multiviewerWindow?.Close();
        }

        public void SetUSK1ForNextTrans(bool v, BMDSwitcherState state)
        {
            /*
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
            */
        }

        public void setUSK1KeyType(int v)
        {
            //multiviewerWindow?.SetUSK1Type(v);
        }

        public void SetUSK1OffAir(BMDSwitcherState state)
        {
            /*
            if (state.TransNextKey1)
            {
                multiviewerWindow.SetUSK1PreviewOn();
            }
            else
            {
                multiviewerWindow.SetUSK1PreviewOff();
            }
            multiviewerWindow?.SetUSK1ProgramOff();
            */
        }

        public void SetUSK1OnAir(BMDSwitcherState state)
        {
            /*
            if (state.TransNextKey1)
            {
                multiviewerWindow.SetUSK1PreviewOff();
            }
            else
            {
                multiviewerWindow.SetUSK1PreviewOn();
            }
            multiviewerWindow?.SetUSK1ProgramOn();
            */
        }


        public void SetPIPPosition(BMDUSKDVESettings state)
        {
            //multiviewerWindow.SetPIPPosition(state);
        }

        public void SetUSK1FillSource(int sourceID)
        {
            //multiviewerWindow.SetPIPFillSource(sourceID);
        }

        public void UpdateMockCameraMovement(CameraMotionEventArgs e)
        {
            //multiviewerWindow.UpdateMockCameraMovement(e);
        }
    }
}
