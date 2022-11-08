using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun.Pilot
{
    internal class CameraAction
    {
        internal string Preset { get; set; }
        internal int ID { get; set; }

        public static implicit operator CameraAction((string preset, int ElementOrder) v)
        {
            return new CameraAction
            {
                Preset = v.preset,
                ID = v.ElementOrder,
            };
        }

        public override string ToString()
        {
            return $"{Preset} <for {ID}>";
        }

    }

    internal class CameraUsage
    {
        internal CamsUseTracker RequiredCameras { get; set; } = new CamsUseTracker();
        internal HashSet<CameraID> FreedCameras { get; set; } = new HashSet<CameraID>();
        internal CamsUseTracker InUse { get; set; } = new CamsUseTracker();
        internal Dictionary<CameraID, List<string>> UnResolvedConflicts { get; set; } = new Dictionary<CameraID, List<string>>();
        internal Dictionary<CameraID, List<string>> AllConflicts { get; set; } = new Dictionary<CameraID, List<string>>();

        internal Dictionary<CameraID, CameraAction> SafeActions { get; set; } = new Dictionary<CameraID, CameraAction>();
        internal Dictionary<CameraID, CameraAction> RiskyActions { get; set; } = new Dictionary<CameraID, CameraAction>();
        internal Dictionary<CameraID, string> Available { get; set; } = new Dictionary<CameraID, string>();

        internal void AddRequirement(CameraID cam, string preset)
        {
            RequiredCameras.Require(cam, preset);
        }

        internal void FreeCamera(CameraID cam)
        {
            RequiredCameras.Free(cam);
            FreedCameras.Add(cam);
        }

        internal string FormatForHTML()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Required:");
            sb.AppendLine(RequiredCameras.Format(true).IndentBlock(1, 4));
            sb.AppendLine("Freed:");
            sb.AppendLine(string.Join(Environment.NewLine, FreedCameras).IndentBlock(1, 4));
            sb.AppendLine("InUse:");
            sb.AppendLine(InUse.Format().IndentBlock(1, 4));
            sb.AppendLine("Un-Resolved-Conflicts:");
            sb.AppendLine(string.Join(Environment.NewLine, UnResolvedConflicts.Select(x => $"{x.Key}{{{string.Join(",", x.Value)}}}")).IndentBlock(1, 4));
            sb.AppendLine("All-Conflicts");
            sb.AppendLine(string.Join(Environment.NewLine, AllConflicts.Select(x => $"{x.Key}{{{string.Join(",", x.Value)}}}")).IndentBlock(1, 4));


            sb.AppendLine("Safe Actions:");
            sb.AppendLine(string.Join(Environment.NewLine, SafeActions.Select(x => $"{x.Key}{{{string.Join(",", x.Value)}}}")).IndentBlock(1, 4));
            sb.AppendLine("UnSafe Actions:");
            sb.AppendLine(string.Join(Environment.NewLine, RiskyActions.Where(x => !SafeActions.ContainsKey(x.Key)).Select(x => $"{x.Key}{{{string.Join(",", x.Value)}}}")).IndentBlock(1, 4));

            return sb.ToString();
        }
    }


}
