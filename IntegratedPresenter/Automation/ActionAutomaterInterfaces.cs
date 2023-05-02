using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;

using SwitcherControl.BMDSwitcher;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrated_Presenter.Automation
{

    internal interface IDynamicControlProvider
    {
        void ConfigureControls(string file, string resourcepath);
    }
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
        Dictionary<string, bool> GetCurrentConditionStatus(Dictionary<string, WatchVariable> watches);
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
    internal delegate Dictionary<string, WatchVariable> ConditionWatchProvider();
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
}
