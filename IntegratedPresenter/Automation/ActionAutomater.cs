using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;

using log4net;

using MIDI_DEBUGGER.MidiDriver;

using SwitcherControl.BMDSwitcher;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Integrated_Presenter.Automation
{

    internal interface ISwitcherDriverProvider
    {
        IBMDSwitcherManager switcherManager { get; }
        BMDSwitcherState switcherState { get; }
    }
    internal interface IAutoTransitionProvider
    {
        void PerformGuardedAutoTransition();
    }
    internal interface IAutomationConditionProvider
    {
        Dictionary<string, bool> GetCurrentConditionStatus();
    }
    internal interface IConfigProvider
    {
        BMDSwitcherConfigSettings _config { get; }
    }
    internal interface IFeatureFlagProvider
    {
        bool AutomationTimer1Enabled { get; }
    }
    internal interface IUserTimerProvider
    {
        void ResetGpTimer1();
    }
    internal interface IMainUIProvider
    {
        void Focus();
    }
    internal interface IPresentationProvider
    {
        string Folder { get; }
        void SetNextSlideTarget(int target);
        Task TakeNextSlide();
    }
    internal interface IAudioDriverProvider
    {
        void OpenAudioPlayer();
        void RestartAudio();
        void PauseAudio();
        void StopAudio();
        void PlayAudio();
        void OpenAudio(string filename);
    }
    internal interface IMediaDriverProvider
    {
        void unmuteMedia();
        void muteMedia();
        void restartMedia();
        void stopMedia();
        void pauseMedia();
        void playMedia();

    }


    class ActionResult
    {
        public ActionResult(TrackedActionState state, bool continueProcessingAutomation = true)
        {
            this.ActionState = state;
            this.ContinueOtherActions = continueProcessingAutomation;
        }

        internal TrackedActionState ActionState { get; set; }
        internal bool ContinueOtherActions { get; set; }
    }




    internal class ActionAutomater
    {

        ILog _logger;
        ISwitcherDriverProvider _switcherProvider;
        IAutoTransitionProvider _autoTransitionProvider;
        IAutomationConditionProvider _automationConditionProvider;
        IConfigProvider _configProvider;
        IFeatureFlagProvider _featureFlagProvider;
        IUserTimerProvider _userTimerProvider;
        IMainUIProvider _mainUIProvider;
        IPresentationProvider _presentationProvider;
        IAudioDriverProvider _audioDriverProvider;
        IMediaDriverProvider _mediaDriverProvider;


        ISQDriver _midiDriver;


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
                                 IMediaDriverProvider mediaDriverProvider)
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

            // hard code this for now...
            _midiDriver = new SQDriver(0, 1, 1);
        }

        internal async Task ExecuteSetupActions(ISlide s)
        {
            _logger.Debug($"Begin Execution of setup actions for slide {s.Title}");
            await Task.Run(async () =>
            {
                bool keepProcessing = true;
                foreach (var task in s.SetupActions)
                {
                    if (keepProcessing)
                    {
                        s.FireOnActionStateChange(task.ID, TrackedActionState.Started);
                        var res = await PerformAutomationAction(task.Action);
                        s.FireOnActionStateChange(task.ID, res.ActionState);
                        keepProcessing = res.ContinueOtherActions;
                    }
                    else
                    {
                        s.FireOnActionStateChange(task.ID, TrackedActionState.Skipped);
                    }
                }
            });
            _logger.Debug($"Completed Execution of setup actions for slide {s.Title}");

        }

        internal async Task ExecuteMainActions(ISlide s)
        {
            _logger.Debug($"Begin Execution of actions for slide {s.Title}");
            await Task.Run(async () =>
            {
                bool keepProcessing = true;
                foreach (var task in s.Actions)
                {
                    if (keepProcessing)
                    {
                        s.FireOnActionStateChange(task.ID, TrackedActionState.Started);
                        var res = await PerformAutomationAction(task.Action);
                        s.FireOnActionStateChange(task.ID, res.ActionState);
                        keepProcessing = res.ContinueOtherActions;
                    }
                    else
                    {
                        s.FireOnActionStateChange(task.ID, TrackedActionState.Skipped);
                    }
                }
            });
            _logger.Debug($"Completed Execution of actions for slide {s.Title}");

        }

        internal async Task<ActionResult> PerformAutomationAction(AutomationAction task)
        {
            if (!task.MeetsConditionsToRun(_automationConditionProvider.GetCurrentConditionStatus()))
            {
                return new ActionResult(TrackedActionState.Skipped);
            }
            bool continueProcessing = true;
            await Task.Run(async () =>
            {
                switch (task.Action)
                {
                    case AutomationActions.PresetSelect:
                        _logger.Debug($"(PerformAutomationAction) -- Preset Select {task.DataI}");
                        _switcherProvider?.switcherManager?.PerformPresetSelect(task.DataI);
                        break;
                    case AutomationActions.ProgramSelect:
                        _logger.Debug($"(PerformAutomationAction) -- Program Select {task.DataI}");
                        _switcherProvider?.switcherManager?.PerformProgramSelect(task.DataI);
                        break;
                    case AutomationActions.AuxSelect:
                        _logger.Debug($"(PerformAutomationAction) -- Aux Select {task.DataI}");
                        _switcherProvider?.switcherManager?.PerformAuxSelect(task.DataI);
                        break;
                    case AutomationActions.USK1Fill:
                        _logger.Debug($"(PerformAutomationAction) -- USK1Fill {task.DataI}");
                        _switcherProvider?.switcherManager?.PerformUSK1FillSourceSelect(task.DataI);
                        break;
                    case AutomationActions.PlacePIP:
                        _logger.Debug($"(PerformAutomationAction) -- PlacePIP. read cfg");
                        PIPPlaceSettings cfg = task?.DataO as PIPPlaceSettings;
                        if (cfg != null)
                        {
                            _logger.Debug($"(PerformAutomationAction) -- PlacePIP at {cfg.ToString()}");
                            var current = _switcherProvider?.switcherManager?.GetCurrentState().DVESettings;
                            var config = cfg.PlaceOverride(current);
                            _switcherProvider?.switcherManager?.SetPIPPosition(config);
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

                    case AutomationActions.OpenAudioPlayer:
                        _logger.Debug($"(PerformAutomationAction) -- Opened Audio Player");
                        _audioDriverProvider.OpenAudioPlayer();
                        _mainUIProvider.Focus();
                        break;
                    case AutomationActions.LoadAudio:
                        string filename = Path.Join(_presentationProvider.Folder, task.DataS);
                        _logger.Debug($"(PerformAutomationAction) -- Load Audio File {filename} to Aux Player");
                        _audioDriverProvider.OpenAudio(filename);
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
                        _logger.Debug($"(PerformAutomationAction) -- delay for {task.DataI} ms");
                        await Task.Delay(task.DataI);
                        break;
                    case AutomationActions.DelayUntil:
                        {
                            _logger.Debug($"(PerformAutomationAction) -- delay until {task.DataS}");

                            // parse time as date
                            if (DateTime.TryParse(task.DataS, out var time))
                            {
                                // compute time to wait
                                DateTime now = DateTime.Now;
                                TimeSpan diff = time - now;
                                if (diff.TotalMilliseconds > 0)
                                {
                                    await Task.Delay((int)diff.TotalMilliseconds);
                                }
                            }

                            break;
                        }

                    case AutomationActions.JumpToSlide:
                        _logger.Debug($"(PerformAutomationAction) -- jump to slide {task.DataI}");
                        continueProcessing = false;
                        _presentationProvider.SetNextSlideTarget(task.DataI);
                        await _presentationProvider.TakeNextSlide();
                        break;

                    case AutomationActions.DriveNextSlide:
                        _logger.Debug($"(PerformAutomationAction) -- drive next slide");
                        continueProcessing = false;
                        // calculate next slide
                        _presentationProvider.SetNextSlideTarget(task.DataI);
                        await _presentationProvider.TakeNextSlide();
                        break;


                    case AutomationActions.WatchSwitcherStateBoolVal:
                    case AutomationActions.WatchSwitcherStateIntVal:
                        // TODO: do we need to process these as such??
                        // or let them be handled as dynamic conditions
                        break;

                    case AutomationActions.MIDISetMute:
                        string ch = (string)task.RawParams[0];
                        bool state = (bool)task.RawParams[1];
                        _logger.Debug($"(PerformAutomationAction) -- midi set mute {ch} {state}");
                        _midiDriver?.SetMute(ch, state);
                        break;
                    case AutomationActions.MIDISetLevel:
                        string chsrc = (string)task.RawParams[0];
                        string chdst = (string)task.RawParams[1];
                        int level = (int)task.RawParams[2];
                        _logger.Debug($"(PerformAutomationAction) -- midi set level {chsrc}->{chdst} {level}");
                        _midiDriver?.SetLevelABS(chsrc, chdst, level);
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

    }
}
