using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IntegratedPresenterAPIInterop
{

    public class LoadedActions
    {
        public bool AutoOnly { get; set; } = false;
        public string Title { get; set; } = "AUTO SEQ";
        public List<AutomationAction> SetupActions { get; set; } = new List<AutomationAction>();
        public List<AutomationAction> MainActions { get; set; } = new List<AutomationAction>();
        public bool AltSources { get; set; } = false;
        public string AltSourceFileName { get; set; } = "";
        public string AltKeySourceFileName { get; set; } = "";

    }

    public static class ActionLoader
    {

        public static bool ValidateActions(string rawText)
        {
            var valid = true;
            try
            {
                List<string> parts = new List<string>();
                var commands = rawText.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => (s + ";").Trim());
                parts = commands.ToList();
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
                        }
                        else if (part.StartsWith("!displaysrc="))
                        {
                            var m = Regex.Match(part, "!displaysrc='(?<fname>.*)';");
                            if (m.Success)
                            {
                            }
                        }
                        else if (part.StartsWith("!keysrc="))
                        {
                            var m = Regex.Match(part, "!keysrc='(?<fname>.*)';");
                            if (m.Success)
                            {
                            }
                        }
                    }
                    else if (part.StartsWith("@"))
                    {
                        var a = AutomationAction.Parse(part.Remove(0, 1));
                    }
                    else if (part.StartsWith("#"))
                    {
                        var title = Regex.Match(part, @"#(?<title>.*);").Groups["title"].Value;
                    }
                    else
                    {
                        var a = AutomationAction.Parse(part);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return valid;
        }


        public static LoadedActions LoadActions(string rawText)
        {
            LoadedActions result = new LoadedActions();
            try
            {
                List<string> parts = new List<string>();
                var commands = rawText.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => (s + ";").Trim());
                parts = commands.ToList();
                result.Title = "AUTO SEQ";
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
                            result.AutoOnly = true;
                        }
                        else if (part.StartsWith("!displaysrc="))
                        {
                            var m = Regex.Match(part, "!displaysrc='(?<fname>.*)';");
                            if (m.Success)
                            {
                                result.AltSources = true;
                                result.AltSourceFileName = m.Groups["fname"].Value;
                            }
                        }
                        else if (part.StartsWith("!keysrc="))
                        {
                            var m = Regex.Match(part, "!keysrc='(?<fname>.*)';");
                            if (m.Success)
                            {
                                result.AltSources = true;
                                result.AltKeySourceFileName = m.Groups["fname"].Value;
                            }
                        }
                    }
                    else if (part.StartsWith("@"))
                    {
                        var a = AutomationAction.Parse(part.Remove(0, 1));
                        result.SetupActions.Add(a);
                    }
                    else if (part.StartsWith("#"))
                    {
                        var title = Regex.Match(part, @"#(?<title>.*);").Groups["title"].Value;
                        result.Title = title;
                    }
                    else
                    {
                        var a = AutomationAction.Parse(part);
                        result.MainActions.Add(a);
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }


        public class LoadActionsResult
        {
            public string Title { get; set; } = "";
            public bool AutoOnly { get; set; } = false;
            public bool AltSources { get; set; } = false;
            public string AltSource { get; set; } = "";
            public string AltKeySource { get; set; } = "";
            public List<TrackedAutomationAction> SetupActions { get; set; } = new List<TrackedAutomationAction>();
            public List<TrackedAutomationAction> Actions { get; set; } = new List<TrackedAutomationAction>();
        }


        public static bool TryLoadActions(string actionText, string presFolder, out LoadActionsResult loaded, bool checkRealMedia = true)
        {
            loaded = null;
            var slide = new LoadActionsResult();
            try
            {
                var commands = actionText.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => (s + ";").Trim());
                var parts = commands.ToList();
                slide.Title = "AUTO SEQ";
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
                            slide.AutoOnly = true;
                        }
                        else if (part.StartsWith("!displaysrc="))
                        {
                            var m = Regex.Match(part, "!displaysrc='(?<fname>.*)';");
                            if (m.Success)
                            {
                                if (File.Exists(Path.Combine(presFolder, m.Groups["fname"].Value)) && checkRealMedia)
                                {
                                    slide.AltSources = true;
                                    slide.AltSource = Path.Combine(presFolder, m.Groups["fname"].Value);
                                }
                                else if (!checkRealMedia)
                                {
                                    slide.AltSources = true;
                                    slide.AltSource = m.Groups["fname"].Value;
                                }
                            }
                        }
                        else if (part.StartsWith("!keysrc="))
                        {
                            var m = Regex.Match(part, "!keysrc='(?<fname>.*)';");
                            if (m.Success)
                            {
                                if (File.Exists(Path.Combine(presFolder, m.Groups["fname"].Value)) && checkRealMedia)
                                {
                                    slide.AltSources = true;
                                    slide.AltKeySource = Path.Combine(presFolder, m.Groups["fname"].Value);
                                }
                                else if (!checkRealMedia)
                                {
                                    slide.AltSources = true;
                                    slide.AltKeySource = m.Groups["fname"].Value;
                                }
                            }
                        }
                    }
                    else if (part.StartsWith("@"))
                    {
                        var a = AutomationAction.Parse(part.Remove(0, 1));
                        var runType = a.Action == AutomationActions.OpsNote ? TrackedActionRunType.Note : TrackedActionRunType.Setup;
                        slide.SetupActions.Add(new TrackedAutomationAction(a, runType));
                    }
                    else if (part.StartsWith("#"))
                    {
                        var title = Regex.Match(part, @"#(?<title>.*);").Groups["title"].Value;
                        slide.Title = title;
                    }
                    else
                    {
                        var a = AutomationAction.Parse(part);
                        var runType = a.Action == AutomationActions.OpsNote ? TrackedActionRunType.Note : TrackedActionRunType.Setup;
                        slide.Actions.Add(new TrackedAutomationAction(a, runType));
                    }
                }
                loaded = slide;
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }

}
