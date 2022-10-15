using Integrated_Presenter.Presentation;

using System;
using System.Collections.Generic;

namespace IntegratedPresenter.Main
{
    internal static class PilotActionBuilder
    {

        internal static void BuildPilotActions(string src, out List<IPilotAction> pilotActions, out List<IPilotAction> emgActions)
        {
            pilotActions = new List<IPilotAction>();
            emgActions = new List<IPilotAction>();
            foreach (var line in src.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                // load emergency actions
                if (line.StartsWith("emg") && PilotDriveNamedPresetWithNamedZoom.TryParse(line.Substring(3, line.Length - 3), out var emgpa))
                {
                    emgActions.Add(emgpa);
                }
                // load as regular actions
                else if (PilotDriveNamedPreset.TryParse(line, out var pcmd))
                {
                    pilotActions.Add(pcmd);
                }
                else if (PilotDriveNamedPresetWithZoom.TryParse(line, out var pcmdwz))
                {
                    pilotActions.Add(pcmdwz);
                }
                else if (PilotDriveNamedPresetWithNamedZoom.TryParse(line, out var pcmdwnz))
                {
                    pilotActions.Add(pcmdwnz);
                }
            }
        }
    }
}