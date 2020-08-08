using Integrated_Presenter.BMDSwitcher;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Integrated_Presenter
{

    public delegate void PresentationStateUpdate(Slide currentslide);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        Timer system_second_timer = new Timer();
        Timer shot_clock_timer = new Timer();
        TimeSpan timeonshot = TimeSpan.Zero;
        TimeSpan warnShottime = new TimeSpan(0, 2, 30);
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            UpdateRealTimeClock();
            UpdateSlideControls();
            UpdateMediaControls();
            UpdateDriveButtonStyle();

            this.PresentationStateUpdated += MainWindow_PresentationStateUpdated;

            system_second_timer.Elapsed += System_second_timer_Elapsed;
            system_second_timer.Interval = 1000;
            system_second_timer.Start();


            _lastState.SetDefault();
            shot_clock_timer.Elapsed += Shot_clock_timer_Elapsed;
            shot_clock_timer.Interval = 1000;

        }

        private void Shot_clock_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeonshot = timeonshot.Add(new TimeSpan(0, 0, 1));
            TbShotClock.Dispatcher.Invoke(() =>
            {
                UpdateShotClock();
            });
        }

        private void UpdateShotClock()
        {
            if (timeonshot > warnShottime)
            {
                TbShotClock.Foreground = Brushes.Red;
            }
            else
            {
                TbShotClock.Foreground = Brushes.Orange;
            }
            TbShotClock.Text = timeonshot.ToString("mm\\:ss");
        }

        private void System_second_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TbTime.Dispatcher.Invoke(() =>
            {
                UpdateRealTimeClock();
            });
        }

        private void UpdateRealTimeClock()
        {
            TbTime.Text = DateTime.Now.ToString("hh:mm:ss");
        }

        private void MainWindow_PresentationStateUpdated(Slide currentslide)
        {
            UpdateSlideNums();
            ResetSlideMediaTimes();
            UpdateSlideControls();
            UpdateMediaControls();
        }

        IBMDSwitcherManager switcherManager;
        BMDSwitcherState switcherState = new BMDSwitcherState();


        #region BMD Switcher

        private void ConnectSwitcher()
        {
            Connection connectWindow = new Connection();
            bool? res = connectWindow.ShowDialog();
            if (res == true)
            {
                switcherManager = new BMDSwitcherManager(this);
                switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
                switcherManager.TryConnect(connectWindow.IP);
                if (!shot_clock_timer.Enabled)
                {
                    shot_clock_timer.Start();
                }
            }
        }

        private void MockConnectSwitcher()
        {
            switcherManager = new MockBMDSwitcherManager(this);
            switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
            switcherManager.TryConnect("localhost");
            if (!shot_clock_timer.Enabled)
            {
                shot_clock_timer.Start();
            }
        }

        private void TakeAutoTransition()
        {
            switcherManager?.PerformAutoTransition();
        }
        private void TakeCutTransition()
        {
            switcherManager?.PerformCutTransition();
        }


        BMDSwitcherState _lastState = new BMDSwitcherState();
        private void SwitcherManager_SwitcherStateChanged(BMDSwitcherState args)
        {
            // update shot clock
            if (args.IsDifferentShot(_lastState))
            {
                timeonshot = TimeSpan.Zero;
                UpdateShotClock();
            }
            // update ui
            switcherState = args;
            _lastState = switcherState.Copy();
            UpdateSwitcherUI();
        }

        private void ClickPreset(int button)
        {
            switcherManager?.PerformPresetSelect(ConvertButtonToSourceID(button));
        }

        private void ClickProgram(int button)
        {
            switcherManager?.PerformProgramSelect(ConvertButtonToSourceID(button));
        }

        private void ToggleUSK1()
        {
            switcherManager?.PerformToggleUSK1();
        }

        private void ToggleDSK1()
        {
            switcherManager?.PerformToggleDSK1();
        }
        private void ToggleTieDSK1()
        {
            switcherManager?.PerformTieDSK1();
        }
        private void AutoDSK1()
        {
            switcherManager?.PerformTakeAutoDSK1();
        }

        private void ToggleDSK2()
        {
            switcherManager?.PerformToggleDSK2();
        }
        private void ToggleTieDSK2()
        {
            switcherManager?.PerformTieDSK2();
        }
        private void AutoDSK2()
        {
            switcherManager?.PerformTakeAutoDSK2();
        }


        private int ConvertButtonToSourceID(int button)
        {
            return SourceLabelMappings[ButtonSourceMappings[button]];
        }

        private int ConvertSourceIDToButton(long sourceId)
        {
            return SourceButtonMappings[LabelSourceMappings[(int)sourceId]];
        }

        private Dictionary<string, int> SourceLabelMappings = new Dictionary<string, int>
        {
            ["left"] = 5,
            ["center"] = 1,
            ["right"] = 6,
            ["organ"] = 2,
            ["slide"] = 4,
            ["cam3"] = 3,
            ["cam7"] = 7,
            ["cam8"] = 8,
            ["null"] = -1,
        };

        public Dictionary<int, string> LabelSourceMappings = new Dictionary<int, string>()
        {
            [1] = "center",
            [2] = "organ",
            [3] = "cam3",
            [4] = "slide",
            [5] = "left",
            [6] = "right",
            [7] = "cam7",
            [8] = "cam8",
            [-1] = "null"
        };


        public Dictionary<int, string> ButtonSourceMappings = new Dictionary<int, string>()
        {
            [1] = "left",
            [2] = "center",
            [3] = "right",
            [4] = "organ",
            [5] = "slide",
            [6] = "cam3",
            [7] = "cam7",
            [8] = "cam8",
            [-1] = "null"
        };

        public Dictionary<string, int> SourceButtonMappings = new Dictionary<string, int>()
        {
            ["left"] = 1,
            ["center"] = 2,
            ["right"] = 3,
            ["organ"] = 4,
            ["slide"] = 5,
            ["cam3"] = 6,
            ["cam7"] = 7,
            ["cam8"] = 8,
            ["null"] = -1
        };



        #endregion


        private void UpdateSwitcherUI()
        {
            UpdatePresetButtonStyles();
            UpdateProgramButtonStyles();
            UpdateUSK1Styles();
            UpdateDSK1Styles();
            UpdateDSK2Styles();
            UpdateFTBButtonStyle();
        }

        private void UpdateUSK1Styles()
        {
            BtnUSK1OnOffAir.Background = (switcherState.USK1OnAir ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }

        private void UpdateDSK1Styles()
        {
            BtnDSK1OnOffAir.Background = (switcherState.DSK1OnAir ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnDSK1Tie.Background = (switcherState.DSK1Tie ? Resources["YellowLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }
        private void UpdateDSK2Styles()
        {
            BtnDSK2OnOffAir.Background = (switcherState.DSK2OnAir ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnDSK2Tie.Background = (switcherState.DSK2Tie ? Resources["YellowLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }

        private void UpdatePresetButtonStyles()
        {
            BtnPreset1.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 1 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset2.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 2 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset3.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 3 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset4.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 4 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset5.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 5 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset6.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 6 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset7.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 7 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnPreset8.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 8 ? Resources["GreenLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }

        private void UpdateProgramButtonStyles()
        {
            BtnProgram1.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 1 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram2.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 2 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram3.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 3 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram4.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 4 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram5.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 5 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram6.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 6 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram7.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 7 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
            BtnProgram8.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 8 ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }

        private void UpdateFTBButtonStyle()
        {
            BtnFTB.Background = (switcherState.FTB ? Resources["RedLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }

        private void UpdateDriveButtonStyle()
        {
            BtnDrive.Background = (SlideDriveVideo ? Resources["YellowLight"] : Resources["GrayLight"]) as RadialGradientBrush;
        }

        private void UpdateSlideNums()
        {
            TbCurrSlide.Text = Presentation?.CurrentSlide.ToString() ?? "0";
            TbSlideCount.Text = Presentation?.SlideCount.ToString() ?? "0";
        }

        private void ResetSlideMediaTimes()
        {
            TbMediaTimeRemaining.Text = "0:00";
            TbMediaTimeCurrent.Text = "0:00";
            TbMediaTimeDurration.Text = "0:00";
        }

        private void UpdateSlideControls()
        {
            if (Presentation != null)
            {
                if (Presentation.SlideCount > 0)
                {
                    BtnNext.Background = Resources["GrayLight"] as RadialGradientBrush;
                    BtnPrev.Background = Resources["GrayLight"] as RadialGradientBrush;
                    return;
                }
            }
            BtnNext.Background = Resources["OffLight"] as RadialGradientBrush;
            BtnPrev.Background = Resources["OffLight"] as RadialGradientBrush;
            UpdateDriveButtonStyle();
        }
        private void UpdateMediaControls()
        {
            if (Presentation != null)
            {
                if (Presentation.Current.Type == SlideType.Video)
                {
                    BtnPlayMedia.Background = Resources["GrayLight"] as RadialGradientBrush;
                    BtnPauseMedia.Background = Resources["GrayLight"] as RadialGradientBrush;
                    BtnRestartMedia.Background = Resources["GrayLight"] as RadialGradientBrush;
                    BtnStopMedia.Background = Resources["GrayLight"] as RadialGradientBrush;
                    return;
                }
            }
            BtnPlayMedia.Background = Resources["OffLight"] as RadialGradientBrush;
            BtnPauseMedia.Background = Resources["OffLight"] as RadialGradientBrush;
            BtnRestartMedia.Background = Resources["OffLight"] as RadialGradientBrush;
            BtnStopMedia.Background = Resources["OffLight"] as RadialGradientBrush;
        }


        private void WindowKeyDown(object sender, KeyEventArgs e)
        {

            // D1-D8 + (LShift)
            #region program/preset bus
            if (e.Key == Key.D1)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(1);
                else
                    ClickPreset(1);
            }
            if (e.Key == Key.D2)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(2);
                else
                    ClickPreset(2);
            }
            if (e.Key == Key.D3)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(3);
                else
                    ClickPreset(3);
            }
            if (e.Key == Key.D4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(4);
                else
                    ClickPreset(4);
            }
            if (e.Key == Key.D5)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(5);
                else
                    ClickPreset(5);
            }
            if (e.Key == Key.D6)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(6);
                else
                    ClickPreset(6);
            }
            if (e.Key == Key.D7)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(7);
                else
                    ClickPreset(7);
            }
            if (e.Key == Key.D8)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(8);
                else
                    ClickPreset(8);
            }
            #endregion

            // arrow keys + (LCtrl)
            #region slide controls
            if (e.Key == Key.Left)
            {
                prevSlide();
            }
            if (e.Key == Key.Right)
            {
                nextSlide();
            }
            if (e.Key == Key.Up)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    restartMedia();
                }
                else
                {
                    playMedia();
                }
            }
            if (e.Key == Key.Down)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    stopMedia();
                }
                else
                {
                    pauseMedia();
                }
            }
            if (e.Key == Key.T)
            {
                SlideDriveVideo_Current();
            }
            #endregion

            // numpad controls keyers
            #region keyers

            if (e.Key == Key.NumPad8)
            {
                ToggleTieDSK1();
            }

            if (e.Key == Key.NumPad5)
            {
                ToggleDSK1();
            }

            if (e.Key == Key.NumPad2)
            {
                AutoDSK1();
            }

            if (e.Key == Key.NumPad9)
            {
                ToggleTieDSK2();
            }

            if (e.Key == Key.NumPad6)
            {
                ToggleDSK2();
            }

            if (e.Key == Key.NumPad3)
            {
                AutoDSK2();
            }

            #endregion

            // fade to black
            if (e.Key == Key.B)
            {
                switcherManager?.PerformToggleFTB();
            }

            // transition controls
            if (e.Key == Key.Space)
            {
                TakeCutTransition();
            }

            if (e.Key == Key.Enter)
            {
                TakeAutoTransition();
            }

            // modifier alt for drive commands
            if (e.Key == Key.RightCtrl)
            {
                SlideDriveVideoCTRL = false;
            }

        }
        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl)
            {
                SlideDriveVideoCTRL = true;
            }
        }


        bool _slideDriveVideo_CTRL = true;

        bool SlideDriveVideoBTN
        {
            set
            {
                _slideDriveVideo_BTN = value;
                UpdateDriveButtonStyle();
            }
        }

        bool SlideDriveVideoCTRL
        {
            set
            {
                _slideDriveVideo_CTRL = value;
                UpdateDriveButtonStyle();
            }
        }

        bool _slideDriveVideo_BTN = true;

        bool SlideDriveVideo
        {
            get => _slideDriveVideo_BTN && _slideDriveVideo_CTRL;
        }

        #region ButtonClicks
        #region PresetButtonClick
        private void ClickPreset1(object sender, RoutedEventArgs e)
        {
            ClickPreset(1);
        }
        private void ClickPreset2(object sender, RoutedEventArgs e)
        {
            ClickPreset(2);
        }
        private void ClickPreset3(object sender, RoutedEventArgs e)
        {
            ClickPreset(3);
        }
        private void ClickPreset4(object sender, RoutedEventArgs e)
        {
            ClickPreset(4);
        }
        private void ClickPreset5(object sender, RoutedEventArgs e)
        {
            ClickPreset(5);
        }
        private void ClickPreset6(object sender, RoutedEventArgs e)
        {
            ClickPreset(6);
        }
        private void ClickPreset7(object sender, RoutedEventArgs e)
        {
            ClickPreset(7);
        }
        private void ClickPreset8(object sender, RoutedEventArgs e)
        {
            ClickPreset(8);
        }
        #endregion

        #region ProgramButtonClick

        private void ClickProgram8(object sender, RoutedEventArgs e)
        {
            ClickProgram(8);
        }
        private void ClickProgram7(object sender, RoutedEventArgs e)
        {
            ClickProgram(7);
        }
        private void ClickProgram6(object sender, RoutedEventArgs e)
        {
            ClickProgram(6);
        }
        private void ClickProgram5(object sender, RoutedEventArgs e)
        {
            ClickProgram(5);
        }
        private void ClickProgram4(object sender, RoutedEventArgs e)
        {
            ClickProgram(4);
        }
        private void ClickProgram3(object sender, RoutedEventArgs e)
        {
            ClickProgram(3);
        }
        private void ClickProgram2(object sender, RoutedEventArgs e)
        {
            ClickProgram(2);
        }
        private void ClickProgram1(object sender, RoutedEventArgs e)
        {
            ClickProgram(1);
        }
        #endregion

        #endregion


        #region SlideDriveVideo


        private async void SlideDriveVideo_Next()
        {
            if (Presentation?.Next != null)
            {
                if (Presentation.Next.Type == SlideType.Liturgy)
                {
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == SourceLabelMappings["slide"])
                    {
                        switcherManager.PerformAutoTransition();
                        await Task.Delay(1000);
                    }
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    switcherManager.PerformAutoOnAirDSK1();

                }
                else
                {
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager.PerformAutoOffAirDSK1();
                        await Task.Delay(1000);
                    }
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    if (switcherState.ProgramID != SourceLabelMappings["slide"])
                    {
                        ClickPreset(SourceButtonMappings["slide"]);
                        await Task.Delay(500);
                        switcherManager.PerformAutoTransition();
                    }
                    if (Presentation.Current.Type == SlideType.Video)
                    {
                        playMedia();
                    }

                }
            }
        }

        private void SlideDriveVideo_Prev()
        {

        }

        private async void SlideDriveVideo_Current()
        {
            if (Presentation?.Current != null)
            {
                if (Presentation.Current.Type == SlideType.Liturgy)
                {
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == SourceLabelMappings["slide"])
                    {
                        switcherManager.PerformAutoTransition();
                        await Task.Delay(1000);
                    }
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    switcherManager.PerformAutoOnAirDSK1();

                }
                else
                {
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager.PerformAutoOffAirDSK1();
                        await Task.Delay(1000);
                    }
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    if (switcherState.ProgramID != SourceLabelMappings["slide"])
                    {
                        ClickPreset(SourceButtonMappings["slide"]);
                        await Task.Delay(500);
                        switcherManager.PerformAutoTransition();
                    }
                    if (Presentation.Current.Type == SlideType.Video)
                    {
                        playMedia();
                    }

                }
            }

        }


        #endregion


        bool activepresentation = false;
        Presentation _pres;
        public Presentation Presentation { get => _pres; }

        public event PresentationStateUpdate PresentationStateUpdated;

        PresenterDisplay _display;

        private void OpenPresentation(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Presentation";
            if (ofd.ShowDialog() == true)
            {
                string path = System.IO.Path.GetDirectoryName(ofd.FileName);
                // create new presentation
                Presentation pres = new Presentation();
                pres.Create(path);
                pres.StartPres();
                _pres = pres;
                activepresentation = true;

                // start display
                _display = new PresenterDisplay(this);
                _display.OnMediaPlaybackTimeUpdated += _display_OnMediaPlaybackTimeUpdated;
                _display.Show();
                slidesUpdated();

                PresentationStateUpdated?.Invoke(Presentation.Current);
            }
        }

        private void _display_OnMediaPlaybackTimeUpdated(object sender, MediaPlaybackTimeEventArgs e)
        {
            // update textblocks
            TbMediaTimeRemaining.Text = e.Remaining.ToString("m\\:ss");
            TbMediaTimeCurrent.Text = e.Current.ToString("m\\:ss");
            TbMediaTimeDurration.Text = e.Length.ToString("m\\:ss");
        }

        private void slidesUpdated()
        {
            _display.ShowSlide();
            // update previews
            PrevPreview.SetMedia(Presentation.Prev);
            CurrentPreview.SetMedia(Presentation.Current);
            NextPreview.SetMedia(Presentation.Next);
            AfterPreview.SetMedia(Presentation.After);
        }


        bool displayPrevAfter = true;
        private void UIUpdateDisplayPrevAfter()
        {
            if (displayPrevAfter)
            {
                AfterPreviewDisplay.Visibility = Visibility.Visible;
                PrevPreviewDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                AfterPreviewDisplay.Visibility = Visibility.Hidden;
                PrevPreviewDisplay.Visibility = Visibility.Hidden;
            }
        }


        #region slideshow commands
        private void nextSlide()
        {
            if (activepresentation)
            {
                if (SlideDriveVideo)
                {
                    SlideDriveVideo_Next();
                }
                else
                {
                    Presentation.NextSlide();
                }
                slidesUpdated();
                PresentationStateUpdated?.Invoke(Presentation.Current);
            }
        }
        private void prevSlide()
        {
            if (activepresentation)
            {
                Presentation.PrevSlide();
                slidesUpdated();
                PresentationStateUpdated?.Invoke(Presentation.Current);
            }
        }

        private void playMedia()
        {
            if (activepresentation)
            {
                Dispatcher.Invoke(() =>
                {
                    _display.StartMediaPlayback();
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.PlayMedia();
                });
            }
        }
        private void pauseMedia()
        {
            if (activepresentation)
            {
                _display.PauseMediaPlayback();
                CurrentPreview.videoPlayer.Volume = 0;
                CurrentPreview.PauseMedia();
            }
        }
        private void stopMedia()
        {
            if (activepresentation)
            {
                _display.StopMediaPlayback();
                CurrentPreview.videoPlayer.Volume = 0;
                CurrentPreview.videoPlayer.Stop();
            }
        }
        private void restartMedia()
        {
            if (activepresentation)
            {
                _display.RestartMediaPlayback();
                CurrentPreview.videoPlayer.Volume = 0;
                CurrentPreview.ReplayMedia();
            }
        }

        #endregion


        #region slide/media buttons
        private void ClickRestartMedia(object sender, RoutedEventArgs e)
        {
            restartMedia();
        }
        private void ClickStopMedia(object sender, RoutedEventArgs e)
        {
            stopMedia();
        }
        private void ClickPauseMedia(object sender, RoutedEventArgs e)
        {
            pauseMedia();
        }
        private void ClickPlayMedia(object sender, RoutedEventArgs e)
        {
            playMedia();
        }
        private void ClickNextSlide(object sender, RoutedEventArgs e)
        {
            nextSlide();
        }
        private void ClickPrevSlide(object sender, RoutedEventArgs e)
        {
            prevSlide();
        }
        #endregion



        private void ClickConnectMock(object sender, RoutedEventArgs e)
        {
            MockConnectSwitcher();
        }
        private void ClickConnect(object sender, RoutedEventArgs e)
        {
            ConnectSwitcher();
        }



        private void ClickSlideDriveVideo(object sender, RoutedEventArgs e)
        {
            SlideDriveVideoBTN = !SlideDriveVideo;
        }

        private void ClickCutTrans(object sender, RoutedEventArgs e)
        {
            TakeCutTransition();
        }
        private void ClickAutoTrans(object sender, RoutedEventArgs e)
        {
            TakeAutoTransition();
        }

        private void ClickDSK1Toggle(object sender, RoutedEventArgs e)
        {
            ToggleDSK1();
        }

        private void ClickDSK1Auto(object sender, RoutedEventArgs e)
        {
            AutoDSK1();
        }
        private void ClickDSK1Tie(object sender, RoutedEventArgs e)
        {
            ToggleTieDSK1();
        }

        private void ClickDSK2Tie(object sender, RoutedEventArgs e)
        {
            ToggleTieDSK2();
        }
        private void ClickDSK2Toggle(object sender, RoutedEventArgs e)
        {
            ToggleDSK2();
        }
        private void ClickDSK2Auto(object sender, RoutedEventArgs e)
        {
            AutoDSK2();
        }
        private void ClickFTB(object sender, RoutedEventArgs e)
        {
            switcherManager?.PerformToggleFTB();
        }

        private void ClickUSK1Toggle(object sender, RoutedEventArgs e)
        {
            ToggleUSK1();
        }

        private void ClickViewPrevAfter(object sender, RoutedEventArgs e)
        {
            displayPrevAfter = !displayPrevAfter;
            UIUpdateDisplayPrevAfter();
        }

        private void ClickTakeSlide(object sender, RoutedEventArgs e)
        {
            SlideDriveVideo_Current();
        }
    }
}
