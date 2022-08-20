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
        internal string CamName { get; }
        internal string PresetName { get; }
        internal string DisplayInfo { get; }
        internal string AltName { get; }
        internal string Status { get; }
        internal List<Guid> ReqIds { get; }

        void Reset();
        void StatusUpdate(string[] args);
    }

    enum PilotActionState
    {
        READY,
        STARTED,
        DONE,
        FAILED,
        SKIPED,
    }


    internal class PilotDriveNamedPreset : IPilotAction
    {
        string CamName;
        string PresetName;
        int Speed;
        bool HasSpeed;


        PilotActionState State = PilotActionState.READY;

        string IPilotAction.DisplayInfo { get => $""; }
        string IPilotAction.CamName { get => CamName; }
        string IPilotAction.PresetName { get => PresetName; }
        string IPilotAction.AltName { get => $""; }
        string IPilotAction.Status { get => State.ToString(); }

        List<Guid> IPilotAction.ReqIds { get => new List<Guid> { moveId }; }

        Guid moveId = Guid.Empty;

        void IPilotAction.Execute(ICCPUPresetMonitor_Executor driverContext, int defaultSpeed)
        {
            moveId = driverContext?.FirePreset_Tracked(CamName, PresetName, HasSpeed ? Speed : defaultSpeed) ?? Guid.Empty;
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
                    if (int.TryParse(g.Value.Substring(1), out int s))
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

        public void Reset()
        {
            moveId = Guid.Empty;
            State = PilotActionState.READY;
        }

        public void StatusUpdate(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "STARTED":
                        State = PilotActionState.STARTED;
                        break;
                    case "COMPLETED":
                        State = PilotActionState.DONE;
                        break;
                    case "FAILED":
                        State = PilotActionState.FAILED;
                        break;
                }
            }
        }
    }

    internal class PilotDriveNamedPresetWithZoom : IPilotAction
    {
        string CamName;
        string PresetName;
        int Speed;
        bool HasSpeed;
        string ZoomDir;
        int ZoomDur;
        bool DoZoom;
        PilotActionState MoveState = PilotActionState.READY;
        PilotActionState ZoomState = PilotActionState.READY;

        string IPilotAction.DisplayInfo { get => $"ZOOM {ZoomDir}{Environment.NewLine}{ZoomDur}ms"; }
        string IPilotAction.CamName { get => CamName; }
        string IPilotAction.PresetName { get => PresetName; }
        string IPilotAction.AltName { get => $""; }

        string IPilotAction.Status
        {
            get
            {
                if (ZoomState == MoveState)
                {
                    return ZoomState.ToString();
                }
                if (ZoomState == PilotActionState.FAILED || MoveState == PilotActionState.FAILED)
                {
                    return PilotActionState.FAILED.ToString();
                }
                if (ZoomState == PilotActionState.STARTED || MoveState == PilotActionState.STARTED)
                {
                    return PilotActionState.STARTED.ToString();
                }
                return "-----";
            }
        }


        List<Guid> IPilotAction.ReqIds { get => new List<Guid> { moveReqId, zoomReqId }; }

        Guid zoomReqId = Guid.Empty;
        Guid moveReqId = Guid.Empty;


        void IPilotAction.Execute(ICCPUPresetMonitor_Executor driverContext, int defaultSpeed)
        {
            moveReqId = driverContext?.FirePreset_Tracked(CamName, PresetName, HasSpeed ? Speed : defaultSpeed) ?? Guid.Empty;
            int dir = ZoomDir.ToLower() == "wide" ? -1 : ZoomDir.ToLower() == "tele" ? 1 : 0;
            zoomReqId = driverContext?.FireZoom_Tracked(CamName, dir, ZoomDur) ?? Guid.Empty;
        }

        internal static bool TryParse(string cmd, out IPilotAction pilot)
        {
            var match = Regex.Match(cmd, @"^drive\[(?<cam>.*)\]\((?<pos>.*)\)(?<speed>@\d+)(\|(?<zdir>.*):(?<zdur>\d+))?;");
            if (match.Success)
            {
                int speed = -1;
                bool doSpeed = false;
                int zdur = 0;
                bool dozoom = false;
                if (match.Groups.TryGetValue("speed", out var g))
                {
                    if (int.TryParse(g.Value.Substring(1), out int s))
                    {
                        speed = s;
                        doSpeed = true;
                    }
                }
                if (match.Groups.TryGetValue("zdur", out var z))
                {
                    if (int.TryParse(z.Value, out int zs))
                    {
                        zdur = zs;
                        dozoom = true;
                    }
                }
                pilot = new PilotDriveNamedPresetWithZoom
                {
                    CamName = match.Groups["cam"].Value,
                    PresetName = match.Groups["pos"].Value,
                    Speed = speed,
                    HasSpeed = doSpeed,
                    DoZoom = dozoom,
                    ZoomDir = match.Groups["zdir"].Value,
                    ZoomDur = zdur,
                };
                return true;
            }
            pilot = null;
            return false;
        }

        public void Reset()
        {
            moveReqId = Guid.Empty;
            zoomReqId = Guid.Empty;
            ZoomState = PilotActionState.READY;
            MoveState = PilotActionState.READY;
        }

        public void StatusUpdate(string[] args)
        {
            if (args.Length > 1)
            {
                PilotActionState state = PilotActionState.READY;
                switch (args.First())
                {
                    case "STARTED":
                        state = PilotActionState.STARTED;
                        break;
                    case "COMPLETED":
                        state = PilotActionState.DONE;
                        break;
                    case "FAILED":
                        state = PilotActionState.FAILED;
                        break;
                }

                if (Guid.TryParse(args.Last(), out Guid guid))
                {
                    if (guid == zoomReqId)
                    {
                        ZoomState = state;
                    }
                    if (guid == moveReqId)
                    {
                        MoveState = state;
                    }
                }
            }

        }
    }

    internal class PilotDriveNamedPresetWithNamedZoom : IPilotAction
    {
        string CamName;
        string PresetName;
        int Speed;
        bool HasSpeed;
        string ZName;
        PilotActionState MoveState = PilotActionState.READY;
        PilotActionState ZoomState = PilotActionState.READY;

        string IPilotAction.DisplayInfo { get => $"ZOOM {ZName}"; }
        string IPilotAction.CamName { get => CamName; }
        string IPilotAction.PresetName { get => PresetName; }
        string IPilotAction.AltName { get => $""; }

        string IPilotAction.Status
        {
            get
            {
                if (ZoomState == MoveState)
                {
                    return ZoomState.ToString();
                }
                if (ZoomState == PilotActionState.FAILED || MoveState == PilotActionState.FAILED)
                {
                    return PilotActionState.FAILED.ToString();
                }
                if (ZoomState == PilotActionState.STARTED || MoveState == PilotActionState.STARTED)
                {
                    return PilotActionState.STARTED.ToString();
                }
                return "-----";
            }
        }


        List<Guid> IPilotAction.ReqIds { get => new List<Guid> { moveReqId, zoomReqId }; }

        Guid zoomReqId = Guid.Empty;
        Guid moveReqId = Guid.Empty;


        void IPilotAction.Execute(ICCPUPresetMonitor_Executor driverContext, int defaultSpeed)
        {
            moveReqId = driverContext?.FirePreset_Tracked(CamName, PresetName, HasSpeed ? Speed : defaultSpeed) ?? Guid.Empty;

            zoomReqId = driverContext?.FireZoomLevel_Tracked(CamName, ZName) ?? Guid.Empty;
        }

        internal static bool TryParse(string cmd, out IPilotAction pilot)
        {
            var match = Regex.Match(cmd, @"^run\[(?<cam>.*)\]\((?<pos>.*)\)(?<speed>@\d+)<(?<zpst>.*)>;");
            if (match.Success)
            {
                int speed = -1;
                bool doSpeed = false;
                int zdur = 0;
                bool dozoom = false;
                if (match.Groups.TryGetValue("speed", out var g))
                {
                    if (int.TryParse(g.Value.Substring(1), out int s))
                    {
                        speed = s;
                        doSpeed = true;
                    }
                }
                pilot = new PilotDriveNamedPresetWithNamedZoom
                {
                    CamName = match.Groups["cam"].Value,
                    PresetName = match.Groups["pos"].Value,
                    Speed = speed,
                    HasSpeed = doSpeed,
                    ZName = match.Groups["zpst"].Value,
                };
                return true;
            }
            pilot = null;
            return false;
        }

        public void Reset()
        {
            moveReqId = Guid.Empty;
            zoomReqId = Guid.Empty;
            ZoomState = PilotActionState.READY;
            MoveState = PilotActionState.READY;
        }

        public void StatusUpdate(string[] args)
        {
            if (args.Length > 1)
            {
                PilotActionState state = PilotActionState.READY;
                switch (args.First())
                {
                    case "STARTED":
                        state = PilotActionState.STARTED;
                        break;
                    case "COMPLETED":
                        state = PilotActionState.DONE;
                        break;
                    case "FAILED":
                        state = PilotActionState.FAILED;
                        break;
                }

                if (Guid.TryParse(args.Last(), out Guid guid))
                {
                    if (guid == zoomReqId)
                    {
                        ZoomState = state;
                    }
                    if (guid == moveReqId)
                    {
                        MoveState = state;
                    }
                }
            }

        }
    }


}
