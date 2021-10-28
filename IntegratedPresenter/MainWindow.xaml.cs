using BMDSwitcherAPI;
using IntegratedPresenter.BMDSwitcher;
using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.ViewModels;
using Microsoft.Win32;
using SlideCreater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using log4net;
using IntegratedPresenter.BMDSwitcher.Mock;

namespace IntegratedPresenter.Main
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
        Timer gp_timer_2 = new Timer();

        TimeSpan timer1span = TimeSpan.Zero;
        TimeSpan timer2span = TimeSpan.Zero;

        TimeSpan timeonshot = TimeSpan.Zero;
        TimeSpan prewarnShottime = new TimeSpan(0, 0, 50);
        TimeSpan warnShottime = new TimeSpan(0, 1, 30);

        BMDSwitcherConfigSettings _config;


        List<SlidePoolSource> SlidePoolButtons;

        private BuildVersion VersionInfo;

        private readonly ILog _logger = LogManager.GetLogger("UserLogger");

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            // load build/version info
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Integrated_Presenter.version.json");
            using (StreamReader sr = new StreamReader(stream))
            {
                string json = sr.ReadToEnd();
                VersionInfo = JsonSerializer.Deserialize<BuildVersion>(json);
            }
            // Set title
            Title = $"Integrated Presenter - {VersionInfo.MajorVersion}.{VersionInfo.MinorVersion}.{VersionInfo.Revision}.{VersionInfo.Build}-{VersionInfo.Mode}";


            // set size
            Width = 1200;
            Height = 500;

            // enable logging
            using (Stream cstream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Integrated_Presenter.Log4Net.config"))
            {
                log4net.Config.XmlConfigurator.Configure(cstream);
            }

            _logger.Info($"{Title} Started.");


            // set a default config
            SetDefaultConfig();
            LoadUserSettings(Configurations.FeatureConfig.IntegratedPresenterFeatures.Default());

            HideAdvancedPresControls();
            HideAdvancedPIPControls();
            HideAuxButtonConrols();

            SlidePoolButtons = new List<SlidePoolSource>() { SlidePoolSource0, SlidePoolSource1, SlidePoolSource2, SlidePoolSource3 };

            ShowHideShortcutsUI();

            // start with no switcher connection so disable all controls correctly
            DisableSwitcherControls();

            UpdateRealTimeClock();
            UpdateSlideControls();
            UpdateMediaControls();
            UpdateSlideModeButtons();
            DisableAuxControls();
            UpdateProgramRowLockButtonUI();

            this.PresentationStateUpdated += MainWindow_PresentationStateUpdated;
            NextPreview.AutoSilentPlayback = true;
            AfterPreview.AutoSilentPlayback = true;
            PrevPreview.AutoSilentPlayback = true;
            NextPreview.AutoSilentReplay = true;
            AfterPreview.AutoSilentReplay = true;
            PrevPreview.AutoSilentReplay = true;

            NextPreview.ShowBlackForActions = false;
            AfterPreview.ShowBlackForActions = false;
            PrevPreview.ShowBlackForActions = false;
            CurrentPreview.ShowBlackForActions = false;
            CurrentPreview.ShowIfMute = true;

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
                    TbShotClock.Foreground = Brushes.Red;
                }
                else if (timeonshot > prewarnShottime)
                {
                    TbShotClock.Foreground = Brushes.Yellow;
                }
                else
                {
                    TbShotClock.Foreground = Brushes.Orange;
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

        private void MainWindow_PresentationStateUpdated(Slide currentslide)
        {
            UpdateSlideNums();
            UpdateSlidePreviewControls();
            ResetSlideMediaTimes();
            UpdateSlideControls();
            UpdateMediaControls();
        }

        IBMDSwitcherManager switcherManager;
        BMDSwitcherState switcherState = new BMDSwitcherState();


        #region BMD Switcher

        private void SwitcherConnectedUiUpdate(bool connected, bool searching = false)
        {
            if (connected)
            {
                tb_switcherConnection.Text = "Connected";
                tb_switcherConnection.Foreground = Brushes.LimeGreen;
            }
            else if (searching)
            {
                tb_switcherConnection.Text = "Searching...";
                tb_switcherConnection.Foreground = Brushes.Yellow;
            }
            else
            {
                tb_switcherConnection.Text = "Disconnected";
                tb_switcherConnection.Foreground = Brushes.Red;
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
                switcherManager = new BMDSwitcherManager(this);
                switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
                switcherManager.OnSwitcherDisconnected += SwitcherManager_OnSwitcherDisconnected;
                SwitcherConnectedUiUpdate(false, true);

                await Task.Delay(100).ConfigureAwait(true);

                // give UI update time
                if (switcherManager.TryConnect(connectWindow.IP))
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
                    _logger.Warn("ConnectSwitcher() -- failed to connect.");
                    SwitcherConnectedUiUpdate(false);
                    switcherManager.SwitcherStateChanged -= SwitcherManager_SwitcherStateChanged;
                    switcherManager.OnSwitcherDisconnected -= SwitcherManager_OnSwitcherDisconnected;
                    switcherManager = null;
                }
            }
        }

        private void SwitcherManager_OnSwitcherDisconnected()
        {
            _logger.Warn("SwitcherManager_OnSwitcherDisconnected() event handled.");
            DisableSwitcherControls();
            // Important part -> set switcherManager to null so we don't try and access it when its disconnected
            switcherManager = null;
            SwitcherConnectedUiUpdate(false);
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
            switcherManager.SwitcherStateChanged += SwitcherManager_SwitcherStateChanged;
            switcherManager.OnSwitcherDisconnected += SwitcherManager_OnSwitcherDisconnected;
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
            BtnTransKey1.Style = (Style)Application.Current.FindResource(style);

            EnableKeyerControls();
            EnableAuxButtons();
            ShowKeyerUI();

        }


        private void ShowKeyerUI()
        {
            if (switcherState?.USK1KeyType == 1)
            {
                BtnDVE.Foreground = Brushes.Orange;
                BtnChroma.Foreground = Brushes.White;
                ChromaControls.Visibility = Visibility.Hidden;
                PIPControls.Visibility = Visibility.Visible;
            }
            if (switcherState?.USK1KeyType == 2)
            {
                BtnDVE.Foreground = Brushes.White;
                BtnChroma.Foreground = Brushes.Orange;
                PIPControls.Visibility = Visibility.Hidden;
                ChromaControls.Visibility = Visibility.Visible;
            }
            BtnDVE.Background = Brushes.Red;
            BtnChroma.Background = Brushes.Red;
            BtnDVE.Cursor = Cursors.Hand;
            BtnChroma.Cursor = Cursors.Hand;
        }

        private void DisableKeyerUI()
        {
            BtnDVE.Foreground = Brushes.WhiteSmoke;
            BtnChroma.Foreground = Brushes.WhiteSmoke;
            BtnDVE.Background = Brushes.WhiteSmoke;
            BtnChroma.Background = Brushes.WhiteSmoke;
            BtnDVE.Cursor = Cursors.Arrow;
            BtnChroma.Cursor = Cursors.Arrow;
            PIPControls.Visibility = Visibility.Hidden;
            ChromaControls.Visibility = Visibility.Hidden;
        }

        private void EnableKeyerControls()
        {

            string style = "SwitcherButton";


            BtnUSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);

            BtnPIPFillProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram8.Style = (Style)Application.Current.FindResource(style);

            BtnChromaFillProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram8.Style = (Style)Application.Current.FindResource(style);

            string pipstyle = "PIPControlButton";
            BtnPIPtoA.Style = (Style)Application.Current.FindResource(pipstyle);
            BtnPIPtoB.Style = (Style)Application.Current.FindResource(pipstyle);
            BtnPIPtoFull.Style = (Style)Application.Current.FindResource(pipstyle);

        }

        private void DisableKeyerControls()
        {

            string style = "SwitcherButton_Disabled";

            BtnUSK1OnOffAir.Style = (Style)Application.Current.FindResource(style);

            BtnPIPFillProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram8.Style = (Style)Application.Current.FindResource(style);

            BtnChromaFillProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnChromaFillProgram8.Style = (Style)Application.Current.FindResource(style);

            style = "OffLight";
            BtnUSK1OnOffAir.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnPIPFillProgram1.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram2.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram3.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram4.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram5.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram6.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram7.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnPIPFillProgram8.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            BtnChromaFillProgram1.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnChromaFillProgram2.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnChromaFillProgram3.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnChromaFillProgram4.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnChromaFillProgram5.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnChromaFillProgram6.Background = (RadialGradientBrush)Application.Current.FindResource(style);
            BtnChromaFillProgram7.Background = (RadialGradientBrush)Application.Current.FindResource(style);

            string pipstyle = "InactivePIPControlButton";
            BtnPIPtoA.Style = (Style)Application.Current.FindResource(pipstyle);
            BtnPIPtoB.Style = (Style)Application.Current.FindResource(pipstyle);
            BtnPIPtoFull.Style = (Style)Application.Current.FindResource(pipstyle);

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
            BtnTransKey1.Style = (Style)Application.Current.FindResource(style);

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
            BtnTransKey1.Background = (RadialGradientBrush)Application.Current.FindResource(style);

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

            BtnPIPtoA.Background = switcherState.USK1KeyFrame == 1 ? Brushes.Orange : Brushes.WhiteSmoke;
            BtnPIPtoA.Foreground = switcherState.USK1KeyFrame == 1 ? Brushes.Orange : Brushes.WhiteSmoke;
            BtnPIPtoB.Background = switcherState.USK1KeyFrame == 2 ? Brushes.Orange : Brushes.WhiteSmoke;
            BtnPIPtoB.Foreground = switcherState.USK1KeyFrame == 2 ? Brushes.Orange : Brushes.WhiteSmoke;
            BtnPIPtoFull.Background = switcherState.USK1KeyFrame == 0 ? Brushes.Orange : Brushes.WhiteSmoke;
            BtnPIPtoFull.Foreground = switcherState.USK1KeyFrame == 0 ? Brushes.Orange : Brushes.WhiteSmoke;
        }

        private void UpdateChromaButtonStyles()
        {
            BtnChromaFillProgram1.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram2.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram3.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram4.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram5.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram6.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram7.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram8.Background = (ConvertSourceIDToButton(switcherState.USK1FillSource) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        private void UpdateKeyerControls()
        {
            ShowKeyerUI();
            UpdatePIPButtonStyles();
            UpdateChromaButtonStyles();
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
            //BtnDrive.Background = (SlideDriveVideo ? Application.Current.FindResource("YellowLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            switch (CurrentSlideMode)
            {
                case 0:
                    BtnNoDrive.Foreground = Brushes.Orange;
                    BtnDrive.Foreground = Brushes.White;
                    BtnJump.Foreground = Brushes.White;
                    break;
                case 1:
                    BtnNoDrive.Foreground = Brushes.White;
                    BtnDrive.Foreground = Brushes.Orange;
                    BtnJump.Foreground = Brushes.White;
                    break;
                case 2:
                    BtnNoDrive.Foreground = Brushes.White;
                    BtnDrive.Foreground = Brushes.White;
                    BtnJump.Foreground = Brushes.Orange;
                    break;
            }
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
                        if (Presentation?.EffectiveCurrent.Type == SlideType.Video || Presentation?.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                        {

                            tbPreviewCurrentVideoDuration.Text = CurrentPreview.MediaTimeRemaining.ToString("\\T\\-mm\\:ss");
                            tbPreviewCurrentVideoDuration.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (Presentation?.Current.Type == SlideType.Video || Presentation?.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
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
                        if (Presentation?.EffectiveCurrent.Type == SlideType.Video || Presentation?.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
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
                        if (Presentation?.Current.Type == SlideType.Video || Presentation?.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
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
                if (Presentation?.Next.Type == SlideType.Video || Presentation?.Next.Type == SlideType.ChromaKeyVideo)
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
                if (Presentation?.After.Type == SlideType.Video || Presentation?.After.Type == SlideType.ChromaKeyVideo)
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
                if (Presentation?.Prev.Type == SlideType.Video || Presentation?.Prev.Type == SlideType.ChromaKeyVideo)
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


        private async void WindowKeyDown(object sender, KeyEventArgs e)
        {

            // dont enable shortcuts when focused on textbox
            if (tbChromaHue.IsFocused || tbChromaGain.IsFocused || tbChromaYSuppress.IsFocused || tbChromaLift.IsFocused || tbChromaNarrow.IsFocused)
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

            if (e.Key == Key.P)
            {
                ToggleViewAdvancedPIP();
            }
            if (e.Key == Key.L)
            {
                ShowPIPLocationControl();
            }
            if (e.Key == Key.O)
            {
                ToggleViewPrevAfter();
            }
            if (e.Key == Key.Q)
            {
                ToggleViewAdvancedPresentation();
            }
            if (e.Key == Key.X)
            {
                ToggleAuxRow();
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
                else if (Keyboard.IsKeyDown(Key.E))
                    TakeSlidePoolSlide(SlidePoolSource0.Slide, 0, false, SlidePoolSource0.Driven);
                else if (Keyboard.IsKeyDown(Key.R))
                    TakeSlidePoolSlide(SlidePoolSource0.Slide, 0, true, SlidePoolSource0.Driven);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(1);
                else
                    ClickPreset(1);
            }
            if (e.Key == Key.D2)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(2);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(2);
                else if (Keyboard.IsKeyDown(Key.E))
                    TakeSlidePoolSlide(SlidePoolSource1.Slide, 1, false, SlidePoolSource1.Driven);
                else if (Keyboard.IsKeyDown(Key.R))
                    TakeSlidePoolSlide(SlidePoolSource1.Slide, 1, true, SlidePoolSource1.Driven);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(2);
                else
                    ClickPreset(2);
            }
            if (e.Key == Key.D3)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(3);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(3);
                else if (Keyboard.IsKeyDown(Key.E))
                    TakeSlidePoolSlide(SlidePoolSource2.Slide, 2, false, SlidePoolSource2.Driven);
                else if (Keyboard.IsKeyDown(Key.R))
                    TakeSlidePoolSlide(SlidePoolSource2.Slide, 2, true, SlidePoolSource2.Driven);
                else if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(3);
                else
                    ClickPreset(3);
            }
            if (e.Key == Key.D4)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    ClickProgram(4);
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    ChangeUSK1FillSource(4);
                else if (Keyboard.IsKeyDown(Key.E))
                    TakeSlidePoolSlide(SlidePoolSource3.Slide, 3, false, SlidePoolSource3.Driven);
                else if (Keyboard.IsKeyDown(Key.R))
                    TakeSlidePoolSlide(SlidePoolSource3.Slide, 3, true, SlidePoolSource3.Driven);
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

            if (e.Key == Key.NumPad1)
            {
                ToggleUSK1Type();
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
            if (e.Key == Key.C)
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
            }

            if (e.Key == Key.Enter)
            {
                // TODO:: Guard with global take transition guard
                _logger.Debug($"USER INPUT [Keyboard] -- ({e.Key}) AutoTransition()");
                TakeAutoTransition();
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

        private bool _FeatureFlag_PresetShot = false;
        private bool _FeatureFlag_PostsetShot = true;

        private bool SetupActionsCompleted = false;
        private bool ActionsCompleted = false;

        private Guid currentslideforactions;

        private async Task ExecuteSetupActions(Slide s)
        {
            _logger.Debug($"Begin Execution of setup actions for slide {s.Title}");
            Dispatcher.Invoke(() =>
            {
                SetupActionsCompleted = false;
                CurrentPreview.SetupComplete(false);
            });
            await Task.Run(async () =>
            {
                foreach (var task in s.SetupActions)
                {
                    await PerformAutomationAction(task);
                }
            });
            Dispatcher.Invoke(() =>
            {
                SetupActionsCompleted = true;
                CurrentPreview.SetupComplete(true);
            });
            _logger.Debug($"Completed Execution of setup actions for slide {s.Title}");
        }

        private async Task ExecuteActionSlide(Slide s)
        {
            _logger.Debug($"Begin Execution of actions for slide {s.Title}");
            Dispatcher.Invoke(() =>
            {
                ActionsCompleted = false;
                CurrentPreview.ActionComplete(false);
            });
            await Task.Run(async () =>
            {
                foreach (var task in s.Actions)
                {
                    await PerformAutomationAction(task);
                }
            });
            Dispatcher.Invoke(() =>
            {
                ActionsCompleted = true;
                CurrentPreview.ActionComplete(true);
            });
            _logger.Debug($"Completed Execution of actions for slide {s.Title}");
        }

        private async Task PerformAutomationAction(AutomationAction task)
        {
            await Task.Run(async () =>
            {
                switch (task.Action)
                {
                    case AutomationActionType.PresetSelect:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Preset Select {task.DataI}");
                            switcherManager?.PerformPresetSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.ProgramSelect:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Program Select {task.DataI}");
                            switcherManager?.PerformProgramSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.AuxSelect:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Aux Select {task.DataI}");
                            switcherManager?.PerformAuxSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.AutoTrans:
                        Dispatcher.Invoke(() =>
                        {
                            //switcherManager?.PerformAutoTransition();
                            _logger.Debug($"(PerformAutomationAction) -- AutoTrans (gaurded)");
                            PerformGuardedAutoTransition();
                        });
                        break;
                    case AutomationActionType.CutTrans:
                        Dispatcher.Invoke(() =>
                        {
                            // Will always allow automation to perform cut transition.
                            // Gaurded Cut transition is only for debouncing/preventing operators using a keyboard from spamming cut requests.
                            _logger.Debug($"(PerformAutomationAction) -- Cut (unguarded)");
                            switcherManager?.PerformCutTransition();
                        });
                        break;
                    case AutomationActionType.AutoTakePresetIfOnSlide:
                        // Take Preset if program source is fed from slides
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                //switcherManager?.PerformAutoTransition();
                                _logger.Debug($"(PerformAutomationAction) -- AutoTrans (guarded), requred since 'slide' source was on air : AutoTakePresetIfOnSlide");
                                PerformGuardedAutoTransition();
                            });
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        break;
                    case AutomationActionType.DSK1On:
                        if (!switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- Toggle DSK1 since current state is OFF and requested state is ON");
                                switcherManager?.PerformToggleDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1Off:
                        if (switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- Toggle DSK1 since current state is ON and requested state is OFF");
                                switcherManager?.PerformToggleDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1FadeOn:
                        if (!switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- FADE ON DSK1 since current state is OFF and requested state is ON");
                                switcherManager?.PerformAutoOnAirDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1FadeOff:
                        if (switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- FADE OFF DSK1 since current state is ON and requested state is OFF");
                                switcherManager?.PerformAutoOffAirDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2On:
                        if (!switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- Toggle DSK2 since current state is OFF and requested state is ON");
                                switcherManager?.PerformToggleDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2Off:
                        if (switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- Toggle DSK2 since current state is ON and requested state is OFF");
                                switcherManager?.PerformToggleDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2FadeOn:
                        if (!switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- FADE ON DSK2 since current state is OFF and requested state is ON");
                                switcherManager?.PerformAutoOnAirDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2FadeOff:
                        if (switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- FADE OFF DSK2 since current state is ON and requested state is OFF");
                                switcherManager?.PerformAutoOffAirDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.RecordStart:
                        break;
                    case AutomationActionType.RecordStop:
                        break;

                    case AutomationActionType.Timer1Restart:
                        if (_FeatureFlag_automationtimer1enabled)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- Reset gp timer 1");
                                ResetGpTimer1();
                            });
                        }
                        break;

                    case AutomationActionType.USK1On:
                        if (!switcherState.USK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- USK1 ON air since current state is OFF and requsted state is ON");
                                switcherManager?.PerformOnAirUSK1();
                            });
                        }
                        break;
                    case AutomationActionType.USK1Off:
                        if (switcherState.USK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                _logger.Debug($"(PerformAutomationAction) -- USK1 OFF air since current state is ON and requsted state is OFF");
                                switcherManager?.PerformOffAirUSK1();
                            });
                        }
                        break;
                    case AutomationActionType.USK1SetTypeChroma:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Configure USK1 for type chroma");
                            switcherManager?.SetUSK1TypeChroma();
                        });
                        break;
                    case AutomationActionType.USK1SetTypeDVE:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Configure USK1 for type DVE PIP");
                            switcherManager?.SetUSK1TypeDVE();
                        });
                        break;

                    case AutomationActionType.OpenAudioPlayer:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Opened Audio Player");
                            OpenAudioPlayer();
                            Focus();
                        });
                        break;
                    case AutomationActionType.LoadAudio:
                        string filename = Path.Join(Presentation.Folder, task.DataS);
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Load Audio File {filename} to Aux Player");
                            audioPlayer.OpenAudio(filename);
                        });
                        break;
                    case AutomationActionType.PlayAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Aux:PlayAudio()");
                            audioPlayer.PlayAudio();
                        });
                        break;
                    case AutomationActionType.StopAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Aux:StopAudio()");
                            audioPlayer.StopAudio();
                        });
                        break;
                    case AutomationActionType.PauseAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Aux:PauseAudio()");
                            audioPlayer.PauseAudio();
                        });
                        break;
                    case AutomationActionType.ReplayAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Aux:RestartAudio()");
                            audioPlayer.RestartAudio();
                        });
                        break;

                    case AutomationActionType.PlayMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- playMedia()");
                            playMedia();
                        });
                        break;
                    case AutomationActionType.PauseMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- pauseMedia()");
                            pauseMedia();
                        });
                        break;
                    case AutomationActionType.StopMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- stopMedia()");
                            stopMedia();
                        });
                        break;
                    case AutomationActionType.RestartMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- restartMedia()");
                            restartMedia();
                        });
                        break;
                    case AutomationActionType.MuteMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- muteMedia()");
                            muteMedia();
                        });
                        break;
                    case AutomationActionType.UnMuteMedia:
                        Dispatcher.Invoke(() =>
                        {
                            _logger.Debug($"(PerformAutomationAction) -- unmuteMedia()");
                            unmuteMedia();
                        });
                        break;


                    case AutomationActionType.DelayMs:
                        _logger.Debug($"(PerformAutomationAction) -- delay for {task.DataI} ms");
                        await Task.Delay(task.DataI);
                        break;
                    case AutomationActionType.None:
                        break;
                    default:
                        _logger.Debug($"(PerformAutomationAction) -- UNKNOWN ACTION {task.Action}");
                        break;
                }
            });
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
                        // doesn't make sense to put preset shot on actions slides. Write it into the script as a @arg1:PresetSelect(#) insead
                        SetupActionsCompleted = false;
                        ActionsCompleted = false;
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
                        // again doesn't make sense to have postset shot. Write it into the script as a @arg:PresetSelect(#) instead
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
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
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
                                await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
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
                                await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                            }
                            _logger.Debug($"SlideDriveVideo_Next(Tied={Tied}) --  Command Preset Select for postset {Presentation.EffectiveCurrent.PostsetId} on slide ({Presentation.CurrentSlide}) of type {Presentation.Current.Type}.");
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
            }

        }

        private void SlideDriveVideo_Action(Slide s)
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
                DisableSlidePoolOverrides();
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
            }

        }

        private void PerformGuardedAutoTransition(bool force_guard = false)
        {
            if (_FeatureFlag_DriveMode_AutoTransitionGuard || force_guard)
            {
                // if guarded, check if transition is already in progress
                if (!switcherState.InTransition)
                {
                    switcherManager?.PerformAutoTransition();
                }
            }
            else
            {
                switcherManager?.PerformAutoTransition();
            }
        }


        #endregion


        bool activepresentation = false;
        Presentation _pres;
        public Presentation Presentation { get => _pres; }

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
                    SetSwitcherSettings();
                }
                if (pres.HasUserConfig)
                {
                    _logger.Info($"Presentation loading specified setting user settings. Re-configuring now.");
                    LoadUserSettings(pres.UserConfig);
                }

                // overwrite display of old presentation if already open
                if (_display != null && _display.IsWindowVisilbe)
                {
                }
                else
                {
                    _logger.Info($"Graphics Engine has no active display window. Creating window now.");
                    _display = new PresenterDisplay(this, false);
                    _display.OnMediaPlaybackTimeUpdated += _display_OnMediaPlaybackTimeUpdated;
                    // start display
                    _display.Show();
                }

                if (_keydisplay == null && !(_keydisplay?.IsWindowVisilbe ?? false))
                {
                    _logger.Info($"Graphics Engine has no active key window. Creating window now.");
                    _keydisplay = new PresenterDisplay(this, true);
                    // no need to get playback event info
                    _keydisplay.Show();
                }

                DisableSlidePoolOverrides();
                slidesUpdated();

                PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
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
            _display?.ShowSlide(false);
            _keydisplay?.ShowSlide(true);

            // mark update for slides
            if (currentslideforactions != currentGuid)
            {
                // new slide - mark changes
                SetupActionsCompleted = false;
                ActionsCompleted = false;
            }

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
                NextPreview.SetupComplete(SetupActionsCompleted);
                NextPreview.ActionComplete(false);
                CurrentPreview.ActionComplete(ActionsCompleted);
                CurrentPreview.SetupComplete(true);
                NextPreview.SetMedia(Presentation.Next, false);
                AfterPreview.SetMedia(Presentation.After, false);
            }
            UpdateSlidePreviewControls();
            UpdatePreviewsPostets();
            currentslideforactions = currentGuid;
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

        private void UpdatePostsetUi(MediaPlayer2 preview, Slide slide)
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
        private void UIUpdateDisplayPrevAfter()
        {
            if (_FeatureFlag_displayPrevAfter)
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
                DisableSlidePoolOverrides();
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
                    DisableSlidePoolOverrides();
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

        private void ClickViewPrevAfter(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleViewPrevAfter();
        }

        private void ToggleViewPrevAfter()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            SetViewPrevAfter(!_FeatureFlag_displayPrevAfter);
        }

        private void SetViewPrevAfter(bool show)
        {
            _FeatureFlag_displayPrevAfter = show;
            cbPrevAfter.IsChecked = _FeatureFlag_displayPrevAfter;
            UIUpdateDisplayPrevAfter();
        }

        private async void ClickTakeSlide(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            await SlideDriveVideo_Current();
        }


        bool _FeatureFlag_viewAdvancedPresentation = false;
        private void ClickViewAdvancedPresentation(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleViewAdvancedPresentation();
        }

        private void ToggleViewAdvancedPresentation()
        {
            SetViewAdvancedPresentation(!_FeatureFlag_viewAdvancedPresentation);
        }

        private void SetViewAdvancedPresentation(bool show)
        {
            _FeatureFlag_viewAdvancedPresentation = show;

            if (_FeatureFlag_viewAdvancedPresentation)
                ShowAdvancedPresControls();
            else
                HideAdvancedPresControls();

            cbAdvancedPresentation.IsChecked = _FeatureFlag_viewAdvancedPresentation;
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

        private async void TakeSlidePoolSlide(Slide s, int num, bool replaceMode, bool driven)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()} slide: {s.Title} from pool source {num} {(replaceMode ? "in replace mode" : "in insert mode")} and {(driven ? "" : "not")} driven");
            for (int i = 0; i < 4; i++)
            {
                if (num != i)
                {
                    SlidePoolButtons[i].Selected = false;
                }
            }

            SlidePoolButtons[num].Selected = true;

            currentpoolsource = SlidePoolButtons[num];

            if (replaceMode)
            {
                Presentation?.NextSlide();
            }

            if (driven)
            {
                await SlideDriveVideo_ToSlide(s);
            }
            else
            {
                SlideUndrive_ToSlide(s);
            }


        }

        private void DisableSlidePoolOverrides()
        {
            for (int i = 0; i < 4; i++)
            {
                SlidePoolButtons[i].Selected = false;
            }
            currentpoolsource?.StopMedia();
            UpdateMediaControls();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // stop timers
            gp_timer_1?.Stop();
            gp_timer_2?.Stop();
            shot_clock_timer?.Stop();
            _display?.Close();
            _keydisplay?.Close();
            switcherManager?.Close();
            audioPlayer?.Close();
            pipctrl?.Close();
            _logger.Info("Integrated Presenter requested to close by USER");
        }

        private void ClickTakeSP0(object sender, Slide s, bool replaceMode, bool driven)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TakeSlidePoolSlide(s, 0, replaceMode, driven);
        }

        private void ClickTakeSP1(object sender, Slide s, bool replaceMode, bool driven)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TakeSlidePoolSlide(s, 1, replaceMode, driven);
        }

        private void ClickTakeSP2(object sender, Slide s, bool replaceMode, bool driven)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TakeSlidePoolSlide(s, 2, replaceMode, driven);
        }

        private void ClickTakeSP3(object sender, Slide s, bool replaceMode, bool driven)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            TakeSlidePoolSlide(s, 3, replaceMode, driven);
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


        private bool _FeatureFlag_showadvancedpipcontrols = false;
        private void ClickViewAdvancedPIP(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleViewAdvancedPIP();
        }

        private void ToggleViewAdvancedPIP()
        {
            SetViewAdvancedPIP(!_FeatureFlag_showadvancedpipcontrols);
        }

        private void SetViewAdvancedPIP(bool show)
        {
            _FeatureFlag_showadvancedpipcontrols = show;
            if (_FeatureFlag_showadvancedpipcontrols)
            {
                ShowAdvancedPIPControls();
            }
            else
            {
                HideAdvancedPIPControls();
            }
            cbAdvancedPresentation.IsChecked = _FeatureFlag_showadvancedpipcontrols;
        }

        private void ShowAdvancedPIPControls()
        {
            grAdvancedPIP.Height = new GridLength(1, GridUnitType.Star);
        }

        private void HideAdvancedPIPControls()
        {
            //var heightreduction = grAdvancedPIP.ActualHeight;
            grAdvancedPIP.Height = new GridLength(0);
            //Height -= heightreduction;
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

        private void SetSwitcherSettings()
        {
            // update UI
            UpdateUIButtonLabels();

            // Update previews
            PrevPreview.SetMVConfigForPostset(_config.MultiviewerConfig);
            CurrentPreview.SetMVConfigForPostset(_config.MultiviewerConfig);
            NextPreview.SetMVConfigForPostset(_config.MultiviewerConfig);
            AfterPreview.SetMVConfigForPostset(_config.MultiviewerConfig);

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
            BtnPIPFillProgram1.Content = config.ButtonName;
            BtnChromaFillProgram1.Content = config.ButtonName;
            BtnAux1.Content = config.ButtonName;
        }

        private void UpdateButton2Labels(ButtonSourceMapping config)
        {
            BtnPreset2.Content = config.ButtonName;
            BtnProgram2.Content = config.ButtonName;
            BtnPIPFillProgram2.Content = config.ButtonName;
            BtnChromaFillProgram2.Content = config.ButtonName;
            BtnAux2.Content = config.ButtonName;
        }
        private void UpdateButton3Labels(ButtonSourceMapping config)
        {
            BtnPreset3.Content = config.ButtonName;
            BtnProgram3.Content = config.ButtonName;
            BtnPIPFillProgram3.Content = config.ButtonName;
            BtnChromaFillProgram3.Content = config.ButtonName;
            BtnAux3.Content = config.ButtonName;
        }
        private void UpdateButton4Labels(ButtonSourceMapping config)
        {
            BtnPreset4.Content = config.ButtonName;
            BtnProgram4.Content = config.ButtonName;
            BtnPIPFillProgram4.Content = config.ButtonName;
            BtnChromaFillProgram4.Content = config.ButtonName;
            BtnAux4.Content = config.ButtonName;
        }

        private void UpdateButton5Labels(ButtonSourceMapping config)
        {
            BtnPreset5.Content = config.ButtonName;
            BtnProgram5.Content = config.ButtonName;
            BtnPIPFillProgram5.Content = config.ButtonName;
            BtnChromaFillProgram5.Content = config.ButtonName;
            BtnAux5.Content = config.ButtonName;
        }
        private void UpdateButton6Labels(ButtonSourceMapping config)
        {
            BtnPreset6.Content = config.ButtonName;
            BtnProgram6.Content = config.ButtonName;
            BtnPIPFillProgram6.Content = config.ButtonName;
            BtnChromaFillProgram6.Content = config.ButtonName;
            BtnAux6.Content = config.ButtonName;
        }
        private void UpdateButton7Labels(ButtonSourceMapping config)
        {
            BtnPreset7.Content = config.ButtonName;
            BtnProgram7.Content = config.ButtonName;
            BtnPIPFillProgram7.Content = config.ButtonName;
            BtnChromaFillProgram7.Content = config.ButtonName;
            BtnAux7.Content = config.ButtonName;
        }
        private void UpdateButton8Labels(ButtonSourceMapping config)
        {
            BtnPreset8.Content = config.ButtonName;
            BtnProgram8.Content = config.ButtonName;
            BtnPIPFillProgram8.Content = config.ButtonName;
            BtnChromaFillProgram8.Content = config.ButtonName;
            BtnAux8.Content = config.ButtonName;
        }


        private void SetDefaultConfig()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            _config = Configurations.SwitcherConfig.DefaultConfig.GetDefaultConfig();
        }

        public BMDSwitcherConfigSettings Config { get => _config; }

        bool _FeatureFlag_showAuxButons = false;
        private void ClickViewAuxOutput(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            ToggleAuxRow();
        }

        private void ToggleAuxRow()
        {
            SetViewAuxRow(!_FeatureFlag_showAuxButons);
        }

        private void SetViewAuxRow(bool show)
        {
            _FeatureFlag_showAuxButons = show;
            if (_FeatureFlag_showAuxButons)
            {
                ShowAuxButtonControls();
            }
            else
            {
                HideAuxButtonConrols();
            }
        }

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

        private void ShowAuxButtonControls()
        {
            _FeatureFlag_showAuxButons = true;
            gridbtns.Width = 770;
            gcAdvancedProjector.Width = new GridLength(1.2, GridUnitType.Star);
        }

        private void HideAuxButtonConrols()
        {
            _FeatureFlag_showAuxButons = false;
            gridbtns.Width = 660;
            gcAdvancedProjector.Width = new GridLength(0);
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


        private void TextEntryMode(object sender, DependencyPropertyChangedEventArgs e)
        {
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
                btnProgramLock.Content = "LOCKED";
                btnProgramLock.Foreground = Brushes.WhiteSmoke;
            }
            else
            {
                btnProgramLock.Content = "UNLOCKED";
                btnProgramLock.Foreground = Brushes.Orange;
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

        private void SetSwitcherKeyerChroma()
        {
            //switcherManager?.ConfigureUSK1Chroma(_config.USKSettings.ChromaSettings);
            _logger.Debug("Commanding Switcher to reconfigure USK1 for chroma key.");
            switcherManager?.SetUSK1TypeChroma();
            // force blocking state update
            if (switcherManager != null)
            {
                SwitcherManager_SwitcherStateChanged(switcherManager?.ForceStateUpdate() ?? new BMDSwitcherState());
                tbChromaHue.Text = switcherState.ChromaSettings.Hue.ToString();
                tbChromaGain.Text = switcherState.ChromaSettings.Gain.ToString();
                tbChromaLift.Text = switcherState.ChromaSettings.Lift.ToString();
                tbChromaYSuppress.Text = switcherState.ChromaSettings.YSuppress.ToString();
                tbChromaNarrow.Text = switcherState.ChromaSettings.Narrow.ToString();
                ShowKeyerUI();
            }
        }

        private void ClickApplyChromaSettings(object sender, RoutedEventArgs e)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            double hue = 0;
            double gain = 0;
            double lift = 0;
            double ysuppress = 0;
            int narrow = 0;
            double.TryParse(tbChromaHue.Text, out hue);
            double.TryParse(tbChromaGain.Text, out gain);
            double.TryParse(tbChromaLift.Text, out lift);
            double.TryParse(tbChromaYSuppress.Text, out ysuppress);
            int.TryParse(tbChromaNarrow.Text, out narrow);

            BMDUSKChromaSettings chromaSettings = new BMDUSKChromaSettings()
            {
                FillSource = (int)((switcherState?.USK1FillSource) ?? _config.USKSettings.ChromaSettings.FillSource),
                Hue = hue,
                Gain = gain,
                Lift = lift,
                YSuppress = ysuppress,
                Narrow = narrow
            };

            switcherManager?.ConfigureUSK1Chroma(chromaSettings);
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
        private void ClickViewAdvancedPIPLocation(object sender, RoutedEventArgs e)
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
            // Update Slide Pools
            SlidePoolSource0.ShowHideShortcuts(ShowShortcuts);
            SlidePoolSource1.ShowHideShortcuts(ShowShortcuts);
            SlidePoolSource2.ShowHideShortcuts(ShowShortcuts);
            SlidePoolSource3.ShowHideShortcuts(ShowShortcuts);

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

            ksc_cf1.Visibility = ShortcutVisibility;
            ksc_cf2.Visibility = ShortcutVisibility;
            ksc_cf3.Visibility = ShortcutVisibility;
            ksc_cf4.Visibility = ShortcutVisibility;
            ksc_cf5.Visibility = ShortcutVisibility;
            ksc_cf6.Visibility = ShortcutVisibility;
            ksc_cf7.Visibility = ShortcutVisibility;
            ksc_cf8.Visibility = ShortcutVisibility;

            ksc_pf1.Visibility = ShortcutVisibility;
            ksc_pf2.Visibility = ShortcutVisibility;
            ksc_pf3.Visibility = ShortcutVisibility;
            ksc_pf4.Visibility = ShortcutVisibility;
            ksc_pf5.Visibility = ShortcutVisibility;
            ksc_pf6.Visibility = ShortcutVisibility;
            ksc_pf7.Visibility = ShortcutVisibility;
            ksc_pf8.Visibility = ShortcutVisibility;
            ksc_pkfa.Visibility = ShortcutVisibility;
            ksc_pkfb.Visibility = ShortcutVisibility;
            ksc_pkff.Visibility = ShortcutVisibility;
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

            ksc_r.Visibility = ShortcutVisibility;

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
        }

        private void ResetPresentationToBegining()
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            Presentation.StartPres();
            slidesUpdated();
            PresentationStateUpdated?.Invoke(Presentation.Current);
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
            SetViewPrevAfter(config.ViewSettings.View_PrevAfterPreviews);
            SetShowEffectiveCurrentPreview(config.ViewSettings.View_PreviewEffectiveCurrent);
            SetViewAdvancedPIP(config.ViewSettings.View_AdvancedDVE);
            SetViewAuxRow(config.ViewSettings.View_AuxOutput);
            SetViewAdvancedPresentation(config.ViewSettings.View_AdvancedPresentation);
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

            if (config.PresentationSettings.StartPresentationMuted)
            {
                muteMedia();
            }
            else
            {
                unmuteMedia();
            }

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
                },
                PresentationSettings = new Configurations.FeatureConfig.IntegratedPresentationFeatures_Presentation()
                {
                    StartPresentationMuted = MediaMuted,
                },
                ViewSettings = new Configurations.FeatureConfig.IntegratedPresentationFeatures_View()
                {
                    View_PrevAfterPreviews = _FeatureFlag_displayPrevAfter,
                    View_AdvancedDVE = _FeatureFlag_showadvancedpipcontrols,
                    View_AdvancedPresentation = _FeatureFlag_viewAdvancedPresentation,
                    View_AuxOutput = _FeatureFlag_showAuxButons,
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
    }
}
