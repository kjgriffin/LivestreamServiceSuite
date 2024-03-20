using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using IntegratedPresenterAPIInterop;

using SharedPresentationAPI.Presentation;

using SwitcherControl.BMDSwitcher;

using System.Collections.Generic;
using System.Threading.Tasks;

using VariableMarkupAttributes.Attributes;

namespace Integrated_Presenter.Automation
{

    internal interface IExtraDynamicControlProvider
    {
        void ConfigureControls(string extraID, string file, string resourcepath, bool overwriteAll);
        void Repaint();
    }

    internal interface IDynamicControlProvider
    {
        void ConfigureControls(string file, string resourcepath, bool overwriteAll);
        void Repaint();
    }
    internal interface ISwitcherDriverProvider : ISwitcherStateProvider
    {
        IBMDSwitcherManager switcherManager { get; }
    }
    public interface ISwitcherStateProvider
    {
        BMDSwitcherState switcherState { get; }
    }
    internal interface IAutoTransitionProvider
    {
        void PerformGuardedAutoTransition();
    }
    internal interface IAutomationConditionProvider
    {
        Dictionary<string, bool> GetConditionals(Dictionary<string, WatchVariable> watches);
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

        ISlide GetCurentSlide();
        void SetNextSlideTarget(int target);
        Task TakeNextSlide();
        Dictionary<string, ExposedVariable> GetExposedVariables();
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

    internal interface IUserConditionProvider
    {
        Dictionary<string, bool> GetActiveUserConditions();
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
