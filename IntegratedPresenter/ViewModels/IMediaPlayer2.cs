using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;

using IntegratedPresenterAPIInterop;

using SharedPresentationAPI.Presentation;

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace IntegratedPresenter.Main
{
    public interface IMediaPlayer2
    {
        bool AutoSilentPlayback { get; set; }
        bool AutoSilentReplay { get; set; }
        TimeSpan MediaLength { get; }
        TimeSpan MediaTimeRemaining { get; }
        bool ShowAutomationPreviews { get; set; }
        bool ShowBlackForActions { get; set; }
        bool ShowIfMute { get; set; }
        bool ShowPostset { get; set; }

        event EventHandler<MediaPlaybackTimeEventArgs> OnMediaLoaded;
        event EventHandler<MediaPlaybackTimeEventArgs> OnMediaPlaybackTimeUpdate;

        void ChangeShowAutomationPreviews();
        void FireOnActionConditionsUpdated(Dictionary<string, bool> conditionals);
        void FireOnSwitcherStateChangedForAutomation(BMDSwitcherState state, BMDSwitcherConfigSettings config);
        void InitializeComponent();
        void MarkMuted();
        void MarkUnMuted();
        void MuteAudioPlayback();
        void PauseMedia();
        void PlayMedia();
        void ReplayMedia();
        void SetMedia(BitmapImage img, SlideType type);
        void SetMedia(ISlide slide, bool asKey);
        void SetMedia(Uri source, SlideType type);
        void SetMVConfigForPostset(BMDMultiviewerSettings config);
        void SetPostset(int id);
        void ShowBlackSource();
        void ShowHideShortcuts(bool show);
        void ShowTileBackground(bool show = true);
        void StopMedia();
        void UnMuteAudioPlayback();
    }
}