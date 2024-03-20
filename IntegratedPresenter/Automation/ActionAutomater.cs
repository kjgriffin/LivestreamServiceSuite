using CCUI_UI;

using Configurations.SwitcherConfig;

using DVIPProtocol.Protocol.Lib.Command.PTDrive;

using IntegratedPresenter.BMDSwitcher.Config;

using IntegratedPresenterAPIInterop;

using log4net;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using VariableMarkupAttributes;

namespace Integrated_Presenter.Automation
{
    internal class ActionAutomater : IActionAutomater
    {

        protected ILog _logger;
        protected ISwitcherDriverProvider _switcherProvider;
        protected IAutoTransitionProvider _autoTransitionProvider;
        protected IAutomationConditionProvider _automationConditionProvider;
        protected IConfigProvider _configProvider;
        protected IFeatureFlagProvider _featureFlagProvider;
        protected IUserTimerProvider _userTimerProvider;
        protected IMainUIProvider _mainUIProvider;
        protected IPresentationProvider _presentationProvider;
        protected IAudioDriverProvider _audioDriverProvider;
        protected IMediaDriverProvider _mediaDriverProvider;
        protected ConditionWatchProvider GetWatches;
        protected IDynamicControlProvider _dynamicControlProvider;
        protected IExtraDynamicControlProvider _extraDynamicControlProvider;
        protected ICCPUPresetMonitor _camPresets;
        protected ICalculatedVariableManager _variableManager;


        internal ActionAutomater(ILog logger,
                                 ISwitcherDriverProvider switcherProvider,
                                 IAutoTransitionProvider autoTransitionProvider,
                                 IAutomationConditionProvider automationConditionProvider,
                                 IConfigProvider configProvider,
                                 IFeatureFlagProvider featureFlagProvider,
                                 IUserTimerProvider userTimerProvider,
                                 IMainUIProvider mainUIProvider,
                                 IPresentationProvider presentationProvider,
                                 IAudioDriverProvider audioDriverProvider,
                                 IMediaDriverProvider mediaDriverProvider,
                                 ConditionWatchProvider watchProvider,
                                 IDynamicControlProvider dynamicControlProvider,
                                 IExtraDynamicControlProvider extraDynamicControlProvider,
                                 ICCPUPresetMonitor camPresets,
                                 ICalculatedVariableManager variableManager)
        {
            _logger = logger;
            _switcherProvider = switcherProvider;
            _autoTransitionProvider = autoTransitionProvider;
            _automationConditionProvider = automationConditionProvider;
            _configProvider = configProvider;
            _featureFlagProvider = featureFlagProvider;
            _userTimerProvider = userTimerProvider;
            _mainUIProvider = mainUIProvider;
            _presentationProvider = presentationProvider;
            _audioDriverProvider = audioDriverProvider;
            _mediaDriverProvider = mediaDriverProvider;
            GetWatches = watchProvider;
            _dynamicControlProvider = dynamicControlProvider;
            _extraDynamicControlProvider = extraDynamicControlProvider;
            _camPresets = camPresets;
            _variableManager = variableManager;
        }

        public virtual async Task<ActionResult> PerformAutomationAction(AutomationAction task, string requesterID)
        {
            if (!task.MeetsConditionsToRun(_automationConditionProvider.GetConditionals(GetWatches())))
            {
                return new ActionResult(TrackedActionState.Skipped);
            }
            bool continueProcessing = true;
            await Task.Run(async () =>
            {
                switch (task.Action)
                {
                    case AutomationActions.PresetSelect:
                        if (task.TryEvaluateAutomationActionParmeter<int>("SourceID", _variableManager, out var pstid))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Preset Select {pstid}");
                            _switcherProvider?.switcherManager?.PerformPresetSelect(pstid);
                        }
                        break;
                    case AutomationActions.ProgramSelect:
                        if (task.TryEvaluateAutomationActionParmeter<int>("SourceID", _variableManager, out var pgid))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Program Select {pgid}");
                            _switcherProvider?.switcherManager?.PerformProgramSelect(pgid);
                        }
                        break;
                    case AutomationActions.AuxSelect:
                        if (task.TryEvaluateAutomationActionParmeter<int>("SourceID", _variableManager, out var auxid))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Aux Select {auxid}");
                            _switcherProvider?.switcherManager?.PerformAuxSelect(auxid);
                        }
                        break;
                    case AutomationActions.USK1Fill:
                        if (task.TryEvaluateAutomationActionParmeter<int>("SourceID", _variableManager, out var ukfid))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- USK1Fill {ukfid}");
                            _switcherProvider?.switcherManager?.PerformUSK1FillSourceSelect(ukfid);
                        }
                        break;
                    case AutomationActions.PlacePIP:
                        _logger.Debug($"(PerformAutomationAction) -- PlacePIP. read cfg");
                        if (task.TryEvaluateAutomationActionParmeter<double>("PosX", _variableManager, out var posX)
                            && task.TryEvaluateAutomationActionParmeter<double>("PosY", _variableManager, out var posY)
                            && task.TryEvaluateAutomationActionParmeter<double>("ScaleX", _variableManager, out var scaleX)
                            && task.TryEvaluateAutomationActionParmeter<double>("ScaleY", _variableManager, out var scaleY)
                            && task.TryEvaluateAutomationActionParmeter<double>("MaskLeft", _variableManager, out var maskL)
                            && task.TryEvaluateAutomationActionParmeter<double>("MaskRight", _variableManager, out var maskR)
                            && task.TryEvaluateAutomationActionParmeter<double>("MaskTop", _variableManager, out var maskT)
                            && task.TryEvaluateAutomationActionParmeter<double>("MaskBottom", _variableManager, out var maskB))
                        {
                            var cfg = new PIPPlaceSettings
                            {
                                PosX = posX,
                                PosY = posY,
                                ScaleX = scaleX,
                                ScaleY = scaleY,
                                MaskLeft = maskL,
                                MaskRight = maskR,
                                MaskTop = maskT,
                                MaskBottom = maskB,
                            };
                            _logger.Debug($"(PerformAutomationAction) -- PlacePIP at {cfg.ToString()}");
                            var current = _switcherProvider?.switcherManager?.GetCurrentState().DVESettings;
                            var config = cfg.PlaceOverride(current);
                            _switcherProvider?.switcherManager?.SetPIPPosition(config);
                        }
                        break;

                    case AutomationActions.ConfigurePATTERN:
                        _logger.Debug($"(PerformAutomationAction) -- ApplyPATTERN. read cfg");
                        if (task.TryEvaluateAutomationActionParmeter<string>("Type", _variableManager, out var pType)
                            && task.TryEvaluateAutomationActionParmeter<bool>("Inverted", _variableManager, out var pInv)
                            && task.TryEvaluateAutomationActionParmeter<double>("Size", _variableManager, out var pSize)
                            && task.TryEvaluateAutomationActionParmeter<double>("Symmetry", _variableManager, out var pSym)
                            && task.TryEvaluateAutomationActionParmeter<double>("Softness", _variableManager, out var pSoft)
                            && task.TryEvaluateAutomationActionParmeter<double>("XOffset", _variableManager, out var pXOff)
                            && task.TryEvaluateAutomationActionParmeter<double>("YOffset", _variableManager, out var pYOff))
                        {
                            var cfill = _switcherProvider?.switcherManager?.GetCurrentState().USK1FillSource;
                            BMDUSKPATTERNSettings ptn = new BMDUSKPATTERNSettings
                            {
                                PatternType = pType,
                                DefaultFillSource = (int)cfill,
                                Inverted = pInv,
                                Size = pSize,
                                Softness = pSoft,
                                Symmetry = pSym,
                                XOffset = pXOff,
                                YOffset = pYOff,
                            };
                            _logger.Debug($"(PerformAutomationAction) -- PlacePIP at {ptn.ToString()}");
                            _switcherProvider?.switcherManager?.ConfigureUSK1PATTERN(ptn);
                        }
                        break;

                    case AutomationActions.AutoTrans:
                        //_switcherProvider?.switcherManager?.PerformAutoTransition();
                        _logger.Debug($"(PerformAutomationAction) -- AutoTrans (gaurded)");
                        _autoTransitionProvider.PerformGuardedAutoTransition();
                        break;
                    case AutomationActions.CutTrans:
                        // Will always allow automation to perform cut transition.
                        // Gaurded Cut transition is only for debouncing/preventing operators using a keyboard from spamming cut requests.
                        _logger.Debug($"(PerformAutomationAction) -- Cut (unguarded)");
                        _switcherProvider?.switcherManager?.PerformCutTransition();
                        break;
                    case AutomationActions.AutoTakePresetIfOnSlide:
                        // Take Preset if program source is fed from slides
                        if (_switcherProvider.switcherState.ProgramID == _configProvider._config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
                        {
                            //_switcherProvider?.switcherManager?.PerformAutoTransition();
                            _logger.Debug($"(PerformAutomationAction) -- AutoTrans (guarded), requred since 'slide' source was on air : AutoTakePresetIfOnSlide");
                            _autoTransitionProvider.PerformGuardedAutoTransition();
                            await Task.Delay((_configProvider._config.MixEffectSettings.Rate / _configProvider._config.VideoSettings.VideoFPS) * 1000);
                        }
                        break;
                    case AutomationActions.DSK1On:
                        if (!_switcherProvider.switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Toggle DSK1 since current state is OFF and requested state is ON");
                            _switcherProvider?.switcherManager?.PerformToggleDSK1();
                        }
                        break;
                    case AutomationActions.DSK1Off:
                        if (_switcherProvider.switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Toggle DSK1 since current state is ON and requested state is OFF");
                            _switcherProvider?.switcherManager?.PerformToggleDSK1();
                        }
                        break;
                    case AutomationActions.DSK1FadeOn:
                        if (!_switcherProvider.switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- FADE ON DSK1 since current state is OFF and requested state is ON");
                            _switcherProvider?.switcherManager?.PerformAutoOnAirDSK1();
                        }
                        break;
                    case AutomationActions.DSK1FadeOff:
                        if (_switcherProvider.switcherState.DSK1OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- FADE OFF DSK1 since current state is ON and requested state is OFF");
                            _switcherProvider?.switcherManager?.PerformAutoOffAirDSK1();
                        }
                        break;

                    case AutomationActions.DSK1TieOn:
                        _logger.Debug($"(PerformAutomationAction) -- TIE DSK1 to Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetTieDSK1(true);
                        break;
                    case AutomationActions.DSK1TieOff:
                        _logger.Debug($"(PerformAutomationAction) -- UNTIE DSK1 to Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetTieDSK1(false);
                        break;



                    case AutomationActions.DSK2On:
                        if (!_switcherProvider.switcherState.DSK2OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Toggle DSK2 since current state is OFF and requested state is ON");
                            _switcherProvider?.switcherManager?.PerformToggleDSK2();
                        }
                        break;
                    case AutomationActions.DSK2Off:
                        if (_switcherProvider.switcherState.DSK2OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Toggle DSK2 since current state is ON and requested state is OFF");
                            _switcherProvider?.switcherManager?.PerformToggleDSK2();
                        }
                        break;
                    case AutomationActions.DSK2FadeOn:
                        if (!_switcherProvider.switcherState.DSK2OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- FADE ON DSK2 since current state is OFF and requested state is ON");
                            _switcherProvider?.switcherManager?.PerformAutoOnAirDSK2();
                        }
                        break;
                    case AutomationActions.DSK2FadeOff:
                        if (_switcherProvider.switcherState.DSK2OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- FADE OFF DSK2 since current state is ON and requested state is OFF");
                            _switcherProvider?.switcherManager?.PerformAutoOffAirDSK2();
                        }
                        break;

                    case AutomationActions.DSK2TieOn:
                        _logger.Debug($"(PerformAutomationAction) -- TIE DSK2 to Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetTieDSK2(true);
                        break;
                    case AutomationActions.DSK2TieOff:
                        _logger.Debug($"(PerformAutomationAction) -- UNTIE DSK2 to Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetTieDSK2(false);
                        break;

                    case AutomationActions.RecordStart:
                        break;
                    case AutomationActions.RecordStop:
                        break;

                    case AutomationActions.Timer1Restart:
                        if (_featureFlagProvider.AutomationTimer1Enabled)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- Reset gp timer 1");
                            _userTimerProvider.ResetGpTimer1();
                        }
                        break;

                    case AutomationActions.BKGDTieOn:
                        _logger.Debug($"(PerformAutomationAction) -- ENABLE BKGD layer on Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetBKDGOnForNextTrans();
                        break;
                    case AutomationActions.BKGDTieOff:
                        _logger.Debug($"(PerformAutomationAction) -- DISABLE BKGD layer on Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetBKDGOffForNextTrans();
                        break;


                    case AutomationActions.USK1On:
                        if (!_switcherProvider.switcherState.USK1OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- USK1 ON air since current state is OFF and requsted state is ON");
                            _switcherProvider?.switcherManager?.PerformOnAirUSK1();
                        }
                        break;
                    case AutomationActions.USK1Off:
                        if (_switcherProvider.switcherState.USK1OnAir)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- USK1 OFF air since current state is ON and requsted state is OFF");
                            _switcherProvider?.switcherManager?.PerformOffAirUSK1();
                        }
                        break;


                    case AutomationActions.USK1TieOn:
                        _logger.Debug($"(PerformAutomationAction) -- ENABLE USK1 layer on Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetKey1OnForNextTrans();
                        break;
                    case AutomationActions.USK1TieOff:
                        _logger.Debug($"(PerformAutomationAction) -- DISABLE USK1 layer on Next Transition");
                        _switcherProvider?.switcherManager?.PerformSetKey1OffForNextTrans();
                        break;

                    case AutomationActions.USK1SetTypeChroma:
                        _logger.Debug($"(PerformAutomationAction) -- Configure USK1 for type chroma");
                        _switcherProvider?.switcherManager?.SetUSK1TypeChroma();
                        break;
                    case AutomationActions.USK1SetTypeDVE:
                        _logger.Debug($"(PerformAutomationAction) -- Configure USK1 for type DVE PIP");
                        _switcherProvider?.switcherManager?.SetUSK1TypeDVE();
                        break;
                    case AutomationActions.USK1SetTypePATTERN:
                        _logger.Debug($"(PerformAutomationAction) -- Configure USK1 for type PATTERN");
                        _switcherProvider?.switcherManager?.SetUSK1TypePATTERN();
                        break;

                    case AutomationActions.OpenAudioPlayer:
                        _logger.Debug($"(PerformAutomationAction) -- Opened Audio Player");
                        _audioDriverProvider.OpenAudioPlayer();
                        _mainUIProvider.Focus();
                        break;
                    case AutomationActions.LoadAudio:
                        if (task.TryEvaluateAutomationActionParmeter<string>("AudioFile", _variableManager, out var audiofile))
                        {
                            string filename = Path.Join(_presentationProvider.Folder, audiofile);
                            _logger.Debug($"(PerformAutomationAction) -- Load Audio File {filename} to Aux Player");
                            _audioDriverProvider.OpenAudio(filename);
                        }
                        break;
                    case AutomationActions.PlayAuxAudio:
                        _logger.Debug($"(PerformAutomationAction) -- Aux:PlayAudio()");
                        _audioDriverProvider.PlayAudio();
                        break;
                    case AutomationActions.StopAuxAudio:
                        _logger.Debug($"(PerformAutomationAction) -- Aux:StopAudio()");
                        _audioDriverProvider.StopAudio();
                        break;
                    case AutomationActions.PauseAuxAudio:
                        _logger.Debug($"(PerformAutomationAction) -- Aux:PauseAudio()");
                        _audioDriverProvider.PauseAudio();
                        break;
                    case AutomationActions.ReplayAuxAudio:
                        _logger.Debug($"(PerformAutomationAction) -- Aux:RestartAudio()");
                        _audioDriverProvider.RestartAudio();
                        break;

                    case AutomationActions.PlayMedia:
                        _logger.Debug($"(PerformAutomationAction) -- playMedia()");
                        _mediaDriverProvider.playMedia();
                        break;
                    case AutomationActions.PauseMedia:
                        _logger.Debug($"(PerformAutomationAction) -- pauseMedia()");
                        _mediaDriverProvider.pauseMedia();
                        break;
                    case AutomationActions.StopMedia:
                        _logger.Debug($"(PerformAutomationAction) -- stopMedia()");
                        _mediaDriverProvider.stopMedia();
                        break;
                    case AutomationActions.RestartMedia:
                        _logger.Debug($"(PerformAutomationAction) -- restartMedia()");
                        _mediaDriverProvider.restartMedia();
                        break;
                    case AutomationActions.MuteMedia:
                        _logger.Debug($"(PerformAutomationAction) -- muteMedia()");
                        _mediaDriverProvider.muteMedia();
                        break;
                    case AutomationActions.UnMuteMedia:
                        _logger.Debug($"(PerformAutomationAction) -- unmuteMedia()");
                        _mediaDriverProvider.unmuteMedia();
                        break;

                    case AutomationActions.DelayMs:
                        if (task.TryEvaluateAutomationActionParmeter<int>("Delay", _variableManager, out var delayms))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- delay for {delayms} ms");
                            await Task.Delay(delayms);
                        }
                        break;
                    case AutomationActions.DelayUntil:
                        {
                            if (task.TryEvaluateAutomationActionParmeter<string>("DateTime", _variableManager, out var datetime))
                            {
                                _logger.Debug($"(PerformAutomationAction) -- delay until {datetime}");
                                // parse time as date
                                if (DateTime.TryParse(datetime, out var time))
                                {
                                    // compute time to wait
                                    DateTime now = DateTime.Now;
                                    TimeSpan diff = time - now;
                                    if (diff.TotalMilliseconds > 0)
                                    {
                                        await Task.Delay((int)diff.TotalMilliseconds);
                                    }
                                }
                            }
                            break;
                        }

                    case AutomationActions.JumpToSlide:
                        if (task.TryEvaluateAutomationActionParmeter<int>("SlideNum", _variableManager, out var slidenum))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- jump to slide {slidenum}");
                            continueProcessing = false;
                            _presentationProvider.SetNextSlideTarget(slidenum);
                            await _presentationProvider.TakeNextSlide();
                        }
                        break;

                    case AutomationActions.DriveNextSlide:
                        _logger.Debug($"(PerformAutomationAction) -- drive next slide");
                        continueProcessing = false;
                        // calculate next slide
                        //_presentationProvider.SetNextSlideTarget(task.DataI);
                        await _presentationProvider.TakeNextSlide();
                        break;

                    case AutomationActions.SetupButtons:
                        if (task.TryEvaluateAutomationActionParmeter<string>("File", _variableManager, out var btnFile)
                            && task.TryEvaluateAutomationActionParmeter<string>("ResourcePath", _variableManager, out var resPath)
                            && task.TryEvaluateAutomationActionParmeter<bool>("Overwrite", _variableManager, out var overwritePanel))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- setup buttons from file: ${btnFile} @{resPath} ({overwritePanel})");
                            _dynamicControlProvider.ConfigureControls(btnFile, resPath, overwritePanel);
                        }
                        else
                        {
                            _logger.Debug($"(PerformAutomationAction) -- ABORT (bad params) setup buttons from file");
                        }
                        break;

                    case AutomationActions.SetupExtras:
                        if (task.TryEvaluateAutomationActionParmeter<string>("ExtraID", _variableManager, out var extraid)
                            && task.TryEvaluateAutomationActionParmeter<string>("File", _variableManager, out var extraFile)
                            && task.TryEvaluateAutomationActionParmeter<string>("ResourcePath", _variableManager, out var eresPath)
                            && task.TryEvaluateAutomationActionParmeter<bool>("Overwrite", _variableManager, out var overwriteExtra))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- setup extras {extraid} from file: ${extraFile} @{eresPath} ({overwriteExtra})");
                            _extraDynamicControlProvider.ConfigureControls(extraid, extraFile, eresPath, overwriteExtra);
                        }
                        else
                        {
                            _logger.Debug($"(PerformAutomationAction) -- ABORT (bad params) setup extras from file");
                        }
                        break;

                    case AutomationActions.ForceRunPostSet:
                        var slide = _presentationProvider.GetCurentSlide();
                        if (slide != null && slide.PostsetEnabled)
                        {
                            int postset = slide.PostsetId;
                            _logger.Debug($"(PerformAutomationAction) -- force run postset {postset}");
                            _switcherProvider.switcherManager.PerformPresetSelect(postset);
                        }
                        else
                        {
                            if (task.TryEvaluateAutomationActionParmeter<bool>("Force", _variableManager, out var forcePost)
                                && task.TryEvaluateAutomationActionParmeter<int>("SourceID", _variableManager, out var postId)
                                && forcePost)
                            {
                                _logger.Debug($"(PerformAutomationAction) -- force run postset with fallback {postId}");
                                _switcherProvider.switcherManager.PerformPresetSelect(postId);
                            }
                        }
                        break;


                    case AutomationActions.FireActivePreset:
                        slide = _presentationProvider.GetCurentSlide();
                        if (slide != null
                            && slide.AutoPilotActions.Any()
                            && task.TryEvaluateAutomationActionParmeter<string>("CamName", _variableManager, out var aCamId))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- fire active preset");

                            // check if we have an active pilot command to fire
                            var pst = slide.AutoPilotActions.FirstOrDefault(x => x.CamName == aCamId);
                            if (pst != null)
                            {
                                pst.Execute(_camPresets, 15);
                            }
                        }
                        break;
                    case AutomationActions.FireCamPreset:
                        if (task.TryEvaluateAutomationActionParmeter<string>("CamName", _variableManager, out var pCamId)
                            && task.TryEvaluateAutomationActionParmeter<string>("PresetName", _variableManager, out var pstName)
                            && task.TryEvaluateAutomationActionParmeter<int>("Speed", _variableManager, out var pstSpeed)
                            && task.TryEvaluateAutomationActionParmeter<string>("ZoomPST", _variableManager, out var zoomPst))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- fire cam preset");

                            _camPresets?.FirePreset_Tracked(pCamId, pstName, pstSpeed);
                            _camPresets?.FireZoomLevel_Tracked(pCamId, zoomPst);
                        }
                        break;
                    case AutomationActions.FireCamDrive:
                        if (task.TryEvaluateAutomationActionParmeter<string>("CamName", _variableManager, out var dCamId)
                            && task.TryEvaluateAutomationActionParmeter<int>("DirX", _variableManager, out var dX)
                            && task.TryEvaluateAutomationActionParmeter<int>("DirY", _variableManager, out var dY)
                            && task.TryEvaluateAutomationActionParmeter<int>("SpeedX", _variableManager, out var sX)
                            && task.TryEvaluateAutomationActionParmeter<int>("SpeedY", _variableManager, out var sY))
                        {
                            _logger.Debug($"(PerformAutomationAction) -- fire cam drive");

                            _camPresets?.PanTiltDrive(dCamId, dX, dY, sX, sY);
                        }
                        break;

                    case AutomationActions.WatchSwitcherStateBoolVal:
                    case AutomationActions.WatchSwitcherStateIntVal:
                    case AutomationActions.WatchStateBoolVal:
                    case AutomationActions.WatchStateIntVal:
                        // TODO: do we need to process these as such??
                        // or let them be handled as dynamic conditions
                        break;

                    case AutomationActions.InitComputedVal:
                        if (task.TryEvaluateAutomationActionParmeter<string>("VarName", _variableManager, out var ivalname)
                            && task.TryEvaluateAutomationActionParmeter<string>("TypeStr", _variableManager, out var ivaltype)
                            && task.TryGetAutomationActionParmeter("VarDefaultVal", out var iinitparam))
                        {
                            AutomationActionArgType atype = AutomationActionArgType.UNKNOWN_TYPE;
                            switch (ivaltype)
                            {
                                case "int": atype = AutomationActionArgType.Integer; break;
                                case "double": atype = AutomationActionArgType.Double; break;
                                case "bool": atype = AutomationActionArgType.Boolean; break;
                                case "string": atype = AutomationActionArgType.String; break;
                            }
                            if (atype != AutomationActionArgType.UNKNOWN_TYPE && iinitparam.IsLiteral)
                            {
                                _variableManager.InitializeVariable(requesterID, ivalname, atype, (string)iinitparam.LiteralValue);
                            }
                        }
                        break;
                    case AutomationActions.WriteComputedVal:
                        if (task.TryEvaluateAutomationActionParmeter<string>("VarName", _variableManager, out var wvalname) && task.TryGetAutomationActionParmeter("VarVal", out var wparam) && _variableManager.TryGetVariableInfo(wvalname, out var vinfo))
                        {
                            if (wparam.IsLiteral)
                            {
                                dynamic val = CalculatedVariable.ParseDynamicVariableValue(vinfo.VarType, wparam.LiteralValue);
                                switch (vinfo.VarType)
                                {
                                    case AutomationActionArgType.Integer:
                                        _variableManager.WriteVariableValue(wvalname, (int)val);
                                        break;
                                    case AutomationActionArgType.String:
                                        _variableManager.WriteVariableValue(wvalname, (string)val);
                                        break;
                                    case AutomationActionArgType.Double:
                                        _variableManager.WriteVariableValue(wvalname, (double)val);
                                        break;
                                    case AutomationActionArgType.Boolean:
                                        _variableManager.WriteVariableValue(wvalname, (bool)val);
                                        break;
                                }
                            }
                            else
                            {
                                switch (vinfo.VarType)
                                {
                                    case AutomationActionArgType.Integer:
                                        if (_variableManager.TryEvaluateVariableValue<int>(wparam.ComputedVaraible, out int ival))
                                        {
                                            _variableManager.WriteVariableValue(wvalname, ival);
                                        }
                                        break;
                                    case AutomationActionArgType.String:
                                        if (_variableManager.TryEvaluateVariableValue<string>(wparam.ComputedVaraible, out string sval))
                                        {
                                            _variableManager.WriteVariableValue(wvalname, sval);
                                        }
                                        break;
                                    case AutomationActionArgType.Double:
                                        if (_variableManager.TryEvaluateVariableValue<double>(wparam.ComputedVaraible, out double dval))
                                        {
                                            _variableManager.WriteVariableValue(wvalname, dval);
                                        }
                                        break;
                                    case AutomationActionArgType.Boolean:
                                        if (_variableManager.TryEvaluateVariableValue<bool>(wparam.ComputedVaraible, out bool bval))
                                        {
                                            _variableManager.WriteVariableValue(wvalname, bval);
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case AutomationActions.SetupComputedTrack:
                        if (task.TryEvaluateAutomationActionParmeter<string>("VarName", _variableManager, out var tvalname) && task.TryGetAutomationActionParmeter("TrackTarget", out var tparam))
                        {
                            if (tparam.IsLiteral)
                            {
                                _variableManager.SetupVariableTrack(tvalname, (string)tparam.LiteralValue);
                            }
                        }
                        break;
                    case AutomationActions.ReleaseComputedTrack:
                        if (task.TryEvaluateAutomationActionParmeter<string>("VarName", _variableManager, out var rvalname))
                        {
                            _variableManager.ReleaseVariableTrack(rvalname);
                        }
                        break;

                    case AutomationActions.RedrawDynamicControls:
                        _dynamicControlProvider.Repaint();
                        _extraDynamicControlProvider.Repaint();
                        break;

                    case AutomationActions.None:
                        break;
                    case AutomationActions.OpsNote:
                        break;
                    default:
                        _logger.Debug($"(PerformAutomationAction) -- UNKNOWN ACTION {task.Action}");
                        break;
                }
            });
            return new ActionResult(TrackedActionState.Done, continueProcessing);
        }

        public void ProvideWatchInfo(ConditionWatchProvider watches)
        {
            GetWatches = watches;
        }

        public void UpdateDriver(IDeviceDriver driver)
        {
            // figure out what type of driver we actually have
            if (driver as ICCPUPresetMonitor != null)
            {
                _camPresets = driver as ICCPUPresetMonitor;
            }
        }
    }
}
