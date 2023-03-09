using Integrated_Presenter.Presentation;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace IntegratedPresenter.Main
{
    public interface ISlide
    {
        List<TrackedAutomationAction> Actions { get; set; }

        bool TryGetPrimaryImage(out BitmapImage img);
        bool TryGetKeyImage(out BitmapImage img);

        bool TryGetPrimaryVideoPath(out string path);
        bool TryGetKeyVideoPath(out string path);

        bool IsControllableMedia();

        //string AltKeySource { get; set; }
        //string AltSource { get; set; }
        //bool AltSources { get; set; }

        bool AutomationEnabled { get; set; }
        bool AutoOnly { get; set; }
        List<IPilotAction> AutoPilotActions { get; set; }
        List<IPilotAction> EmergencyActions { get; set; }
        Guid Guid { get; set; }
        //string KeySource { get; set; }
        bool PostsetEnabled { get; set; }
        int PostsetId { get; set; }
        string PreAction { get; set; }
        //int PresetId { get; set; }
        List<TrackedAutomationAction> SetupActions { get; set; }
        //string Source { get; set; }
        string Title { get; set; }
        SlideType Type { get; set; }

        event AutomationActionUpdateEventArgs OnActionUpdated;

        void FireOnActionStateChange(Guid ActionID, TrackedActionState newState);
        //void LoadActions(string folder);
        //void LoadActions_Common(string folder);
        void ResetAllActionsState();
    }

    public enum SlideLocationStyle
    {
        PHYSICAL,
        MEMORY,
    }

}