using IntegratedPresenter.BMDSwitcher.Config;

using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IntegratedPresenter.Main
{
    public class Presentation
    {

        public bool HasSwitcherConfig { get; private set; } = false;
        public BMDSwitcher.Config.BMDSwitcherConfigSettings SwitcherConfig { get; private set; }

        public bool HasUserConfig { get; private set; } = false;
        public Configurations.FeatureConfig.IntegratedPresenterFeatures UserConfig { get; private set; }

        public string Folder { get; set; }

        public List<Slide> Slides { get; set; } = new List<Slide>();

        public static Slide EmptySlide = new Slide() { Source = "", Type = SlideType.Empty };

        public bool Create(string folder)
        {
            Slides.Clear();
            Folder = folder;

            // get all files in directory and hope they are slides
            var files = Directory.GetFiles(Folder).Where(f => Regex.Match(f, "\\d+_.*").Success).OrderBy(p => Convert.ToInt32(Regex.Match(Path.GetFileName(p), "(?<slidenum>\\d+).*").Groups["slidenum"].Value)).ToList();
            foreach (var file in files)
            {
                var filename = Regex.Match(Path.GetFileName(file), @"\d+_(?<type>[^-\.]+)(-(?<action>[^\.]+))?(?<drive>\.nodrive)?\.(?<extension>.*)");
                string name = filename.Groups["type"].Value;
                string action = filename.Groups["action"].Value;
                string drive = filename.Groups["drive"].Value;
                string extension = filename.Groups["extension"].Value;

                // skip unrecognized files
                List<string> valid = new List<string>() { "mp4", "png", "txt" };
                if (!valid.Contains(extension.ToLower()))
                {
                    continue;
                }
                if (Regex.Match(Path.GetFileName(file), "Resource_.*").Success)
                {
                    // ignore all resources files.
                    // these should be refered to by 'real' slides that need them
                    continue;
                }

                SlideType type;
                // look at the name to determine the type
                switch (name)
                {
                    case "Full":
                        type = SlideType.Full;
                        break;
                    case "Liturgy":
                        type = SlideType.Liturgy;
                        break;
                    case "Video":
                        type = SlideType.Video;
                        break;
                    case "ChromaKeyVideo":
                        type = SlideType.ChromaKeyVideo;
                        break;
                    case "ChromaKeyStill":
                        type = SlideType.ChromaKeyStill;
                        break;
                    case "Action":
                        type = SlideType.Action;
                        break;
                    default:
                        type = SlideType.Empty;
                        break;
                }
                Slide s = new Slide() { Source = file, Type = type, PreAction = action };
                if (drive == ".nodrive")
                {
                    s.AutomationEnabled = false;
                }
                if (s.Type == SlideType.Action)
                {
                    s.LoadActions(folder);
                }
                Slides.Add(s);
            }

            // attack keyfiles to slides
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"Key_\d+").Success).ToList();
            foreach (var file in files)
            {
                var num = Regex.Match(file, @"Key_(?<num>\d+)").Groups["num"].Value;
                int snum = Convert.ToInt32(num);
                Slides[snum].KeySource = file;
            }

            // attach postshot to slides
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"Postset_\d+").Success).ToList();
            foreach (var file in files)
            {
                var num = Regex.Match(file, @"Postset_(?<num>\d+)").Groups["num"].Value;
                int snum = Convert.ToInt32(num);
                try
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        var n = sr.ReadToEnd();
                        int sid = Convert.ToInt32(n);
                        Slides[snum].PostsetId = sid;
                        Slides[snum].PostsetEnabled = true;
                    }
                }
                catch (Exception)
                {
                }
            }

            // find switcher config file if it exists
            var configfile = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"BMDSwitcherConfig").Success).FirstOrDefault();

            if (File.Exists(configfile))
            {
                SwitcherConfig = BMDSwitcherConfigSettings.Load(configfile);
                HasSwitcherConfig = true;
            }

            // find user config file if it exists
            configfile = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"IntegratedPresenterUserConfig").Success).FirstOrDefault();

            if (File.Exists(configfile))
            {
                using (StreamReader sr = new StreamReader(configfile))
                {
                    var cfg = sr.ReadToEnd();
                    UserConfig = JsonSerializer.Deserialize<Configurations.FeatureConfig.IntegratedPresenterFeatures>(cfg);
                    HasUserConfig = true;
                }
            }


            return false;
        }


        public int SlideCount { get => Slides.Count; }

        public int CurrentSlide { get => _currentSlide + 1; }

        private int _currentSlide = 0;
        private int _virtualCurrentSlide = 0;

        public Slide Prev
        {
            get
            {
                if (_virtualCurrentSlide - 1 >= 0)
                {
                    return Slides[_virtualCurrentSlide - 1];
                }
                else
                {
                    return EmptySlide;
                }
            }
        }


        public Slide Override { get; set; }
        public bool OverridePres { get; set; } = false;

        public Slide EffectiveCurrent
        {
            get
            {
                if (OverridePres && Override != null)
                {
                    return Override;
                }
                return Current;
            }
        }

        public Slide Current { get => Slides[_currentSlide]; }
        public Slide Next
        {
            get
            {
                if (_virtualCurrentSlide + 1 < SlideCount)
                {
                    return Slides[_virtualCurrentSlide + 1];
                }
                else
                {
                    return EmptySlide;
                }
            }
        }
        public Slide After
        {
            get
            {
                if (_virtualCurrentSlide + 2 < SlideCount)
                {
                    return Slides[_virtualCurrentSlide + 2];
                }
                else
                {
                    return EmptySlide;
                }
            }
        }

        public void NextSlide()
        {
            Prev?.ResetAllActionsState();
            if (_virtualCurrentSlide + 1 < SlideCount)
            {
                if (Math.Abs((_currentSlide + 1) - _virtualCurrentSlide) != 1)
                {
                    // we're skipping around in time
                    // reset current slide
                    EffectiveCurrent.ResetAllActionsState();
                }

                _virtualCurrentSlide += 1;
                _currentSlide = _virtualCurrentSlide;
            }
            else
            {
                _virtualCurrentSlide = SlideCount - 1;
                _currentSlide = SlideCount - 1;
            }
        }

        public void SkipNextSlide()
        {
            if (_virtualCurrentSlide + 1 < SlideCount)
            {
                _virtualCurrentSlide += 1;
            }
        }

        public void PrevSlide()
        {
            if (_virtualCurrentSlide - 1 >= 0)
            {
                EffectiveCurrent.ResetAllActionsState();
                Prev.ResetAllActionsState();
                _virtualCurrentSlide -= 1;
                _currentSlide = _virtualCurrentSlide;
            }
            else
            {
                _currentSlide = 0;
                _virtualCurrentSlide = 0;
            }
        }

        public void SkipPrevSlide()
        {
            if (_virtualCurrentSlide - 1 >= 0)
            {
                _virtualCurrentSlide -= 1;
            }
        }

        public void StartPres()
        {
            // need to reset all slides
            foreach (var s in Slides)
            {
                s?.ResetAllActionsState();
            }
            //EffectiveCurrent?.ResetAllActionsState();
            _currentSlide = 0;
            _virtualCurrentSlide = 0;
        }

    }


    public enum TrackedActionState
    {
        Ready,
        Started,
        Done,
        Skipped,
    }

    public enum TrackedActionRunType
    {
        Setup,
        Main,
        Note,
    }

    public class TrackedAutomationAction
    {
        public Guid ID { get; }
        public TrackedActionState State { get; set; }
        public TrackedActionRunType RunType { get; set; }
        public AutomationAction Action { get; set; }

        public TrackedAutomationAction(AutomationAction action, TrackedActionRunType runType)
        {
            Action = action;
            State = TrackedActionState.Ready;
            RunType = runType;
            ID = Guid.NewGuid();
        }

    }

    public delegate void AutomationActionUpdateEventArgs(TrackedAutomationAction updatedAction);


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


    public enum SlideType
    {
        Full,
        Liturgy,
        Video,
        ChromaKeyVideo,
        ChromaKeyStill,
        Action,
        Empty
    }

}
