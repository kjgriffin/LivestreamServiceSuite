using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Integrated_Presenter
{
    public class Presentation
    {

        public async void SuspendInProgressAutomation()
        {
            // cancel all current operations
            tokenSource.Cancel();
            // yield control for a ms to try and get the token cancellation to propogate
            await Task.Delay(10);
            tokenSource = new CancellationTokenSource();
            // set a new cancel token
        }

        public CancellationTokenSource tokenSource;

        public string Folder { get; set; }

        public List<Slide> Slides { get; set; } = new List<Slide>();

        public static Slide EmptySlide = new Slide() { Source = "", Type = SlideType.Empty };

        public bool Create(string folder)
        {
            tokenSource = new CancellationTokenSource();
            Slides.Clear();
            Folder = folder;

            // get all files in directory and hope they are slides
            var files = Directory.GetFiles(Folder).Where(f => Regex.Match(f, "\\d+_.*").Success).OrderBy(p => Convert.ToInt32(Regex.Match(Path.GetFileName(p), "(?<slidenum>\\d+).*").Groups["slidenum"].Value)).ToList();
            foreach (var file in files)
            {
                var filename = Regex.Match(Path.GetFileName(file), "\\d+_(?<type>[^-]*)-?(?<action>.*)\\.(?<extension>.*)");
                string name = filename.Groups["type"].Value;
                string action = filename.Groups["action"].Value;
                string extension = filename.Groups["extension"].Value;

                // skip unrecognized files
                List<string> valid = new List<string>() { "mp4", "png", "txt" };
                if (!valid.Contains(extension.ToLower()))
                {
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
                Slide s = new Slide() { Source = file, Type = type };
                if (s.Type == SlideType.Action)
                {
                    s.LoadActions();
                }
                Slides.Add(s);
            }

            // attach keyfiles to slides
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"Key_\d+").Success).ToList();
            foreach (var file in files)
            {
                var num = Regex.Match(file, @"Key_(?<num>\d+)").Groups["num"].Value;
                int snum = Convert.ToInt32(num);
                Slides[snum].KeySource = file;
            }

            // attach post-scripts to slides
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"Script_\d+").Success).ToList();
            foreach (var file in files)
            {
                // turn file into list of automation actions
                // add actions as post-actions to slide
                using (StreamReader sr = new StreamReader(file))
                {
                    var lines = sr.ReadToEnd().Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => (s + ";").Trim()).ToList();
                    foreach (var line in lines)
                    {
                        var num = Regex.Match(file, @"Script_(?<num>\d+)").Groups["num"].Value;
                        int snum = Convert.ToInt32(num);
                        AutomationAction action = AutomationAction.Parse(line);
                        Slides[snum].PostActions.Add(action);
                    }
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
            if (_virtualCurrentSlide + 1 < SlideCount)
            {
                _virtualCurrentSlide += 1;
                _currentSlide = _virtualCurrentSlide;
            }
            else
            {
                _virtualCurrentSlide = SlideCount - 1;
                _currentSlide = SlideCount - 1;
            }
            SuspendInProgressAutomation();
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
                _virtualCurrentSlide -= 1;
                _currentSlide = _virtualCurrentSlide;
            }
            else
            {
                _currentSlide = 0;
                _virtualCurrentSlide = 0;
            }
            SuspendInProgressAutomation();
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
            _currentSlide = 0;
            _virtualCurrentSlide = 0;
            SuspendInProgressAutomation();
        }

    }

    public class Slide
    {
        public SlideType Type { get; set; }
        public string Source { get; set; }
        public string KeySource { get; set; }
        //public string PreAction { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public List<AutomationAction> SetupActions { get; set; } = new List<AutomationAction>();
        public List<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
        public List<AutomationAction> PostActions { get; set; } = new List<AutomationAction>();
        public string Title { get; set; } = "";
        public bool AutoOnly { get; set; } = false;

        public void LoadActions()
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
                            if (part == ("!fullauto;"))
                            {
                                AutoOnly = true;
                            }
                        }
                        else if (part.StartsWith("@"))
                        {
                            var a = AutomationAction.Parse(part.Remove(0, 1));
                            SetupActions.Add(a);
                        }
                        else if (part.StartsWith("#"))
                        {
                            var title = Regex.Match(part, @"#(?<title>.*);").Groups["title"].Value;
                            Title = title;
                        }
                        else
                        {
                            var a = AutomationAction.Parse(part);
                            Actions.Add(a);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

    }

    public class AutomationAction
    {
        public AutomationActionType Action { get; set; } = AutomationActionType.None;
        public string Message { get; set; } = "";
        public int DataI { get; set; } = 0;
        public string DataS { get; set; } = "";


        public static AutomationAction Parse(string command)
        {
            AutomationAction a = new AutomationAction();
            a.Action = AutomationActionType.None;
            a.DataI = 0;
            a.DataS = "";
            a.Message = "";

            if (command.StartsWith("arg0:"))
            {
                var res = Regex.Match(command, @"arg0:(?<commandname>.*?)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                string msg = res.Groups["msg"].Value;
                a.Message = msg;
                switch (cmd)
                {
                    case "AutoTrans":
                        a.Action = AutomationActionType.AutoTrans;
                        break;
                    case "CutTrans":
                        a.Action = AutomationActionType.CutTrans;
                        break;
                    case "AutoTakePresetIfOnSlide":
                        a.Action = AutomationActionType.AutoTakePresetIfOnSlide;
                        break;

                    case "DSK1On":
                        a.Action = AutomationActionType.DSK1On;
                        break;
                    case "DSK1Off":
                        a.Action = AutomationActionType.DSK1Off;
                        break;
                    case "DSK1FadeOn":
                        a.Action = AutomationActionType.DSK1FadeOn;
                        break;
                    case "DSK1FadeOff":
                        a.Action = AutomationActionType.DSK1FadeOff;
                        break;

                    case "DSK2On":
                        a.Action = AutomationActionType.DSK2On;
                        break;
                    case "DSK2Off":
                        a.Action = AutomationActionType.DSK2Off;
                        break;
                    case "DSK2FadeOn":
                        a.Action = AutomationActionType.DSK2FadeOn;
                        break;
                    case "DSK2FadeOff":
                        a.Action = AutomationActionType.DSK2FadeOff;
                        break;

                    case "USK1On":
                        a.Action = AutomationActionType.USK1On;
                        break;
                    case "USK1Off":
                        a.Action = AutomationActionType.USK1Off;
                        break;
                    case "USK1SetTypeChroma":
                        a.Action = AutomationActionType.USK1SetTypeChroma;
                        break;
                    case "USK1SetTypeDVE":
                        a.Action = AutomationActionType.USK1SetTypeDVE;
                        break;

                    case "RecordStart":
                        a.Action = AutomationActionType.RecordStart;
                        break;
                    case "RecordStop":
                        a.Action = AutomationActionType.RecordStop;
                        break;

                    case "OpenAudioPlayer":
                        a.Action = AutomationActionType.OpenAudioPlayer;
                        break;
                    case "PlayAuxAudio":
                        a.Action = AutomationActionType.PlayAuxAudio;
                        break;
                    case "StopAuxAudio":
                        a.Action = AutomationActionType.StopAuxAudio;
                        break;
                    case "PauseAuxAudio":
                        a.Action = AutomationActionType.PauseAuxAudio;
                        break;
                    case "ReplayAuxAudio":
                        a.Action = AutomationActionType.ReplayAuxAudio;
                        break;

                    case "PlayMedia":
                        a.Action = AutomationActionType.PlayMedia;
                        break;
                    case "PauseMedia":
                        a.Action = AutomationActionType.PauseMedia;
                        break;
                    case "StopMedia":
                        a.Action = AutomationActionType.StopMedia;
                        break;
                    case "RestartMedia":
                        a.Action = AutomationActionType.RestartMedia;
                        break;
                    case "MuteMedia":
                        a.Action = AutomationActionType.MuteMedia;
                        break;
                    case "UnMuteMedia":
                        a.Action = AutomationActionType.UnMuteMedia;
                        break;

                    case "DriveNextSlide":
                        a.Action = AutomationActionType.DriveNextSlide;
                        break;
                    case "Timer1Restart":
                        a.Action = AutomationActionType.Timer1Restart;
                        break;
                }
            }
            if (command.StartsWith("arg1:"))
            {
                var res = Regex.Match(command, @"arg1:(?<commandname>.*?)\((?<param>.*)\)(\[(?<msg>.*)\])?;");
                string cmd = res.Groups["commandname"].Value;
                string arg1 = res.Groups["param"].Value;
                string msg = res.Groups["msg"].Value;
                a.Message = msg;
                switch (cmd)
                {
                    case "PresetSelect":
                        a.Action = AutomationActionType.PresetSelect;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "ProgramSelect":
                        a.Action = AutomationActionType.ProgramSelect;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "AuxSelect":
                        a.Action = AutomationActionType.AuxSelect;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "DelayMs":
                        a.Action = AutomationActionType.DelayMs;
                        a.DataI = Convert.ToInt32(arg1);
                        break;
                    case "LoadAudioFile":
                        a.Action = AutomationActionType.LoadAudio;
                        a.DataS = arg1;
                        break;
                    default:
                        break;
                }
            }


            return a;
        }

    }

    public enum AutomationActionType
    {
        PresetSelect,
        ProgramSelect,

        AutoTrans,
        CutTrans,

        DSK1On,
        DSK1Off,
        DSK1FadeOn,
        DSK1FadeOff,

        DSK2On,
        DSK2Off,
        DSK2FadeOn,
        DSK2FadeOff,

        USK1Off,
        USK1On,

        USK1SetTypeChroma,
        USK1SetTypeDVE,

        RecordStart,
        RecordStop,

        AutoTakePresetIfOnSlide,


        OpenAudioPlayer,
        LoadAudio,
        PlayAuxAudio,
        StopAuxAudio,
        PauseAuxAudio,
        ReplayAuxAudio,


        DelayMs,

        None,
        DriveNextSlide,
        Timer1Restart,
        PlayMedia,
        PauseMedia,
        StopMedia,
        RestartMedia,
        MuteMedia,
        UnMuteMedia,
        AuxSelect,
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
