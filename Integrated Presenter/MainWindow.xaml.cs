using BMDSwitcherAPI;
using Integrated_Presenter.BMDHyperdeck;
using Integrated_Presenter.BMDSwitcher;
using Integrated_Presenter.BMDSwitcher.Config;
using Integrated_Presenter.ViewModels;
using Microsoft.Win32;
using SlideCreater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        BMDSwitcherConfigSettings _config;


        BMDHyperdeckManager mHyperdeckManager;

        List<SlidePoolSource> SlidePoolButtons;

        private BuildVersion VersionInfo;

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
            Title = $"Integrated Presenter - {VersionInfo.MajorVersion}.{VersionInfo.MinorVersion}.{VersionInfo.Revision}.{VersionInfo.Build}";





            mHyperdeckManager = new BMDHyperdeckManager();
            mHyperdeckManager.OnMessageFromHyperDeck += MHyperdeckManager_OnMessageFromHyperDeck;

            // set a default config
            SetDefaultConfig();

            HideAdvancedPresControls();
            HideAdvancedPIPControls();
            HideAuxButtonConrols();

            SlidePoolButtons = new List<SlidePoolSource>() { SlidePoolSource0, SlidePoolSource1, SlidePoolSource2, SlidePoolSource3 };

            ShowHideShortcutsUI();

            UpdateRealTimeClock();
            UpdateSlideControls();
            UpdateMediaControls();
            UpdateSlideModeButtons();
            DisableAuxControls();
            UpdateProgramRowLockButtonUI();
            UpdateRecordButtonUI();

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

        private void MHyperdeckManager_OnMessageFromHyperDeck(object sender, string message)
        {
            hyperDeckMonitorWindow?.AddMessage(message);
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
            UpdateSlidePreviewControls();
            ResetSlideMediaTimes();
            UpdateSlideControls();
            UpdateMediaControls();
        }

        IBMDSwitcherManager switcherManager;
        BMDSwitcherState switcherState = new BMDSwitcherState();


        #region BMD Switcher

        private void ConnectSwitcher()
        {
            Connection connectWindow = new Connection("Connect to Switcher", "Switcher IP Address:", "192.168.2.120");
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
            SetSwitcherSettings();
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
            SetSwitcherSettings();
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

            // update pip ui
            pipctrl?.PIPSettingsUpdated(switcherState.DVESettings);

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

        private void ClickPreset(int button)
        {
            switcherManager?.PerformPresetSelect(ConvertButtonToSourceID(button));
        }

        private void ClickAux(int button)
        {
            switcherManager?.PerformAuxSelect(ConvertButtonToSourceID(button));
        }

        private void ClickProgram(int button)
        {
            if (!IsProgramRowLocked)
            {
                switcherManager?.PerformProgramSelect(ConvertButtonToSourceID(button));
            }
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

            BtnDSK2Tie.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2OnOffAir.Style = (Style)Application.Current.FindResource(style);
            BtnDSK2Auto.Style = (Style)Application.Current.FindResource(style);

            BtnFTB.Style = (Style)Application.Current.FindResource(style);
            BtnCBars.Style = (Style)Application.Current.FindResource(style);

            BtnAutoTrans.Style = (Style)Application.Current.FindResource(style);
            BtnCutTrans.Style = (Style)Application.Current.FindResource(style);

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

        private bool showCurrentVideoTimeOnPreview = true;

        private void UpdateSlideCurrentPreviewTimes()
        {
            Dispatcher.Invoke(() =>
            {
                if (showCurrentVideoTimeOnPreview)
                {
                    if (ShowEffectiveCurrentPreview)
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
                if (showCurrentVideoTimeOnPreview)
                {
                    if (ShowEffectiveCurrentPreview)
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


        private void WindowKeyDown(object sender, KeyEventArgs e)
        {

            // dont enable shortcuts when focused on textbox
            if (tbChromaHue.IsFocused || tbChromaGain.IsFocused || tbChromaYSuppress.IsFocused || tbChromaLift.IsFocused || tbChromaNarrow.IsFocused)
            {
                return;
            }

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


            // recording

            if (e.Key == Key.F5)
            {
                TryStartRecording();
            }

            if (e.Key == Key.F6)
            {
                TryStopRecording();
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
                    audioPlayer?.RestartAudio();
                }
                if (e.Key == Key.F2)
                {
                    audioPlayer?.StopAudio();
                }
                if (e.Key == Key.F3)
                {
                    audioPlayer?.PauseAudio();
                }
                if (e.Key == Key.F4)
                {
                    audioPlayer?.PlayAudio();
                }
            }

            if (e.Key == Key.M)
            {
                MediaMuted = !MediaMuted;
                if (MediaMuted)
                {
                    muteMedia();
                }
                else
                {
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
                    ClickAux(10);
                else
                    switcherManager?.PerformToggleFTB();
            }

            // color bars
            if (e.Key == Key.C)
            {
                if (Keyboard.IsKeyDown(Key.Z))
                    ClickAux(11);
                else
                    SetProgramColorBars();
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


        private bool SetupActionsCompleted = false;
        private bool ActionsCompleted = false;

        private Guid currentslideforactions;

        private async Task ExecuteSetupActions(Slide s)
        {
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
        }

        private async Task ExecuteActionSlide(Slide s)
        {
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
                            switcherManager?.PerformPresetSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.ProgramSelect:
                        Dispatcher.Invoke(() =>
                        {
                            switcherManager?.PerformProgramSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.AuxSelect:
                        Dispatcher.Invoke(() =>
                        {
                            switcherManager?.PerformAuxSelect(task.DataI);
                        });
                        break;
                    case AutomationActionType.AutoTrans:
                        Dispatcher.Invoke(() =>
                        {
                            switcherManager?.PerformAutoTransition();
                        });
                        break;
                    case AutomationActionType.CutTrans:
                        Dispatcher.Invoke(() =>
                        {
                            switcherManager?.PerformCutTransition();
                        });
                        break;
                    case AutomationActionType.AutoTakePresetIfOnSlide:
                        // Take Preset if program source is fed from slides
                        if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformAutoTransition();
                            });
                            await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                        }
                        break;
                    case AutomationActionType.DSK1On:
                        if (!switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformToggleDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1Off:
                        if (switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformToggleDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1FadeOn:
                        if (!switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformAutoOnAirDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK1FadeOff:
                        if (switcherState.DSK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformAutoOffAirDSK1();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2On:
                        if (!switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformToggleDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2Off:
                        if (switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformToggleDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2FadeOn:
                        if (!switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformAutoOnAirDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.DSK2FadeOff:
                        if (switcherState.DSK2OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformAutoOffAirDSK2();
                            });
                        }
                        break;
                    case AutomationActionType.RecordStart:
                        Dispatcher.Invoke(() =>
                        {
                            TryStartRecording();
                        });
                        break;
                    case AutomationActionType.RecordStop:
                        Dispatcher.Invoke(() =>
                        {
                            TryStopRecording();
                        });
                        break;

                    case AutomationActionType.Timer1Restart:
                        if (automationtimer1enabled)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                timer1span = TimeSpan.Zero;
                            });
                        }
                        break;

                    case AutomationActionType.USK1On:
                        if (!switcherState.USK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformOnAirUSK1();
                            });
                        }
                        break;
                    case AutomationActionType.USK1Off:
                        if (switcherState.USK1OnAir)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                switcherManager?.PerformOffAirUSK1();
                            });
                        }
                        break;
                    case AutomationActionType.USK1SetTypeChroma:
                        Dispatcher.Invoke(() =>
                        {
                            switcherManager?.SetUSK1TypeChroma();
                        });
                        break;
                    case AutomationActionType.USK1SetTypeDVE:
                        Dispatcher.Invoke(() =>
                        {
                            switcherManager?.SetUSK1TypeDVE();
                        });
                        break;

                    case AutomationActionType.OpenAudioPlayer:
                        Dispatcher.Invoke(() =>
                        {
                            OpenAudioPlayer();
                            Focus();
                        });
                        break;
                    case AutomationActionType.LoadAudio:
                        string filename = Path.Join(Presentation.Folder, task.DataS);
                        Dispatcher.Invoke(() =>
                        {
                            audioPlayer.OpenAudio(filename);
                        });
                        break;
                    case AutomationActionType.PlayAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            audioPlayer.PlayAudio();
                        });
                        break;
                    case AutomationActionType.StopAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            audioPlayer.StopAudio();
                        });
                        break;
                    case AutomationActionType.PauseAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            audioPlayer.PauseAudio();
                        });
                        break;
                    case AutomationActionType.ReplayAuxAudio:
                        Dispatcher.Invoke(() =>
                        {
                            audioPlayer.RestartAudio();
                        });
                        break;

                    case AutomationActionType.PlayMedia:
                        Dispatcher.Invoke(() =>
                        {
                            playMedia();
                        });
                        break;
                    case AutomationActionType.PauseMedia:
                        Dispatcher.Invoke(() =>
                        {
                            pauseMedia();
                        });
                        break;
                    case AutomationActionType.StopMedia:
                        Dispatcher.Invoke(() =>
                        {
                            stopMedia();
                        });
                        break;
                    case AutomationActionType.RestartMedia:
                        Dispatcher.Invoke(() =>
                        {
                            restartMedia();
                        });
                        break;
                    case AutomationActionType.MuteMedia:
                        Dispatcher.Invoke(() =>
                        {
                            muteMedia();
                        });
                        break;
                    case AutomationActionType.UnMuteMedia:
                        Dispatcher.Invoke(() =>
                        {
                            unmuteMedia();
                        });
                        break;


                    case AutomationActionType.DelayMs:
                        await Task.Delay(task.DataI);
                        break;
                    case AutomationActionType.None:
                        break;
                    default:
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

        private async void SlideDriveVideo_Next()
        {
            if (Presentation?.Next != null)
            {
                if (Presentation.Next.Type == SlideType.Action)
                {
                    SetupActionsCompleted = false;
                    ActionsCompleted = false;
                    // run stetup actions
                    await ExecuteSetupActions(Presentation.Next);

                    if (Presentation.Next.AutoOnly)
                    {
                        // for now we won't support running 2 back to back fullauto slides.
                        // There really shouldn't be any need.
                        // We also cant run a script's setup actions immediatley afterward.
                        // again it shouldn't be nessecary, since in both cases you can add it to the fullauto slide's setup actions
                        Presentation.NextSlide();
                    }
                    // Perform slide actions
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    await ExecuteActionSlide(Presentation.EffectiveCurrent);
                }
                else if (Presentation.Next.Type == SlideType.Liturgy)
                {
                    // turn of usk1 if chroma keyer
                    if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                    {
                        switcherManager?.PerformOffAirUSK1();
                    }
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    Presentation.NextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    if (Presentation.OverridePres == true)
                    {
                        Presentation.OverridePres = false;
                        slidesUpdated();
                        PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    }
                    switcherManager?.PerformAutoOnAirDSK1();

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
                        switcherManager?.PerformAutoTransition();
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

                }
                else
                {
                    // turn of usk1 if chroma keyer
                    if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                    {
                        switcherManager?.PerformOffAirUSK1();
                    }
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager?.PerformAutoOffAirDSK1();
                        await Task.Delay((_config.DownstreamKey1Config.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
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
                        playMedia();
                        await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                    }
                    if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        ClickPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }

                }
                // At this point we've switched to the slide
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);
            }
        }

        private async void SlideUndrive_ToSlide(Slide s)
        {
            if (s != null && Presentation != null)
            {
                Presentation.Override = s;
                Presentation.OverridePres = true;
                slidesUpdated();
                PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
            }
        }

        private async void SlideDriveVideo_ToSlide(Slide s)
        {
            if (s != null && Presentation != null)
            {
                if (s.Type == SlideType.Action)
                {
                    // Run Setup Actions
                    await ExecuteSetupActions(s);
                    // Execute Slide Actions
                    Presentation.Override = s;
                    Presentation.OverridePres = true;
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    await ExecuteActionSlide(s);
                }
                else if (s.Type == SlideType.Liturgy)
                {
                    // turn of usk1 if chroma keyer
                    if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                    {
                        switcherManager?.PerformOffAirUSK1();
                    }
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
                else if (s.Type == SlideType.ChromaKeyStill || s.Type == SlideType.ChromaKeyVideo)
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
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    switcherManager?.PerformPresetSelect((int)previewsource);

                    // set slide
                    Presentation.Override = s;
                    Presentation.OverridePres = true;
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);


                    // start mediaplayout
                    if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                    {
                        playMedia();
                        await Task.Delay(_config?.PrerollSettings.ChromaVideoPreRoll ?? 0);
                    }

                    // turn on chroma key once playout has started
                    switcherManager?.PerformOnAirUSK1();

                }
                else
                {
                    // turn of usk1 if chroma keyer
                    if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                    {
                        switcherManager?.PerformOffAirUSK1();
                    }
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager?.PerformAutoOffAirDSK1();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    Presentation.Override = s;
                    Presentation.OverridePres = true;
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                    if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                    {
                        playMedia();
                        await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                    }

                    if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        ClickPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }


                }
                // At this point we've switched to the slide
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);
            }

        }

        private void SlideDriveVideo_Action(Slide s)
        {
            switch (s.PreAction)
            {
                case "t1restart":
                    if (automationtimer1enabled)
                    {
                        timer1span = TimeSpan.Zero;
                    }
                    break;
                case "mastercaution2":
                    //MasterCautionState = 2;
                    //UpdateMasterCautionDisplay();
                    break;
                case "startrecord":
                    if (automationrecordstartenabled)
                    {
                        mHyperdeckManager?.StartRecording();
                        isRecording = true;
                        UpdateRecordButtonUI();
                    }
                    break;
                default:
                    break;
            }
        }

        private async void SlideDriveVideo_Current()
        {
            if (Presentation?.EffectiveCurrent != null)
            {
                DisableSlidePoolOverrides();
                currentpoolsource = null;
                if (Presentation.EffectiveCurrent.Type == SlideType.Action)
                {
                    // Re-run setup actions
                    await ExecuteSetupActions(Presentation.EffectiveCurrent);
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    // Run Actions
                    await ExecuteActionSlide(Presentation.EffectiveCurrent);
                }
                else if (Presentation.EffectiveCurrent.Type == SlideType.Liturgy)
                {
                    // turn of usk1 if chroma keyer
                    if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                    {
                        switcherManager?.PerformOffAirUSK1();
                    }
                    // make sure slides aren't the program source
                    if (switcherState.ProgramID == _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                    switcherManager?.PerformAutoOnAirDSK1();

                }
                else if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyStill || Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
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
                        switcherManager?.PerformAutoTransition();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    switcherManager?.PerformPresetSelect((int)previewsource);

                    // set slide
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                    // start mediaplayout
                    if (Presentation.EffectiveCurrent.Type == SlideType.ChromaKeyVideo)
                    {
                        playMedia();
                        await Task.Delay(_config?.PrerollSettings.ChromaVideoPreRoll ?? 0);
                    }

                    // turn on chroma key once playout has started
                    switcherManager?.PerformOnAirUSK1();

                }
                else
                {
                    // turn of usk1 if chroma keyer
                    if (switcherState.USK1OnAir && switcherState.USK1KeyType == 2)
                    {
                        switcherManager?.PerformOffAirUSK1();
                    }
                    if (switcherState.DSK1OnAir)
                    {
                        switcherManager?.PerformAutoOffAirDSK1();
                        await Task.Delay((_config.MixEffectSettings.Rate / _config.VideoSettings.VideoFPS) * 1000);
                    }
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);

                    if (Presentation.EffectiveCurrent.Type == SlideType.Video)
                    {
                        playMedia();
                        await Task.Delay(_config?.PrerollSettings.VideoPreRoll ?? 0);
                    }

                    if (switcherState.ProgramID != _config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                    {
                        ClickPreset(_config.Routing.Where(r => r.KeyName == "slide").First().ButtonId);
                        await Task.Delay(500);
                        switcherManager?.PerformAutoTransition();
                    }


                }
                // Do Action on current slide
                SlideDriveVideo_Action(Presentation.EffectiveCurrent);
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

                // overwrite display of old presentation if already open
                if (_display != null && _display.IsWindowVisilbe)
                {
                }
                else
                {
                    _display = new PresenterDisplay(this, false);
                    _display.OnMediaPlaybackTimeUpdated += _display_OnMediaPlaybackTimeUpdated;
                    // start display
                    _display.Show();
                }

                if (_keydisplay == null && !(_keydisplay?.IsWindowVisilbe ?? false))
                {
                    _keydisplay = new PresenterDisplay(this, true);
                    // no need to get playback event info
                    _keydisplay.Show();
                }

                DisableSlidePoolOverrides();
                slidesUpdated();

                PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
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


        public bool ShowEffectiveCurrentPreview = true;

        private void ClickToggleShowEffectiveCurrentPreview(object sender, RoutedEventArgs e)
        {
            ShowEffectiveCurrentPreview = !ShowEffectiveCurrentPreview;
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
                if (ShowEffectiveCurrentPreview)
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
            currentslideforactions = currentGuid;
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
                if (CurrentSlideMode == 1)
                {
                    SlideDriveVideo_Next();
                }
                else if (CurrentSlideMode == 2)
                {
                    Presentation.SkipNextSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                }
                else
                {
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
                    Presentation.SkipPrevSlide();
                    slidesUpdated();
                    PresentationStateUpdated?.Invoke(Presentation.EffectiveCurrent);
                }
                else
                {
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
                    if (!Presentation.OverridePres || ShowEffectiveCurrentPreview)
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
                if (!Presentation.OverridePres || ShowEffectiveCurrentPreview)
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
                if (!Presentation.OverridePres || ShowEffectiveCurrentPreview)
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
                if (!Presentation.OverridePres || ShowEffectiveCurrentPreview)
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



        private void ClickSlideNoDriveVideo(object sender, RoutedEventArgs e)
        {
            SetBtnSlideMode(0);
        }


        private void ClickSlideDriveVideo(object sender, RoutedEventArgs e)
        {
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
            ToggleViewPrevAfter();
        }

        private void ToggleViewPrevAfter()
        {
            displayPrevAfter = !displayPrevAfter;
            UIUpdateDisplayPrevAfter();
            cbPrevAfter.IsChecked = displayPrevAfter;
        }

        private void ClickTakeSlide(object sender, RoutedEventArgs e)
        {
            SlideDriveVideo_Current();
        }


        bool _viewAdvancedPresentation = false;
        private void ClickViewAdvancedPresentation(object sender, RoutedEventArgs e)
        {
            ToggleViewAdvancedPresentation();
        }

        private void ToggleViewAdvancedPresentation()
        {
            _viewAdvancedPresentation = !_viewAdvancedPresentation;

            if (_viewAdvancedPresentation)
                ShowAdvancedPresControls();
            else
                HideAdvancedPresControls();

            cbAdvancedPresentation.IsChecked = _viewAdvancedPresentation;

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

        private void TakeSlidePoolSlide(Slide s, int num, bool replaceMode, bool driven)
        {

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
                SlideDriveVideo_ToSlide(s);
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
            _display?.Close();
            _keydisplay?.Close();
            switcherManager?.Close();
            hyperDeckMonitorWindow?.Close();
            audioPlayer?.Close();
            pipctrl?.Close();
        }

        private void ClickTakeSP0(object sender, Slide s, bool replaceMode, bool driven)
        {
            TakeSlidePoolSlide(s, 0, replaceMode, driven);
        }

        private void ClickTakeSP1(object sender, Slide s, bool replaceMode, bool driven)
        {
            TakeSlidePoolSlide(s, 1, replaceMode, driven);
        }

        private void ClickTakeSP2(object sender, Slide s, bool replaceMode, bool driven)
        {
            TakeSlidePoolSlide(s, 2, replaceMode, driven);
        }

        private void ClickTakeSP3(object sender, Slide s, bool replaceMode, bool driven)
        {
            TakeSlidePoolSlide(s, 3, replaceMode, driven);
        }

        private void ClickConfigureSwitcher(object sender, RoutedEventArgs e)
        {
            SetSwitcherSettings();
        }

        private void USK1RuntoA()
        {
            switcherManager?.PerformUSK1RunToKeyFrameA();
        }

        private void USK1RuntoB()
        {
            switcherManager?.PerformUSK1RunToKeyFrameB();
        }

        private void USK1RuntoFull()
        {
            switcherManager?.PerformUSK1RunToKeyFrameFull();
        }

        private void ClickPIPRunToFull(object sender, RoutedEventArgs e)
        {
            USK1RuntoFull();
        }

        private void ChangeUSK1FillSource(int source)
        {
            switcherManager?.PerformUSK1FillSourceSelect(ConvertButtonToSourceID(source));
        }

        private void ClickPIP1(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(1);
        }

        private void ClickPIP2(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(2);
        }

        private void ClickPIP3(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(3);
        }

        private void ClickPIP4(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(4);
        }

        private void ClickPIP5(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(5);
        }

        private void ClickPIP6(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(6);
        }

        private void ClickPIP7(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(7);
        }

        private void ClickPIP8(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(8);
        }


        private bool showadvancedpipcontrols = false;
        private void ClickViewAdvancedPIP(object sender, RoutedEventArgs e)
        {
            ToggleViewAdvancedPIP();
        }

        private void ToggleViewAdvancedPIP()
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
            cbAdvancedPresentation.IsChecked = showadvancedpipcontrols;

        }

        private void ShowAdvancedPIPControls()
        {
            grAdvancedPIP.Height = new GridLength(1, GridUnitType.Star);
        }

        private void HideAdvancedPIPControls()
        {
            grAdvancedPIP.Height = new GridLength(0);
        }

        private void ToggleTransBkgd()
        {
            switcherManager?.PerformToggleBackgroundForNextTrans();
        }

        private void ToggleTransKey1()
        {
            switcherManager?.PerformToggleKey1ForNextTrans();
        }

        private void ClickTransBkgd(object sender, RoutedEventArgs e)
        {
            ToggleTransBkgd();
        }

        private void ClickTransKey1(object sender, RoutedEventArgs e)
        {
            ToggleTransKey1();
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
            SetSwitcherSettings();
        }

        private void SetSwitcherSettings()
        {
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
            BMDSwitcherConfigSettings cfg = new BMDSwitcherConfigSettings()
            {
                VideoSettings = new BMDSwitcherVideoSettings()
                {
                    VideoFPS = 30,
                    VideoHeight = 1080,
                    VideoWidth = 1920
                },
                AudioSettings = new BMDSwitcherAudioSettings()
                {
                    ProgramOutGain = 2,
                    XLRInputGain = 6,
                },
                DefaultAuxSource = (int)BMDSwitcherVideoSources.ME1Prog,
                Routing = new List<ButtonSourceMapping>() {
                    new ButtonSourceMapping() { KeyName = "left", ButtonId = 1, ButtonName = "PULPIT", PhysicalInputId = 8, LongName = "PULPIT", ShortName = "PLPT" },
                    new ButtonSourceMapping() { KeyName = "center", ButtonId = 2, ButtonName = "CENTER", PhysicalInputId = 7, LongName = "CENTER", ShortName = "CNTR" },
                    new ButtonSourceMapping() { KeyName = "right", ButtonId = 3, ButtonName = "LECTERN", PhysicalInputId = 6, LongName = "LECTERN", ShortName = "LTRN" },
                    new ButtonSourceMapping() { KeyName = "organ", ButtonId = 4, ButtonName = "ORGAN", PhysicalInputId = 5, LongName = "ORGAN", ShortName = "ORGN" },
                    new ButtonSourceMapping() { KeyName = "slide", ButtonId = 5, ButtonName = "SLIDE", PhysicalInputId = 4, LongName = "SLIDESHOW", ShortName = "SLDE" },
                    new ButtonSourceMapping() { KeyName = "key", ButtonId = 6, ButtonName = "AKEY", PhysicalInputId = 3, LongName = "ALPHA KEY", ShortName = "AKEY" },
                    new ButtonSourceMapping() { KeyName = "proj", ButtonId = 7, ButtonName = "PROJ", PhysicalInputId = 2, LongName = "PROJECTOR", ShortName = "PROJ" },
                    new ButtonSourceMapping() { KeyName = "c1", ButtonId = 8, ButtonName = "CAM1", PhysicalInputId = 1, LongName = "HDMI 1", ShortName = "CAM1" },
                    new ButtonSourceMapping() { KeyName = "cf1", ButtonId = 9, ButtonName = "CLF1", PhysicalInputId = (int)BMDSwitcherVideoSources.CleanFeed1, LongName = "CLEAN FEED 1", ShortName = "CLF1" },
                    new ButtonSourceMapping() { KeyName = "cf2", ButtonId = 0, ButtonName = "CLF2", PhysicalInputId = (int)BMDSwitcherVideoSources.CleanFeed2, LongName = "CLEAN FEED 2", ShortName = "CLF2" },
                    new ButtonSourceMapping() { KeyName = "black", ButtonId = 10, ButtonName = "BLACK", PhysicalInputId = (int)BMDSwitcherVideoSources.Black, LongName = "BLACK", ShortName = "BLK" },
                    new ButtonSourceMapping() { KeyName = "cbar", ButtonId = 11, ButtonName = "CBAR", PhysicalInputId = (int)BMDSwitcherVideoSources.ColorBars, LongName = "COLOR BARS", ShortName = "CBAR" },
                    new ButtonSourceMapping() { KeyName = "program", ButtonId = 12, ButtonName = "PRGM", PhysicalInputId = (int)BMDSwitcherVideoSources.ME1Prog, LongName = "PROGRAM", ShortName = "PRGM" },
                    new ButtonSourceMapping() { KeyName = "preview", ButtonId = 13, ButtonName = "PREV", PhysicalInputId = (int)BMDSwitcherVideoSources.ME1Prev, LongName = "PREVIEW", ShortName = "PREV" },
                },
                MixEffectSettings = new BMDMixEffectSettings()
                {
                    Rate = 30,
                    FTBRate = 30,
                },
                MultiviewerConfig = new BMDMultiviewerSettings()
                {
                    Layout = (int)_BMDSwitcherMultiViewLayout.bmdSwitcherMultiViewLayoutProgramTop, // 12
                    Window2 = 8,
                    Window3 = 7,
                    Window4 = 6,
                    Window5 = 5,
                    Window6 = 4,
                    Window7 = 3,
                    Window8 = 2,
                    Window9 = 1
                },
                DownstreamKey1Config = new BMDDSKSettings()
                {
                    InputFill = 4,
                    InputCut = 3,
                    Clip = 0.5,
                    Gain = 0.35,
                    Rate = 30,
                    Invert = 0,
                    IsPremultipled = 0,
                    IsMasked = 0,
                    MaskTop = -5.5f,
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
                USKSettings = new BMDUSKSettings()
                {
                    IsDVE = 1,
                    IsChroma = 0,
                    PIPSettings = new BMDUSKDVESettings()
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
                            PositionY = -5.4,
                            SizeX = 0.4,
                            SizeY = 0.4
                        },
                        KeyFrameB = new KeyFrameSettings()
                        {
                            PositionX = 23,
                            PositionY = -5.4,
                            SizeX = 0.4,
                            SizeY = 0.4
                        }
                    },
                    ChromaSettings = new BMDUSKChromaSettings()
                    {
                        FillSource = 4,
                        Hue = 321.8,
                        Gain = 0.652,
                        YSuppress = 0.595,
                        Lift = 0.095,
                        Narrow = 0
                    },
                },
                PrerollSettings = new PrerollSettings()
                {
                    VideoPreRoll = 2000,
                    ChromaVideoPreRoll = 2000,
                }

            };


            _config = cfg;
        }

        public BMDSwitcherConfigSettings Config { get => _config; }

        bool showAuxButons = false;
        private void ClickViewAuxOutput(object sender, RoutedEventArgs e)
        {
            ToggleAuxRow();
        }

        private void ToggleAuxRow()
        {
            showAuxButons = !showAuxButons;
            if (showAuxButons)
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
            ClickAux(1);
        }

        private void ClickAux2(object sender, RoutedEventArgs e)
        {
            ClickAux(2);
        }

        private void ClickAux3(object sender, RoutedEventArgs e)
        {
            ClickAux(3);
        }

        private void ClickAux4(object sender, RoutedEventArgs e)
        {
            ClickAux(4);
        }

        private void ClickAux5(object sender, RoutedEventArgs e)
        {
            ClickAux(5);
        }

        private void ClickAux6(object sender, RoutedEventArgs e)
        {
            ClickAux(6);
        }

        private void ClickAux7(object sender, RoutedEventArgs e)
        {
            ClickAux(7);
        }
        private void ClickAux8(object sender, RoutedEventArgs e)
        {
            ClickAux(8);
        }
        private void ClickAuxPgm(object sender, RoutedEventArgs e)
        {
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
        }

        private void ShowAuxButtonControls()
        {
            showAuxButons = true;
            gridbtns.Width = 770;
            gcAdvancedProjector.Width = new GridLength(1.2, GridUnitType.Star);
        }

        private void HideAuxButtonConrols()
        {
            showAuxButons = false;
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
                try
                {
                    projectorSerialPort.Open();
                }
                catch (Exception)
                {
                    return;
                }
                projectorconnected = true;
            }
        }

        private void ClickConnectProjector(object sender, RoutedEventArgs e)
        {
            ConnectProjector();
        }



        bool isRecording = false;

        private void UpdateRecordButtonUI()
        {
            if (isRecording)
            {
                btnRecording.Background = Brushes.Red;
            }
            else
            {
                btnRecording.Background = Brushes.Gray;
            }
        }

        private void ClickConnectHyperdeck(object sender, RoutedEventArgs e)
        {
            mHyperdeckManager?.Connect();
        }

        private void ClickRecordStart(object sender, RoutedEventArgs e)
        {
            mHyperdeckManager?.StartRecording();
        }

        private void ClickRecordStop(object sender, RoutedEventArgs e)
        {
            mHyperdeckManager?.StopRecording();
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

        HyperDeckMonitorWindow hyperDeckMonitorWindow;

        private void OpenHyperdeckMonitorWindow(object sender, RoutedEventArgs e)
        {
            if (hyperDeckMonitorWindow == null)
            {
                hyperDeckMonitorWindow = new HyperDeckMonitorWindow();
                hyperDeckMonitorWindow.OnTextCommand += HyperDeckMonitorWindow_OnTextCommand;
            }
            if (hyperDeckMonitorWindow.IsClosed)
            {
                hyperDeckMonitorWindow = new HyperDeckMonitorWindow();
                hyperDeckMonitorWindow.OnTextCommand += HyperDeckMonitorWindow_OnTextCommand;
            }

            hyperDeckMonitorWindow.Show();




        }

        private void HyperDeckMonitorWindow_OnTextCommand(object sender, string cmd)
        {
            mHyperdeckManager?.Send(cmd);
        }

        private void ClickToggleRecording(object sender, RoutedEventArgs e)
        {
            if (mHyperdeckManager != null && mHyperdeckManager.IsConnected)
            {
                if (isRecording)
                {
                    isRecording = false;
                    mHyperdeckManager?.StopRecording();
                }
                else
                {
                    isRecording = true;
                    mHyperdeckManager?.StartRecording();
                }
                UpdateRecordButtonUI();
            }
            else
            {
                isRecording = false;
                UpdateRecordButtonUI();
            }
        }

        private void TryStartRecording()
        {
            if (mHyperdeckManager != null && mHyperdeckManager.IsConnected)
            {
                isRecording = true;
                mHyperdeckManager?.StartRecording();
            }
            UpdateRecordButtonUI();
        }

        private void TryStopRecording()
        {
            if (mHyperdeckManager != null && mHyperdeckManager.IsConnected)
            {
                isRecording = false;
                mHyperdeckManager?.StopRecording();
            }
            UpdateRecordButtonUI();
        }

        bool automationtimer1enabled = true;
        bool automationrecordstartenabled = true;

        private void ClickToggleAutomationTimer1(object sender, RoutedEventArgs e)
        {
            automationtimer1enabled = !automationtimer1enabled;
            miTimer1Restart.IsChecked = automationtimer1enabled;
        }

        private void ClickToggleAutomationRecordingStart(object sender, RoutedEventArgs e)
        {
            automationrecordstartenabled = !automationrecordstartenabled;
            miStartRecord.IsChecked = automationrecordstartenabled;
        }

        private void ClickDisconnectProjector(object sender, RoutedEventArgs e)
        {
            if (projectorconnected)
            {
                projectorSerialPort.Close();
            }
        }

        private void ClickDisconnectHyperdeck(object sender, RoutedEventArgs e)
        {
            mHyperdeckManager?.Disconnect();
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
            IsProgramRowLocked = !IsProgramRowLocked;
        }

        public AudioPlayer audioPlayer;

        private void ClickViewAudioPlayer(object sender, RoutedEventArgs e)
        {
            OpenAudioPlayer();
        }

        private void OpenAudioPlayer()
        {
            if (audioPlayer != null)
            {
                if (!audioPlayer.IsVisible)
                {
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
                audioPlayer = new AudioPlayer(this);
                audioPlayer.Show();
            }

        }

        private void ClickCBars(object sender, RoutedEventArgs e)
        {
            SetProgramColorBars();
        }

        private void SetProgramColorBars()
        {
            switcherManager?.PerformProgramSelect((int)BMDSwitcherVideoSources.ColorBars);
        }

        private void ClickToggleShowCurrentVideoCountdownTimer(object sender, RoutedEventArgs e)
        {
            ToggleShowCurrentVideoCountdownTimer();
        }

        private void ToggleShowCurrentVideoCountdownTimer()
        {
            showCurrentVideoTimeOnPreview = !showCurrentVideoTimeOnPreview;
            cbShowCurrentVideoCountdownTimer.IsChecked = showCurrentVideoTimeOnPreview;
        }

        private void ClickDVEMode(object sender, RoutedEventArgs e)
        {
            SetSwitcherKeyerDVE();
        }

        private void ClickChromaMode(object sender, RoutedEventArgs e)
        {
            SetSwitcherKeyerChroma();
        }

        private void SetSwitcherKeyerDVE()
        {
            //switcherManager?.ConfigureUSK1PIP(_config.USKSettings.PIPSettings);
            switcherManager?.SetUSK1TypeDVE();
            // force blocking state update
            SwitcherManager_SwitcherStateChanged(switcherManager?.ForceStateUpdate());
            ShowKeyerUI();
        }

        private void SetSwitcherKeyerChroma()
        {
            //switcherManager?.ConfigureUSK1Chroma(_config.USKSettings.ChromaSettings);
            switcherManager?.SetUSK1TypeChroma();
            // force blocking state update
            SwitcherManager_SwitcherStateChanged(switcherManager?.ForceStateUpdate() ?? new BMDSwitcherState());
            tbChromaHue.Text = switcherState.ChromaSettings.Hue.ToString();
            tbChromaGain.Text = switcherState.ChromaSettings.Gain.ToString();
            tbChromaLift.Text = switcherState.ChromaSettings.Lift.ToString();
            tbChromaYSuppress.Text = switcherState.ChromaSettings.YSuppress.ToString();
            tbChromaNarrow.Text = switcherState.ChromaSettings.Narrow.ToString();
            ShowKeyerUI();
        }

        private void ClickApplyChromaSettings(object sender, RoutedEventArgs e)
        {
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
            ChangeUSK1FillSource(1);
        }

        private void ClickChroma2(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(2);
        }

        private void ClickChroma3(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(3);
        }

        private void ClickChroma4(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(4);
        }

        private void ClickChroma5(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(5);
        }

        private void ClickChroma6(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(6);
        }

        private void ClickChroma7(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(7);
        }

        private void ClickChroma8(object sender, RoutedEventArgs e)
        {
            ChangeUSK1FillSource(8);
        }

        private void LoadDefault(object sender, RoutedEventArgs e)
        {
            SetSwitcherSettings();
        }

        PIPControl pipctrl;
        private void ClickViewAdvancedPIPLocation(object sender, RoutedEventArgs e)
        {
            ShowPIPLocationControl();
        }

        private void ShowPIPLocationControl()
        {
            if (pipctrl == null)
            {
                pipctrl = new PIPControl(this, SetPIPPosition, SetKeyFrameAOnSwitcher, SetKeyFrameBOnSwitcher, switcherState?.DVESettings ?? _config.USKSettings.PIPSettings);
            }
            if (pipctrl.HasClosed)
            {
                pipctrl = new PIPControl(this, SetPIPPosition, SetKeyFrameAOnSwitcher, SetKeyFrameBOnSwitcher, switcherState?.DVESettings ?? _config.USKSettings.PIPSettings);
            }
            pipctrl.Show();
            pipctrl.Focus();
        }

        private void ClickPIPRunToA(object sender, RoutedEventArgs e)
        {
            switcherManager?.PerformUSK1RunToKeyFrameA();
        }

        private void ClickPIPRunToB(object sender, RoutedEventArgs e)
        {
            switcherManager?.PerformUSK1RunToKeyFrameB();
        }

        bool ShowRecordButton = false;
        private void ToggleViewRecordButton()
        {
            ShowRecordButton = !ShowRecordButton;
            if (ShowRecordButton)
            {
                cbRecordButton.IsChecked = true;
                borderBtnRecording.Visibility = Visibility.Visible;
            }
            else
            {
                cbRecordButton.IsChecked = false;
                borderBtnRecording.Visibility = Visibility.Collapsed;
            }
        }

        private void ClickViewRecordButton(object sender, RoutedEventArgs e)
        {
            ToggleViewRecordButton();
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
            ToggleShowShortcuts();
        }

        private void ToggleMutePres(object sender, RoutedEventArgs e)
        {
            if (MediaMuted)
            {
                muteMedia();
            }
            else
            {
                unmuteMedia();
            }
        }

        private void ClickShowOnlineHelp(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/kjgriffin/LivestreamServiceSuite/wiki/Integrated-Presenter-Shortcuts");
        }
    }
}
