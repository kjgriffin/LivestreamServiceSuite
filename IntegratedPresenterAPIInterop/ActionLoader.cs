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
    }

}
