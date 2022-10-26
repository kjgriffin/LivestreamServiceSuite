using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{

    internal enum CameraID
    {
        PULPIT,
        CENTER,
        LECTERN,
        ORGAN,
        SLIDE,
        BACK,
    }

    internal class CamsUseTracker
    {
        internal Dictionary<CameraID, HashSet<string>> Presets { get; private set; } = new Dictionary<CameraID, HashSet<string>>();
        internal void Require(CameraID cam, string preset)
        {
            HashSet<string> presets;
            if (Presets.TryGetValue(cam, out presets))
            {
                presets.Add(preset);
            }
            else
            {
                presets = new HashSet<string>(preset.ItemAsEnumerable());
                Presets[cam] = presets;
            }
        }
        internal void Free(CameraID cam)
        {
            if (Presets.ContainsKey(cam))
            {
                Presets.Remove(cam);
            }
        }

        internal CamsUseTracker Copy()
        {
            CamsUseTracker res = new();
            foreach (var cam in Presets)
            {
                foreach (var pst in cam.Value)
                {
                    res.Require(cam.Key, pst);
                }
            }
            return res;
        }

        internal Dictionary<CameraID, List<string>> ComputeConflicts(CamsUseTracker inUse, bool strict = true)
        {
            Dictionary<CameraID, List<string>> res = new Dictionary<CameraID, List<string>>();

            foreach (var kvp in inUse.Presets)
            {
                if (Presets.TryGetValue(kvp.Key, out var reqs))
                {
                    // if in strict mode warn about every preset for this camera
                    if (strict)
                    {
                        res[kvp.Key] = reqs.ToList();
                    }
                    else
                    {
                        // only warn about presets that are different
                        var conflicts = reqs.Where(x => !kvp.Value.Contains(x)).ToList(); // oooooh this smells like an expensive operation- but then again for small values of n O(n^2) is smallish....
                        if (conflicts.Any())
                        {
                            res[kvp.Key] = conflicts;
                        }
                    }
                }
            }
            return res;
        }

        internal string Format()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in Presets)
            {
                sb.Append($"{kvp.Key}:{{");
                sb.Append(string.Join(",", kvp.Value));
                sb.Append("}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

    }



    internal class CameraUsage
    {
        internal CamsUseTracker RequiredCameras { get; set; } = new CamsUseTracker();
        internal HashSet<CameraID> FreedCameras { get; set; } = new HashSet<CameraID>();
        internal CamsUseTracker InUse { get; set; } = new CamsUseTracker();
        internal Dictionary<CameraID, List<string>> UnResolvedConflicts { get; set; } = new Dictionary<CameraID, List<string>>();
        internal Dictionary<CameraID, List<string>> AllConflicts { get; set; } = new Dictionary<CameraID, List<string>>();

        internal Dictionary<CameraID, string> SafeActions { get; set; } = new Dictionary<CameraID, string>();
        internal Dictionary<CameraID, string> Actions { get; set; } = new Dictionary<CameraID, string>();
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

        internal string Format()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Required:");
            sb.AppendLine(RequiredCameras.Format().IndentBlock(1, 4));
            sb.AppendLine("Freed:");
            sb.AppendLine(string.Join(Environment.NewLine, FreedCameras).IndentBlock(1, 4));
            sb.AppendLine("InUse:");
            sb.AppendLine(InUse.Format().IndentBlock(1, 4));
            sb.AppendLine("Un-Resolved-Conflicts:");
            sb.AppendLine(string.Join(Environment.NewLine, UnResolvedConflicts.Select(x => $"{x.Key}{{{(string.Join(",", x.Value))}}}")).IndentBlock(1, 4));
            sb.AppendLine("All-Conflicts");
            sb.AppendLine(string.Join(Environment.NewLine, AllConflicts.Select(x => $"{x.Key}{{{(string.Join(",", x.Value))}}}")).IndentBlock(1, 4));

            return sb.ToString();
        }
    }

    internal static class FlightPlanner
    {

        /*
            Attempt to analyze the service and figure out when/what camera movements are required.
            Here's probably a decent place to try and ratify/adjust any postset stuff that might be wrong/missed

            TODO: should require some sort of info about what cameras/presets are available/to use
         */
        public static List<ParsedLSBElement> PlanFlight(this List<ParsedLSBElement> serviceElements, LSBImportOptions options)
        {

            // track some state about where our cameras are
            // track some state about where we are in the service
            // TODO- may need to double iterate the service to figure out what's happening
            // then once we know we can better plan based on a lookahead

            if (options.FlightPlanning)
            {
                SmoothFlight(serviceElements, options);
            }

            return serviceElements;
        }

        private static void SmoothFlight(List<ParsedLSBElement> service, LSBImportOptions options)
        {

            StringBuilder sb = new StringBuilder();
            // this is the v1 algorithm for camera pilot

            // general idea is to chunk the service into logical blocks of content
            // that we know how to schedule cameras for (eg. lituryg-corperate, hymn, sermon, announcment, reading etc.)

            // mutating call that will attach block info to service
            Blockify(service);

            // once blocked we'll analyze for camera usage requirements based on blocks
            AsignCameraUseage(service);

            // then we need to schedule transitions on block boundaries based on where cameras are req/freed
            AsignSequencialStateBasedConflicts(service);

            // once we have a plan we can then attach the pilot actions

        }

        private static void AsignSequencialStateBasedConflicts(List<ParsedLSBElement> service)
        {
            CamsUseTracker InUse = new CamsUseTracker();

            foreach (var elem in service.Where(x => !x.FilterFromOutput))
            {
                // since pilot happens after slide runs
                // if the camera is freed it will not conflict if used later
                foreach (var free in elem.CameraUse?.FreedCameras)
                {
                    InUse.Free(free);
                }

                // based on curent state find if any requirements conflict
                elem.CameraUse.AllConflicts = InUse.ComputeConflicts(elem.CameraUse.RequiredCameras);
                elem.CameraUse.UnResolvedConflicts = InUse.ComputeConflicts(elem.CameraUse.RequiredCameras, false);

                // update state
                foreach (var req in elem.CameraUse?.RequiredCameras?.Presets)
                {
                    foreach (var preq in req.Value)
                    {
                        InUse.Require(req.Key, preq);
                    }
                }

                // report copy of state
                elem.CameraUse.InUse = InUse.Copy();
            }
        }

        private static void AsignCameraUseage(List<ParsedLSBElement> service)
        {
            foreach (var elem in service.Where(x => !x.FilterFromOutput))
            {
                CameraUsage cams = new CameraUsage();
                // look at block type and translate to usage
                switch (elem.BlockType)
                {
                    case BlockType.UNKNOWN:
                        break;
                    case BlockType.LITURGY_CORPERATE:
                        cams.AddRequirement(CameraID.CENTER, "front");
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.LECTERN);
                        cams.FreeCamera(CameraID.ORGAN);
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.ANNOUNCEMENTS:
                        break;
                    case BlockType.MISC_CORPERATE:
                        cams.AddRequirement(CameraID.CENTER, "front");
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.READING:
                        cams.AddRequirement(CameraID.LECTERN, "reading");
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.CENTER);
                        cams.FreeCamera(CameraID.ORGAN);
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.SERMON:
                        cams.AddRequirement(CameraID.PULPIT, "sermon");
                        cams.AddRequirement(CameraID.CENTER, "sermon");
                        cams.AddRequirement(CameraID.LECTERN, "sermon");
                        cams.FreeCamera(CameraID.ORGAN);
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.HYMN:
                        cams.AddRequirement(CameraID.ORGAN, "organ");
                        cams.AddRequirement(CameraID.BACK, "wide");
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.CENTER);
                        cams.FreeCamera(CameraID.LECTERN);
                        break;
                    case BlockType.HYMN_ORGAN:
                        cams.AddRequirement(CameraID.ORGAN, "organ");
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.CENTER);
                        cams.FreeCamera(CameraID.LECTERN);
                        break;
                    case BlockType.HYMN_OTHER:
                        cams.AddRequirement(CameraID.ORGAN, "piano?");
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.CENTER);
                        cams.FreeCamera(CameraID.LECTERN);
                        break;
                    case BlockType.HYMN_INTRO:
                        cams.AddRequirement(CameraID.ORGAN, "organ");
                        break;
                    case BlockType.IGNORED:
                        break;
                    case BlockType.PRELUDE:
                        cams.AddRequirement(CameraID.ORGAN, "prelude");
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.POSTLUDE:
                        cams.AddRequirement(CameraID.ORGAN, "postlude");
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.ANTHEM:
                        cams.AddRequirement(CameraID.ORGAN, "anthem");
                        cams.AddRequirement(CameraID.LECTERN, "anthem"); // TODO: not sure if this is right
                        cams.AddRequirement(CameraID.CENTER, "anthem");
                        cams.AddRequirement(CameraID.PULPIT, "anthem");
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.OPENING:
                        cams.AddRequirement(CameraID.PULPIT, "opening");
                        cams.AddRequirement(CameraID.CENTER, "front");
                        cams.AddRequirement(CameraID.LECTERN, "opening");
                        cams.FreeCamera(CameraID.ORGAN);
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    case BlockType.CREED:
                        cams.AddRequirement(CameraID.BACK, "wide");
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.CENTER);
                        cams.FreeCamera(CameraID.LECTERN);
                        cams.FreeCamera(CameraID.ORGAN);
                        break;
                    case BlockType.TITLEPAGE:
                        cams.FreeCamera(CameraID.PULPIT);
                        cams.FreeCamera(CameraID.CENTER);
                        cams.FreeCamera(CameraID.LECTERN);
                        cams.FreeCamera(CameraID.ORGAN);
                        cams.FreeCamera(CameraID.BACK);
                        break;
                    default:
                        break;
                }
                elem.CameraUse = cams;
            }
        }


        private static void Blockify(List<ParsedLSBElement> service)
        {
            foreach (var element in service)
            {
                var cType = element.LSBElement?.BlockType();
                element.BlockType = cType ?? BlockType.UNKNOWN;
            }
        }


    }


    internal class BlockedServiceElement
    {
        internal List<ILSBElement> Elements { get; set; } = new List<ILSBElement>();
        internal BlockType BlockType { get; set; } = BlockType.UNKNOWN;

        public override string ToString()
        {
            return $"{BlockType}";
        }

    }

    internal enum BlockType
    {
        UNKNOWN,
        LITURGY_CORPERATE,
        ANNOUNCEMENTS,
        MISC_CORPERATE,
        READING,
        SERMON,
        HYMN,
        HYMN_ORGAN,
        HYMN_OTHER,
        HYMN_INTRO,
        IGNORED,
        PRELUDE,
        POSTLUDE,
        ANTHEM,
        OPENING,
        CREED,
        TITLEPAGE,
    }


}
