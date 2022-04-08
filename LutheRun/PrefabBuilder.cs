using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun
{
    internal static class PrefabBuilder
    {

        public static ExternalPrefab BuildHymnIntroSlides(LSBElementHymn hymn, bool useUpNextForHymns)
        {
            // we can use the new up-next tabs if we have a hymn #
            var match = Regex.Match(hymn.Caption, @"(?<number>\d+)?(?<name>.*)");
            string name = match.Groups["name"]?.Value.Trim() ?? "";
            string number = match.Groups["number"]?.Value.Trim().Length > 0 ? ("LSB " + match.Groups["number"]?.Value.Trim()) : "";
            if (!string.IsNullOrWhiteSpace(number) && useUpNextForHymns)
            {
                string upnextcmd = $"#upnext(\"Next Hymn\", \"{name}\", \"{number}\")";
                var nl = Environment.NewLine;
                string postcmd = string.Join(Environment.NewLine, 
                    "{",
                    "$>#Organ Intro;", 
                    "$>!displaysrc='#_Liturgy.png';",
                    "$>!keysrc='Key_#.png';",
                    "$>@arg1:PresetSelect(5)[Preset Organ];", 
                    "$>@arg1:DelayMs(100);",
                    "$>@arg0:AutoTrans[Take Organ];",
                    "}");
                return new ExternalPrefab($"{upnextcmd.Trim()}{nl}{postcmd.Trim()}", "upnext") { IndentReplacementIndentifier = "$>"};
            }
            else
            {
                return new ExternalPrefab("#organintro", "organintro");
            }


        }

    }
}
