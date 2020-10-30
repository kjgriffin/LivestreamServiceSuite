using BMDSwitcherAPI;
using Integrated_Presenter.BMDSwitcher;
using Integrated_Presenter.BMDSwitcher.Config;
using Integrated_Presenter.RemoteControl;
using Integrated_Presenter.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VonEX;

namespace Integrated_Presenter
{

    public delegate void PresentationStateUpdate(Slide currentslide);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        BMDSwitcherConfigSettings _config;

        List<SlidePoolSource> SlidePoolButtons;

        RemoteControlActionSynchronizer synchronizer;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            synchronizer = new RemoteControlActionSynchronizer(this);


            // set a default config
            SetDefaultConfig();


            HideAdvancedPresControls();
            HideAdvancedPIPControls();
            HideAdvancedProjectorControls();

            SlidePoolButtons = new List<SlidePoolSource>() { SlidePoolSource0, SlidePoolSource1, SlidePoolSource2, SlidePoolSource3 };

            UpdateRealTimeClock();
            UpdateSlideControls();
            UpdateMediaControls();
            UpdateDriveButtonStyle();
            UpdateProjectorButtonStyles();

            UpdateMasterCautionDisplay();

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


        #region Clock/Timers
        Timer system_second_timer = new Timer();
        Timer shot_clock_timer = new Timer();
        Timer gp_timer_1 = new Timer();

        TimeSpan timer1span = TimeSpan.Zero;

        TimeSpan timeonshot = TimeSpan.Zero;
        TimeSpan warnShottime = new TimeSpan(0, 2, 30);


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

        #endregion



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

        #region Connections
        private void ConnectSwitcher()
        {
            Connection connectWindow = new Connection("Connect to Switcher", "Switcher IP Address");
            bool? res = connectWindow.ShowDialog();
            if (res == true)
            {
                switcherManager = new BMDSwitcherManager(this);
                switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
                if (switcherManager.TryConnect(connectWindow.IP))
                {
                    EnableSwitcherControls();
                }
                if (!shot_clock_timer.Enabled)
                {
                    shot_clock_timer.Start();
                }
            }
            // load current config
            synchronizer.ActionConfigureSwitcher(_config);
        }

        private void MockConnectSwitcher()
        {
            switcherManager = new MockBMDSwitcherManager(this);
            switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
            switcherManager.TryConnect("localhost");
            EnableSwitcherControls();
            if (!shot_clock_timer.Enabled)
            {
                shot_clock_timer.Start();
            }
            // load current config
            synchronizer.ActionConfigureSwitcher(_config);
        }

        #endregion

        public void TakeAutoTransition()
        {
            switcherManager?.PerformAutoTransition();
        }
        public void TakeCutTransition()
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

        public void SetPreset(int button)
        {
            switcherManager?.PerformPresetSelect(ConvertButtonToSourceID(button));
        }

        public void SetProgram(int button)
        {
            switcherManager?.PerformProgramSelect(ConvertButtonToSourceID(button));
        }

        public void ToggleUSK1()
        {
            switcherManager?.PerformToggleUSK1();
        }

        public void ToggleDSK1()
        {
            switcherManager?.PerformToggleDSK1();
        }
        public void ToggleTieDSK1()
        {
            switcherManager?.PerformTieDSK1();
        }
        public void AutoDSK1()
        {
            switcherManager?.PerformTakeAutoDSK1();
        }

        public void ToggleDSK2()
        {
            switcherManager?.PerformToggleDSK2();
        }
        public void ToggleTieDSK2()
        {
            switcherManager?.PerformTieDSK2();
        }
        public void AutoDSK2()
        {
            switcherManager?.PerformTakeAutoDSK2();
        }


        private int ConvertButtonToSourceID(int button)
        {
            var res = _config.Routing.Where(r => r.ButtonId == button).FirstOrDefault();
            if (res != null)
            {
                return res.PhysicalInputId;
            }
            return -1;
        }

        private int ConvertSourceIDToButton(long sourceId)
        {
            var res = _config.Routing.Where(r => r.PhysicalInputId == sourceId).FirstOrDefault();
            if (res != null)
            {
                return res.ButtonId;
            }
            return -1;
        }

        #endregion

        private void UpdateSwitcherUI()
        {
            UpdatePresetButtonStyles();
            UpdateProgramButtonStyles();
            UpdateTransButtonStyles();
            UpdateUSK1Styles();
            UpdateDSK1Styles();
            UpdateDSK2Styles();
            UpdateFTBButtonStyle();
            UpdatePIPButtonStyles();
        }

        private void UpdateUSK1Styles()
        {
            BtnUSK1OnOffAir.Background = (switcherState.USK1OnAir ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
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

            BtnPIPFillProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram8.Style = (Style)Application.Current.FindResource(style);


            BtnDSK1Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1Auto.Style = (Style)Application.Current.FindResource(style);

            BtnDSK2Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2Auto.Style = (Style)Application.Current.FindResource(style);

            BtnFTB.Style = (Style)Application.Current.FindResource(style);

            BtnAutoTrans.Style = (Style)Application.Current.FindResource(style);
            BtnCutTrans.Style = (Style)Application.Current.FindResource(style);

            BtnUSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);


            string pipstyle = "PIPControlButton";
            BtnPIPtoA.Style = (Style)Application.Current.FindResource(pipstyle);
            BtnPIPtoB.Style = (Style)Application.Current.FindResource(pipstyle);
            BtnPIPtoFull.Style = (Style)Application.Current.FindResource(pipstyle);

            BtnBackgroundTrans.Style = (Style)Application.Current.FindResource(style);
            BtnTransKey1.Style = (Style)Application.Current.FindResource(style);


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

        private void UpdateTransButtonStyles()
        {
            BtnBackgroundTrans.Background = (switcherState.TransNextBackground ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnTransKey1.Background = (switcherState.TransNextKey1 ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdatePIPButtonStyles()
        {
            BtnPIPFillProgram1.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram2.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram3.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram4.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram5.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram6.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram7.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram8.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;

            BtnPIPtoA.Background = switcherState.USK1KeyFrame == 1 ? Brushes.Red : Brushes.Transparent;
            BtnPIPtoB.Background = switcherState.USK1KeyFrame == 2 ? Brushes.Red : Brushes.Transparent;
            BtnPIPtoFull.Background = switcherState.USK1KeyFrame == 0 ? Brushes.Red : Brushes.Transparent;
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
                    SetProgram(1);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(1);
                else
                    SetPreset(1);
            }
            if (e.Key == Key.D2)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(2);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(2);
                else
                    SetPreset(2);
            }
            if (e.Key == Key.D3)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(3);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(3);
                else
                    SetPreset(3);
            }
            if (e.Key == Key.D4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(4);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(4);
                else
                    SetPreset(4);
            }
            if (e.Key == Key.D5)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(5);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(5);
                else
                    SetPreset(5);
            }
            if (e.Key == Key.D6)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(6);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(6);
                else
                    SetPreset(6);
            }
            if (e.Key == Key.D7)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(7);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(7);
                else
                    SetPreset(7);
            }
            if (e.Key == Key.D8)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    SetProgram(8);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangePIPFillSource(8);
                else
                    SetPreset(8);
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

            if (e.Key == Key.NumPad4)
            {
                ToggleUSK1();
            }

            if (e.Key == Key.Divide)
            {
                USK1RuntoA();
            }

            if (e.Key == Key.Multiply)
            {
                USK1RuntoB();
            }

            if (e.Key == Key.Subtract)
            {
                USK1RuntoFull();
            }

            if (e.Key == Key.NumPad0)
            {
                ToggleTransBkgd();
            }

            if (e.Key == Key.Decimal)
            {
                ToggleTransKey1();
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
            synchronizer.ActionPreset(1);
        }
        private void ClickPreset2(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(2);
        }
        private void ClickPreset3(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(3);
        }
        private void ClickPreset4(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(4);
        }
        private void ClickPreset5(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(5);
        }
        private void ClickPreset6(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(6);
        }
        private void ClickPreset7(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(7);
        }
        private void ClickPreset8(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPreset(8);
        }
        #endregion

        #region ProgramButtonClick

        private void ClickProgram8(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(8);
        }
        private void ClickProgram7(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(7);
        }
        private void ClickProgram6(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(6);
        }
        private void ClickProgram5(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(5);
        }
        private void ClickProgram4(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(4);
        }
        private void ClickProgram3(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(3);
        }
        private void ClickProgram2(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(2);
        }
        private void ClickProgram1(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionProgram(1);
        }
        #endregion

        #endregion


        #region SlideDriveVideo


        public async void SlideDriveVideo_Next()
        {
            if (Presentation?.Next != null)
            {
                if (Presentation.Next.Type == SlideType.Liturgy)
                {
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
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
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        SetPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }
                    if (Presentation.Current.Type == SlideType.Video)
                    {
                        playMedia();
                    }
                }
                // At this point we've switched to the slide
                SlideDriveVideo_Action(Presentation.Current);
            }
        }

        public async void SlideDriveVideo_ToSlide(Slide s)
        {
            if (s != null && Presentation != null)
            {
                if (s.Type == SlideType.Liturgy)
                {
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
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
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    Presentation.Override = s;
                    Presentation.OverridePres = true;
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        SetPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }
                    if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                    {
                        playMedia();
                    }
                }
                // At this point we've switched to the slide
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);
            }

        }

        private void SlideDriveVideo_Action(Slide s)
        {
            switch (s.Action)
            {
                case "t1restart":
                    timer1span = TimeSpan.Zero;
                    break;
                case "mastercaution2":
                    MasterCautionState = 2;
                    UpdateMasterCautionDisplay();
                    break;
                default:
                    break;
            }
        }

        public async void SlideDriveVideo_Current()
        {
            if (Presentation?.Current != null)
            {
                DisableSlidePoolOverrides();
                currentpoolsource = null;
                if (Presentation.Current.Type == SlideType.Liturgy)
                {
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    switcherManager?.PerformAutoOnAirDSK1();

                }
                else
                {
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager?.PerformAutoOffAirDSK1();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.Current);
                    if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        SetPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }

                    if (Presentation.Current.Type == SlideType.Video)
                    {
                        playMedia();
                    }
                }
                // Do Action on current slide
                SlideDriveVideo_Action(Presentation.Current);
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
                if (_display != null && _display.IsWindowVisilbe)
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
        public void nextSlide()
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
        public void prevSlide()
        {
            if (activepresentation)
            {
                DisableSlidePoolOverrides();
                Presentation.PrevSlide();
                slidesUpdated();
                PresentationStateUpdated?.Invoke(Presentation.Current);
            }
        }

        public void playMedia()
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
        public void pauseMedia()
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
        public void stopMedia()
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
        public void restartMedia()
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
            synchronizer.ActionRestartMedia();
        }
        private void ClickStopMedia(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionStopMedia();
        }
        private void ClickPauseMedia(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPauseMedia();
        }
        private void ClickPlayMedia(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionStartMedia();
        }
        private void ClickNextSlide(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionNextSlide();
        }
        private void ClickPrevSlide(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionPrevSlide();
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


        public void ToggleSlideDriveVideo(bool val)
        {
            SlideDriveVideoBTN = val;
        }

        private void ClickSlideDriveVideo(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionToggleDrive(!SlideDriveVideo);
        }

        private void ClickCutTrans(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionCutTransition();
        }
        private void ClickAutoTrans(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionAutoTransition();
        }

        private void ClickDSK1Toggle(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionToggleDSK1();
        }

        private void ClickDSK1Auto(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionAutoDSK1();
        }
        private void ClickDSK1Tie(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionTieDSK1();
        }

        private void ClickDSK2Tie(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionTieDSK2();
        }
        private void ClickDSK2Toggle(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionToggleDSK2();
        }
        private void ClickDSK2Auto(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionAutoDSK2();
        }
        private void ClickFTB(object sender, RoutedEventArgs e)
        {
            switcherManager?.PerformToggleFTB();
        }

        private void ClickUSK1Toggle(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionToggleDSK1();
        }

        private void ClickViewPrevAfter(object sender, RoutedEventArgs e)
        {
            displayPrevAfter = !displayPrevAfter;
            UIUpdateDisplayPrevAfter();
        }

        private void ClickTakeSlide(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionTakeSlide();
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
            synchronizer.ActionResetGPTimer1();
        }

        public void ResetGPTimer1()
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
            synchronizer.ActionConfigureSwitcher(_config);
        }

        public void USK1RuntoA()
        {
            switcherManager?.PerformUSK1RunToKeyFrameA();
        }

        public void USK1RuntoB()
        {
            switcherManager?.PerformUSK1RunToKeyFrameB();
        }

        public void USK1RuntoFull()
        {
            switcherManager?.PerformUSK1RunToKeyFrameFull();
        }

        private void ClickPIPRunToOnScreenBox(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionUSK1RuntoKeyA();
        }

        private void ClickPIPRunToOffScreenBox(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionUSK1RuntoKeyB();
        }

        private void ClickPIPRunToFull(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionUSK1RunToFull();
        }

        public void ChangePIPFillSource(int source)
        {
            switcherManager?.PerformUSK1FillSourceSelect(ConvertButtonToSourceID(source));
        }

        private void ClickPIP1(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(1);
        }

        private void ClickPIP2(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(2);
        }

        private void ClickPIP3(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(3);
        }

        private void ClickPIP4(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(4);
        }

        private void ClickPIP5(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(5);
        }

        private void ClickPIP6(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(6);
        }

        private void ClickPIP7(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(7);
        }

        private void ClickPIP8(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionSetUSK1FillSource(8);
        }


        private bool showadvancedpipcontrols = false;
        private void ClickViewAdvancedPIP(object sender, RoutedEventArgs e)
        {
            showadvancedpipcontrols = !showadvancedpipcontrols;

            if (showadvancedpipcontrols)
            {
                ShowAdvancedPIPControls();
            }
            else
            {
                HideAdvancedPIPControls();
            }

        }

        private void ShowAdvancedPIPControls()
        {
            grAdvancedPIP.Height = new GridLength(1, GridUnitType.Star);
        }

        private void HideAdvancedPIPControls()
        {
            grAdvancedPIP.Height = new GridLength(0);
        }

        public void ToggleTransBkgd()
        {
            switcherManager?.PerformToggleBackgroundForNextTrans();
        }

        public void ToggleTransKey1()
        {
            switcherManager?.PerformToggleKey1ForNextTrans();
        }

        private void ClickTransBkgd(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionToggleLayerBKGD();
        }

        private void ClickTransKey1(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionToggleLayerKey1();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "BMDSwitcherConfig";
            sfd.AddExtension = true;
            sfd.DefaultExt = "json";
            sfd.Filter = "JSON Files (*.json)|*.json";
            if (sfd.ShowDialog() == true)
            {
                _config?.Save(sfd.FileName);
            }
        }

        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                _config = BMDSwitcherConfigSettings.Load(ofd.FileName);
            }
            synchronizer.ActionConfigureSwitcher(_config);
        }

        public void SetSwitcherSettings(BMDSwitcherConfigSettings config)
        {
            // redundant unless this originated from a remote request
            _config = config;
            // update UI
            UpdateUIButtonLabels();
            // config switcher
            switcherManager?.ConfigureSwitcher(_config);
        }

        private void UpdateUIButtonLabels()
        {
            foreach (var btn in _config.Routing)
            {
                switch (btn.ButtonId)
                {
                    case 1:
                        UpdateButton1Labels(btn);
                        break;
                    case 2:
                        UpdateButton2Labels(btn);
                        break;
                    case 3:
                        UpdateButton3Labels(btn);
                        break;
                    case 4:
                        UpdateButton4Labels(btn);
                        break;
                    case 5:
                        UpdateButton5Labels(btn);
                        break;
                    case 6:
                        UpdateButton6Labels(btn);
                        break;
                    case 7:
                        UpdateButton7Labels(btn);
                        break;
                    case 8:
                        UpdateButton8Labels(btn);
                        break;
                    default:
                        break;
                }
            }
        }

        private void UpdateButton1Labels(ButtonSourceMapping config)
        {
            BtnPreset1.Content = config.ButtonName;
            BtnProgram1.Content = config.ButtonName;
            BtnPIPFillProgram1.Content = config.ButtonName;
        }

        private void UpdateButton2Labels(ButtonSourceMapping config)
        {
            BtnPreset2.Content = config.ButtonName;
            BtnProgram2.Content = config.ButtonName;
            BtnPIPFillProgram2.Content = config.ButtonName;
        }
        private void UpdateButton3Labels(ButtonSourceMapping config)
        {
            BtnPreset3.Content = config.ButtonName;
            BtnProgram3.Content = config.ButtonName;
            BtnPIPFillProgram3.Content = config.ButtonName;
        }
        private void UpdateButton4Labels(ButtonSourceMapping config)
        {
            BtnPreset4.Content = config.ButtonName;
            BtnProgram4.Content = config.ButtonName;
            BtnPIPFillProgram4.Content = config.ButtonName;
        }

        private void UpdateButton5Labels(ButtonSourceMapping config)
        {
            BtnPreset5.Content = config.ButtonName;
            BtnProgram5.Content = config.ButtonName;
            BtnPIPFillProgram5.Content = config.ButtonName;
        }
        private void UpdateButton6Labels(ButtonSourceMapping config)
        {
            BtnPreset6.Content = config.ButtonName;
            BtnProgram6.Content = config.ButtonName;
            BtnPIPFillProgram6.Content = config.ButtonName;
        }
        private void UpdateButton7Labels(ButtonSourceMapping config)
        {
            BtnPreset7.Content = config.ButtonName;
            BtnProgram7.Content = config.ButtonName;
            BtnPIPFillProgram7.Content = config.ButtonName;
        }
        private void UpdateButton8Labels(ButtonSourceMapping config)
        {
            BtnPreset8.Content = config.ButtonName;
            BtnProgram8.Content = config.ButtonName;
            BtnPIPFillProgram8.Content = config.ButtonName;
        }


        private void SetDefaultConfig()
        {
            BMDSwitcherConfigSettings cfg = new BMDSwitcherConfigSettings()
            {
                VideoSettings = new BMDSwitcherVideoSettings()
                {
                    VideoFPS = 30,
                    VideoHeight = 1080,
                    VideoWidth = 1920
                },
                Routing = new List<ButtonSourceMapping>() {
                    new ButtonSourceMapping() { KeyName = "left", ButtonId = 1, ButtonName = "PULPIT", PhysicalInputId = 5, LongName = "PULPIT", ShortName = "PLPT" },
                    new ButtonSourceMapping() { KeyName = "center", ButtonId = 2, ButtonName = "CENTER", PhysicalInputId = 1, LongName = "CENTER", ShortName = "CNTR" },
                    new ButtonSourceMapping() { KeyName = "right", ButtonId = 3, ButtonName = "LECTERN", PhysicalInputId = 6, LongName = "LECTERN", ShortName = "LTRN" },
                    new ButtonSourceMapping() { KeyName = "organ", ButtonId = 4, ButtonName = "ORGAN", PhysicalInputId = 2, LongName = "ORGAN", ShortName = "ORGN" },
                    new ButtonSourceMapping() { KeyName = "slide", ButtonId = 5, ButtonName = "SLIDE", PhysicalInputId = 4, LongName = "SLIDESHOW", ShortName = "SLDE" },
                    new ButtonSourceMapping() { KeyName = "c3", ButtonId = 6, ButtonName = "CAM3", PhysicalInputId = 3, LongName = "CAMERA 3", ShortName = "CAM3" },
                    new ButtonSourceMapping() { KeyName = "c7", ButtonId = 7, ButtonName = "CAM7", PhysicalInputId = 7, LongName = "CAMERA 7", ShortName = "CAM7" },
                    new ButtonSourceMapping() { KeyName = "c8", ButtonId = 8, ButtonName = "CAM8", PhysicalInputId = 8, LongName = "CAMERA 8", ShortName = "CAM8" },
                },
                MixEffectSettings = new BMDMixEffectSettings()
                {
                    Rate = 30
                },
                MultiviewerConfig = new BMDMultiviewerSettings()
                {
                    Layout = (int)_BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop, // 12
                    Window2 = 5,
                    Window3 = 1,
                    Window4 = 6,
                    Window5 = 2,
                    Window6 = 4,
                    Window7 = 3,
                    Window8 = 7,
                    Window9 = 8
                },
                DownstreamKey1Config = new BMDDSKSettings()
                {
                    InputFill = 4,
                    InputCut = 0,
                    Clip = 0.3,
                    Gain = 0.06,
                    Rate = 30,
                    Invert = 1,
                    IsPremultipled = 0,
                    IsMasked = 1,
                    MaskTop = -5.4f,
                    MaskBottom = -9,
                    MaskLeft = -16,
                    MaskRight = 16
                },
                DownstreamKey2Config = new BMDDSKSettings()
                {
                    InputFill = 4,
                    InputCut = 0,
                    Clip = 1,
                    Gain = 1,
                    Rate = 30,
                    Invert = 1,
                    IsPremultipled = 0,
                    IsMasked = 1,
                    MaskTop = 9,
                    MaskBottom = -9,
                    MaskLeft = 0,
                    MaskRight = 16
                },
                PIPSettings = new BMDUSKSettings()
                {
                    DefaultFillSource = 1,
                    IsBordered = 0,
                    IsMasked = 0,
                    MaskTop = 0,
                    MaskBottom = 0,
                    MaskLeft = 0,
                    MaskRight = 0,
                    Current = new KeyFrameSettings()
                    {
                        PositionX = 9.6,
                        PositionY = -6,
                        SizeX = 0.4,
                        SizeY = 0.4
                    },
                    KeyFrameA = new KeyFrameSettings()
                    {
                        PositionX = 9.6,
                        PositionY = -6,
                        SizeX = 0.4,
                        SizeY = 0.4
                    },
                    KeyFrameB = new KeyFrameSettings()
                    {
                        PositionX = 23,
                        PositionY = -6,
                        SizeX = 0.4,
                        SizeY = 0.4
                    }
                }
            };


            _config = cfg;
        }

        public BMDSwitcherConfigSettings Config { get => _config; }

        bool showAdvancedProjector = false;
        private void ClickViewAdvancedProjector(object sender, RoutedEventArgs e)
        {
            showAdvancedProjector = !showAdvancedProjector;
            if (showAdvancedProjector)
            {
                ShowAdvancedProjectorControls();
            }
            else
            {
                HideAdvancedProjectorControls();
            }
        }

        private void ClickProjector1(object sender, RoutedEventArgs e)
        {
            ClickProjectorButton(1);
        }

        private void ClickProjector2(object sender, RoutedEventArgs e)
        {
            ClickProjectorButton(2);
        }

        private void ClickProjector3(object sender, RoutedEventArgs e)
        {
            ClickProjectorButton(3);
        }

        private void ClickProjector4(object sender, RoutedEventArgs e)
        {
            ClickProjectorButton(4);
        }

        private void ClickProjector5(object sender, RoutedEventArgs e)
        {
            ClickProjectorButton(5);
        }

        private void ClickProjector6(object sender, RoutedEventArgs e)
        {
            ClickProjectorButton(6);
        }


        private void ClickProjectorButton(int btn)
        {
            if (projectorconnected)
            {
                projectorSerialPort.Write(btn.ToString());
            }
        }

        private void UpdateProjectorButtonNames()
        {

        }

        private void UpdateProjectorButtonStyles()
        {
            if (projectorconnected)
            {
                BtnProjector1.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnProjector2.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnProjector3.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnProjector4.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnProjector5.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnProjector6.Style = Application.Current.FindResource("SwitcherButton") as Style;
                return;
            }
            BtnProjector1.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnProjector2.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnProjector3.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnProjector4.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnProjector5.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnProjector6.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
        }

        private void ShowAdvancedProjectorControls()
        {
            gridbtns.Width = 770;
            gcAdvancedProjector.Width = new GridLength(1.2, GridUnitType.Star);
        }

        private void HideAdvancedProjectorControls()
        {
            gridbtns.Width = 660;
            gcAdvancedProjector.Width = new GridLength(0);
        }

        bool projectorconnected = false;
        SerialPort? projectorSerialPort;
        private void ConnectProjector()
        {
            var ports = SerialPort.GetPortNames();
            ConnectComPort dialog = new ConnectComPort(ports.ToList());
            dialog.ShowDialog();
            if (dialog.Selected)
            {
                string port = dialog.Port;

                if (projectorSerialPort != null)
                {
                    if (projectorSerialPort.IsOpen)
                    {
                        projectorSerialPort.Close();
                    }
                }
                projectorSerialPort = new SerialPort(port, 9600);
                projectorSerialPort.Open();
                projectorconnected = true;
            }
            UpdateProjectorButtonStyles();
        }

        private void ClickConnectProjector(object sender, RoutedEventArgs e)
        {
            ConnectProjector();
        }


        /// <summary>
        /// 0 = gray (good), 1 = orange (warn), 2 = red (error)
        /// </summary>
        int MasterCautionState = 0;

        private void ClickBtnWarnings(object sender, RoutedEventArgs e)
        {
            synchronizer.ActionClearMasterCaution();
        }

        public void ClearMasterCaution()
        {
            MasterCautionState = 0;
            UpdateMasterCautionDisplay();
        }

        private void UpdateMasterCautionDisplay()
        {
            switch (MasterCautionState)
            {
                case 0:
                    btnMasterCaution.Background = Brushes.Gray;
                    break;
                case 1:
                    btnMasterCaution.Background = Brushes.Orange;
                    break;
                case 2:
                    btnMasterCaution.Background = Brushes.Red;
                    break;
                default:
                    break;
            }
        }


        private void ClickAcceptOverrideControl(object sender, RoutedEventArgs e)
        {
            synchronizer.OpenConnectionAsMaster(); 
        }

        private void ClickStartOverrideControl(object sender, RoutedEventArgs e)
        {
            synchronizer.OpenConnectionAsSlave();
        }
    }
}
