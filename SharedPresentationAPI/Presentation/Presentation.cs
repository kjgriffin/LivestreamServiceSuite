﻿using CCU.Config;

using IntegratedPresenter.BMDSwitcher.Config;

using IntegratedPresenterAPIInterop;

using SharedPresentationAPI.Presentation;

using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

using VariableMarkupAttributes;
using VariableMarkupAttributes.Attributes;

namespace SharedPresentationAPI.Presentation
{


    [ExposesWatchableVariables]
    public class Presentation : IPresentation
    {

        public bool HasSwitcherConfig { get; internal set; } = false;
        public IntegratedPresenter.BMDSwitcher.Config.BMDSwitcherConfigSettings SwitcherConfig { get; internal set; }

        public bool HasUserConfig { get; private set; } = false;
        public Configurations.FeatureConfig.IntegratedPresenterFeatures UserConfig { get; private set; }

        public bool HasCCUConfig { get; internal set; } = false;
        public CCPUConfig CCPUConfig { get; internal set; }

        public string Folder { get; set; }

        public List<ISlide> Slides { get; set; } = new List<ISlide>();

        public static ISlide EmptySlide = new Slide() { Source = "", Type = SlideType.Empty };
        public Dictionary<string, WatchVariable> WatchedVariables { get; private set; } = new Dictionary<string, WatchVariable>();
        public HashSet<string> OwnedVariables { get; private set; } = new HashSet<string>();

        public bool Create(string folder)
        {
            Slides.Clear();
            Folder = folder;

            // get all files in directory and hope they are slides
            var allfiles = Directory.GetFiles(Folder);
            var numberedfiles = allfiles.Where(f => Regex.Match(Path.GetFileName(f), @"^\d+_.*").Success);
            var files = numberedfiles.OrderBy(p => Convert.ToInt32(Regex.Match(Path.GetFileName(p), "(?<slidenum>\\d+).*").Groups["slidenum"].Value)).ToList();
            foreach (var file in files)
            {
                var filename = Regex.Match(Path.GetFileName(file), @"\d+_(?<type>[^-\.]+)(-(?<action>[^\.]+))?(?<drive>\.nodrive)?\.(?<extension>.*)");
                string name = filename.Groups["type"].Value;
                string action = filename.Groups["action"].Value;
                string drive = filename.Groups["drive"].Value;
                string extension = filename.Groups["extension"].Value;

                // skip unrecognized files
                List<string> valid = new List<string>() { "mp4", "png", "txt", "gif" };
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
                    try
                    {
                        using (StreamReader sr = new StreamReader(Path.Combine(folder, s.Source)))
                        {
                            var text = sr.ReadToEnd();
                            if (ActionLoader.TryLoadActions(text, folder, out var res))
                            {
                                s.Actions = res.Actions;
                                s.SetupActions = res.SetupActions;
                                s.Title = res.Title;
                                s.AltSources = res.AltSources;
                                s.AltSource = res.AltSource;
                                s.AltKeySource = res.AltKeySource;
                                s.AutoOnly = res.AutoOnly;
                                s.ForceRunOnLoad = res.ForceRunOnLoad;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                }
                Slides.Add(s);
            }

            // attach keyfiles to slides
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"Key_\d+").Success).ToList();
            foreach (var file in files)
            {
                var num = Regex.Match(file, @"Key_(?<num>\d+)").Groups["num"].Value;
                int snum = Convert.ToInt32(num);
                var slide = Slides[snum] as Slide;
                if (slide != null)
                {
                    slide.KeySource = file;
                }
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

            // attach pilot to slides
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"Pilot_\d+").Success).ToList();
            foreach (var file in files)
            {
                var num = Regex.Match(file, @"Pilot_(?<num>\d+)").Groups["num"].Value;
                if (int.TryParse(num, out int snum))
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            // load the pilot actions...
                            var src = sr.ReadToEnd();
                            List<IPilotAction> pilotActions, emgActions;
                            PilotActionBuilder.BuildPilotActions(src, out pilotActions, out emgActions);

                            Slides[snum].AutoPilotActions = pilotActions;
                            Slides[snum].EmergencyActions = emgActions;
                        }
                    }
                    catch (Exception)
                    {
                    }
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

            // find ccpu config file
            configfile = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"CCU-Config").Success).FirstOrDefault();

            if (File.Exists(configfile))
            {
                using (StreamReader sr = new StreamReader(configfile))
                {
                    var cfg = sr.ReadToEnd();
                    try
                    {
                        CCPUConfig = JsonSerializer.Deserialize<CCPUConfig_Extended>(cfg);
                        HasCCUConfig = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            // load raw text resources
            files = Directory.GetFiles(folder).Where(f => Regex.Match(f, @"RawResource_.+\.txt").Success).ToList();
            foreach (var file in files)
            {
                var key = Regex.Match(file, @"RawResource_(?<key>.+)\.txt").Groups["key"].Value;
                using (StreamReader sr = new StreamReader(file))
                {
                    // load the pilot actions...
                    var src = sr.ReadToEnd();
                    RawTextResources[key] = src;
                }
            }


            ComputeAggregateWatchAndCalculatedVariables();

            return false;
        }

        public int SlideCount { get => Slides.Count; }

        [ExposedAsVariable(nameof(CurrentSlide))]
        public int CurrentSlide { get => _currentSlide + 1; }

        [ExposedAsVariable(nameof(CurrentSlide0Index))]
        public int CurrentSlide0Index { get => _currentSlide; }

        private int _currentSlide = 0;
        private int _virtualCurrentSlide = 0;

        public ISlide Prev
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


        public ISlide Override { get; set; }
        public bool OverridePres { get; set; } = false;

        public ISlide EffectiveCurrent
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

        public ISlide Current { get => Slides[_currentSlide]; }
        public ISlide Next
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
        public ISlide After
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

        public Dictionary<string, string> RawTextResources { get; internal set; } = new Dictionary<string, string>();

        public void NextSlide()
        {
            Prev?.ResetAllActionsState();
            After?.ResetAllActionsState();
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

        public void SetNextSlideJump(int target)
        {
            if (target - 1 >= 0 && target - 1 < SlideCount)
            {
                // treat this as having someone use skip mode to get to just prior to the target jump point
                _virtualCurrentSlide = target - 1;
            }
            // jump to 0??
            if (target == 0)
            {
                _virtualCurrentSlide = target;
            }
        }

        public void PrevSlide()
        {
            if (_virtualCurrentSlide - 1 >= 0)
            {
                After?.ResetAllActionsState();
                EffectiveCurrent?.ResetAllActionsState();
                Prev?.ResetAllActionsState();
                Next?.ResetAllActionsState();
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

        public void StartPres(int snum = 0)
        {
            // need to reset all slides
            foreach (var s in Slides)
            {
                s?.ResetAllActionsState();
            }
            //EffectiveCurrent?.ResetAllActionsState();
            if (snum > Slides.Count && snum > 0)
            {
                snum = Slides.Count - 1;
            }

            _currentSlide = snum;
            _virtualCurrentSlide = snum;

        }

        public void ComputeAggregateWatchAndCalculatedVariables()
        {
            // find every slide with actions the require watches
            Dictionary<string, WatchVariable> variables = new Dictionary<string, WatchVariable>();

            foreach (var slide in Slides)
            {
                var allSlideActions = slide.Actions.Concat(slide.SetupActions);

                foreach (var action in allSlideActions.Where(x => x.Action.Action == AutomationActions.WatchSwitcherStateBoolVal || x.Action.Action == AutomationActions.WatchStateBoolVal))
                {
                    string vname = (string)action.Action.Parameters[2].LiteralValue;
                    string wpath = (string)action.Action.Parameters[0].LiteralValue;
                    object expectation = action.Action.Parameters[1].LiteralValue;
                    if (variables.ContainsKey(vname) && variables[vname].VPath != wpath && variables[vname].ExpectedVal != expectation)
                    {
                        Debugger.Break();
                        // TODO: consider if we need to make some sneaky, dynamic swapping and adjust a few things to auto-unmask
                    }
                    //else
                    {
                        variables[vname] = new WatchVariable(wpath, expectation, AutomationActionArgType.Boolean);
                    }
                }
                foreach (var action in allSlideActions.Where(x => x.Action.Action == AutomationActions.WatchSwitcherStateIntVal || x.Action.Action == AutomationActions.WatchStateIntVal))
                {
                    string vname = (string)action.Action.Parameters[2].LiteralValue;
                    string wpath = (string)action.Action.Parameters[0].LiteralValue;
                    object expectation = action.Action.Parameters[1].LiteralValue;
                    if (variables.ContainsKey(vname) && variables[vname].VPath != wpath && variables[vname].ExpectedVal != expectation)
                    {
                        Debugger.Break();
                        // TODO: consider if we need to make some sneaky, dynamic swapping and adjust a few things to auto-unmask
                    }
                    //else
                    {
                        variables[vname] = new WatchVariable(wpath, expectation, AutomationActionArgType.Integer);
                    }
                }

                foreach (var action in allSlideActions.Where(x => x.Action.Action == AutomationActions.InitComputedVal))
                {
                    string vname = (string)action.Action.Parameters[0].LiteralValue;
                    OwnedVariables.Add(vname);
                }
            }

            WatchedVariables = variables;
        }
    }


}
