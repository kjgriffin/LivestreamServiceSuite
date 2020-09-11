using Integrated_Presenter.BMDSwitcher;
using Integrated_Presenter.ViewModels;
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
        Timer gp_timer_1 = new Timer();

        TimeSpan timer1span = TimeSpan.Zero;

        TimeSpan timeonshot = TimeSpan.Zero;
        TimeSpan warnShottime = new TimeSpan(0, 2, 30);

        SwitcherBusViewModel PresetRow;


        List<SlidePoolSource> SlidePoolButtons;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            HideAdvancedPresControls();

            SlidePoolButtons = new List<SlidePoolSource>() { SlidePoolSource0, SlidePoolSource1, SlidePoolSource2, SlidePoolSource3 };

            PresetRow = new SwitcherBusViewModel(8, SourceLabelMappings.Select(p => (p.Value, p.Key)).ToList());


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

            gp_timer_1.Interval = 1000;
            gp_timer_1.Elapsed += Gp_timer_1_Elapsed;
            gp_timer_1.Start();
        }

        private void Gp_timer_1_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer1span = timer1span.Add(new TimeSpan(0, 0, 1));
            UpdateGPTimer1();
        }

        private void UpdateGPTimer1()
        {
            TbGPTimer1.Dispatcher.Invoke(() =>
            {
                TbGPTimer1.Text = timer1span.ToString("mm\\:ss");
            });
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
                switcherManager.SwitcherStateChanged += PresetRow.OnSwitcherStateChanged;
                if (switcherManager.TryConnect(connectWindow.IP))
                {
                    EnableSwitcherControls();
                }
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
            switcherManager.SwitcherStateChanged += PresetRow.OnSwitcherStateChanged;
            switcherManager.TryConnect("localhost");
            EnableSwitcherControls();
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
            // update viewmodels

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
            //BtnUSK1OnOffAir.Background = (switcherState.USK1OnAir ? Application.Current.FindResource("RedLight"): Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateDSK1Styles()
        {
            BtnDSK1OnOffAir.Background = (switcherState.DSK1OnAir ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnDSK1Tie.Background = (switcherState.DSK1Tie ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }
        private void UpdateDSK2Styles()
        {
            BtnDSK2OnOffAir.Background = (switcherState.DSK2OnAir ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnDSK2Tie.Background = (switcherState.DSK2Tie ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void EnableSwitcherControls()
        {
            string style = "SwitcherButton";
            BtnPreset1.Style = (Style)Application.Current.FindResource(style);
            BtnPreset2.Style = (Style)Application.Current.FindResource(style);
            BtnPreset3.Style = (Style)Application.Current.FindResource(style);
            BtnPreset4.Style = (Style)Application.Current.FindResource(style);
            BtnPreset5.Style = (Style)Application.Current.FindResource(style);
            BtnPreset6.Style = (Style)Application.Current.FindResource(style);
            BtnPreset7.Style = (Style)Application.Current.FindResource(style);
            BtnPreset8.Style = (Style)Application.Current.FindResource(style);

            BtnProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnProgram8.Style = (Style)Application.Current.FindResource(style);

            BtnDSK1Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1Auto.Style = (Style)Application.Current.FindResource(style);

            BtnDSK2Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2Auto.Style = (Style)Application.Current.FindResource(style);

            BtnFTB.Style = (Style)Application.Current.FindResource(style);

            BtnAutoTrans.Style = (Style)Application.Current.FindResource(style);
            BtnCutTrans.Style = (Style)Application.Current.FindResource(style);



        }

        private void DisableSwitcherControls()
        {

            BtnPreset1.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset2.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset3.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset4.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset5.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset6.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset7.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
            BtnPreset8.Style = (Style)Application.Current.FindResource("SwitcherButton_Disabled");
        }

        private void UpdatePresetButtonStyles()
        {
            BtnPreset1.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 1 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset2.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 2 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset3.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 3 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset4.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 4 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset5.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 5 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset6.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 6 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset7.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 7 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPreset8.Background = (ConvertSourceIDToButton(switcherState.PresetID) == 8 ? Application.Current.FindResource("GreenLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateProgramButtonStyles()
        {
            BtnProgram1.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram2.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram3.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram4.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram5.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram6.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram7.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnProgram8.Background = (ConvertSourceIDToButton(switcherState.ProgramID) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateFTBButtonStyle()
        {
            BtnFTB.Background = (switcherState.FTB ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateDriveButtonStyle()
        {
            BtnDrive.Background = (SlideDriveVideo ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
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

                    BtnNext.Style = Application.Current.FindResource("SwitcherButton") as Style;
                    BtnPrev.Style = Application.Current.FindResource("SwitcherButton") as Style;
                    BtnTake.Style = Application.Current.FindResource("SwitcherButton") as Style;

                    BtnNext.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnPrev.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnTake.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    return;
                }
            }
            BtnNext.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnPrev.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnTake.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            UpdateDriveButtonStyle();
        }

        private void UpdateMediaControls()
        {
            if (Presentation != null)
            {
                if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                {
                    BtnPlayMedia.Style = Application.Current.FindResource("SwitcherButton") as Style;
                    BtnPauseMedia.Style = Application.Current.FindResource("SwitcherButton") as Style;
                    BtnRestartMedia.Style = Application.Current.FindResource("SwitcherButton") as Style;
                    BtnStopMedia.Style = Application.Current.FindResource("SwitcherButton") as Style;
                    BtnPlayMedia.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnPauseMedia.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnRestartMedia.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnStopMedia.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    return;
                }
            }
            BtnPlayMedia.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnPauseMedia.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnRestartMedia.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnStopMedia.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnPlayMedia.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnPauseMedia.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnRestartMedia.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnStopMedia.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
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
                    switcherManager?.PerformAutoOnAirDSK1();

                }
                else
                {
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager?.PerformAutoOffAirDSK1();
                        await Task.Delay(1000);
                    }
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    if (switcherState.ProgramID != SourceLabelMappings["slide"])
                    {
                        ClickPreset(SourceButtonMappings["slide"]);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }
                    if (Presentation.Current.Type == SlideType.Video)
                    {
                        playMedia();
                    }

                }
            }
        }

        private async void SlideDriveVideo_ToSlide(Slide s)
        {
            if (s != null && Presentation != null)
            {
                if (s.Type == SlideType.Liturgy)
                {
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == SourceLabelMappings["slide"])
                    {
                        switcherManager.PerformAutoTransition();
                        await Task.Delay(1000);
                    }
                    Presentation.Override = s;
                    Presentation.OverridePres = true;
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    switcherManager?.PerformAutoOnAirDSK1();

                }
                else
                {
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager?.PerformAutoOffAirDSK1();
                        await Task.Delay(1000);
                    }
                    Presentation.Override = s;
                    Presentation.OverridePres = true;
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    if (switcherState.ProgramID != SourceLabelMappings["slide"])
                    {
                        ClickPreset(SourceButtonMappings["slide"]);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }
                    if (Presentation.EffectiveCurrent.Type == SlideType.Video)
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
                DisableSlidePoolOverrides();
                currentpoolsource = null;
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

                // overwrite display of old presentation if already open
                if (_display != null)
                {

                }
                else
                {
                    _display = new PresenterDisplay(this);
                    _display.OnMediaPlaybackTimeUpdated += _display_OnMediaPlaybackTimeUpdated;
                    // start display
                    _display.Show();
                }
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
                DisableSlidePoolOverrides();
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
                DisableSlidePoolOverrides();
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
                    if (!Presentation.OverridePres)
                    {
                        CurrentPreview.videoPlayer.Volume = 0;
                        CurrentPreview.PlayMedia();
                    }
                    else
                    {
                        currentpoolsource.PlayMedia();
                    }
                });
            }
        }
        private void pauseMedia()
        {
            if (activepresentation)
            {
                _display.PauseMediaPlayback();
                if (!Presentation.OverridePres)
                {
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.PauseMedia();
                }
                else
                {
                    currentpoolsource.PauseMedia();
                }
            }
        }
        private void stopMedia()
        {
            if (activepresentation)
            {
                _display.StopMediaPlayback();
                if (!Presentation.OverridePres)
                {
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.videoPlayer.Stop();
                }
                else
                {
                    currentpoolsource.StopMedia();
                }
            }
        }
        private void restartMedia()
        {
            if (activepresentation)
            {
                _display.RestartMediaPlayback();
                if (!Presentation.OverridePres)
                {
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.ReplayMedia();
                }
                else
                {
                    currentpoolsource.RestartMedia();
                }
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


        bool _viewAdvancedPresentation = false;
        private void ClickViewAdvancedPresentation(object sender, RoutedEventArgs e)
        {
            _viewAdvancedPresentation = !_viewAdvancedPresentation;

            if (_viewAdvancedPresentation)
                ShowAdvancedPresControls();
            else
                HideAdvancedPresControls();
        }

        private void ShowAdvancedPresControls()
        {
            Width = Width + Width / 4;
            gcAdvancedPresentation.Width = new GridLength(1, GridUnitType.Star);
        }

        private void HideAdvancedPresControls()
        {
            Width = Width - Width / 5;
            gcAdvancedPresentation.Width = new GridLength(0);
        }



        private void ClickResetgpTimer1(object sender, MouseButtonEventArgs e)
        {
            gp_timer_1.Stop();
            timer1span = TimeSpan.Zero;
            UpdateGPTimer1();
            gp_timer_1.Start();
        }


        SlidePoolSource currentpoolsource = null;

        private void TakeSlidePoolSlide(Slide s, int num, bool slideoverride)
        {

            for (int i = 0; i < 4; i++)
            {
                if (num != i)
                {
                    SlidePoolButtons[i].Selected = false;
                }
            }

            if (slideoverride)
            {
                currentpoolsource = SlidePoolButtons[num];
            }

            if (slideoverride)
            {
                SlideDriveVideo_ToSlide(s);
                slidesUpdated();
                PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
            }
            else
            {
                SlideDriveVideo_Current();
            }
        }

        private void DisableSlidePoolOverrides()
        {
            for (int i = 0; i < 4; i++)
            {
                SlidePoolButtons[i].Selected = false;
            }
            Presentation.OverridePres = false;
            currentpoolsource?.StopMedia();
            UpdateMediaControls();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _display?.Close();
            switcherManager?.Close();
        }

        private void ClickTakeSP0(object sender, EventArgs e)
        {
            SlidePoolSource sps = sender as SlidePoolSource;
            Slide s = new Slide() { Source = sps.Source.ToString(), Type = sps.Type };
            TakeSlidePoolSlide(s, 0, sps.Selected);
        }

        private void ClickTakeSP1(object sender, EventArgs e)
        {
            SlidePoolSource sps = sender as SlidePoolSource;
            Slide s = new Slide() { Source = sps.Source.ToString(), Type = sps.Type };
            TakeSlidePoolSlide(s, 1, sps.Selected);
        }

        private void ClickTakeSP2(object sender, EventArgs e)
        {
            SlidePoolSource sps = sender as SlidePoolSource;
            Slide s = new Slide() { Source = sps.Source.ToString(), Type = sps.Type };
            TakeSlidePoolSlide(s, 2, sps.Selected);
        }

        private void ClickTakeSP3(object sender, EventArgs e)
        {
            SlidePoolSource sps = sender as SlidePoolSource;
            Slide s = new Slide() { Source = sps.Source.ToString(), Type = sps.Type };
            TakeSlidePoolSlide(s, 3, sps.Selected);
        }

        private void ClickConfigureSwitcher(object sender, RoutedEventArgs e)
        {
            switcherManager.ConfigureSwitcher();
        }

        private void ClickPIPRunToOnScreenBox(object sender, RoutedEventArgs e)
        {
            switcherManager.PerformUSK1RunToKeyFrameA();
        }

        private void ClickPIPRunToOffScreenBox(object sender, RoutedEventArgs e)
        {
            switcherManager.PerformUSK1RunToKeyFrameB();
        }

        private void ClickPIPRunToFull(object sender, RoutedEventArgs e)
        {
            switcherManager.PerformUSK1RunToKeyFrameFull();
        }
    }
}
