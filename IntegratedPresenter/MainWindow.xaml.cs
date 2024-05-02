﻿using ATEMSharedState.SwitcherState;

using CCU.Config;

using CCUI_UI;

using CommonVersionInfo;

using Configurations.FeatureConfig;

using Integrated_Presenter.Automation;
using Integrated_Presenter.DynamicDrivers;
using Integrated_Presenter.ViewModels;
using Integrated_Presenter.Windows;

using IntegratedPresenter.BMDSwitcher;
using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.BMDSwitcher.Mock;

using IntegratedPresenterAPIInterop;

using log4net;

using Microsoft.Win32;

using SharedPresentationAPI.Presentation;

using SwitcherControl.BMDSwitcher;
using SwitcherControl.Safe;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using VariableMarkupAttributes;
using VariableMarkupAttributes.Attributes;

namespace IntegratedPresenter.Main
{

    public delegate void PresentationStateUpdate(ISlide currentslide);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISwitcherDriverProvider, IAutoTransitionProvider, IConfigProvider, IFeatureFlagProvider, IUserTimerProvider, IMainUIProvider, IPresentationProvider, IAudioDriverProvider, IMediaDriverProvider, IDynamicControlProvider, IExtraDynamicControlProvider, IUserConditionProvider
    {


        Timer system_second_timer = new Timer();
        Timer shot_clock_timer = new Timer();
        Timer gp_timer_1 = new Timer();
        Timer gp_timer_2 = new Timer();

        TimeSpan timer1span = TimeSpan.Zero;
        TimeSpan timer2span = TimeSpan.Zero;

        TimeSpan timeonshot = TimeSpan.Zero;
        TimeSpan prewarnShottime = new TimeSpan(0, 0, 50);
        TimeSpan warnShottime = new TimeSpan(0, 1, 30);

        BMDSwitcherConfigSettings _config;

        System.Threading.ManualResetEvent autoTransMRE = new System.Threading.ManualResetEvent(true);


        ICCPUPresetMonitor _camMonitor;


        private BuildVersion VersionInfo;

        //event EventHandler OnConditionalsUpdated;

        ConditionalsVariablesCalculator _condVarCalculator;

        ISlideScriptActionAutomater _slideActionEngine;
        IActionAutomater _dynamicActionEngine;
        IDynamicDriver _dynamicDriver;
        IExtraDynamicDriver _spareDriver;

        private readonly ILog _logger = LogManager.GetLogger("UserLogger");


        Brush redBrush;
        Brush greenBrush;
        Brush yellowBrush;
        Brush tealBrush;
        Brush lightBlueBrush;
        Brush darkBrush;
        Brush lightBrush;
        Brush grayBrush;
        Brush whiteBrush;

        Brush greenLight;
        Brush redLight;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            redBrush = FindResource("redBrush") as Brush;
            greenBrush = FindResource("greenBrush") as Brush;
            yellowBrush = FindResource("yellowBrush") as Brush;
            tealBrush = FindResource("tealBrush") as Brush;
            lightBlueBrush = FindResource("lightBlueBrush") as Brush;
            darkBrush = FindResource("darkBrush") as Brush;
            lightBrush = FindResource("lightBrush") as Brush;
            grayBrush = FindResource("grayBrush") as Brush;
            whiteBrush = FindResource("whiteBrush") as Brush;

            greenLight = FindResource("GreenLight") as Brush;
            redLight = FindResource("RedLight") as Brush;


            // load build/version info
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Integrated_Presenter.version.json");
            using (StreamReader sr = new StreamReader(stream))
            {
                string json = sr.ReadToEnd();
                VersionInfo = JsonSerializer.Deserialize<BuildVersion>(json);
            }
            // Set title
            _nominalTitle = $"Integrated Presenter - {VersionInfo.MajorVersion}.{VersionInfo.MinorVersion}.{VersionInfo.Revision}.{VersionInfo.Build}-{VersionInfo.Mode}";

            Title = _nominalTitle;

            // enable logging
            using (Stream cstream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Integrated_Presenter.Log4Net.config"))
            {
                log4net.Config.XmlConfigurator.Configure(cstream);
            }

            _logger.Info($"{Title} Started.");


            // setup conditional manager
            _condVarCalculator = new ConditionalsVariablesCalculator(this, this, this);
            _condVarCalculator.OnConditionalsChanged += _condVarCalculator_OnConditionalsChanged;
            InitActionAutomater();

            // initialize conditionals
            _Cond1.Value = false;
            _Cond2.Value = false;
            _Cond3.Value = false;
            _Cond4.Value = false;

            Btn_Cond1.DataContext = _Cond1;
            Btn_Cond2.DataContext = _Cond2;
            Btn_Cond3.DataContext = _Cond3;
            Btn_Cond4.DataContext = _Cond4;

            _logger.Info("Starting Camera Server");

            ILog pilotLogger = LogManager.GetLogger("PilotLogger");

            // by default start real server
            _camMonitor = new CCPUPresetMonitor(headless: true, pilotLogger, this);
            _camMonitor.OnCommandUpdate += _camMonitor_OnCommandUpdate;

            pilotUI.OnModeChanged += PilotUI_OnModeChanged;
            pilotUI.OnTogglePilotMode += PilotUI_OnTogglePilotMode;
            pilotUI.OnUserRequestForManualReRun += PilotUI_OnUserRequestForManualReRun;
            pilotUI.OnUserRequestForManualZoomBump += PilotUI_OnUserRequestForManualZoomBump;

            // set a default config
            SetDefaultConfig();
            SetSwitcherSettings(false);
            LoadUserSettings(Configurations.FeatureConfig.IntegratedPresenterFeatures.Default());

            ShowHideShortcutsUI();

            // start with no switcher connection so disable all controls correctly
            DisableSwitcherControls();

            keyPatternControls.InitUIDrivers(this.ConvertSourceIDToButton, this.ConvertButtonToSourceID);
            chromaControls.InitUIDrivers(this.ConvertSourceIDToButton, this.ConvertButtonToSourceID);
            dvepipControls.InitUIDrivers(this.ConvertSourceIDToButton, this.ConvertButtonToSourceID);
            chromaControls.SetLogger(_logger);
            dvepipControls.SetLogger(_logger);

            // update PIP place hotkeys
            dvepipControls.UpdateUIPIPPlaceKeys(_lastState);

            UpdateRealTimeClock();
            UpdateSlideControls();
            UpdateMediaControls();
            UpdateSlideModeButtons();
            DisableAuxControls();
            UpdateProgramRowLockButtonUI();

            UpdateSlideMediaTabControls();

            this.PresentationStateUpdated += MainWindow_PresentationStateUpdated;
            NextPreview.AutoSilentPlayback = true;
            AfterPreview.AutoSilentPlayback = true;
            PrevPreview.AutoSilentPlayback = true;
            NextPreview.AutoSilentReplay = true;
            AfterPreview.AutoSilentReplay = true;
            PrevPreview.AutoSilentReplay = true;

            SpeculativeJumpPreviewDisplay.Visibility = Visibility.Hidden;

            SpeculativeJumpPreview.AutoSilentReplay = true;

            NextPreview.ShowBlackForActions = false;
            AfterPreview.ShowBlackForActions = false;
            PrevPreview.ShowBlackForActions = false;
            CurrentPreview.ShowBlackForActions = false;

            SpeculativeJumpPreview.ShowBlackForActions = false;
            SpeculativeJumpPreview.ShowAutomationPreviews = true;

            CurrentPreview.ShowIfMute = true;

            CurrentPreview.ShowAutomationPreviews = false;
            NextPreview.ShowAutomationPreviews = true;

            CurrentPreview.OnMediaPlaybackTimeUpdate += CurrentPreview_OnMediaPlaybackTimeUpdate;
            NextPreview.OnMediaLoaded += NextPreview_OnMediaLoaded;
            AfterPreview.OnMediaLoaded += AfterPreview_OnMediaLoaded;
            PrevPreview.OnMediaLoaded += PrevPreview_OnMediaLoaded;

            system_second_timer.Elapsed += System_second_timer_Elapsed;
            system_second_timer.Interval = 1000;
            system_second_timer.Start();


            _lastState.SetDefault();
            shot_clock_timer.Elapsed += Shot_clock_timer_Elapsed;
            shot_clock_timer.Interval = 1000;

            gp_timer_1.Interval = 1000;
            gp_timer_1.Elapsed += Gp_timer_1_Elapsed;
            gp_timer_1.Start();

            gp_timer_2.Interval = 1000;
            gp_timer_2.Elapsed += Gp_timer_2_Elapsed1;
            gp_timer_2.Start();

            UpdatePilotUI();

            // generate matrix ui
            SpoolDynamicControls();
        }

        private void _condVarCalculator_OnConditionalsChanged(object sender, EventArgs e)
        {
            FireOnConditionalsUpdated();
        }

        private void SpoolDynamicControls()
        {
            _dynamicActionEngine = new ActionAutomater(_logger,
                                                            this,
                                                            this,
                                                            _condVarCalculator,
                                                            this,
                                                            this,
                                                            this,
                                                            this,
                                                            this,
                                                            this,
                                                            this,
                                                            () => Presentation?.WatchedVariables ?? new Dictionary<string, WatchVariable>(),
                                                            this,
                                                            this,
                                                            this._camMonitor,
                                                            _condVarCalculator);
            // when loading config driver will re-supply automation with watched variables

            // load the dynamic driver
            _dynamicDriver = new DynamicMatrixDriver(dynamicControlPanel, _dynamicActionEngine, _condVarCalculator, _condVarCalculator);
            //_dynamicDriver.ConfigureControls(File.ReadAllText(@"D:\Downloads\controlseg.txt"), _pres?.Folder ?? "");

            // build the spare panel
            _spareDriver = new SpareDynamicMatrixDriver(this, _dynamicActionEngine, _condVarCalculator, _condVarCalculator);
        }

        private void InitActionAutomater()
        {
            // for now main window still kinda owns too much
            // but we're beginning to extract functionality
            _slideActionEngine = new SlideActionAutomater(_logger,
                                                this,
                                                this,
                                                _condVarCalculator,
                                                this,
                                                this,
                                                this,
                                                this,
                                                this,
                                                this,
                                                this,
                                                () => Presentation?.WatchedVariables,
                                                this,
                                                this,
                                                this._camMonitor,
                                                _condVarCalculator);
        }

        private void PilotUI_OnUserRequestForManualReRun(object sender, int e)
        {
            PilotFireLast(e);
        }

        private void PilotUI_OnUserRequestForManualZoomBump(object sender, (string cam, int dir, int zms) e)
        {
            PilotFireZoomBump(e.cam, "", e.dir, e.zms);
        }


        string _nominalTitle = "";

        private void PilotUI_OnTogglePilotMode()
        {
            ToggleAutoPilot();
        }

        private void PilotUI_OnModeChanged(PilotMode newMode)
        {
            PilotMode = newMode;
        }

        private void _camMonitor_OnCommandUpdate(string cName, params string[] args)
        {
            UpdatePilotUIStatus(cName, args);
        }

        private void Gp_timer_2_Elapsed1(object sender, ElapsedEventArgs e)
        {
            timer2span = timer2span.Add(new TimeSpan(0, 0, 1));
            UpdateGPTimer2();
        }

        private void CurrentPreview_OnMediaPlaybackTimeUpdate(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateSlideCurrentPreviewTimes();
        }

        private void PrevPreview_OnMediaLoaded(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateSlidePreviewControls();
        }

        private void AfterPreview_OnMediaLoaded(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateSlidePreviewControls();
        }

        private void NextPreview_OnMediaLoaded(object sender, MediaPlaybackTimeEventArgs e)
        {
            UpdateSlidePreviewControls();
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
                if (timer1span.Hours > 0)
                {
                    TbGPTimer1.Text = timer1span.ToString("%h\\:mm\\:ss");
                }
                else
                {
                    TbGPTimer1.Text = timer1span.ToString("mm\\:ss");
                }
            });
        }

        private void UpdateGPTimer2()
        {
            TbGPTimer2.Dispatcher.Invoke(() =>
            {
                if (timer2span.Hours > 0)
                {
                    TbGPTimer2.Text = timer2span.ToString("%h\\:mm\\:ss");
                }
                else
                {
                    TbGPTimer2.Text = timer2span.ToString("mm\\:ss");
                }
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
            TbShotClock.Dispatcher.Invoke(() =>
            {
                if (timeonshot > warnShottime)
                {
                    TbShotClock.Foreground = redBrush;
                }
                else if (timeonshot > prewarnShottime)
                {
                    TbShotClock.Foreground = yellowBrush;
                }
                else
                {
                    TbShotClock.Foreground = lightBlueBrush;
                }
                if (timeonshot.Hours > 0)
                {
                    TbShotClock.Text = timeonshot.ToString("%h\\:mm\\:ss");
                }
                else
                {
                    TbShotClock.Text = timeonshot.ToString("mm\\:ss");
                }
            });
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

        private void MainWindow_PresentationStateUpdated(ISlide currentslide)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => MainWindow_PresentationStateUpdated(currentslide));
                return;
            }
            UpdateSlideNums();
            UpdateSlidePreviewControls();
            ResetSlideMediaTimes();
            UpdateSlideControls();
            UpdateMediaControls();
            UpdatePilotUI();
            UpdateSpeculativeSlideJumpUI();

            _condVarCalculator?.NotifyOfChange();
        }

        IBMDSwitcherManager switcherManager;
        BMDSwitcherState switcherState = new BMDSwitcherState();


        #region BMD Switcher

        private void SwitcherConnectedUiUpdate(bool connected, bool searching = false)
        {
            if (connected)
            {
                tb_switcherConnection.Text = "Connected";
                tb_switcherConnection.Foreground = greenBrush;
            }
            else if (searching)
            {
                tb_switcherConnection.Text = "Searching...";
                tb_switcherConnection.Foreground = yellowBrush;
            }
            else
            {
                tb_switcherConnection.Text = "Disconnected";
                tb_switcherConnection.Foreground = redBrush;
            }
        }

        private async Task ConnectSwitcher()
        {
            if (switcherManager != null)
            {
                // we already have a connection
                _logger.Debug("ConnectSwitcher() called, but switcherManager already initialized. Ignored.");
                return;
            }
            Connection connectWindow = new Connection("Connect to Switcher", "Switcher IP Address:", "192.168.2.120");
            bool? res = connectWindow.ShowDialog();
            if (res == true)
            {
                SwitcherConnectedUiUpdate(true);

                // Try an use a thread safe variant
                //switcherManager = new BMDSwitcherManager(this, autoTransMRE);
                switcherManager = new SafeBMDSwitcher(autoTransMRE, _logger, this.Title);
                ApplyNewSwitcher(switcherManager);
                //keyPatternControls.SetSwitcherDriver(switcherManager);


                switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
                switcherManager.OnSwitcherConnectionChanged += SwitcherManager_OnSwitcherConnectionChanged;
                SwitcherConnectedUiUpdate(false, true);

                await Task.Delay(100).ConfigureAwait(true);

                // give UI update time
                switcherManager.TryConnect(connectWindow.IP);
            }
        }

        private void ApplyNewSwitcher(IBMDSwitcherManager switcher)
        {
            keyPatternControls.SetSwitcherDriver(switcher);
            chromaControls.SetSwitcherDriver(switcher);
            dvepipControls.SetSwitcherDriver(switcher);
        }

        private void SwitcherManager_OnSwitcherConnectionChanged(object sender, bool connected)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => SwitcherManager_OnSwitcherConnectionChanged(sender, connected));
                return;
            }

            _logger.Warn("SwitcherManager_OnSwitcherConnectionChanged event handled.");

            if (connected)
            {
                _logger.Debug("ConnectSwitcher() -- successfull, switcherManager initialized.");
                EnableSwitcherControls();
                // load current config
                SetSwitcherSettings();
                if (!shot_clock_timer.Enabled)
                {
                    shot_clock_timer.Start();
                }
                SwitcherConnectedUiUpdate(true);
            }
            else
            {
                _logger.Warn("SwitcherManager_OnSwitcherDisconnected() event handled.");
                DisableSwitcherControls();
                _logger.Warn("ConnectSwitcher() -- failed to connect.");
                SwitcherConnectedUiUpdate(false);
                switcherManager.SwitcherStateChanged -= SwitcherManager_SwitcherStateChanged;
                switcherManager.OnSwitcherConnectionChanged -= SwitcherManager_OnSwitcherConnectionChanged;
                switcherManager = null;
                ApplyNewSwitcher(switcherManager);
                //keyPatternControls.SetSwitcherDriver(switcherManager);
            }
        }

        private void MockConnectSwitcher()
        {
            if (switcherManager != null)
            {
                _logger.Debug("MockConnectSwitcher() called, but switcherManager already initialied. Ignored.");
                // we already have a connection
                return;
            }
            switcherManager = new MockBMDSwitcherManager(this);
            ApplyNewSwitcher(switcherManager);
            //keyPatternControls.SetSwitcherDriver(switcherManager);
            switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
            switcherManager.OnSwitcherConnectionChanged += SwitcherManager_OnSwitcherConnectionChanged;
            (switcherManager as MockBMDSwitcherManager)?.UpdateCCUConfig(Presentation?.CCPUConfig as CCPUConfig_Extended);
            switcherManager.TryConnect("localhost");
            _logger.Debug("MockConnectSwitcher() initialized switcherManger with mock switcher.");
            SwitcherConnectedUiUpdate(true);
            EnableSwitcherControls();
            if (!shot_clock_timer.Enabled)
            {
                shot_clock_timer.Start();
            }
            // load current config
            SetSwitcherSettings();
        }


        public void TakeAutoTransition()
        {
            // use guard mode
            PerformGuardedAutoTransition();
        }

        private bool _FeatureFlag_GaurdCutTransition = true;


        Stopwatch cuttranstimeout = new Stopwatch();
        private bool _cutTransCompletedTimeout = true;

        private void RequestCutTransition()
        {
            if (!_FeatureFlag_GaurdCutTransition)
            {
                TakeCutTransition();
            }
            else
            {
                PerformGuardCutTransition();
            }
        }

        private void TakeCutTransition()
        {
            _cutTransCompletedTimeout = false;
            cuttranstimeout.Reset();
            cuttranstimeout.Start();
            switcherManager?.PerformCutTransition();
        }

        private void PerformGuardCutTransition()
        {
            // enforce a 100ms timeout between requests for cut transition.
            // intended only for responses to user input. Main case is accounting for a twitchy space bar. @Carl Khul this one's for you :)
            if (cuttranstimeout.ElapsedMilliseconds > 100 || _cutTransCompletedTimeout)
            {
                // safe to request cut transition
                // can also stop timer since we're all good
                cuttranstimeout.Stop();
                _cutTransCompletedTimeout = true;
                TakeCutTransition();
            }
        }


        BMDSwitcherState _lastState = new BMDSwitcherState();
        private void SwitcherManager_SwitcherStateChanged(BMDSwitcherState args)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => SwitcherManager_SwitcherStateChanged(args));
                return;
            }

            if (args == null)
            {
                return;
            }
            // update shot clock
            if (args.IsDifferentShot(_lastState))
            {
                timeonshot = TimeSpan.Zero;
                UpdateShotClock();
                TotalShots += 1;
                GpT1Shots += 1;
                GpT2Shots += 1;
            }
            // update ui
            switcherState = args;
            _lastState = switcherState.Copy();

            // update pip ui
            pipctrl?.PIPSettingsUpdated(switcherState.DVESettings, switcherState.USK1FillSource);

            // update viewmodels
            UpdateSwitcherUI();

            keyPatternControls.UpdateFromSwitcherState(switcherState);
            chromaControls.UpdateFromSwitcherState(switcherState);
            dvepipControls.UpdateFromSwitcherState(switcherState);

            // conditions can change if they're watched
            //FireOnConditionalsUpdated();
            _condVarCalculator?.NotifyOfChange();
        }

        public void ForceStateUpdateOnSwitcher()
        {
            Dispatcher.Invoke(() =>
            {
                switcherManager?.ForceStateUpdate();
            });
        }

        public void ClickPreset(int button)
        {
            _logger.Debug($"USER INPUT requested Preset Source Changed to button {button}. Switcher commaned to change preset source.");
            switcherManager?.PerformPresetSelect(ConvertButtonToSourceID(button));
        }

        private void ClickAux(int button)
        {
            _logger.Debug($"USER INPUT requested Aux Source Changed to button {button}. Switcher commaned to change aux source.");
            switcherManager?.PerformAuxSelect(ConvertButtonToSourceID(button));
        }

        private void ClickProgram(int button)
        {
            _logger.Debug($"USER INPUT requested Program Source Changed to button {button}");
            if (!IsProgramRowLocked)
            {
                _logger.Debug($"Commanding Switcher to change program source to button {button}");
                switcherManager?.PerformProgramSelect(ConvertButtonToSourceID(button));
            }
        }

        private void ToggleUSK1()
        {
            _logger.Debug($"Commanding Switcher to toggle USK1 ON/Off air");
            switcherManager?.PerformToggleUSK1();
        }

        private void ToggleDSK1()
        {
            _logger.Debug($"Commanding Switcher to toggle DSK1 ON/Off air");
            switcherManager?.PerformToggleDSK1();
        }
        private void ToggleTieDSK1()
        {
            _logger.Debug($"Commanding Switcher to toggle DSK1 tied state");
            switcherManager?.PerformTieDSK1();
        }
        private void AutoDSK1()
        {
            _logger.Debug($"Commanding Switcher to Auto Fade ON/OFF DSK1");
            switcherManager?.PerformTakeAutoDSK1();
        }

        private void ToggleDSK2()
        {
            _logger.Debug($"Commanding Switcher to toggle DSK2 ON/OFF air");
            switcherManager?.PerformToggleDSK2();
        }
        private void ToggleTieDSK2()
        {
            _logger.Debug($"Commanding Switcher to toggle DSK2 tied state");
            switcherManager?.PerformTieDSK2();
        }
        private void AutoDSK2()
        {
            _logger.Debug($"Commanding Switcher to Auto Fade ON/OFF DSK2");
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
            if (!CheckAccess())
            {
                Dispatcher.Invoke(UpdateSwitcherUI);
                return;
            }
            UpdatePresetButtonStyles();
            UpdateProgramButtonStyles();
            UpdateAuxButtonStyles();
            UpdateTransButtonStyles();
            UpdateUSK1Styles();
            UpdateDSK1Styles();
            UpdateDSK2Styles();
            UpdateFTBButtonStyle();
            UpdateCBarsStyle();
            UpdateKeyerControls();

            // optionally update previews
            CurrentPreview?.FireOnSwitcherStateChangedForAutomation(_lastState, _config);
            NextPreview?.FireOnSwitcherStateChangedForAutomation(_lastState, _config);



            pilotUI?.FireOnSwitcherStateChangedForAutomation(_lastState, _config, PredictNextSlideTakesLiveCam());
        }

        private bool PredictNextSlideTakesLiveCam()
        {
            // liturgy does
            // full does not (usually)
            // video does not (usually)
            // slide.... (perhaps we ought to read actions and see if any set the program source, or call an auto trans?)
            // default to yes??
            if (Presentation?.Next?.Type == SlideType.Liturgy || Presentation?.Next?.Type == SlideType.Action)
            {
                return true;
            }

            return false;
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



            BtnDSK1Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1Auto.Style = (Style)Application.Current.FindResource(style);
            BtnDSK1Auto.Background = (RadialGradientBrush)Application.Current.FindResource("GrayLight");

            BtnDSK2Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2Auto.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2Auto.Background = (RadialGradientBrush)Application.Current.FindResource("GrayLight");

            BtnFTB.Style = (Style)Application.Current.FindResource(style);
            BtnCBars.Style = (Style)Application.Current.FindResource(style);

            BtnAutoTrans.Style = (Style)Application.Current.FindResource(style);
            BtnAutoTrans.Background = (RadialGradientBrush)Application.Current.FindResource("GrayLight");
            BtnCutTrans.Style = (Style)Application.Current.FindResource(style);
            BtnCutTrans.Background = (RadialGradientBrush)Application.Current.FindResource("GrayLight");

            BtnBackgroundTrans.Style = (Style)Application.Current.FindResource(style);

            EnableKeyerControls();
            EnableAuxButtons();
            ShowKeyerUI();

        }


        private void ShowKeyerUI()
        {
            BtnDVE.IsEnabled = true;
            BtnChroma.IsEnabled = true;
            BtnPattern.IsEnabled = true;

            BtnDVE.Cursor = Cursors.Hand;
            BtnChroma.Cursor = Cursors.Hand;
            BtnPattern.Cursor = Cursors.Hand;

            if (switcherState?.USK1KeyType == 1)
            {
                BtnDVE.Foreground = whiteBrush;
                BtnChroma.Foreground = tealBrush;
                BtnPattern.Foreground = tealBrush;

                BtnDVE.Background = tealBrush;
                BtnChroma.Background = darkBrush;
                BtnPattern.Background = darkBrush;

                ChromaControls.Visibility = Visibility.Hidden;
                PIPControls.Visibility = Visibility.Visible;
                PATTERNControls.Visibility = Visibility.Hidden;

                btnViewAdvancedFlyKeySettings.Visibility = Visibility.Visible;
            }
            else if (switcherState?.USK1KeyType == 2)
            {
                BtnDVE.Foreground = tealBrush;
                BtnChroma.Foreground = whiteBrush;
                BtnPattern.Foreground = tealBrush;

                BtnDVE.Background = darkBrush;
                BtnChroma.Background = tealBrush;
                BtnPattern.Background = darkBrush;

                PIPControls.Visibility = Visibility.Hidden;
                ChromaControls.Visibility = Visibility.Visible;
                PATTERNControls.Visibility = Visibility.Hidden;

                btnViewAdvancedFlyKeySettings.Visibility = Visibility.Hidden;
            }
            else if (switcherState?.USK1KeyType == 3)
            {
                BtnDVE.Foreground = tealBrush;
                BtnChroma.Foreground = tealBrush;
                BtnPattern.Foreground = whiteBrush;

                BtnDVE.Background = darkBrush;
                BtnChroma.Background = darkBrush;
                BtnPattern.Background = tealBrush;

                PIPControls.Visibility = Visibility.Hidden;
                ChromaControls.Visibility = Visibility.Hidden;
                PATTERNControls.Visibility = Visibility.Visible;

                btnViewAdvancedFlyKeySettings.Visibility = Visibility.Visible;
            }
            else
            {
                BtnDVE.Foreground = tealBrush;
                BtnChroma.Foreground = tealBrush;
                BtnPattern.Foreground = tealBrush;
                BtnDVE.Background = darkBrush;
                BtnChroma.Background = darkBrush;
                BtnPattern.Background = darkBrush;
            }
        }

        private void DisableKeyerUI()
        {
            btnViewAdvancedFlyKeySettings.Visibility = Visibility.Hidden;

            BtnDVE.IsEnabled = false;
            BtnChroma.IsEnabled = false;
            BtnPattern.IsEnabled = false;

            BtnDVE.Foreground = lightBrush;
            BtnChroma.Foreground = lightBrush;
            BtnPattern.Foreground = lightBrush;


            BtnDVE.Background = darkBrush;
            BtnChroma.Background = darkBrush;
            BtnPattern.Background = darkBrush;


            BtnDVE.Cursor = Cursors.Arrow;
            BtnChroma.Cursor = Cursors.Arrow;
            BtnPattern.Cursor = Cursors.Arrow;

            PATTERNControls.Visibility = Visibility.Hidden;
            PIPControls.Visibility = Visibility.Visible;
            ChromaControls.Visibility = Visibility.Hidden;
        }

        private void EnableKeyerControls()
        {
            keyPatternControls.EnableControls(true);
            chromaControls.EnableControls(true);
            dvepipControls.EnableControls(true);
            string style = "SwitcherButton";

            BtnUSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnTransKey1.Style = (Style)Application.Current.FindResource(style);
        }

        private void DisableKeyerControls()
        {
            keyPatternControls.EnableControls(false);
            chromaControls.EnableControls(false);
            dvepipControls.EnableControls(false);
            string style = "SwitcherButton_Disabled";

            BtnUSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnTransKey1.Style = (Style)Application.Current.FindResource(style);

            style = "OffLight";
            BtnUSK1OnOffAir.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnTransKey1.Background = (RadialGradientBrush)Application.Current.FindResource(style);
        }


        private void DisableSwitcherControls()
        {

            string style = "SwitcherButton_Disabled";

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
            BtnCBars.Style = (Style)Application.Current.FindResource(style);

            BtnAutoTrans.Style = (Style)Application.Current.FindResource(style);
            BtnCutTrans.Style = (Style)Application.Current.FindResource(style);

            BtnBackgroundTrans.Style = (Style)Application.Current.FindResource(style);

            style = "OffLight";
            BtnPreset1.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset2.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset3.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset4.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset5.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset6.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset7.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPreset8.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnProgram1.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram2.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram3.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram4.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram5.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram6.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram7.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnProgram8.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnDSK1Tie.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnDSK1OnOffAir.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnDSK1Auto.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnDSK2Tie.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnDSK2OnOffAir.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnDSK2Auto.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnFTB.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnCBars.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnAutoTrans.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnCutTrans.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnBackgroundTrans.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            DisableKeyerControls();
            DisableAuxControls();
            DisableKeyerUI();

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

        private void UpdateAuxButtonStyles()
        {
            BtnAux1.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux2.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux3.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux4.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux5.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux6.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux7.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAux8.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnAuxPgm.Background = (ConvertSourceIDToButton(switcherState.AuxID) == 12 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }


        private void UpdateTransButtonStyles()
        {
            BtnBackgroundTrans.Background = (switcherState.TransNextBackground ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnTransKey1.Background = (switcherState.TransNextKey1 ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateKeyerControls()
        {
            ShowKeyerUI();
        }

        private void UpdateFTBButtonStyle()
        {
            BtnFTB.Background = (switcherState.FTB ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateCBarsStyle()
        {
            if (switcherState.ProgramID == (int)BMDSwitcherVideoSources.ColorBars)
            {
                BtnCBars.Background = Application.Current.FindResource("RedLight") as RadialGradientBrush;
            }
            else if (switcherState.PresetID == (int)BMDSwitcherVideoSources.ColorBars)
            {
                BtnCBars.Background = Application.Current.FindResource("GreenLight") as RadialGradientBrush;
            }
            else
            {
                BtnCBars.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
            }
        }

        private void UpdateSlideModeButtons()
        {
            switch (CurrentSlideMode)
            {
                case 0:
                    BtnNoDrive.Foreground = whiteBrush;
                    BtnDrive.Foreground = tealBrush;
                    BtnJump.Foreground = tealBrush;

                    BtnNoDrive.Background = tealBrush;
                    BtnDrive.Background = darkBrush;
                    BtnJump.Background = darkBrush;
                    break;
                case 1:
                    BtnNoDrive.Foreground = tealBrush;
                    BtnDrive.Foreground = whiteBrush;
                    BtnJump.Foreground = tealBrush;

                    BtnNoDrive.Background = darkBrush;
                    BtnDrive.Background = tealBrush;
                    BtnJump.Background = darkBrush;
                    break;
                case 2:
                    BtnNoDrive.Foreground = tealBrush;
                    BtnDrive.Foreground = tealBrush;
                    BtnJump.Foreground = whiteBrush;

                    BtnNoDrive.Background = darkBrush;
                    BtnDrive.Background = darkBrush;
                    BtnJump.Background = tealBrush;
                    break;
            }
        }

        private void UpdateSlideNums()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(UpdateSlideNums);
                return;
            }
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
            UpdateSlideModeButtons();
            UpdateSlidePreviewControls();
        }

        private bool _FeatureFlag_showCurrentVideoTimeOnPreview = true;

        private void UpdateSlideCurrentPreviewTimes()
        {
            Dispatcher.Invoke(() =>
            {
                if (_FeatureFlag_showCurrentVideoTimeOnPreview)
                {
                    if (_FeatureFlag_ShowEffectiveCurrentPreview)
                    {
                        if (Presentation?.EffectiveCurrent.IsControllableMedia() == true)
                        {

                            tbPreviewCurrentVideoDuration.Text = CurrentPreview.MediaTimeRemaining.ToString("\\T\\-mm\\:ss");
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (Presentation?.Current.IsControllableMedia() == true)
                        {
                            tbPreviewCurrentVideoDuration.Text = CurrentPreview.MediaTimeRemaining.ToString("\\T\\-mm\\:ss");
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    tbPreviewCurrentVideoDuration.Visibility = Visibility.Hidden;
                    tbPreviewCurrentVideoDuration.Text = "";
                }
            });
        }

        private void UpdateSlidePreviewControls()
        {
            Dispatcher.Invoke(() =>
            {
                if (_FeatureFlag_showCurrentVideoTimeOnPreview)
                {
                    if (_FeatureFlag_ShowEffectiveCurrentPreview)
                    {
                        if (Presentation?.EffectiveCurrent.IsControllableMedia() == true)
                        {

                            tbPreviewCurrentVideoDuration.Text = CurrentPreview.MediaTimeRemaining.ToString("\\T\\-mm\\:ss");
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Hidden;
                            tbPreviewCurrentVideoDuration.Text = "";
                        }
                    }
                    else
                    {
                        if (Presentation?.Current.IsControllableMedia() == true)
                        {
                            tbPreviewCurrentVideoDuration.Text = CurrentPreview.MediaTimeRemaining.ToString("\\T\\-mm\\:ss");
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Hidden;
                            tbPreviewCurrentVideoDuration.Text = "";
                        }
                    }
                }
                else
                {
                    tbPreviewCurrentVideoDuration.Visibility = Visibility.Hidden;
                    tbPreviewCurrentVideoDuration.Text = "";
                }
                if (Presentation?.Next.IsControllableMedia() == true)
                {
                    NextPreview.PlayMedia();
                    if (NextPreview.MediaLength != TimeSpan.Zero)
                    {
                        tbPreviewNextVideoDuration.Text = NextPreview.MediaLength.ToString("\\T\\-mm\\:ss");
                        tbPreviewNextVideoDuration.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    tbPreviewNextVideoDuration.Visibility = Visibility.Hidden;
                    tbPreviewNextVideoDuration.Text = "";
                }
                if (Presentation?.After.IsControllableMedia() == true)
                {
                    AfterPreview.PlayMedia();
                    if (AfterPreview.MediaLength != TimeSpan.Zero)
                    {
                        tbPreviewAfterVideoDuration.Text = AfterPreview.MediaLength.ToString("\\T\\-mm\\:ss");
                        tbPreviewAfterVideoDuration.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    tbPreviewAfterVideoDuration.Visibility = Visibility.Hidden;
                    tbPreviewAfterVideoDuration.Text = "";
                }
                if (Presentation?.Prev.IsControllableMedia() == true)
                {
                    PrevPreview.PlayMedia();
                    if (PrevPreview.MediaLength != TimeSpan.Zero)
                    {
                        tbPreviewPrevVideoDuration.Text = PrevPreview.MediaLength.ToString("\\T\\-mm\\:ss");
                        tbPreviewPrevVideoDuration.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    tbPreviewPrevVideoDuration.Visibility = Visibility.Hidden;
                    tbPreviewPrevVideoDuration.Text = "";
                }

            });
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



        private async void WindowPreviewKeyDown(object sender, KeyEventArgs e)
        {

            // dont enable shortcuts when focused on textbox
            if (chromaControls.HasTextEntryFocus())
            {
                return;
            }

            _logger.Debug($"User Input [Keyboard] -- ({e.Key})");

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                IsProgramRowLocked = false;
            }

            // shortcuts
            if (e.Key == Key.S)
            {
                ToggleShowShortcuts();
            }

            // extra features

            if (e.Key == Key.L)
            {
                ShowPIPLocationControl();
            }

            if (e.Key == Key.F11)
            {
                _spareDriver.Focus();
                // prevent extra??
                e.Handled = true;
                return;
            }


            // audio

            if (e.Key == Key.A)
            {
                OpenAudioPlayer();
                Focus();
            }

            if (audioPlayer != null)
            {
                if (e.Key == Key.F1)
                {
                    _logger.Debug("User Input [Keyboard] -- (F1) RestartAudio");
                    audioPlayer?.RestartAudio();
                }
                if (e.Key == Key.F2)
                {
                    _logger.Debug("User Input [Keyboard] -- (F2) StopAudio");
                    audioPlayer?.StopAudio();
                }
                if (e.Key == Key.F3)
                {
                    _logger.Debug("User Input [Keyboard] -- (F3) PauseAudio");
                    audioPlayer?.PauseAudio();
                }
                if (e.Key == Key.F4)
                {
                    _logger.Debug("User Input [Keyboard] -- (F4) PlayAudio");
                    audioPlayer?.PlayAudio();
                }
            }

            if (e.Key == Key.M)
            {
                MediaMuted = !MediaMuted;
                if (MediaMuted)
                {
                    _logger.Debug("User Input [Keyboard] -- (m) Media Muted");
                    muteMedia();
                }
                else
                {
                    _logger.Debug("User Input [Keyboard] -- (m) Media UnMuted");
                    unmuteMedia();
                }
            }

            // D1-D8 + (LShift)
            #region program/preset bus
            if (e.Key == Key.D1)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(1);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(1);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(1);
                else if (Keyboard.IsKeyDown(Key.OemTilde))
                    PilotFireLast(1);
                else
                    ClickPreset(1);
            }
            if (e.Key == Key.D2)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(2);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(2);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(2);
                else if (Keyboard.IsKeyDown(Key.OemTilde))
                    PilotFireLast(2);
                else
                    ClickPreset(2);
            }
            if (e.Key == Key.D3)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(3);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(3);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(3);
                else if (Keyboard.IsKeyDown(Key.OemTilde))
                    PilotFireLast(3);
                else
                    ClickPreset(3);
            }
            if (e.Key == Key.D4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(4);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(4);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(4);
                else
                    ClickPreset(4);
            }
            if (e.Key == Key.D5)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(5);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(5);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(5);
                else
                    ClickPreset(5);
            }
            if (e.Key == Key.D6)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(6);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(6);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(6);
                else
                    ClickPreset(6);
            }
            if (e.Key == Key.D7)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(7);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(7);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(7);
                else
                    ClickPreset(7);
            }
            if (e.Key == Key.D8)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(8);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(8);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(8);
                else
                    ClickPreset(8);
            }
            if (e.Key == Key.D9)
            {
                if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(9);
            }
            if (e.Key == Key.D0)
            {
                if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(0);
            }
            #endregion

            if (e.Key == Key.OemTilde)
            {
                if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(12);
            }

            // arrow keys + (LCtrl)
            #region slide controls

            if (e.Key == Key.Home)
            {
                _logger.Debug("USER INPUT [Keyboard] -- (home) ResetPresentationToBeginning");
                ResetPresentationToBegining();
            }

            if (e.Key == Key.Left)
            {
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) prevSlide()");
                prevSlide();
            }
            if (e.Key == Key.Right)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    // do a tied-next slide
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) PerformTiedNextSlide()");
                    await PerformTiedNextSlide();
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) nextSlide()");
                    await nextSlide();
                }
            }
            if (e.Key == Key.Up)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) restartMedia()");
                    restartMedia();
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) playMedia()");
                    playMedia();
                }
            }
            if (e.Key == Key.Down)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) stopMedia()");
                    stopMedia();
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) pauseMedia()");
                    pauseMedia();
                }
            }
            if (e.Key == Key.T)
            {
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) SlideDriveVideo_Current()");
                await SlideDriveVideo_Current();
            }
            #endregion

            // automation
            if (e.Key == Key.PageUp)
            {
                if (Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Toggle State of Conditional 3");
                    SetCondition3(!_Cond3.Value);
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Toggle State of Conditional 1");
                    SetCondition1(!_Cond1.Value);
                }
            }
            if (e.Key == Key.PageDown)
            {
                if (Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Toggle State of Conditional 4");
                    SetCondition4(!_Cond4.Value);
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Toggle State of Conditional 2");
                    SetCondition2(!_Cond2.Value);
                }
            }

            // CCU
            if (e.Key == Key.Y)
            {
                ToggleAutoPilot();
            }
            if (e.Key == Key.U)
            {
                _camMonitor?.ShowUI();
            }

            if (e.Key == Key.OemPipe)
            {
                TogglePilotMode();
            }



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

            //if (e.Key == Key.NumPad1)
            //{
            //ToggleUSK1Type();
            //}

            if (e.Key == Key.C || e.Key == Key.D || e.Key == Key.P)
            {
                if (Keyboard.IsKeyDown(Key.NumPad1))
                {
                    switch (e.Key)
                    {
                        case Key.C:
                            SetSwitcherKeyerChroma();
                            break;
                        case Key.D:
                            SetSwitcherKeyerDVE();
                            break;
                        case Key.P:
                            SetSwitcherKeyerPATTERN();
                            break;
                    }
                }
            }

            if (e.Key == Key.Divide)
            {
                //USK1RuntoA();
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Set PIP to Preset 3");
                dvepipControls.SetupPIPToPresetPosition(3);
            }
            if (e.Key == Key.Multiply)
            {
                //USK1RuntoB();
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Set PIP to Preset 4");
                dvepipControls.SetupPIPToPresetPosition(4);
            }
            if (e.Key == Key.Subtract)
            {
                //USK1RuntoFull();
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Set PIP to Preset 5");
                dvepipControls.SetupPIPToPresetPosition(5);
            }
            if (e.Key == Key.OemOpenBrackets)
            {
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Set PIP to Preset 1");
                dvepipControls.SetupPIPToPresetPosition(1);
            }
            if (e.Key == Key.OemCloseBrackets)
            {
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Set PIP to Preset 2");
                dvepipControls.SetupPIPToPresetPosition(2);
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
                if (Keyboard.IsKeyDown(Key.Z))
                {
                    ClickAux(10);
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Commanding Switcher to Toggle Fade to Black.");
                    switcherManager?.PerformToggleFTB();
                }
            }

            // color bars
            if (e.Key == Key.C && !Keyboard.IsKeyDown(Key.NumPad1))
            {
                if (Keyboard.IsKeyDown(Key.Z))
                {
                    ClickAux(11);
                }
                else
                {
                    _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) Commanding Switcher to set Program Source to Color Bars.");
                    SetProgramColorBars();
                }
            }

            // transition controls
            if (e.Key == Key.Space)
            {
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) RequestCutTransition()");
                RequestCutTransition();

                if (_m_integratedPresenterFeatures?.InterfaceSettings?.SpaceKeyOnlyAsCutTrans == true)
                {
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Enter)
            {
                // TODO:: Guard with global take transition guard
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) AutoTransition()");
                TakeAutoTransition();

                if (_m_integratedPresenterFeatures?.InterfaceSettings?.EnterKeyOnlyAsAutoTrans == true)
                {
                    e.Handled = true;
                    return;
                }
            }

            // modifier for slide mode commands
            // prioritize skip over drive
            if (e.Key == Key.RightShift)
            {
                OverrideSlideModeWithKey(2);
            }
            else if (e.Key == Key.RightCtrl)
            {
                OverrideSlideModeWithKey(1);
            }

        }

        private void PilotFireLast(int v)
        {
            //if (PilotMode == -1)
            //{
            pilotUI?.FireLast(v, _camMonitor);
            //}
            // let it re-fire existing slide
        }

        private void PilotFireZoomBump(string cam, string zpresetmod, int dir, int zms)
        {
            _camMonitor.ChirpZoom_RELATIVE(cam, dir, zms);
        }

        private void TogglePilotMode()
        {
            if (PilotMode == PilotMode.STD)
            {
                PilotMode = PilotMode.LAST;
            }
            else if (PilotMode == PilotMode.LAST)
            {
                PilotMode = PilotMode.EMG;
            }
            else
            {
                PilotMode = PilotMode.STD;
            }
        }

        private void ToggleUSK1Type()
        {
            if (switcherState?.USK1KeyType == 1)
            {
                // change to Chroma
                SetSwitcherKeyerChroma();
            }
            else
            {
                // set DVE
                SetSwitcherKeyerDVE();
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightCtrl || e.Key == Key.RightShift)
            {
                ReleaseSlideModeHotkey();
            }

            if (e.Key == Key.LeftShift)
            {
                IsProgramRowLocked = true;
            }
        }

        // 0 = normal, 1 = drive, 2 = skip

        // keys
        private int _currentKeySlideMode = 1;
        private bool _currentKeyOverride = false;
        // buttons
        private int _currentBtnSlideMode = 1;

        private int CurrentSlideMode
        {
            get
            {
                if (_currentKeyOverride)
                {
                    if (_currentBtnSlideMode == _currentKeySlideMode)
                    {
                        return 0;
                    }
                    else
                    {
                        return _currentKeySlideMode;
                    }
                }
                else
                {
                    return _currentBtnSlideMode;
                }
            }
        }

        private void SetBtnSlideMode(int value)
        {
            _currentBtnSlideMode = value;
            UpdateSlideModeButtons();
        }

        private void OverrideSlideModeWithKey(int value)
        {
            _currentKeySlideMode = value;
            _currentKeyOverride = true;
            UpdateSlideModeButtons();
        }

        private void ReleaseSlideModeHotkey()
        {
            _currentKeyOverride = false;
            UpdateSlideModeButtons();
        }


        #region ButtonClicks
        #region PresetButtonClick
        private void ClickPreset1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(1);
        }
        private void ClickPreset2(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(2);
        }
        private void ClickPreset3(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(3);
        }
        private void ClickPreset4(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(4);
        }
        private void ClickPreset5(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(5);
        }
        private void ClickPreset6(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(6);
        }
        private void ClickPreset7(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(7);
        }
        private void ClickPreset8(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickPreset(8);
        }
        #endregion

        #region ProgramButtonClick

        private void ClickProgram8(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(8);
        }
        private void ClickProgram7(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(7);
        }
        private void ClickProgram6(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(6);
        }
        private void ClickProgram5(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(5);
        }
        private void ClickProgram4(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(4);
        }
        private void ClickProgram3(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(3);
        }
        private void ClickProgram2(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(2);
        }
        private void ClickProgram1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickProgram(1);
        }
        #endregion

        #endregion


        #region SlideDriveVideo

        private bool _FeatureFlag_PostsetShot = true;
        private bool _FeatureFlag_AutomationPreview = true;

        private bool _FeatureFlag_MRETransition = true; // This should be safe enough

        private async Task ExecuteSetupActions(ISlide s)
        {
            await _slideActionEngine.ExecuteSetupActions(s);
        }

        private async Task ExecuteActionSlide(ISlide s)
        {
            await _slideActionEngine.ExecuteMainActions(s);
        }

        private bool MediaMuted = false;

        private void unmuteMedia()
        {
            MediaMuted = false;
            CurrentPreview.MarkUnMuted();
            _display?.UnMuteMedia();
            Dispatcher.Invoke(() =>
            {
                miMute.IsChecked = MediaMuted;
            });
        }

        private void muteMedia()
        {
            MediaMuted = true;
            CurrentPreview.MarkMuted();
            _display?.MuteMedia();
            Dispatcher.Invoke(() =>
            {
                miMute.IsChecked = MediaMuted;
            });
        }

        private async Task SlideDriveVideo_Next(bool Tied = false)
        {
            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) invoked.");
            if (Presentation?.Next != null)
            {

                if (Presentation.Next.AutomationEnabled)
                {
                    if (Presentation.Next.Type == SlideType.Action)
                    {
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Starting Setup Actions for Action Slide ({Presentation.CurrentSlide}) <next {Presentation.Next.Title}>");
                        // run stetup actions
                        await ExecuteSetupActions(Presentation.Next);
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Finished running Setup Actions for Action Slide ({Presentation.CurrentSlide}) <next {Presentation.Next.Title}>");

                        if (Presentation.Next.AutoOnly)
                        {
                            // for now we won't support running 2 back to back fullauto slides.
                            // There really shouldn't be any need.
                            // We also cant run a script's setup actions immediatley afterward.
                            // again it shouldn't be nessecary, since in both cases you can add it to the fullauto slide's setup actions
                            //_logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- AutoOnly so advancing to NextSlide() from Slide ({Presentation.CurrentSlide}) <next {Presentation.Next.Title}>");
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- AutoOnly so advancing to NextSlide() from Slide ({Presentation.CurrentSlide}) <next {Presentation.Next.Title}>. Will re-run automation on next slide.");
                            Presentation.NextSlide();
                            slidesUpdated();

                            // let pilot run in full auto
                            PerformAutoPilotActions(Presentation.EffectiveCurrent.AutoPilotActions);

                            await SlideDriveVideo_Next(Tied);
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- completed AutoOnly. Skipping rest of actions.");
                            return;
                        }
                        // Perform slide actions
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Taking NextSlide() from Slide ({Presentation.CurrentSlide}) of type ACTION");
                        Presentation.NextSlide();
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Starting Actions for Action Slide ({Presentation.CurrentSlide})");
                        await ExecuteActionSlide(Presentation.EffectiveCurrent);
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Finished running Actions for Action Slide ({Presentation.CurrentSlide})");

                        // we will let scripts run a postset if required
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command preset select for postset shot. For Action type slide ({Presentation.CurrentSlide}).");
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                    else if (Presentation.Next.Type == SlideType.Liturgy)
                    {
                        // liturgy will handle presetshot as a require-preset
                        // will set the preset if not selected
                        // not sure this will ever be used, since it does enforce (meaning that manual override won't be availalbe)...
                        // for now we won't implement a preset shot, since I'm not sure what we'd expect it to do.

                        // turn of usk1 if chroma keyer
                        if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Commanding USK1 OFF air for LITURY type slide ({Presentation.CurrentSlide}).");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        // make sure slides aren't the program source
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            //switcherManager?.PerformAutoTransition();
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Commanding AutoTrans (gaurded) since current source is 'slide'. For LITUGY type slide ({Presentation.CurrentSlide}).");
                            PerformGuardedAutoTransition();

                            if (!_FeatureFlag_MRETransition)
                            {
                                _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for delay so that autotrans has completed. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                            }
                            else
                            {
                                await Task.Run(() =>
                                                               {
                                                                   _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for signal that autotrans has completed. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                                                   if (autoTransMRE.WaitOne(TimeSpan.FromMilliseconds(1500)))
                                                                   {
                                                                       _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Autotrans signaled complete. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                                                   }
                                                                   else
                                                                   {
                                                                       _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Autotrans not signaled- timed out after 1500ms. Continuing anyways. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                                                   }
                                                               });
                            }

                        }
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Taking NextSlide() from ({Presentation.CurrentSlide}) of type LITURGY");
                        Presentation.NextSlide();
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        if (Presentation.OverridePres == true)
                        {
                            Presentation.OverridePres = false;
                            slidesUpdated();
                            PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        }
                        // wait for slides to change
                        await Task.Delay((int)(16.6 * 3)); // 3 frames enough?

                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Commanding DSK1 FADE ON. For LITUGY type slide ({Presentation.CurrentSlide}).");
                        switcherManager?.PerformAutoOnAirDSK1();
                        // request auto transition if tied and slides aren't preset source

                        bool waitfortrans = false;
                        if (Tied && switcherState.PresetID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Commanding AutoTrans (gaurded) since Tied transition requested and current prest source is not 'slide'. For LITURGY type slide ({Presentation.CurrentSlide}).");
                            PerformGuardedAutoTransition();
                            waitfortrans = true;
                        }

                        // Handle a postshot selection by setting up the preset
                        // wait for auto transition to clear
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            if (waitfortrans)
                            {
                                _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for autotrans to complete. For LITURGY type slide ({Presentation.CurrentSlide}).");

                                if (!_FeatureFlag_MRETransition)
                                {
                                    _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for delay that autotrans has completed. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                    await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                                }
                                else
                                {
                                    await Task.Run(() =>
                                    {
                                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for signal that autotrans has completed. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                        if (autoTransMRE.WaitOne(TimeSpan.FromMilliseconds(1500)))
                                        {
                                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Autotrans signaled complete. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                        }
                                        else
                                        {
                                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Autotrans not signaled- timed out after 1500ms. Continuing anyways. For LITURGY type slide ({Presentation.CurrentSlide}).");
                                        }
                                    });
                                }
                            }
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command preset select for postset shot. For LITURGY type slide ({Presentation.CurrentSlide}).");
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                    else if (Presentation.Next.Type == SlideType.ChromaKeyStill || Presentation.Next.Type == SlideType.ChromaKeyVideo)
                    {
                        // turn of downstream keys
                        if (switcherState.DSK1OnAir)
                        {
                            switcherManager?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }

                        // set usk1 to chroma (using curently loaded settings)
                        if (switcherState.USK1OnAir)
                        {
                            switcherManager?.PerformOffAirUSK1();
                        }
                        if (switcherState.USK1KeyType != 2)
                        {
                            switcherManager?.SetUSK1TypeChroma();
                        }
                        // select fill source to be slide, since slide is marked as key it must be the key source
                        switcherManager?.PerformUSK1FillSourceSelect(_config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId);

                        // pull slide off air (and then reset the preview to the old source)
                        long previewsource = switcherState.PresetID;
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            //switcherManager?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        switcherManager?.PerformPresetSelect((int)previewsource);

                        // next slide
                        Presentation.NextSlide();
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        if (Presentation.OverridePres == true)
                        {
                            Presentation.OverridePres = false;
                            slidesUpdated();
                            PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        }

                        // start mediaplayout
                        if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                        {
                            playMedia();
                            await Task.Delay(_config?.PrerollSettings.ChromaVideoPreRoll ?? 0);
                        }

                        // turn on chroma key once playout has started
                        switcherManager?.PerformOnAirUSK1();

                        // chroma keys won't do postset shots

                    }
                    else
                    {
                        // turn off usk1 if chroma keyer
                        if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command switcher USK1 to go OFF Air for slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        if (switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command switcher DSK1 to go OFF Air for slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                            switcherManager?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Taking NextSlide() from ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                        Presentation.NextSlide();
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        if (Presentation.OverridePres == true)
                        {
                            Presentation.OverridePres = false;
                            slidesUpdated();
                            PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        }
                        if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- playMedia() for slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                            playMedia();
                            await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                        }
                        bool waitfortrans = false;
                        if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command switcher to Preset 'slide' source for slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                            ClickPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                            await Task.Delay(_config.PrerollSettings.PresetSelectDelay);
                            //switcherManager?.PerformAutoTransition();
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command AutoTrans [Gaurded] for slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                            PerformGuardedAutoTransition();
                            waitfortrans = true;
                        }

                        // wait for pre-roll to clear the preset before setting up a new preset shot
                        // wait for auto transition to clear
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            if (waitfortrans)
                            {
                                _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Wait for AutoTrans complete on slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");

                                if (!_FeatureFlag_MRETransition)
                                {
                                    _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for delay that autotrans has completed. For FULL/VIDEO type slide ({Presentation.CurrentSlide}).");
                                    await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                                }
                                else
                                {
                                    await Task.Run(() =>
                                    {
                                        _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Waiting for signal that autotrans has completed. For FULL/VIDEO type slide ({Presentation.CurrentSlide}).");
                                        if (autoTransMRE.WaitOne(TimeSpan.FromMilliseconds(1500)))
                                        {
                                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Autotrans signaled complete. For FULL/VIDEO type slide ({Presentation.CurrentSlide}).");
                                        }
                                        else
                                        {
                                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Autotrans not signaled- timed out after 1500ms. Continuing anyways. For FULL/VIDEO type slide ({Presentation.CurrentSlide}).");
                                        }
                                    });
                                }
                            }
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- Command preset select for postset shot. For LITURGY type slide ({Presentation.CurrentSlide}).");
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }


                    }
                }
                else
                {
                    _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) -- [No Automation] Taking NextSlide() on slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    if (Presentation.OverridePres == true)
                    {
                        Presentation.OverridePres = false;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    }

                }
                // At this point we've switched to the slide
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);

                PerformAutoPilotActions(Presentation.EffectiveCurrent.AutoPilotActions);
            }
        }

        private void SlideUndrive_ToSlide(Slide s)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (s != null && Presentation != null)
            {
                Presentation.Override = s;
                Presentation.OverridePres = true;
                slidesUpdated();
                PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
            }
        }

        private async Task SlideDriveVideo_ToSlide(Slide s)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (s != null && Presentation != null)
            {
                if (s.AutomationEnabled)
                {
                    if (s.Type == SlideType.Action)
                    {
                        _logger.Debug($"SlideDriveVideo_ToSlide -- About to ExecuteSetupActions() for slide {s.Title}");
                        // Run Setup Actions
                        await ExecuteSetupActions(s);
                        // Execute Slide Actions
                        Presentation.Override = s;
                        Presentation.OverridePres = true;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        _logger.Debug($"SlideDriveVideo_ToSlide -- About to ExecuteActionSlide() for slide {s.Title}");
                        await ExecuteActionSlide(s);
                    }
                    else if (s.Type == SlideType.Liturgy)
                    {
                        // turn of usk1 if chroma keyer
                        if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to go OFF AIR on USK1 for slide {s.Title}");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        // make sure slides aren't the program source
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to transition from 'slide' source to preset source for slide {s.Title}");
                            //switcherManager?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        Presentation.Override = s;
                        Presentation.OverridePres = true;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        switcherManager?.PerformAutoOnAirDSK1();

                        // Handle a postshot selection by setting up the preset
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to set preset source for slide's postset for slide {s.Title}");
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                    else if (s.Type == SlideType.ChromaKeyStill || s.Type == SlideType.ChromaKeyVideo)
                    {
                        // turn of downstream keys
                        if (switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to go OFF AIR with DSK1 for set for slide {s.Title}");
                            switcherManager?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }

                        // set usk1 to chroma (using curently loaded settings)
                        if (switcherState.USK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to go ON AIR with USK1 for slide slide {s.Title}");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        if (switcherState.USK1KeyType != 2)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to configure USK1 for chromakey for slide {s.Title}");
                            switcherManager?.SetUSK1TypeChroma();
                        }
                        // select fill source to be slide, since slide is marked as key it must be the key source
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to set USK1 fill source to 'slide' slide {s.Title}");
                        switcherManager?.PerformUSK1FillSourceSelect(_config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId);

                        // pull slide off air (and then reset the preview to the old source)
                        long previewsource = switcherState.PresetID;
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to take 'slide' source OFF AIR and transition to preset source for slide {s.Title}");
                            //switcherManager?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to set preset source back to {previewsource} for slide {s.Title}");
                        switcherManager?.PerformPresetSelect((int)previewsource);

                        // set slide
                        Presentation.Override = s;
                        Presentation.OverridePres = true;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);


                        // start mediaplayout
                        if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Begin Media playback for slide {s.Title}");
                            playMedia();
                            await Task.Delay(_config?.PrerollSettings.ChromaVideoPreRoll ?? 0);
                        }

                        // turn on chroma key once playout has started
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to go ON AIR on USK1 for slide {s.Title}");
                        switcherManager?.PerformOnAirUSK1();

                    }
                    else
                    {
                        // turn of usk1 if chroma keyer
                        if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to go OFF AIR on USK1 for slide {s.Title}");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        if (switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Commanding switcher to go OFF AIR on DSK1 for slide {s.Title}");
                            switcherManager?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        Presentation.Override = s;
                        Presentation.OverridePres = true;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                        if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Begin Media playback for slide {s.Title}");
                            playMedia();
                            await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                        }

                        bool waitfortrans = false;
                        if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Command switcher to select 'slide' for preset source for slide {s.Title}");
                            ClickPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                            await Task.Delay(_config.PrerollSettings.PresetSelectDelay);
                            //switcherManager?.PerformAutoTransition();
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Command switcher to transition 'slide' form preset source to program source for slide {s.Title}");
                            PerformGuardedAutoTransition();
                            waitfortrans = true;
                        }

                        // Handle a postshot selection by setting up the preset
                        // wait for auto transition to clear
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Command switcher to select preset source for postset shot for slide {s.Title}");
                            if (waitfortrans)
                            {
                                await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                            }
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                }
                else
                {
                    // just go to the next slide
                    // next slide
                    _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. Take NextSlide() for slide {s.Title}");
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    if (Presentation.OverridePres == true)
                    {
                        Presentation.OverridePres = false;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    }

                }
                // At this point we've switched to the slide
                _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {s.Type}. About to call SlideDriveVideo_Action() for slide {s.Title}");
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);


                PerformAutoPilotActions(Presentation.EffectiveCurrent.AutoPilotActions);
            }

        }

        private void SlideDriveVideo_Action(ISlide s)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switch (s.PreAction)
            {
                case "t1restart":
                    if (_FeatureFlag_automationtimer1enabled)
                    {
                        ResetGpTimer1();
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task SlideDriveVideo_Current()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (Presentation?.EffectiveCurrent != null)
            {
                _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. About to call DisableSlidePoolOverrides() for slide {Presentation.EffectiveCurrent.Title}");
                currentpoolsource = null;
                if (Presentation.EffectiveCurrent.AutomationEnabled)
                {
                    if (Presentation.EffectiveCurrent.Type == SlideType.Action)
                    {
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. About to re-run SetupActions for slide {Presentation.EffectiveCurrent.Title}");
                        // Re-run setup actions
                        await ExecuteSetupActions(Presentation.EffectiveCurrent);
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. About to re-run Actions for slide {Presentation.EffectiveCurrent.Title}");
                        // Run Actions
                        await ExecuteActionSlide(Presentation.EffectiveCurrent);

                        // we will let scripts run a postset if required
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            _logger.Debug($"SlideDriveVideo_Current() -- Command preset select for postset shot. For Action type slide ({Presentation.CurrentSlide}).");
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                    else if (Presentation.EffectiveCurrent.Type == SlideType.Liturgy)
                    {
                        // turn of usk1 if chroma keyer
                        if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go OFF AIR for USK1 for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        // make sure slides aren't the program source
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to transition from 'slide' as program source to preset source for slide {Presentation.EffectiveCurrent.Title}");
                            //switcherManager?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go ON AIR for DSK1 for slide {Presentation.EffectiveCurrent.Title}");
                        switcherManager?.PerformAutoOnAirDSK1();

                        // Handle a postshot selection by setting up the preset
                        // wait for auto transition to clear
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to select preset source for slide's postset shot for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                    else if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyStill || Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                    {
                        // turn of downstream keys
                        if (switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go OFF AIR for DSK1 for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }

                        // set usk1 to chroma (using curently loaded settings)
                        if (switcherState.USK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go OFF AIR for USK1 for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        if (switcherState.USK1KeyType != 2)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to configure USK1 as chromakey for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.SetUSK1TypeChroma();
                        }
                        // select fill source to be slide, since slide is marked as key it must be the key source
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to set USK1 fill source to 'slide' for slide {Presentation.EffectiveCurrent.Title}");
                        switcherManager?.PerformUSK1FillSourceSelect(_config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId);

                        // pull slide off air (and then reset the preview to the old source)
                        long previewsource = switcherState.PresetID;
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to transition program source from 'slide' to preset source for slide {Presentation.EffectiveCurrent.Title}");
                            //switcherManager?.PerformAutoTransition();
                            PerformGuardedAutoTransition();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to select preset source to {previewsource} for slide {Presentation.EffectiveCurrent.Title}");
                        switcherManager?.PerformPresetSelect((int)previewsource);

                        // set slide
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                        // start mediaplayout
                        if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Begin media playback for slide {Presentation.EffectiveCurrent.Title}");
                            playMedia();
                            await Task.Delay(_config?.PrerollSettings.ChromaVideoPreRoll ?? 0);
                        }

                        // turn on chroma key once playout has started
                        _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go ON AIR for USK1 for slide {Presentation.EffectiveCurrent.Title}");
                        switcherManager?.PerformOnAirUSK1();

                    }
                    else
                    {
                        // turn of usk1 if chroma keyer
                        if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go OFF AIR for USK1 for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.PerformOffAirUSK1();
                        }
                        if (switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to go OFF AIR for DSK1 for slide {Presentation.EffectiveCurrent.Title}");
                            switcherManager?.PerformAutoOffAirDSK1();
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                        if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Begin media playback for slide {Presentation.EffectiveCurrent.Title}");
                            playMedia();
                            await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                        }

                        bool waitfortrans = false;
                        if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to select 'slide' as preset source for slide {Presentation.EffectiveCurrent.Title}");
                            ClickPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                            await Task.Delay(_config.PrerollSettings.PresetSelectDelay);
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to transition preset source 'slide' to program source for slide {Presentation.EffectiveCurrent.Title}");
                            PerformGuardedAutoTransition();
                            waitfortrans = true;
                        }

                        // Handle a postshot selection by setting up the preset
                        // wait for auto transition to clear
                        if (Presentation?.EffectiveCurrent.PostsetEnabled == true && _FeatureFlag_PostsetShot)
                        {
                            _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. Commanding switcher to select preset source for slide's postset shot for slide {Presentation.EffectiveCurrent.Title}");
                            if (waitfortrans)
                            {
                                await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                            }
                            switcherManager?.PerformPresetSelect(Presentation.EffectiveCurrent.PostsetId);
                        }
                    }
                }
                // Do nothing for nodrive slides
                // Do Action on current slide
                _logger.Debug($"SlideDriveVideo_ToSlide -- SLIDE type is {Presentation.EffectiveCurrent.Type}. About to call SlideDriveVideo_Action() for slide {Presentation.EffectiveCurrent.Title}");
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);


                PerformAutoPilotActions(Presentation.EffectiveCurrent.AutoPilotActions);
            }

        }

        private void PerformGuardedAutoTransition(bool force_guard = false)
        {
            if (_FeatureFlag_DriveMode_AutoTransitionGuard || force_guard)
            {
                // if guarded, check if transition is already in progress
                if (!switcherState.InTransition)
                {
                    if (_FeatureFlag_MRETransition)
                    {
                        autoTransMRE.Reset();
                    }
                    switcherManager?.PerformAutoTransition();
                }
            }
            else
            {
                switcherManager?.PerformAutoTransition();
                if (_FeatureFlag_MRETransition)
                {
                    autoTransMRE.Reset();
                }
            }
        }

        // TODO: add this to config....
        private bool _FeatureFlag_EnableAutoPilot = true;
        private void PerformAutoPilotActions(List<IPilotAction> actions)
        {
            if (_FeatureFlag_EnableAutoPilot)
            {
                _logger.Info($"Performing AutoPilotActions for slide.");
                foreach (var preset in actions)
                {
                    _logger.Info($"Requesting execution of preset: {preset.DisplayInfo}");
                    preset.Execute(_camMonitor, 8);
                }
            }
        }


        #endregion


        bool activepresentation = false;
        IPresentation _pres;
        public IPresentation Presentation { get => _pres; }

        public event PresentationStateUpdate PresentationStateUpdated;

        PresenterDisplay _display;
        PresenterDisplay _keydisplay;

        private void OpenPresentation(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Presentation";
            if (ofd.ShowDialog() == true)
            {
                string path = System.IO.Path.GetDirectoryName(ofd.FileName);

                // change title?
                string pname = path;
                Title = $"{_nominalTitle} ({pname})";

                // create new presentation
                Presentation pres = new Presentation();
                pres.Create(path);
                pres.StartPres();
                _pres = pres;
                activepresentation = true;
                _logger.Info($"Loaded Presentation from {path}");

                // apply config if required
                if (pres.HasSwitcherConfig)
                {
                    _logger.Info($"Presentation loading specified switcher configuration. Re-configuring now.");
                    _config = pres.SwitcherConfig;
                    SetSwitcherSettings(false);
                }
                if (pres.HasUserConfig)
                {
                    _logger.Info($"Presentation loading specified setting user settings. Re-configuring now.");
                    LoadUserSettings(pres.UserConfig);
                }
                if (pres.HasCCUConfig)
                {
                    _logger.Info($"Presentation loading specified ccu config settings. Re-configuring now.");
                    _camMonitor?.LoadConfig(pres.CCPUConfig);
                }

                // overwrite display of old presentation if already open
                if (_display != null && _display.IsWindowVisilbe)
                {
                }
                else
                {
                    _logger.Info($"Graphics Engine has no active display window. Creating window now.");
                    _display = new PresenterDisplay(this, false);
                    player.AuxDisplay = _display;
                    _display.OnMediaPlaybackTimeUpdated += _display_OnMediaPlaybackTimeUpdated;
                    // start display
                    _display.Show();
                }

                if (_keydisplay == null || _keydisplay?.IsWindowVisilbe == false)
                {
                    _logger.Info($"Graphics Engine has no active key window. Creating window now.");
                    _keydisplay = new PresenterDisplay(this, true);
                    // no need to get playback event info
                    _keydisplay.Show();
                }

                slidesUpdated();

                //FireOnConditionalsUpdated();
                _condVarCalculator.ReleaseVariables(ICalculatedVariableManager.PRESENTATION_OWNED_VARIABLE);
                _condVarCalculator?.NotifyOfChange();

                PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                if (Presentation.EffectiveCurrent.ForceRunOnLoad)
                {
                    SlideDriveVideo_Current();
                }
            }
            // preserve mute status
            if (MediaMuted)
            {
                muteMedia();
            }
        }

        private void _display_OnMediaPlaybackTimeUpdated(object sender, MediaPlaybackTimeEventArgs e)
        {
            // update textblocks
            TbMediaTimeRemaining.Text = e.Remaining.ToString("m\\:ss");
            TbMediaTimeCurrent.Text = e.Current.ToString("m\\:ss");
            TbMediaTimeDurration.Text = e.Length.ToString("m\\:ss");
            // synchronize current preview
            if (Math.Abs(CurrentPreview.videoPlayer.Position.TotalMilliseconds - e.Current.TotalMilliseconds) > 400)
            {
                CurrentPreview.videoPlayer.Position = e.Current;
            }
        }


        public bool _FeatureFlag_ShowEffectiveCurrentPreview = true;

        private void ClickToggleShowEffectiveCurrentPreview(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleShowEffectiveCurrentPreview();
        }

        private void ToggleShowEffectiveCurrentPreview()
        {
            SetShowEffectiveCurrentPreview(!_FeatureFlag_ShowEffectiveCurrentPreview);
        }

        private void SetShowEffectiveCurrentPreview(bool showeffective)
        {
            _FeatureFlag_ShowEffectiveCurrentPreview = showeffective;
            cbShowEffectiveCurrent.IsChecked = _FeatureFlag_ShowEffectiveCurrentPreview;
        }


        private Guid currentGuid;

        private void slidesUpdated()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(slidesUpdated);
                return;
            }

            _display?.ShowSlide(false);
            _keydisplay?.ShowSlide(true);

            // update previews
            if (Presentation != null)
            {
                PrevPreview.SetMedia(Presentation.Prev, false);
                if (_FeatureFlag_ShowEffectiveCurrentPreview)
                {
                    if (currentGuid != Presentation.EffectiveCurrent.Guid)
                    {
                        CurrentPreview.SetMedia(Presentation.EffectiveCurrent, false);
                        currentGuid = Presentation.EffectiveCurrent.Guid;
                    }
                }
                else
                {
                    if (currentGuid != Presentation.Current.Guid)
                    {
                        CurrentPreview.SetMedia(Presentation.Current, false);
                        currentGuid = Presentation.Current.Guid;
                    }
                }
                NextPreview.SetMedia(Presentation.Next, false);
                AfterPreview.SetMedia(Presentation.After, false);
            }
            UpdateSlidePreviewControls();
            UpdatePreviewsPostets();

            // update previews
            CurrentPreview?.FireOnSwitcherStateChangedForAutomation(_lastState, _config);
            NextPreview?.FireOnSwitcherStateChangedForAutomation(_lastState, _config);
        }

        private void UpdatePreviewsPostets()
        {
            // update previews
            if (Presentation != null)
            {
                UpdatePostsetUi(PrevPreview, Presentation.Prev);
                if (_FeatureFlag_ShowEffectiveCurrentPreview)
                {
                    UpdatePostsetUi(CurrentPreview, Presentation.EffectiveCurrent);
                }
                else
                {
                    UpdatePostsetUi(CurrentPreview, Presentation.Current);
                }
                UpdatePostsetUi(NextPreview, Presentation.Next);
                UpdatePostsetUi(AfterPreview, Presentation.After);
            }
        }

        private void UpdatePostsetUi(IMediaPlayer2 preview, ISlide slide)
        {
            if (slide.PostsetEnabled && _FeatureFlag_PostsetShot)
            {
                preview.ShowPostset = true;
                preview.SetPostset(slide.PostsetId);
            }
            else
            {
                preview.ShowPostset = false;
            }
        }


        bool _FeatureFlag_displayPrevAfter = true;


        #region slideshow commands

        private async Task PerformTiedNextSlide()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            bool tied = false;
            // only makes sense to do this in drive mode
            if (CurrentSlideMode == 1)
            {
                // only makes sense to do a tied-next slide for liturgy type slides
                tied = true;
            }
            // do a next slide and a gaurded auto-trans
            await nextSlide(tied);
        }

        private async Task nextSlide(bool Tied = false)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (activepresentation)
            {
                if (CurrentSlideMode == 1)
                {
                    _logger.Debug("nextSlide() -- Calling SlideDriveVideo_Next");
                    await SlideDriveVideo_Next(Tied);
                }
                else if (CurrentSlideMode == 2)
                {
                    _logger.Debug("nextSlide() -- skipping next slide");
                    Presentation.SkipNextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                }
                else
                {
                    _logger.Debug("nextSlide() -- overriding next slide");
                    Presentation.OverridePres = false;
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                }
            }
        }
        private void prevSlide()
        {
            if (activepresentation)
            {
                if (CurrentSlideMode == 2)
                {
                    _logger.Debug("nextSlide() -- skipping prev slide");
                    Presentation.SkipPrevSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                }
                else
                {
                    _logger.Debug("nextSlide() -- going to previous slide");
                    Presentation.OverridePres = false;
                    Presentation.PrevSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                }
            }
        }

        private void playMedia()
        {
            if (activepresentation)
            {
                Dispatcher.Invoke(() =>
                {
                    _display.StartMediaPlayback();
                    _keydisplay.StartMediaPlayback();
                    if (!Presentation.OverridePres || _FeatureFlag_ShowEffectiveCurrentPreview)
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
            OnPlaybackStateChanged?.Invoke(this, new MediaPlaybackEventArgs(MediaPlaybackEventArgs.State.Play));
        }
        private void pauseMedia()
        {
            if (activepresentation)
            {
                _display.PauseMediaPlayback();
                _keydisplay.PauseMediaPlayback();
                if (!Presentation.OverridePres || _FeatureFlag_ShowEffectiveCurrentPreview)
                {
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.PauseMedia();
                }
                else
                {
                    currentpoolsource.PauseMedia();
                }
            }
            OnPlaybackStateChanged?.Invoke(this, new MediaPlaybackEventArgs(MediaPlaybackEventArgs.State.Pause));
        }
        private void stopMedia()
        {
            if (activepresentation)
            {
                _display.StopMediaPlayback();
                _keydisplay.StopMediaPlayback();
                if (!Presentation.OverridePres || _FeatureFlag_ShowEffectiveCurrentPreview)
                {
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.videoPlayer.Stop();
                }
                else
                {
                    currentpoolsource.StopMedia();
                }
            }
            OnPlaybackStateChanged?.Invoke(this, new MediaPlaybackEventArgs(MediaPlaybackEventArgs.State.Stop));
        }
        private void restartMedia()
        {
            if (activepresentation)
            {
                _display.RestartMediaPlayback();
                _keydisplay.RestartMediaPlayback();
                if (!Presentation.OverridePres || _FeatureFlag_ShowEffectiveCurrentPreview)
                {
                    CurrentPreview.videoPlayer.Volume = 0;
                    CurrentPreview.ReplayMedia();
                }
                else
                {
                    currentpoolsource.RestartMedia();
                }
            }
            OnPlaybackStateChanged?.Invoke(this, new MediaPlaybackEventArgs(MediaPlaybackEventArgs.State.Restart));
        }

        public event EventHandler<MediaPlaybackEventArgs> OnPlaybackStateChanged;

        public class MediaPlaybackEventArgs : EventArgs
        {
            public MediaPlaybackEventArgs(State state)
            {
                PlaybackState = state;
            }
            public enum State
            {
                Play,
                Stop,
                Pause,
                Restart,
            }

            public State PlaybackState { get; set; }

        }


        #endregion


        #region slide/media buttons
        private void ClickRestartMedia(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            restartMedia();
        }
        private void ClickStopMedia(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            stopMedia();
        }
        private void ClickPauseMedia(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            pauseMedia();
        }
        private void ClickPlayMedia(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            playMedia();
        }
        private async void ClickNextSlide(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            await nextSlide();
        }
        private void ClickPrevSlide(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            prevSlide();
        }
        #endregion



        private void ClickConnectMock(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            MockConnectSwitcher();
        }
        private async void ClickConnect(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            await ConnectSwitcher();
        }



        private void ClickSlideNoDriveVideo(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetBtnSlideMode(0);
        }


        private void ClickSlideDriveVideo(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetBtnSlideMode(1);
            /*
            if (CurrentSlideMode == 1)
            {
                SetBtnSlideMode(0);
            }
            else
            {
                SetBtnSlideMode(1);
            }
            */
        }

        private void ClickSlideSkipMode(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetBtnSlideMode(2);
            /*
            if (CurrentSlideMode == 2)
            {
                SetBtnSlideMode(0);
            }
            else
            {
                SetBtnSlideMode(2);
            }
            */
        }


        private void ClickCutTrans(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            RequestCutTransition();
        }
        private void ClickAutoTrans(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TakeAutoTransition();
        }

        private void ClickDSK1Toggle(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleDSK1();
        }

        private void ClickDSK1Auto(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            AutoDSK1();
        }
        private void ClickDSK1Tie(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleTieDSK1();
        }

        private void ClickDSK2Tie(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleTieDSK2();
        }
        private void ClickDSK2Toggle(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleDSK2();
        }
        private void ClickDSK2Auto(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            AutoDSK2();
        }
        private void ClickFTB(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformToggleFTB();
        }

        private void ClickUSK1Toggle(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleUSK1();
        }

        private async void ClickTakeSlide(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            await SlideDriveVideo_Current();
        }

        private void ClickResetgpTimer1(object sender, MouseButtonEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ResetGpTimer1();
        }

        private void ResetGpTimer1()
        {
            gp_timer_1.Stop();
            timer1span = TimeSpan.Zero;
            UpdateGPTimer1();
            gp_timer_1.Start();
            GpT1Shots = 0;

        }


        SlidePoolSource currentpoolsource = null;

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // stop timers
            system_second_timer?.Stop();
            gp_timer_1?.Stop();
            gp_timer_2?.Stop();
            shot_clock_timer?.Stop();
            _display?.Close();
            _keydisplay?.Close();
            switcherManager?.Close();
            audioPlayer?.Close();
            pipctrl?.Close();
            _camMonitor?.Shutdown();
            _logger.Info("Integrated Presenter requested to close by USER");
            Application.Current.Shutdown();
        }

        private void ClickConfigureSwitcher(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetSwitcherSettings();
        }

        private void USK1RuntoA()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformUSK1RunToKeyFrameA();
        }

        private void USK1RuntoB()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformUSK1RunToKeyFrameB();
        }

        private void USK1RuntoFull()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformUSK1RunToKeyFrameFull();
        }

        private void ClickPIPRunToFull(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            USK1RuntoFull();
        }

        public void ChangeUSK1FillSource(int source)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformUSK1FillSourceSelect(ConvertButtonToSourceID(source));
        }

        private void ClickPIP1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(1);
        }

        private void ClickPIP2(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(2);
        }

        private void ClickPIP3(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(3);
        }

        private void ClickPIP4(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(4);
        }

        private void ClickPIP5(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(5);
        }

        private void ClickPIP6(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(6);
        }

        private void ClickPIP7(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(7);
        }

        private void ClickPIP8(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(8);
        }


        private void ToggleTransBkgd()
        {
            switcherManager?.PerformToggleBackgroundForNextTrans();
        }

        public void ToggleTransKey1()
        {
            switcherManager?.PerformToggleKey1ForNextTrans();
        }

        private void ClickTransBkgd(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleTransBkgd();
        }

        private void ClickTransKey1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleTransKey1();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
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
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                _config = BMDSwitcherConfigSettings.Load(ofd.FileName);
            }
            SetSwitcherSettings();
        }

        private void SetSwitcherSettings(bool resetPIPPositions = true)
        {
            // update UI
            UpdateUIButtonLabels();

            // Update previews
            PrevPreview.SetMVConfigForPostset(_config.MultiviewerConfig);
            CurrentPreview.SetMVConfigForPostset(_config.MultiviewerConfig);
            NextPreview.SetMVConfigForPostset(_config.MultiviewerConfig);
            AfterPreview.SetMVConfigForPostset(_config.MultiviewerConfig);

            chromaControls.NewConfig(_config);
            dvepipControls.NewConfig(_config);
            keyPatternControls.NewConfig(_config);

            // update PIP place hotkeys
            dvepipControls.UpdateUIPIPPlaceKeys(switcherState);

            // config switcher
            switcherManager?.ConfigureSwitcher(_config, resetPIPPositions);
        }

        private void UpdateUIButtonLabels()
        {
            keyPatternControls.UpdateButtonLabels(_config.Routing);
            chromaControls.UpdateButtonLabels(_config.Routing);
            dvepipControls.UpdateButtonLabels(_config.Routing);
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
                    case 12:
                        UpdateButtonPgmLabels(btn);
                        break;
                    default:
                        break;
                }
            }
        }

        private void UpdateButtonPgmLabels(ButtonSourceMapping config)
        {
            BtnAuxPgm.Content = config.ButtonName;
        }

        private void UpdateButton1Labels(ButtonSourceMapping config)
        {
            BtnPreset1.Content = config.ButtonName;
            BtnProgram1.Content = config.ButtonName;
            BtnAux1.Content = config.ButtonName;
        }

        private void UpdateButton2Labels(ButtonSourceMapping config)
        {
            BtnPreset2.Content = config.ButtonName;
            BtnProgram2.Content = config.ButtonName;
            BtnAux2.Content = config.ButtonName;
        }
        private void UpdateButton3Labels(ButtonSourceMapping config)
        {
            BtnPreset3.Content = config.ButtonName;
            BtnProgram3.Content = config.ButtonName;
            BtnAux3.Content = config.ButtonName;
        }
        private void UpdateButton4Labels(ButtonSourceMapping config)
        {
            BtnPreset4.Content = config.ButtonName;
            BtnProgram4.Content = config.ButtonName;
            BtnAux4.Content = config.ButtonName;
        }

        private void UpdateButton5Labels(ButtonSourceMapping config)
        {
            BtnPreset5.Content = config.ButtonName;
            BtnProgram5.Content = config.ButtonName;
            BtnAux5.Content = config.ButtonName;
        }
        private void UpdateButton6Labels(ButtonSourceMapping config)
        {
            BtnPreset6.Content = config.ButtonName;
            BtnProgram6.Content = config.ButtonName;
            BtnAux6.Content = config.ButtonName;
        }
        private void UpdateButton7Labels(ButtonSourceMapping config)
        {
            BtnPreset7.Content = config.ButtonName;
            BtnProgram7.Content = config.ButtonName;
            BtnAux7.Content = config.ButtonName;
        }
        private void UpdateButton8Labels(ButtonSourceMapping config)
        {
            BtnPreset8.Content = config.ButtonName;
            BtnProgram8.Content = config.ButtonName;
            BtnAux8.Content = config.ButtonName;
        }


        private void SetDefaultConfig()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            _config = Configurations.SwitcherConfig.DefaultConfig.GetDefaultConfig();
        }

        public BMDSwitcherConfigSettings Config { get => _config; }


        private void ClickAux1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(1);
        }

        private void ClickAux2(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(2);
        }

        private void ClickAux3(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(3);
        }

        private void ClickAux4(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(4);
        }

        private void ClickAux5(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(5);
        }

        private void ClickAux6(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(6);
        }

        private void ClickAux7(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(7);
        }
        private void ClickAux8(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(8);
        }
        private void ClickAuxPgm(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ClickAux(12);
        }


        private void EnableAuxButtons()
        {
            BtnAux1.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux2.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux3.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux4.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux5.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux6.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux7.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAux8.Style = Application.Current.FindResource("SwitcherButton") as Style;
            BtnAuxPgm.Style = Application.Current.FindResource("SwitcherButton") as Style;
        }

        private void DisableAuxControls()
        {
            BtnAux1.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux2.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux3.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux4.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux5.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux6.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux7.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAux8.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
            BtnAuxPgm.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;

            BtnAux1.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux2.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux3.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux4.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux5.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux6.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux7.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAux8.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
            BtnAuxPgm.Background = Application.Current.FindResource("OffLight") as RadialGradientBrush;
        }


        private void SetPIPPosition(BMDUSKDVESettings config)
        {
            Dispatcher.Invoke(() =>
            {
                switcherManager?.SetPIPPosition(config);
            });
        }

        private void ClickOpenPIPLocationControls(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ShowPIPLocationControl();
        }

        private void SetKeyFrameAOnSwitcher(BMDUSKDVESettings config)
        {
            Dispatcher.Invoke(() =>
            {
                switcherManager?.SetPIPKeyFrameA(config);
            });
        }

        private void SetKeyFrameBOnSwitcher(BMDUSKDVESettings config)
        {
            Dispatcher.Invoke(() =>
            {
                switcherManager?.SetPIPKeyFrameB(config);
            });
        }


        bool _FeatureFlag_automationtimer1enabled = true;

        private void ClickToggleAutomationTimer1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetEnableSermonTimer(!_FeatureFlag_automationtimer1enabled);
        }

        private void SetEnableSermonTimer(bool enabled)
        {
            _FeatureFlag_automationtimer1enabled = enabled;
            miTimer1Restart.IsChecked = _FeatureFlag_automationtimer1enabled;
        }

        private bool ProgramRowLocked = true;

        private bool IsProgramRowLocked
        {
            get => ProgramRowLocked;
            set
            {
                ProgramRowLocked = value;
                if (ProgramRowLocked)
                {
                }
                UpdateProgramRowLockButtonUI();
            }
        }

        private void UpdateProgramRowLockButtonUI()
        {
            if (IsProgramRowLocked)
            {
                //btnProgramLock.Content = "LOCKED";
                //btnProgramLock.Foreground = Brushes.WhiteSmoke;
                imgPgmBusLockIcon.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Icons/WhiteLock.png"));
                pgmBusLockColor.Color = (Color)FindResource("teal");
            }
            else
            {
                //btnProgramLock.Content = "UNLOCKED";
                //btnProgramLock.Foreground = Brushes.Orange;
                imgPgmBusLockIcon.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Icons/WhiteOpenLock.png"));
                pgmBusLockColor.Color = (Color)FindResource("red");
            }
        }

        private void ClickBtnProgramLock(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            IsProgramRowLocked = !IsProgramRowLocked;
        }

        public AudioPlayer audioPlayer;

        private void ClickViewAudioPlayer(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            OpenAudioPlayer();
        }

        private void OpenAudioPlayer()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (audioPlayer != null)
            {
                if (!audioPlayer.IsVisible)
                {
                    _logger.Info($"Audio player is no longer visible. Re-initialize audio player.");
                    audioPlayer = new AudioPlayer(this);
                    audioPlayer.Show();
                }
                else
                {
                    audioPlayer.WindowState = WindowState.Normal;
                }
            }
            else
            {
                _logger.Info($"Audio player not initialized. Creating new audio player.");
                audioPlayer = new AudioPlayer(this);
                audioPlayer.Show();
            }

        }

        private void ClickCBars(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetProgramColorBars();
        }

        private void SetProgramColorBars()
        {
            switcherManager?.PerformProgramSelect((int)BMDSwitcherVideoSources.ColorBars);
        }

        private void ClickToggleShowCurrentVideoCountdownTimer(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleShowCurrentVideoCountdownTimer();
        }

        private void ToggleShowCurrentVideoCountdownTimer()
        {
            _FeatureFlag_showCurrentVideoTimeOnPreview = !_FeatureFlag_showCurrentVideoTimeOnPreview;
            cbShowCurrentVideoCountdownTimer.IsChecked = _FeatureFlag_showCurrentVideoTimeOnPreview;
        }

        private void ClickDVEMode(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetSwitcherKeyerDVE();
        }
        private void ClickPATTERNMode(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetSwitcherKeyerPATTERN();
        }

        private void ClickChromaMode(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetSwitcherKeyerChroma();
        }

        private void SetSwitcherKeyerDVE()
        {
            //switcherManager?.ConfigureUSK1PIP(_config.USKSettings.PIPSettings);
            _logger.Debug("Commanding Switcher to reconfigure USK1 for DVE PIP.");
            switcherManager?.SetUSK1TypeDVE();
            // force blocking state update
            SwitcherManager_SwitcherStateChanged(switcherManager?.ForceStateUpdate());
            if (switcherManager != null)
            {
                ShowKeyerUI();
            }
        }

        private void SetSwitcherKeyerPATTERN()
        {
            //switcherManager?.ConfigureUSK1PIP(_config.USKSettings.PIPSettings);
            _logger.Debug("Commanding Switcher to reconfigure USK1 for DVE PIP.");
            switcherManager?.SetUSK1TypePATTERN();
            // force blocking state update
            SwitcherManager_SwitcherStateChanged(switcherManager?.ForceStateUpdate());
            if (switcherManager != null)
            {
                ShowKeyerUI();
            }
        }


        private void SetSwitcherKeyerChroma()
        {
            //switcherManager?.ConfigureUSK1Chroma(_config.USKSettings.ChromaSettings);
            _logger.Debug("Commanding Switcher to reconfigure USK1 for chroma key.");
            switcherManager?.SetUSK1TypeChroma();
            // force blocking state update
            if (switcherManager != null)
            {
                SwitcherManager_SwitcherStateChanged(switcherManager?.ForceStateUpdate() ?? new BMDSwitcherState());
                //chromaControls.UpdateChromaSettings(switcherState.ChromaSettings);
                //chromaControls.tbChromaHue.Text = switcherState.ChromaSettings.Hue.ToString();
                //chromaControls.tbChromaGain.Text = switcherState.ChromaSettings.Gain.ToString();
                //chromaControls.tbChromaLift.Text = switcherState.ChromaSettings.Lift.ToString();
                //chromaControls.tbChromaYSuppress.Text = switcherState.ChromaSettings.YSuppress.ToString();
                //chromaControls.tbChromaNarrow.Text = switcherState.ChromaSettings.Narrow.ToString();
                ShowKeyerUI();
            }
        }

        private void ClickChroma1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(1);
        }

        private void ClickChroma2(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(2);
        }

        private void ClickChroma3(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(3);
        }

        private void ClickChroma4(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(4);
        }

        private void ClickChroma5(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(5);
        }

        private void ClickChroma6(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(6);
        }

        private void ClickChroma7(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(7);
        }

        private void ClickChroma8(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ChangeUSK1FillSource(8);
        }

        private void LoadDefault(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetSwitcherSettings();
        }

        PIPControl pipctrl;
        private void ClickViewAdvancedFlyKeySettings(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ShowPIPLocationControl();
        }

        private void ShowPIPLocationControl()
        {
            if (pipctrl == null)
            {
                pipctrl = new PIPControl(this, SetPIPPosition, SetKeyFrameAOnSwitcher, SetKeyFrameBOnSwitcher, switcherState?.DVESettings ?? _config.USKSettings.PIPSettings, switcherState?.USK1FillSource, ConvertButtonToSourceID);
            }
            if (pipctrl.HasClosed)
            {
                pipctrl = new PIPControl(this, SetPIPPosition, SetKeyFrameAOnSwitcher, SetKeyFrameBOnSwitcher, switcherState?.DVESettings ?? _config.USKSettings.PIPSettings, switcherState?.USK1FillSource, ConvertButtonToSourceID);
            }
            pipctrl.Show();
            pipctrl.Focus();
        }

        private void ClickPIPRunToA(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformUSK1RunToKeyFrameA();
        }

        private void ClickPIPRunToB(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            switcherManager?.PerformUSK1RunToKeyFrameB();
        }

        private bool _showshortcuts = false;
        public bool ShowShortcuts
        {
            get => _showshortcuts;
            set
            {
                _showshortcuts = value;
                ShortcutVisibility = _showshortcuts ? Visibility.Visible : Visibility.Collapsed;
                ShowHideShortcutsUI();
            }
        }

        private void ShowHideShortcutsUI()
        {
            // audio player
            audioPlayer?.ShowHideShortcuts(ShowShortcuts);

            // media players
            CurrentPreview?.ShowHideShortcuts(ShowShortcuts);

            // Update MainWindow UI
            #region MainWindowUI
            ksc_cut.Visibility = ShortcutVisibility;
            ksc_auto.Visibility = ShortcutVisibility;
            ksc_bkgd.Visibility = ShortcutVisibility;

            ksc_usk1.Visibility = ShortcutVisibility;
            ksc_usk1t.Visibility = ShortcutVisibility;
            ksc_usk1m.Visibility = ShortcutVisibility;

            chromaControls.ShowHideShortcutsUI(ShowShortcuts);
            dvepipControls.ShowHideShortcutsUI(ShowShortcuts);
            keyPatternControls.ShowShortcuts(_showshortcuts);

            ksc_cond1.Visibility = ShortcutVisibility;
            ksc_cond2.Visibility = ShortcutVisibility;
            ksc_cond3.Visibility = ShortcutVisibility;
            ksc_cond4.Visibility = ShortcutVisibility;

            ksc_pl.Visibility = ShortcutVisibility;

            ksc_s1.Visibility = ShortcutVisibility;
            ksc_s2.Visibility = ShortcutVisibility;
            ksc_s3.Visibility = ShortcutVisibility;
            ksc_s4.Visibility = ShortcutVisibility;
            ksc_s5.Visibility = ShortcutVisibility;
            ksc_s6.Visibility = ShortcutVisibility;
            ksc_s7.Visibility = ShortcutVisibility;
            ksc_s8.Visibility = ShortcutVisibility;

            ksc_ps1.Visibility = ShortcutVisibility;
            ksc_ps2.Visibility = ShortcutVisibility;
            ksc_ps3.Visibility = ShortcutVisibility;
            ksc_ps4.Visibility = ShortcutVisibility;
            ksc_ps5.Visibility = ShortcutVisibility;
            ksc_ps6.Visibility = ShortcutVisibility;
            ksc_ps7.Visibility = ShortcutVisibility;
            ksc_ps8.Visibility = ShortcutVisibility;

            ksc_mplay.Visibility = ShortcutVisibility;
            ksc_mpause.Visibility = ShortcutVisibility;
            ksc_mstop.Visibility = ShortcutVisibility;
            ksc_mrestart.Visibility = ShortcutVisibility;

            ksc_sdm.Visibility = ShortcutVisibility;
            ksc_sp.Visibility = ShortcutVisibility;
            ksc_sn.Visibility = ShortcutVisibility;
            ksc_st.Visibility = ShortcutVisibility;

            ksc_slidereset.Visibility = ShortcutVisibility;

            ksc_stest.Visibility = ShortcutVisibility;
            ksc_ftb.Visibility = ShortcutVisibility;

            ksc_dsk1.Visibility = ShortcutVisibility;
            ksc_dsk1a.Visibility = ShortcutVisibility;
            ksc_dsk1t.Visibility = ShortcutVisibility;
            ksc_dsk2.Visibility = ShortcutVisibility;
            ksc_dsk2a.Visibility = ShortcutVisibility;
            ksc_dsk2t.Visibility = ShortcutVisibility;

            ksc_ap.Visibility = ShortcutVisibility;
            ksc_a1.Visibility = ShortcutVisibility;
            ksc_a2.Visibility = ShortcutVisibility;
            ksc_a3.Visibility = ShortcutVisibility;
            ksc_a4.Visibility = ShortcutVisibility;
            ksc_a5.Visibility = ShortcutVisibility;
            ksc_a6.Visibility = ShortcutVisibility;
            ksc_a7.Visibility = ShortcutVisibility;
            ksc_a8.Visibility = ShortcutVisibility;

            #endregion

        }

        public Visibility ShortcutVisibility { get; set; } = Visibility.Collapsed;

        private void ToggleShowShortcuts()
        {
            ShowShortcuts = !ShowShortcuts;
            miShowShortcuts.IsChecked = ShowShortcuts;
        }

        private void ClickShowKeyboardShortcuts(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleShowShortcuts();
        }

        private void ToggleMutePres(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (MediaMuted)
            {
                unmuteMedia();
            }
            else
            {
                muteMedia();
            }
        }

        private void ClickShowOnlineHelp(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/kjgriffin/LivestreamServiceSuite/wiki/Integrated-Presenter-Shortcuts");
        }

        private void ClickSetVideoPrerollDelay(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            // NOTE FOR NOW ONLY CHANGE VIDEOS, NOT CHROMA KEY VIDEOS
            InputForm input = new InputForm(_config.PrerollSettings.VideoPreRoll.ToString(), "VIDEO PREROLL (MS)");
            if (input.ShowDialog() == true)
            {
                int preroll = _config.PrerollSettings.VideoPreRoll;
                try
                {
                    preroll = Convert.ToInt32(input.UserInput);
                }
                catch
                {
                }
                _config.PrerollSettings.VideoPreRoll = preroll;
            }
        }

        private void ClickSetDrivePresetSelectDelay(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            InputForm input = new InputForm(_config.PrerollSettings.PresetSelectDelay.ToString(), "PRESET SELECT DELAY (MS)");
            if (input.ShowDialog() == true)
            {
                int preroll = _config.PrerollSettings.PresetSelectDelay;
                try
                {
                    preroll = Convert.ToInt32(input.UserInput);
                }
                catch
                {
                }
                _config.PrerollSettings.PresetSelectDelay = preroll;
            }
        }

        private bool _FeatureFlag_DriveMode_AutoTransitionGuard = true;

        private void ToggleDriveAutoTransGuard()
        {
            SetEnableDriveAutoTransGuard(!_FeatureFlag_DriveMode_AutoTransitionGuard);
            miDriveAutoTransGuard.IsChecked = _FeatureFlag_DriveMode_AutoTransitionGuard;
        }

        private void SetEnableDriveAutoTransGuard(bool enabled)
        {
            _FeatureFlag_DriveMode_AutoTransitionGuard = enabled;
            miDriveAutoTransGuard.IsChecked = _FeatureFlag_DriveMode_AutoTransitionGuard;
        }

        private void ClickToggleDriveAutoTransGuard(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleDriveAutoTransGuard();
        }

        private void ResetSlideshow_Click(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ResetPresentationToBegining();

            if (Presentation?.EffectiveCurrent.ForceRunOnLoad == true)
            {
                SlideDriveVideo_Current();
            }
        }

        private void ResetPresentationToBegining()
        {
            if (Presentation == null)
            {
                return;
            }
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            Presentation?.StartPres();
            slidesUpdated();
            PresentationStateUpdated?.Invoke(Presentation?.Current);
        }

        private void ClickResetgpTimer2(object sender, MouseButtonEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ResetGpTimer2();
        }

        private void ResetGpTimer2()
        {
            gp_timer_2.Stop();
            timer2span = TimeSpan.Zero;
            UpdateGPTimer2();
            gp_timer_2.Start();
            GpT2Shots = 0;
        }


        private int _totalShots = 0;
        private int _gpT1Shots = 0;
        private int _gpT2Shots = 0;

        private int TotalShots
        {
            get => _totalShots;
            set
            {
                _totalShots = value;
                UpdateTOSShots();
            }
        }

        private int GpT1Shots
        {
            get => _gpT1Shots;
            set
            {
                _gpT1Shots = value;
                UpdateGPT1Shots();
            }
        }

        private int GpT2Shots
        {
            get => _gpT2Shots;
            set
            {
                _gpT2Shots = value;
                UpdateGP2Shots();
            }
        }

        private void ClickResetTotalShotCount(object sender, MouseButtonEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TotalShots = 0;
        }

        private void UpdateTOSShots()
        {
            tb_tos_TotalShots.Dispatcher.Invoke(() =>
            {
                tb_tos_TotalShots.Text = _totalShots.ToString();
            });
        }

        private void UpdateGPT1Shots()
        {
            tb_t1_shots.Dispatcher.Invoke(() =>
            {
                tb_t1_shots.Text = _gpT1Shots.ToString();
            });
        }

        private void UpdateGP2Shots()
        {
            tb_t2_shots.Dispatcher.Invoke(() =>
            {
                tb_t2_shots.Text = _gpT2Shots.ToString();
            });
        }

        private void ClickToggleCutTransGuard(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleCutTransGuard();
        }

        private void ToggleCutTransGuard()
        {
            SetCutTransGuard(!_FeatureFlag_GaurdCutTransition);
        }

        private void SetCutTransGuard(bool enabled)
        {
            _FeatureFlag_GaurdCutTransition = enabled;
            miCutTransitionGuard.IsChecked = _FeatureFlag_GaurdCutTransition;
        }


        private void SetShowAutomationPreviews(bool show)
        {
            _FeatureFlag_AutomationPreview = show;
            if (show)
            {
                CurrentPreview.ShowAutomationPreviews = false;
                NextPreview.ShowAutomationPreviews = true;
            }
            else
            {
                CurrentPreview.ShowAutomationPreviews = false;
                NextPreview.ShowAutomationPreviews = false;
            }
        }

        private void ClickTogglePostsetFeature(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TogglePostset();
        }

        private void TogglePostset()
        {
            SetPostsetEnabled(!_FeatureFlag_PostsetShot);
        }

        private void SetPostsetEnabled(bool enabled)
        {
            _FeatureFlag_PostsetShot = enabled;
            miPostset.IsChecked = _FeatureFlag_PostsetShot;
            PrevPreview.ShowPostset = _FeatureFlag_PostsetShot;
            CurrentPreview.ShowPostset = _FeatureFlag_PostsetShot;
            NextPreview.ShowPostset = _FeatureFlag_PostsetShot;
            AfterPreview.ShowPostset = _FeatureFlag_PostsetShot;
            UpdatePreviewsPostets();
        }


        private Configurations.FeatureConfig.IntegratedPresenterFeatures IntegratedPresenterFeatures { get => _m_integratedPresenterFeatures; }
        private Configurations.FeatureConfig.IntegratedPresenterFeatures _m_integratedPresenterFeatures = Configurations.FeatureConfig.IntegratedPresenterFeatures.Default();
        private void LoadUserSettings(Configurations.FeatureConfig.IntegratedPresenterFeatures config)
        {
            _m_integratedPresenterFeatures = config;

            SetShowEffectiveCurrentPreview(config.ViewSettings.View_PreviewEffectiveCurrent);
            if (config.ViewSettings.View_DefaultOpenAdvancedPIPLocation)
            {
                ShowPIPLocationControl();
            }
            if (config.ViewSettings.View_DefaultOpenAudioPlayer)
            {
                OpenAudioPlayer();
            }

            SetCutTransGuard(config.AutomationSettings.EnableCutTransGuard);
            SetEnableSermonTimer(config.AutomationSettings.EnableSermonTimer);
            SetEnableDriveAutoTransGuard(config.AutomationSettings.EnableDriveModeAutoTransGuard);
            SetPostsetEnabled(config.AutomationSettings.EnablePostset);

            SetInteractionSettings(config.InterfaceSettings);

            if (config.PresentationSettings.StartPresentationMuted)
            {
                muteMedia();
            }
            else
            {
                unmuteMedia();
            }

        }

        private void SetInteractionSettings(IntegratedPresentationFeatures_Interface interfaceSettings)
        {
            miEnterButtonClickGuard.IsChecked = interfaceSettings.EnterKeyOnlyAsAutoTrans;
            miCutTransitionGuard.IsChecked = interfaceSettings.SpaceKeyOnlyAsCutTrans;
        }

        private Configurations.FeatureConfig.IntegratedPresenterFeatures GetCurrentUserSettings()
        {
            return new Configurations.FeatureConfig.IntegratedPresenterFeatures()
            {
                AutomationSettings = new Configurations.FeatureConfig.IntegratedPresentationFeatures_Automation()
                {
                    EnableCutTransGuard = _FeatureFlag_GaurdCutTransition,
                    EnableDriveModeAutoTransGuard = _FeatureFlag_DriveMode_AutoTransitionGuard,
                    EnablePostset = _FeatureFlag_PostsetShot,
                    EnableSermonTimer = _FeatureFlag_automationtimer1enabled,
                    EnableAutomationStepsPreview = _FeatureFlag_AutomationPreview,
                },
                PresentationSettings = new Configurations.FeatureConfig.IntegratedPresentationFeatures_Presentation()
                {
                    StartPresentationMuted = MediaMuted,
                },
                ViewSettings = new Configurations.FeatureConfig.IntegratedPresentationFeatures_View()
                {
                    View_PrevAfterPreviews = _FeatureFlag_displayPrevAfter,
                    View_AdvancedPresentation = false,
                    View_PreviewEffectiveCurrent = _FeatureFlag_ShowEffectiveCurrentPreview,
                    View_DefaultOpenAdvancedPIPLocation = IntegratedPresenterFeatures.ViewSettings.View_DefaultOpenAdvancedPIPLocation,
                    View_DefaultOpenAudioPlayer = IntegratedPresenterFeatures.ViewSettings.View_DefaultOpenAudioPlayer,
                },
            };
        }

        private void LoadUserDefault(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            LoadUserSettings(Configurations.FeatureConfig.IntegratedPresenterFeatures.Default());
        }

        private void LoadUserSettings(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Files (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                LoadUserSettings(Configurations.FeatureConfig.IntegratedPresenterFeatures.Load(ofd.FileName));
            }
        }

        private void SaveUserSettings(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "IntegratedPresenterUserConfig";
            sfd.AddExtension = true;
            sfd.DefaultExt = "json";
            sfd.Filter = "JSON Files (*.json)|*.json";
            if (sfd.ShowDialog() == true)
            {
                GetCurrentUserSettings()?.Save(sfd.FileName);
            }
        }

        private void ClickDisconnectSwitcher(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            if (switcherManager != null)
            {
                switcherManager.Disconnect();
            }
        }

        private void ClickToggleMRETransitionFeature(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleMRETransitionFeature();
        }
        private void ToggleMRETransitionFeature()
        {
            _FeatureFlag_MRETransition = !_FeatureFlag_MRETransition;
            miMRETransitions.IsChecked = _FeatureFlag_MRETransition;
            if (_FeatureFlag_MRETransition)
            {
                autoTransMRE.Reset();
            }
        }

        private void clickENTEROnlyForTrans(object sender, RoutedEventArgs e)
        {
            _m_integratedPresenterFeatures.InterfaceSettings.EnterKeyOnlyAsAutoTrans = miEnterButtonClickGuard.IsChecked;
        }

        private void clickSPACEOnlyForTrans(object sender, RoutedEventArgs e)
        {
            _m_integratedPresenterFeatures.InterfaceSettings.SpaceKeyOnlyAsCutTrans = miSpaceButtonClickGuard.IsChecked;
        }


        UIValue<bool> _Cond1 = new UIValue<bool>();
        UIValue<bool> _Cond2 = new UIValue<bool>();
        UIValue<bool> _Cond3 = new UIValue<bool>();
        UIValue<bool> _Cond4 = new UIValue<bool>();


        /*
        private Dictionary<string, ExposedVariable> GetExposedVariables()
        {
            // currently get switcher state
            Dictionary<string, ExposedVariable> switcherVars = VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(switcherState);
            // report presentation state
            Dictionary<string, ExposedVariable> presVars = new Dictionary<string, ExposedVariable>();
            if (_pres != null)
            {
                presVars = VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(_pres);
            }
            // report pilot state
            Dictionary<string, ExposedVariable> pilotVars = new Dictionary<string, ExposedVariable>();

            return new Dictionary<string, ExposedVariable>(switcherVars.Concat(presVars).Concat(pilotVars));
        }

        private Dictionary<string, bool> GetCurrentConditionStatuses(Dictionary<string, WatchVariable> externalWatches)
        {
            var conditions = new Dictionary<string, bool>
            {
                ["1"] = _Cond1.Value,
                ["2"] = _Cond2.Value,
                ["3"] = _Cond3.Value,
                ["4"] = _Cond4.Value,
            };

            // Assumes that the curent state of the switcher is acurate
            // i.e. doesn't immediately re-poll... perhaps this is ok??
            // effectively just make sure to script delays sufficient to process anything we might depend on

            // extract required switcher state to satisfy requests
            var exposedVariables = GetExposedVariables(); // VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(switcherState);

            // try to find each requested value
            if (externalWatches != null)
            {
                foreach (var watch in externalWatches)
                {
                    bool evaluation = false;
                    // use reflection to find a matching value
                    if (exposedVariables.TryGetValue(watch.Value.VPath, out var eVal))
                    {
                        dynamic val = eVal.Value;
                        // process equation
                        switch (watch.Value.VType)
                        {
                            case AutomationActionArgType.Integer:
                                // yeah.... so int's are 32 bit and longs are 64 bit
                                // so just to be safe do it with longs (even if we describe it as an int)
                                // because some bmd switcher state uses longs
                                evaluation = (long)val == (long)watch.Value.ExpectedVal;
                                break;
                            case AutomationActionArgType.String:
                                evaluation = (string)val == (string)watch.Value.ExpectedVal;
                                break;
                            case AutomationActionArgType.Double:
                                evaluation = (double)val == (double)watch.Value.ExpectedVal;
                                break;
                            case AutomationActionArgType.Boolean:
                                evaluation = (bool)val == (bool)watch.Value.ExpectedVal;
                                break;
                        }
                    }

                    conditions[watch.Key] = evaluation;
                }
            }


            return conditions;

        }
        */

        private void FireOnConditionalsUpdated()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(FireOnConditionalsUpdated);
                return;
            }

            var cstatus = _condVarCalculator.GetConditionals(Presentation?.WatchedVariables);
            PrevPreview?.FireOnActionConditionsUpdated(cstatus);
            CurrentPreview?.FireOnActionConditionsUpdated(cstatus);
            NextPreview?.FireOnActionConditionsUpdated(cstatus);
            AfterPreview?.FireOnActionConditionsUpdated(cstatus);
            UpdateSpeculativeSlideJumpUI();

            //this.OnConditionalsUpdated.Invoke(this, new EventArgs());
        }

        private void UpdateSpeculativeSlideJumpUI()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(UpdateSpeculativeSlideJumpUI);
                return;
            }
            //var cstatus = (this as IAutomationConditionProvider).GetConditionals(_pres?.WatchedVariables ?? new Dictionary<string, WatchVariable>());
            var cstatus = _condVarCalculator?.GetConditionals(_pres?.WatchedVariables ?? new Dictionary<string, WatchVariable>());
            // Speculative Update for jump slides
            var willrun = false;
            var jtarget = Presentation?.Next?.SetupActions?.FirstOrDefault(x => x.Action?.Action == AutomationActions.JumpToSlide);
            if (jtarget == null)
            {
                jtarget = Presentation?.Next?.Actions?.FirstOrDefault(x => x.Action?.Action == AutomationActions.JumpToSlide);
            }
            if (jtarget != null)
            {
                // figure out if the action will run
                willrun = jtarget.Action.MeetsConditionsToRun(cstatus);
                if (willrun)
                {
                    if (jtarget.Action.TryEvaluateAutomationActionParmeter<int>("SlideNum", _condVarCalculator, out var index))
                    {
                        if (index >= 0 && index < Presentation.SlideCount)
                        {
                            var tslide = Presentation.Slides[index];
                            SpeculativeJumpPreview.SetMedia(tslide, false);
                            SpeculativeJumpPreviewDisplay.Visibility = Visibility.Visible;

                            if (tslide.Type == SlideType.Video || tslide.Type == SlideType.ChromaKeyVideo)
                            {
                                // Speculative don't auto-play the media here
                                //SpeculativeJumpPreview.PlayMedia();
                                if (SpeculativeJumpPreview.MediaLength != TimeSpan.Zero)
                                {
                                    tbPreviewSpeculativeJumpVideoDuration.Text = SpeculativeJumpPreview.MediaLength.ToString("\\T\\-mm\\:ss");
                                    tbPreviewSpeculativeJumpVideoDuration.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                tbPreviewSpeculativeJumpVideoDuration.Visibility = Visibility.Hidden;
                                tbPreviewSpeculativeJumpVideoDuration.Text = "";
                            }




                            // only need to do this if visible
                            SpeculativeJumpPreview?.FireOnActionConditionsUpdated(cstatus);
                        }
                    }
                }
            }
            if (!willrun)
            {
                SpeculativeJumpPreviewDisplay.Visibility = Visibility.Hidden;
            }

        }

        private void SetCondition1(bool value)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()} with arg value {value}");
            _Cond1.Value = value;
            //FireOnConditionalsUpdated();
            _condVarCalculator?.NotifyOfChange();
        }

        private void SetCondition2(bool value)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()} with arg value {value}");
            _Cond2.Value = value;
            //FireOnConditionalsUpdated();
            _condVarCalculator?.NotifyOfChange();
        }

        private void SetCondition3(bool value)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()} with arg value {value}");
            _Cond3.Value = value;
            //FireOnConditionalsUpdated();
            _condVarCalculator?.NotifyOfChange();
        }

        private void SetCondition4(bool value)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()} with arg value {value}");
            _Cond4.Value = value;
            //FireOnConditionalsUpdated();
            _condVarCalculator?.NotifyOfChange();
        }


        private void ClickToggleCond1(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Click: Toggle Cond 1");
            SetCondition1(!_Cond1.Value);
        }

        private void ClickToggleCond2(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Click: Toggle Cond 2");
            SetCondition2(!_Cond2.Value);
        }
        private void ClickToggleCond3(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Click: Toggle Cond 3");
            SetCondition3(!_Cond3.Value);
        }

        private void ClickToggleCond4(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Click: Toggle Cond 4");
            SetCondition4(!_Cond4.Value);
        }


        private void ClickToggleShowAutomationPreviews(object sender, RoutedEventArgs e)
        {
            SetShowAutomationPreviews(!_FeatureFlag_AutomationPreview);
        }




        private int _slideMediaActiveTab = 0;
        int SlideMediaActiveTab
        {
            get => _slideMediaActiveTab;
            set
            {
                _slideMediaActiveTab = value;
                UpdateSlideMediaTabControls();
            }
        }
        private void UpdateSlideMediaTabControls()
        {
            Dispatcher.Invoke(() =>
            {
                if (_slideMediaActiveTab == 0)
                {
                    slideControlGroup.Visibility = Visibility.Visible;
                    btnSlideCtrlTab.Foreground = whiteBrush;
                    btnSlideCtrlTab.Background = tealBrush;
                    mediaControlGroup.Visibility = Visibility.Hidden;
                    btnMediaCtrlTab.Foreground = tealBrush;
                    btnMediaCtrlTab.Background = darkBrush;
                }
                else
                {
                    slideControlGroup.Visibility = Visibility.Hidden;
                    btnSlideCtrlTab.Foreground = tealBrush;
                    btnSlideCtrlTab.Background = darkBrush;
                    mediaControlGroup.Visibility = Visibility.Visible;
                    btnMediaCtrlTab.Foreground = whiteBrush;
                    btnMediaCtrlTab.Background = tealBrush;
                }
            });
        }

        private void ClickSlideCtrlTab(object sender, RoutedEventArgs e)
        {
            SlideMediaActiveTab = 0;
        }

        private void ClickMediaCtrlTab(object sender, RoutedEventArgs e)
        {
            SlideMediaActiveTab = 1;
        }


        private void ClickOpenCCUDriverUI(object sender, RoutedEventArgs e)
        {
            _camMonitor?.ShowUI();
        }

        private void ClickToggleAutoPilot(object sender, RoutedEventArgs e)
        {
            ToggleAutoPilot();
        }

        private void ToggleAutoPilot()
        {
            _FeatureFlag_EnableAutoPilot = !_FeatureFlag_EnableAutoPilot;
            Dispatcher.Invoke(() =>
            {
                miCCUEnabled.IsChecked = _FeatureFlag_EnableAutoPilot;
            });
            UpdatePilotUI();
        }

        private void UpdatePilotUI()
        {
            Dispatcher.Invoke(() =>
            {
                pilotUI.UpdateUI(_FeatureFlag_EnableAutoPilot,
                                 Presentation?.EffectiveCurrent?.AutoPilotActions ?? new List<IPilotAction>(),
                                 Presentation?.Next?.AutoPilotActions ?? new List<IPilotAction>(),
                                 Presentation?.EffectiveCurrent?.EmergencyActions ?? new List<IPilotAction>(),
                                 Presentation?.CurrentSlide ?? 0,
                                 PilotMode);
                pilotUI.FireOnSwitcherStateChangedForAutomation(_lastState, _config, PredictNextSlideTakesLiveCam());
            });
        }


        PilotMode PilotMode
        {
            get => _pilotMode;
            set
            {
                _pilotMode = value;
                UpdatePilotUI();
            }
        }


        PilotMode _pilotMode = PilotMode.STD;

        private void UpdatePilotUIStatus(string camName, params string[] args)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => UpdatePilotUIStatus(camName, args));
                return;
            }

            pilotUI.UpdateUIStatus(camName, args);
        }



        bool m_hotreloadpresentation = false;
        PresentationHotReloadService hotReloadService;
        private void ClickToggleHotReloadPresentation(object sender, RoutedEventArgs e)
        {
            ToggleHotReloadPres();
        }

        private void ToggleHotReloadPres()
        {
            m_hotreloadpresentation = !m_hotreloadpresentation;
            miHotReloadPres.IsChecked = m_hotreloadpresentation;

            Dispatcher.Invoke(() =>
            {
                tbHotReloadStatus.Text = m_hotreloadpresentation ? "READY" : "OFF";
                tbHotReloadStatus.Foreground = m_hotreloadpresentation ? tealBrush : whiteBrush;
            });


            if (m_hotreloadpresentation)
            {
                Title = $"{_nominalTitle} (Hot Load Presentation Mode)";

                hotReloadService = new PresentationHotReloadService();
                hotReloadService.OnHotReloadReady += HotReloadService_OnHotReloadReady;
                hotReloadService.StartListening();
            }
            else
            {
                Title = $"{_nominalTitle}";

                hotReloadService.OnHotReloadReady -= HotReloadService_OnHotReloadReady;
                hotReloadService.StopListening();
            }
        }

        private void HotReloadService_OnHotReloadReady(object sender, EventArgs e)
        {
            LoadHotPresentation();
        }

        private async Task LoadHotPresentation()
        {
            int lastSlide = Math.Max(Presentation?.CurrentSlide - 1 ?? 0, 0);

            Dispatcher.Invoke(() =>
            {
                tbHotReloadStatus.Text = "RELOADING";
                tbHotReloadStatus.Foreground = yellowBrush;
            });

            // can we load on a seperate task thread, and only then swap to UI ??
            IPresentation pres = null;

            await Task.Run(() =>
            {
                // create new presentation
                pres = MirroredPresentationBuilder.Create();
            });

            Internal_StartHotLoadPresentation(pres, lastSlide);

            Dispatcher.Invoke(() =>
            {
                tbHotReloadStatus.Text = "LOADED";
                tbHotReloadStatus.Foreground = greenBrush;
            });
        }

        private void Internal_StartHotLoadPresentation(IPresentation pres, int lastSlide)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => Internal_StartHotLoadPresentation(pres, lastSlide));
                return;
            }

            pres.StartPres(lastSlide);
            _pres = pres;
            activepresentation = true;
            _logger.Info($"Hot Loaded Presentation from memory files");

            // apply config if required
            if (pres.HasSwitcherConfig)
            {
                _logger.Info($"Presentation loading specified switcher configuration. Re-configuring now.");
                _config = pres.SwitcherConfig;
                SetSwitcherSettings(false);
            }
            if (pres.HasUserConfig)
            {
                _logger.Info($"Presentation loading specified setting user settings. Re-configuring now.");
                LoadUserSettings(pres.UserConfig);
            }
            if (pres.HasCCUConfig)
            {
                _logger.Info($"Presentation loading specified ccu config settings. Re-configuring now.");
                _camMonitor?.LoadConfig(pres.CCPUConfig);
                (switcherManager as MockBMDSwitcherManager)?.UpdateCCUConfig(pres.CCPUConfig as CCPUConfig_Extended);
            }

            // overwrite display of old presentation if already open
            if (_display != null && _display.IsWindowVisilbe)
            {
            }
            else
            {
                _logger.Info($"Graphics Engine has no active display window. Creating window now.");
                _display = new PresenterDisplay(this, false);
                player.AuxDisplay = _display;
                _display.OnMediaPlaybackTimeUpdated += _display_OnMediaPlaybackTimeUpdated;
                // start display
                _display.Show();
            }

            if (_keydisplay == null || _keydisplay?.IsWindowVisilbe == false)
            {
                _logger.Info($"Graphics Engine has no active key window. Creating window now.");
                _keydisplay = new PresenterDisplay(this, true);
                // no need to get playback event info
                _keydisplay.Show();
            }

            slidesUpdated();

            //FireOnConditionalsUpdated();
            // release all variables related to old presentation
            _condVarCalculator.ReleaseVariables(ICalculatedVariableManager.PRESENTATION_OWNED_VARIABLE);
            _condVarCalculator?.NotifyOfChange();

            // clears all emg/last and curent actions...
            // if we're hotreload into the pres we may want a way to 'clear' just the curent so it gets going again
            // then if we get real fancy we can try and figure out what the 'last' and 'emg' should be based on previous slides
            Dictionary<string, IPilotAction> calculatedLast = new Dictionary<string, IPilotAction>();
            Dictionary<string, IPilotAction> calculatedEmg = new Dictionary<string, IPilotAction>();

            // based on curent slide num...
            // 'run' each slide up to curent slide and have it overwrite into the calculations
            for (int i = 0; i < Math.Min(lastSlide, pres.SlideCount - 1); i++)
            {
                foreach (var act in pres.Slides[i].AutoPilotActions)
                {
                    calculatedLast[act.CamName] = act;
                }
                foreach (var act in pres.Slides[i].EmergencyActions)
                {
                    calculatedEmg[act.CamName] = act;
                }
            }

            pilotUI.ClearState(calculatedLast, calculatedEmg);

            PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

            if (Presentation.EffectiveCurrent.ForceRunOnLoad)
            {
                SlideDriveVideo_Current();
            }


            // preserve mute status
            if (MediaMuted)
            {
                muteMedia();
            }

        }

        bool m_mockCameras = false;
        private void ClickConnectMockCameras(object sender, RoutedEventArgs e)
        {
            m_mockCameras = !m_mockCameras;
            miConnectMockCameras.IsChecked = m_mockCameras;
            ToggleMockCameras();
        }

        private void ToggleMockCameras()
        {
            _camMonitor?.Shutdown();
            if (_camMonitor != null)
            {
                _camMonitor.OnCommandUpdate -= _camMonitor_OnCommandUpdate;

                var mock = _camMonitor as MockCCUMonitor;
                if (mock != null)
                {
                    mock.OnCameraMoved -= Mock_OnCameraMoved;
                }
            }

            if (m_mockCameras)
            {
                MockCCUMonitor mon = new MockCCUMonitor();
                _camMonitor = mon;

                mon.OnCameraMoved += Mock_OnCameraMoved;
            }
            else
            {
                _camMonitor = new CCPUPresetMonitor(true, _logger, this);
            }

            if (Presentation?.HasCCUConfig == true)
            {
                _camMonitor.LoadConfig(Presentation.CCPUConfig);
            }

            _camMonitor.OnCommandUpdate += _camMonitor_OnCommandUpdate;

            // update drivers
            _slideActionEngine.UpdateDriver(_camMonitor);
            _dynamicActionEngine.UpdateDriver(_camMonitor);
        }

        private void Mock_OnCameraMoved(object sender, CameraUpdateEventArgs e)
        {
            var mock = switcherManager as MockBMDSwitcherManager;
            if (mock != null)
            {
                mock.UpdateMockCameraMovement(e);
            }
        }
        private void clickLoadDynamicButtons(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Dynamic Buttons";
            if (_pres != null)
            {
                ofd.InitialDirectory = _pres.Folder;
            }
            if (ofd.ShowDialog() == true)
            {
                var filetext = File.ReadAllText(ofd.FileName);
                LoadDynamicButtons(filetext, Path.GetDirectoryName(ofd.FileName), true);
            }
        }

        private void LoadDynamicButtons(string filetext, string resourcepath, bool overwriteAll)
        {
            try
            {
                _dynamicDriver?.ConfigureControls(filetext, resourcepath, overwriteAll, _condVarCalculator);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
        }

        private void LoadExtraDynamicButtons(string extraID, string filetext, string resourcepath, bool overwriteAll)
        {
            try
            {
                // TODO: somewhere we may need to eventually have this USE the extraID
                _spareDriver?.ConfigureControls(filetext, resourcepath, overwriteAll, _condVarCalculator);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
        }



        private void clickShowSparePanel(object sender, RoutedEventArgs e)
        {
            _spareDriver.ShowUI();
        }


        #region ActionAutomater Providers
        IBMDSwitcherManager ISwitcherDriverProvider.switcherManager { get => switcherManager; }
        BMDSwitcherState ISwitcherStateProvider.switcherState { get => switcherState; }
        BMDSwitcherConfigSettings IConfigProvider._config { get => _config; }
        bool IFeatureFlagProvider.AutomationTimer1Enabled { get => _FeatureFlag_automationtimer1enabled; }
        string IPresentationProvider.Folder { get => Presentation?.Folder; }

        Dictionary<string, ExposedVariable> IPresentationProvider.GetExposedVariables()
        {
            if (_pres != null)
            {
                return VariableAttributeFinderHelpers.FindPropertiesExposedAsVariables(_pres);
            }
            return new Dictionary<string, ExposedVariable>();
        }

        ISlide IPresentationProvider.GetCurentSlide()
        {
            return Presentation?.EffectiveCurrent;
        }

        void IPresentationProvider.SetNextSlideTarget(int target)
        {
            Presentation.SetNextSlideJump(target);
            slidesUpdated();
        }
        async Task IPresentationProvider.TakeNextSlide()
        {
            // think it's ok here to fire and forget??
            slidesUpdated();
            await SlideDriveVideo_Next();
            slidesUpdated();
        }
        void IAutoTransitionProvider.PerformGuardedAutoTransition()
        {
            PerformGuardedAutoTransition();
        }

        void IUserTimerProvider.ResetGpTimer1()
        {
            ResetGpTimer1();
        }
        void IMainUIProvider.Focus()
        {
            Dispatcher.Invoke(Focus);
        }
        void IAudioDriverProvider.OpenAudioPlayer()
        {
            Dispatcher.Invoke(OpenAudioPlayer);
        }
        void IAudioDriverProvider.RestartAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioPlayer?.RestartAudio();
            });
        }
        void IAudioDriverProvider.PauseAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioPlayer?.PauseAudio();
            });
        }
        void IAudioDriverProvider.StopAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioPlayer?.StopAudio();
            });
        }
        void IAudioDriverProvider.PlayAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioPlayer?.PlayAudio();
            });
        }
        void IAudioDriverProvider.OpenAudio(string filename)
        {
            Dispatcher.Invoke(() =>
            {
                audioPlayer?.OpenAudio(filename);
            });
        }
        void IMediaDriverProvider.unmuteMedia()
        {
            Dispatcher.Invoke(unmuteMedia);
        }
        void IMediaDriverProvider.muteMedia()
        {
            Dispatcher.Invoke(muteMedia);
        }
        void IMediaDriverProvider.restartMedia()
        {
            Dispatcher.Invoke(restartMedia);
        }
        void IMediaDriverProvider.stopMedia()
        {
            Dispatcher.Invoke(stopMedia);
        }
        void IMediaDriverProvider.pauseMedia()
        {
            Dispatcher.Invoke(pauseMedia);
        }
        void IMediaDriverProvider.playMedia()
        {
            Dispatcher.Invoke(playMedia);
        }


        void IDynamicControlProvider.ConfigureControls(string file, string resourcepath, bool overwriteAll)
        {
            // call assumes its running from presentation
            // so point it there

            string text = "";
            string path = resourcepath;
            if (resourcepath == "%pres%")
            {
                if (_pres != null)
                {
                    path = _pres?.Folder ?? resourcepath;
                    if (_pres.RawTextResources.TryGetValue(file, out var ftext))
                    {
                        text = ftext;
                    }
                }
            }

            if (text != string.Empty)
            {
                LoadDynamicButtons(text, path, overwriteAll);
            }
        }

        void IExtraDynamicControlProvider.ConfigureControls(string extraID, string file, string resourcepath, bool overwriteAll)
        {
            // call assumes its running from presentation
            // so point it there

            string text = "";
            string path = resourcepath;
            if (resourcepath == "%pres%")
            {
                if (_pres != null)
                {
                    path = _pres?.Folder ?? resourcepath;
                    if (_pres.RawTextResources.TryGetValue(file, out var ftext))
                    {
                        text = ftext;
                    }
                }
            }

            if (text != string.Empty)
            {
                LoadExtraDynamicButtons(extraID, text, path, overwriteAll);
            }

        }

        Dictionary<string, bool> IUserConditionProvider.GetActiveUserConditions()
        {
            var conditions = new Dictionary<string, bool>
            {
                ["1"] = _Cond1.Value,
                ["2"] = _Cond2.Value,
                ["3"] = _Cond3.Value,
                ["4"] = _Cond4.Value,
            };
            return conditions;
        }

        void IDynamicControlProvider.Repaint()
        {
            _dynamicDriver?.Repaint();
        }

        void IExtraDynamicControlProvider.Repaint()
        {
            _spareDriver?.Repaint();
        }

        #endregion

        AuxPlayer player = new AuxPlayer();
        private void ClickOpenAuxPlayer(object sender, RoutedEventArgs e)
        {
            player.Show();
        }
    }
}
