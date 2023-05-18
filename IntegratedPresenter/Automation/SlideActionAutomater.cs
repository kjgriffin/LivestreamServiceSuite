using CCUI_UI;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;

using log4net;

using SharedPresentationAPI.Presentation;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Integrated_Presenter.Automation
{
    internal class SlideActionAutomater : ActionAutomater, ISlideScriptActionAutomater
    {

        internal SlideActionAutomater(ILog logger,
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
                                 ICCPUPresetMonitor camPresetProvider)
            : base(logger, switcherProvider, autoTransitionProvider, automationConditionProvider, configProvider, featureFlagProvider, userTimerProvider, mainUIProvider, presentationProvider, audioDriverProvider, mediaDriverProvider, watchProvider, dynamicControlProvider, camPresetProvider)
        {
        }

        public async Task ExecuteSetupActions(ISlide s)
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

        public async Task ExecuteMainActions(ISlide s)
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
    }
}
