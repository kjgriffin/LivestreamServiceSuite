using CCUI_UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Integrated_Presenter.Presentation
{
    public interface IPilotAction
    {
        internal void Execute(ICCPUPresetMonitor_Executor driverContext, int defaultSpeed);
        internal string DisplayInfo { get; }
    }

    internal class PilotDriveNamedPreset : IPilotAction
    {
        string CamName;
        string PresetName;
        int Speed;
        bool HasSpeed;

        string IPilotAction.DisplayInfo { get => $"[{CamName}] {PresetName}"; }
        void IPilotAction.Execute(ICCPUPresetMonitor_Executor driverContext, int defaultSpeed)
        {
            driverContext?.FirePreset(CamName, PresetName, HasSpeed ? Speed : defaultSpeed);
        }

        internal static bool TryParse(string cmd, out IPilotAction pilot)
        {
            var match = Regex.Match(cmd, @"^move\[(?<cam>.*)\]\((?<pos>.*)\)(?<speed>@\d+);");
            if (match.Success)
            {
                int speed = -1;
                bool doSpeed = false;
                if (match.Groups.TryGetValue("speed", out var g))
                {
                    if (int.TryParse(g.Value.Substring(1, g.Value.Length - 2), out int s))
                    {
                        speed = s;
                        doSpeed = true;
                    }
                }
                pilot = new PilotDriveNamedPreset
                {
                    CamName = match.Groups["cam"].Value,
                    PresetName = match.Groups["pos"].Value,
                    Speed = speed,
                    HasSpeed = doSpeed,
                };
                return true;
            }
            pilot = null;
            return false;
        }
    }
}
