using CCU.Config;

using Configurations.FeatureConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using IntegratedPresenterAPIInterop;

using VariableMarkupAttributes.Attributes;

namespace SharedPresentationAPI.Presentation
{
    [ExposesWatchableVariables]
    public interface IPresentation
    {
        ISlide After { get; }
        CCPUConfig CCPUConfig { get; }
        ISlide Current { get; }
        [ExposedAsVariable(nameof(CurrentSlide))]
        int CurrentSlide { get; }
        ISlide EffectiveCurrent { get; }
        string Folder { get; set; }
        bool HasCCUConfig { get; }
        bool HasSwitcherConfig { get; }
        bool HasUserConfig { get; }
        ISlide Next { get; }
        ISlide Override { get; set; }
        bool OverridePres { get; set; }
        ISlide Prev { get; }
        int SlideCount { get; }
        List<ISlide> Slides { get; set; }
        BMDSwitcherConfigSettings SwitcherConfig { get; }
        IntegratedPresenterFeatures UserConfig { get; }
        Dictionary<string, WatchVariable> WatchedVariables { get; }
        HashSet<string> OwnedVariables { get; }
        Dictionary<string, string> RawTextResources { get; }
        bool Create(string folder);
        void NextSlide();
        void PrevSlide();
        void SetNextSlideJump(int target);
        void SkipNextSlide();
        void SkipPrevSlide();
        void StartPres(int snum = 0);

    }
}