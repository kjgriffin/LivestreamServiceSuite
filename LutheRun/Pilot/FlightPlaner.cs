using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun.Pilot
{
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
