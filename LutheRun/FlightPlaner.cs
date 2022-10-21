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

            // once blocked we'll assign cameras
            // for the blocks
            // then we need to schedule transitions on block boundaries
            // once we have a plan we can then attach the pilot actions



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
        IGNORED,
        PRELUDE,
        POSTLUDE,
        ANTHEM,
        OPENING,
        CREED,
        TITLEPAGE,
    }


}
