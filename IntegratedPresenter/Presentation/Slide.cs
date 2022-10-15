
using BMDSwitcherAPI;

using Integrated_Presenter.Presentation;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace IntegratedPresenter.Main
{
    public class Slide : ISlide
    {
        public SlideType Type { get; set; }
        public bool AutomationEnabled { get; set; } = true;
        public string Source { get; set; }
        public string KeySource { get; set; }

        public bool AltSources { get; set; } = false;
        public string AltSource { get; set; }
        public string AltKeySource { get; set; }

        public string PreAction { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public List<TrackedAutomationAction> SetupActions { get; set; } = new List<TrackedAutomationAction>();
        public List<TrackedAutomationAction> Actions { get; set; } = new List<TrackedAutomationAction>();
        public List<IPilotAction> AutoPilotActions { get; set; } = new List<IPilotAction>();
        public List<IPilotAction> EmergencyActions { get; set; } = new List<IPilotAction>();
        public string Title { get; set; } = "";
        public bool AutoOnly { get; set; } = false;

        public int PresetId { get; set; }
        public bool PostsetEnabled { get; set; } = false;
        public int PostsetId { get; set; }

        public bool TryGetPrimaryImage(out BitmapImage img)
        {
            img = null;
            if (AltSources)
            {
                if (!string.IsNullOrEmpty(AltSource))
                {
                    img = new BitmapImage(new Uri(AltSource));
                    return true;
                }
            }
            if (!string.IsNullOrEmpty(Source) && HasPrimaryImage())
            {
                img = new BitmapImage(new Uri(Source));
                return true;
            }
            return false;
        }
        public bool TryGetKeyImage(out BitmapImage img)
        {
            img = null;
            if (AltSources)
            {
                if (!string.IsNullOrEmpty(AltKeySource))
                {
                    img = new BitmapImage(new Uri(AltKeySource));
                    return true;
                }
            }
            if (!string.IsNullOrEmpty(KeySource))
            {
                img = new BitmapImage(new Uri(KeySource));
                return true;
            }
            return false;
        }

        public bool TryGetPrimaryVideoPath(out string path)
        {
            if (Type == SlideType.ChromaKeyVideo || Type == SlideType.Video)
            {
                path = Source;
                return true;
            }
            else if (Type == SlideType.Action && AltSources)
            {
                if (AltSource?.EndsWith(".mp4") == true)
                {
                    path = AltSource;
                    return true;
                }
            }
            path = string.Empty;
            return false;
        }
        public bool TryGetKeyVideoPath(out string path)
        {
            if (KeySource?.EndsWith(".mp4") == true)
            {
                path = AltSource;
                return true;
            }
            else if (AltKeySource?.EndsWith(".mp4") == true)
            {
                path = AltKeySource;
                return true;
            }
            path = string.Empty;
            return false;
        }


        public bool HasPrimaryImage()
        {
            if (AltSources)
            {
                if (!string.IsNullOrEmpty(AltSource))
                {
                    return true;
                }
            }
            if (!string.IsNullOrEmpty(Source) && Type != SlideType.Action)
            {
                return true;
            }
            return false;
        }
        public bool HasKeyImage()
        {
            if (AltSources)
            {
                if (!string.IsNullOrEmpty(AltKeySource))
                {
                    return true;
                }
            }
            if (!string.IsNullOrEmpty(KeySource))
            {
                return true;
            }
            return false;
        }


        public void FireOnActionStateChange(Guid ActionID, TrackedActionState newState)
        {
            var action = SetupActions.FirstOrDefault(a => a.ID == ActionID) ?? Actions.FirstOrDefault(a => a.ID == ActionID);
            if (action == null)
            {
                return;
            }
            action.State = newState;
            OnActionUpdated?.Invoke(action);
        }

        public void ResetAllActionsState()
        {
            foreach (var action in SetupActions)
            {
                action.State = TrackedActionState.Ready;
            }
            foreach (var action in Actions)
            {
                action.State = TrackedActionState.Ready;
            }
        }

        public event AutomationActionUpdateEventArgs OnActionUpdated;


        public void LoadActions_Common(string folder)
        {
            if (Type == SlideType.Action)
            {
                try
                {
                    string text;
                    using (StreamReader sr = new StreamReader(Source))
                    {
                        text = sr.ReadToEnd();
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        var loaded = ActionLoader.LoadActions(text);
                        Title = loaded.Title;
                        AutoOnly = loaded.AutoOnly;
                        if (loaded.AltSources)
                        {
                            bool srcoverrideError = false;
                            string srcfilename = "";
                            if (!string.IsNullOrEmpty(loaded.AltSourceFileName))
                            {
                                srcfilename = Path.Combine(folder, loaded.AltSourceFileName);
                                if (!File.Exists(srcfilename))
                                {
                                    srcoverrideError = true;
                                }
                            }
                            string keyfilename = "";
                            if (!string.IsNullOrEmpty(loaded.AltKeySourceFileName))
                            {
                                keyfilename = Path.Combine(folder, loaded.AltKeySourceFileName);
                                if (File.Exists(keyfilename))
                                {
                                    srcoverrideError = true;
                                }
                            }
                            if (!srcoverrideError)
                            {
                                AltSources = true;
                                AltSource = srcfilename;
                                AltKeySource = keyfilename;
                            }
                        }
                        foreach (var action in loaded.SetupActions)
                        {
                            var runType = action.Action == AutomationActions.OpsNote ? TrackedActionRunType.Note : TrackedActionRunType.Setup;
                            SetupActions.Add(new TrackedAutomationAction(action, runType));
                        }
                        foreach (var action in loaded.MainActions)
                        {
                            var runType = action.Action == AutomationActions.OpsNote ? TrackedActionRunType.Note : TrackedActionRunType.Setup;
                            Actions.Add(new TrackedAutomationAction(action, runType));
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        


    }

}
