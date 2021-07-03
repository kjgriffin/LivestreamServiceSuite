using IntegratedPresenter.Presentation;
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

namespace IntegratedPresenter.BMDSwitcher.Mock
{

    /// <summary>
    /// Interaction logic for MockMultiviewerWindow.xaml
    /// </summary>
    public partial class MockMultiviewerWindow : Window
    {

        private readonly Uri img_LECTERN = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/leftshot.png");
        private readonly Uri img_CENTER = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/centershot.png");
        private readonly Uri img_PULPIT = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/rightshot.png");
        private readonly Uri img_ORGAN = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/organshot.png");
        private readonly Uri img_CBARS = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/cbars.png");
        private readonly Uri img_BLACK = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/black.png");
        private readonly Uri img_VIDEO = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/videofile.png");
        private readonly Uri img_GREENSCREEN = new Uri("pack://application:,,,/MockSwitcher;component/Mock/Images/greenscreen1.png");

        private Dictionary<int, string> SourceMap;
        public MockMultiviewerWindow(Dictionary<int, string> sourcemap)
        {
            InitializeComponent();
            SourceMap = sourcemap;
            SourceMap.Add((int)BMDSwitcherVideoSources.ColorBars, "colorbars");
            ProgramSource = (int)BMDSwitcherVideoSources.ColorBars;
            PresetSource = (int)BMDSwitcherVideoSources.ColorBars;
            DSK1 = false;
            DSK2 = false;
            USK1PreviewOn = false;
            USK1ProgramOn = false;
            USK1KeyType = 1;
            PreviewChromaKey.Visibility = Visibility.Hidden;
            ProgramChromaKey.Visibility = Visibility.Hidden;
        }


        private int ProgramSource;
        private int PresetSource;

        private bool dsk1;
        private bool DSK1
        {
            get => dsk1;
            set
            {
                dsk1 = value;
                if (dsk1)
                {
                    ImgProgramLowerThird.Source = ImgSlide.Source;
                }
            }
        }
        private bool DSK2;

        private bool USK1PreviewOn;
        private bool USK1ProgramOn;

        private int USK1KeyType;

        private ImageSource InputSourceToImage(int inputID)
        {
            if (SourceMap.ContainsKey(inputID))
            {
                switch (SourceMap[inputID])
                {
                    case "left":
                        return new BitmapImage(img_LECTERN);
                    case "center":
                        return new BitmapImage(img_CENTER);
                    case "right":
                        return new BitmapImage(img_PULPIT);
                    case "organ":
                        return new BitmapImage(img_ORGAN);
                    case "slide":
                        return ImgSlide.Source;
                    case "key":
                        return ImgKey.Source;
                    case "colorbars":
                        return new BitmapImage(img_CBARS);
                    default:
                        return new BitmapImage(img_BLACK);
                }
            }
            else
            {
                return new BitmapImage(img_BLACK);
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
            UpdateSourceFromKey(ImgKey, slide);


            if (ProgramSource == 4)
            {
                UpdateSourceFromAux(ImgProgram, slide);
            }
            if (PresetSource == 4)
            {
                UpdateSourceFromAux(ImgPreset, slide);
            }

            if (ProgramSource == 3)
            {
                UpdateSourceFromAux(ImgProgram, slide);
            }
            if (PresetSource == 3)
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

        private void UpdateSourceFromKey(Image control, Slide slide)
        {
            if (slide.Type == SlideType.Empty)
            {
                control.Source = new BitmapImage(img_BLACK);
            }
            else if (slide.Type == SlideType.Action)
            {
                control.Source = new BitmapImage(img_BLACK);
            }
            else
            {
                if (slide.KeySource != null && slide.KeySource != "")
                {
                    control.Source = new BitmapImage(new Uri(slide.KeySource));
                }
                else
                {
                    control.Source = new BitmapImage(img_BLACK);
                }
            }
        }

        private void UpdateSourceFromAux(Image control, Slide slide)
        {
            if (slide.Type == SlideType.Video)
            {
                control.Source = new BitmapImage(img_VIDEO);
            }
            else if (slide.Type == SlideType.Empty)
            {
                control.Source = new BitmapImage(img_BLACK);
            }
            else if (slide.Type == SlideType.ChromaKeyStill)
            {
                control.Source = new BitmapImage(new Uri(slide.Source));
            }
            else if (slide.Type == SlideType.ChromaKeyVideo)
            {
                control.Source = new BitmapImage(img_GREENSCREEN);
            }
            else if (slide.Type == SlideType.Action)
            {
                control.Source = new BitmapImage(img_BLACK);
            }
            else
            {
                control.Source = new BitmapImage(new Uri(slide.Source));
            }


        }

        public double dskliturgyopacity = 0.63;

        public void ShowProgramDSK1()
        {
            DSK1 = true;
            ImgProgramLowerThird.Source = ImgSlide.Source;
            ImgProgramLowerThird.Opacity = dskliturgyopacity;
        }

        public void ShowPresetTieDSK1(bool tie)
        {
            // only show if dsk1 not on air
            if (!DSK1 && tie)
            {
                ImgPresetLowerThird.Source = ImgSlide.Source;
            }
            else
            {
                ImgPresetLowerThird.Source = null;
            }
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
                    To = dskliturgyopacity,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadein, ImgProgramLowerThird);
                Storyboard.SetTargetProperty(fadein, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadein);
                sb.Begin();
                await Task.Delay(1000);
                ImgProgramLowerThird.Opacity = dskliturgyopacity;
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
                    From = dskliturgyopacity,
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

        public void ShowPresetTieDSK2(bool tie)
        {
            // only show if dsk2 not on air
            if (!DSK2 && tie)
            {
                ImgPresetSplit.Source = ImgSlide.Source;
            }
            else
            {
                ImgPresetSplit.Source = null;
            }
        }

        public void ForcePresetTieDSK1(bool show)
        {
            if (show)
            {
                ImgPresetLowerThird.Source = ImgSlide.Source;
            }
            else
            {
                ImgPresetLowerThird.Source = null;
            }
        }

        public void ForcePresetTieDSK2(bool show)
        {
            if (show)
            {
                ImgPresetSplit.Source = ImgSlide.Source;
            }
            else
            {
                ImgPresetSplit.Source = null;
            }
        }


        public async void FadeInProgramDSK2()
        {
            ImgProgramSplit.Source = ImgSlide.Source;
            if (DSK2 != true)
            {
                ImgProgramSplit.Opacity = 0;
                var fadein = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadein, ImgProgramSplit);
                Storyboard.SetTargetProperty(fadein, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadein);
                sb.Begin();
                await Task.Delay(1000);
                ImgProgramSplit.Opacity = 1;
                sb.Stop();
            }
            DSK2 = true;
        }


        public void HideProgramDSK2()
        {
            DSK2 = false;
            ImgProgramSplit.Source = null;
        }

        public async void FadeOutProgramDSK2()
        {
            if (DSK2 != false)
            {
                var fadeout = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadeout, ImgProgramSplit);
                Storyboard.SetTargetProperty(fadeout, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadeout);
                sb.Begin();
                await Task.Delay(1000);
                ImgProgramSplit.Source = null;
                sb.Stop();
            }
            DSK2 = false;
            ImgProgramSplit.Source = null;
        }



        public async void SetFTB(bool black)
        {
            if (black)
            {
                ProgramFTB.Opacity = 0;
                ProgramFTB.Visibility = Visibility.Visible;
                var fadein = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadein, ProgramFTB);
                Storyboard.SetTargetProperty(fadein, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadein);
                sb.Begin();
                await Task.Delay(1000);
                sb.Stop();
                ProgramFTB.Opacity = 1;
            }
            else
            {
                ProgramFTB.Opacity = 1;
                ProgramFTB.Visibility = Visibility.Visible;
                var fadeout = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1)
                };
                Storyboard.SetTarget(fadeout, ProgramFTB);
                Storyboard.SetTargetProperty(fadeout, new PropertyPath(Image.OpacityProperty));
                var sb = new Storyboard();
                sb.Children.Add(fadeout);
                sb.Begin();
                await Task.Delay(1000);
                sb.Stop();
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

        public async void SetUSK1PreviewOn()
        {
            USK1PreviewOn = true;
            if (USK1KeyType == 2)
            {
                PreviewChromaKey.Visibility = Visibility.Visible;
            }
        }

        public async void SetUSK1PreviewOff()
        {
            USK1PreviewOn = false;
            PreviewChromaKey.Visibility = Visibility.Hidden;
        }

        public async void SetUSK1ProgramOn()
        {
            USK1ProgramOn = true;
            if (USK1KeyType == 2)
            {
                ProgramChromaKey.Visibility = Visibility.Visible;
            }
        }

        public async void SetUSK1ProgramOff()
        {
            USK1ProgramOn = false;
            ProgramChromaKey.Visibility = Visibility.Hidden;
        }

        public async void SetUSK1Type(int type)
        {
            USK1KeyType = type;
            if (USK1KeyType == 2)
            {
                if (USK1ProgramOn)
                {
                    ProgramChromaKey.Visibility = Visibility.Visible;
                    return;
                }
                if (USK1PreviewOn)
                {
                    ProgramChromaKey.Visibility = Visibility.Visible;
                    return;
                }
            }
            ProgramChromaKey.Visibility = Visibility.Hidden;
            PreviewChromaKey.Visibility = Visibility.Hidden;

        }

    }
}
