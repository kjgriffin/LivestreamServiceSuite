
using Integrated_Presenter.Presentation;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntegratedPresenter.Main
{
    public class Slide
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

        public void LoadActions(string folder)
        {
            if (Type == SlideType.Action)
            {
                try
                {
                    List<string> parts = new List<string>();
                    using (StreamReader sr = new StreamReader(Source))
                    {
                        string text = sr.ReadToEnd();
                        var commands = text.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => (s + ";").Trim());
                        parts = commands.ToList();
                    }
                    Title = "AUTO SEQ";
                    foreach (var part in parts)
                    {
                        // parse into commands
                        if (part == ";")
                        {

                        }
                        else if (part.StartsWith("!"))
                        {
                            if (part == "!fullauto;")
                            {
                                AutoOnly = true;
                            }
                            else if (part.StartsWith("!displaysrc="))
                            {
                                var m = Regex.Match(part, "!displaysrc='(?<fname>.*)';");
                                if (m.Success)
                                {
                                    if (File.Exists(Path.Combine(folder, m.Groups["fname"].Value)))
                                    {
                                        AltSources = true;
                                        AltSource = Path.Combine(folder, m.Groups["fname"].Value);
                                    }
                                }
                            }
                            else if (part.StartsWith("!keysrc="))
                            {
                                var m = Regex.Match(part, "!keysrc='(?<fname>.*)';");
                                if (m.Success)
                                {
                                    if (File.Exists(Path.Combine(folder, m.Groups["fname"].Value)))
                                    {
                                        AltSources = true;
                                        AltKeySource = Path.Combine(folder, m.Groups["fname"].Value);
                                    }
                                }
                            }
                        }
                        else if (part.StartsWith("@"))
                        {
                            var a = AutomationAction.Parse(part.Remove(0, 1));
                            var runType = a.Action == AutomationActions.OpsNote ? TrackedActionRunType.Note : TrackedActionRunType.Setup;
                            SetupActions.Add(new TrackedAutomationAction(a, runType));
                        }
                        else if (part.StartsWith("#"))
                        {
                            var title = Regex.Match(part, @"#(?<title>.*);").Groups["title"].Value;
                            Title = title;
                        }
                        else
                        {
                            var a = AutomationAction.Parse(part);
                            var runType = a.Action == AutomationActions.OpsNote ? TrackedActionRunType.Note : TrackedActionRunType.Setup;
                            Actions.Add(new TrackedAutomationAction(a, runType));
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
