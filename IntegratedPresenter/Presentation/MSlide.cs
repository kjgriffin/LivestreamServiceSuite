
using BMDSwitcherAPI;

using CommonGraphics;

using Integrated_Presenter.Presentation;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace IntegratedPresenter.Main
{
    public class MSlide : ISlide
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

        //public int PresetId { get; set; }
        public bool PostsetEnabled { get; set; } = false;
        public int PostsetId { get; set; }

        public bool TryGetPrimaryImage(out BitmapImage img)
        {
            img = new BitmapImage();
            if (Type == SlideType.Video)
            {
                return false;
            }
            if (AltSources)
            {
                if (!string.IsNullOrEmpty(AltSource))
                {
                    MemoryStream bs = new MemoryStream();
                    using (var f = MemoryMappedFile.OpenExisting(AltSource))
                    using (var stream = f.CreateViewStream())
                    {
                        stream.CopyTo(bs);
                        img = bs.ToBitmapImage();
                    }

                    return true;
                }
            }
            if (!string.IsNullOrEmpty(Source))
            {
                MemoryStream bs = new MemoryStream();
                using (var f = MemoryMappedFile.OpenExisting(Source))
                using (var stream = f.CreateViewStream())
                {
                    stream.CopyTo(bs);
                    img = bs.ToBitmapImage();
                }

                return true;
            }
            img = null;
            return false;
        }
        public bool TryGetKeyImage(out BitmapImage img)
        {
            img = new BitmapImage();
            if (AltSources)
            {
                if (!string.IsNullOrEmpty(AltKeySource))
                {
                    MemoryStream bs = new MemoryStream();
                    using (var f = MemoryMappedFile.OpenExisting(AltKeySource))
                    using (var stream = f.CreateViewStream())
                    {
                        stream.CopyTo(bs);
                        img = bs.ToBitmapImage();
                    }

                    return true;
                }
            }
            if (!string.IsNullOrEmpty(KeySource))
            {
                MemoryStream bs = new MemoryStream();
                using (var f = MemoryMappedFile.OpenExisting(KeySource))
                using (var stream = f.CreateViewStream())
                {
                    stream.CopyTo(bs);
                    img = bs.ToBitmapImage();
                }

                return true;
            }
            img = null;
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
            if (!string.IsNullOrEmpty(Source))
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
