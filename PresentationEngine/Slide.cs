using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntegratedPresenter.Presentation
{
    public class Slide
    {
        public SlideType Type { get; set; }
        public bool AutomationEnabled { get; set; } = true;
        public string Source { get; set; }
        public string KeySource { get; set; }
        public string PreAction { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public List<AutomationAction> SetupActions { get; set; } = new List<AutomationAction>();
        public List<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
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

}
