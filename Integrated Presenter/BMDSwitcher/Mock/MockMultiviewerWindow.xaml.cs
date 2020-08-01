using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter.BMDSwitcher.Mock
{

    /// <summary>
    /// Interaction logic for MockMultiviewerWindow.xaml
    /// </summary>
    public partial class MockMultiviewerWindow : Window
    {
        private Dictionary<int, string> SourceMap;
        public MockMultiviewerWindow(Dictionary<int, string> sourcemap)
        {
            InitializeComponent();
            SourceMap = sourcemap;
        }


        private int ProgramSource;
        private int PresetSource;

        private bool DSK1;
        private bool DSK2;

        private ImageSource InputSourceToImage(int inputID)
        {
            if (SourceMap.ContainsKey(inputID))
            {
                switch (SourceMap[inputID])
                {
                    case "left":
                        return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/leftcam.png"));
                    case "center":
                        return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/centercam.png"));
                    case "right":
                        return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/rightcam.png"));
                    case "organ":
                        return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/organcam.png"));
                    case "slide":
                        return ImgSlide.Source;
                    default:
                        return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/black.png"));
                }
            }
            else
            {
                return new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/black.png"));
            }
        }

        public void SetPreviewSource(int inputID)
        {
            PresetSource = inputID;
            ImgPreset.Source = InputSourceToImage(inputID);
            ImgProgram_presetbgnd.Source = InputSourceToImage(inputID);
        }

        public void SetProgramSource(int inputID)
        {
            ProgramSource = inputID;
            ImgProgram.Source = InputSourceToImage(inputID);
            ImgPreset_pgmbgnd.Source = InputSourceToImage(inputID);
        }

        public void UpdateAuxSource(Slide slide)
        {
            UpdateSourceFromAux(ImgSlide, slide);
            if (ProgramSource == 4)
            {
                UpdateSourceFromAux(ImgProgram, slide);
            }
            if (PresetSource == 4)
            {
                UpdateSourceFromAux(ImgPreset, slide);
            }

            if (DSK1)
            {
                UpdateSourceFromAux(ImgProgramLowerThird, slide);
            }
            if (DSK2)
            {
                UpdateSourceFromAux(ImgProgramSplit, slide);
            }
        }

        private void UpdateSourceFromAux(Image control, Slide slide)
        {
            if (slide.Type == SlideType.Video)
            {
                control.Source = new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/videofile.png"));
            }
            else if (slide.Type == SlideType.Empty)
            {
                control.Source = new BitmapImage(new Uri("pack://application:,,,/BMDSwitcher/Mock/Images/black.png"));
            }
            else
            {
                control.Source = new BitmapImage(new Uri(slide.Source));
            }


        }

        public void ShowProgramDSK1()
        {
            DSK1 = true;
            ImgProgramLowerThird.Source = ImgSlide.Source;
            ImgProgramLowerThird.Opacity = 1;
        }

        public async void FadeInProgramDSK1()
        {
            ImgProgramLowerThird.Source = ImgSlide.Source;
            if (DSK1 != true)
            {
                ImgProgramLowerThird.Opacity = 0;
                var fadein = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadein, ImgProgramLowerThird);
                Storyboard.SetTargetProperty(fadein, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadein);
                sb.Begin();
                await Task.Delay(1000);
                ImgProgramLowerThird.Opacity = 1;
                sb.Stop();
            }
            DSK1 = true;
        }

        public void HideProgramDSK1()
        {
            DSK1 = false;
            ImgProgramLowerThird.Source = null;
        }

        public async void FadeOutProgramDSK1()
        {
            if (DSK1 != false)
            {
                var fadeout = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadeout, ImgProgramLowerThird);
                Storyboard.SetTargetProperty(fadeout, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadeout);
                sb.Begin();
                await Task.Delay(1000);
                ImgProgramLowerThird.Source = null;
                sb.Stop();
            }
            DSK1 = false;
            ImgProgramLowerThird.Source = null;
        }

        public void ShowProgramDSK2()
        {
            DSK2 = true;
            ImgProgramSplit.Source = ImgSlide.Source;
        }

        public void HideProgramDSK2()
        {
            DSK2 = false;
            ImgProgramSplit.Source = null;
        }


        public void SetFTB(bool black)
        {
            if (black)
            {
                ProgramFTB.Visibility = Visibility.Visible;
            }
            else
            {
                ProgramFTB.Visibility = Visibility.Hidden;
            }
        }

        public async void CrossFadeTransition(BMDSwitcherState finalstate)
        {
            // cross fade
            var pgmtopresetfade = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1)
            };
            Storyboard.SetTarget(pgmtopresetfade, ImgProgram);
            Storyboard.SetTargetProperty(pgmtopresetfade, new PropertyPath(Image.OpacityProperty));
            var sb_pgm = new Storyboard();
            sb_pgm.Children.Add(pgmtopresetfade);
            sb_pgm.Begin();


            var presettopgmfade = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1)
            };
            Storyboard.SetTarget(presettopgmfade, ImgProgram_presetbgnd);
            Storyboard.SetTargetProperty(presettopgmfade, new PropertyPath(Image.OpacityProperty));
            var sb_preset = new Storyboard();
            sb_preset.Children.Add(presettopgmfade);
            sb_preset.Begin();

            await Task.Delay(1000);

            ProgramSource = (int)finalstate.ProgramID;
            PresetSource = (int)finalstate.PresetID;
            ImgProgram.Source = InputSourceToImage((int)finalstate.ProgramID);
            ImgProgram.Opacity = 1;
            ImgProgram_presetbgnd.Opacity = 0;
            sb_pgm.Stop();
            sb_preset.Stop();
            ImgProgram_presetbgnd.Source = InputSourceToImage((int)finalstate.PresetID);
            ImgPreset.Source = InputSourceToImage((int)finalstate.PresetID);
            ImgPreset_pgmbgnd.Source = InputSourceToImage((int)finalstate.ProgramID);

        }

    }
}
