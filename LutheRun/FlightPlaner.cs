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
    internal class FlightPlanner
    {

        /*
            Attempt to analyze the service and figure out when/what camera movements are required.
            Here's probably a decent place to try and ratify/adjust any postset stuff that might be wrong/missed

            TODO: should require some sort of info about what cameras/presets are available/to use
         */
        internal static void PlanFlight(LSBImportOptions options, List<ParsedLSBElement> serviceElements)
        {

            // track some state about where our cameras are
            // track some state about where we are in the service
            // TODO- may need to double iterate the service to figure out what's happening
            // then once we know we can better plan based on a lookahead

            SmoothFlight(serviceElements.Select(x => x.LSBElement).ToList(), options);


        }

        private static void SmoothFlight(List<ILSBElement> service, LSBImportOptions options)
        {

            StringBuilder sb = new StringBuilder();
            // this is the v1 algorithm for camera pilot

            // general idea is to chunk the service into logical blocks of content
            // that we know how to schedule cameras for (eg. lituryg-corperate, hymn, sermon, announcment, reading etc.)
            var blocks = Blockify(service);

            // once blocked we'll assign cameras
            // for the blocks
            // then we need to schedule transitions on block boundaries
            // once we have a plan we can then attach the pilot actions

            foreach (var block in blocks)
            {
                int indent = 10;
                sb.AppendLine($"[{block.BlockType}] >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                foreach (var elem in block.Elements)
                {
                    //sb.AppendLine(elem.XenonAutoGen(options, ref indent, 4));
                    sb.AppendLine(elem.GetType().ToString());
                }
                sb.AppendLine($"[{block.BlockType}] <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            }

            var x = sb.ToString();
            Debug.WriteLine("OUTPUT:");
            Debug.WriteLine(x);

        }


        private static List<BlockedServiceElement> Blockify(List<ILSBElement> service)
        {
            List<BlockedServiceElement> result = new List<BlockedServiceElement>();

            BlockType lastType = BlockType.UNKNOWN;
            BlockedServiceElement cBlock = new BlockedServiceElement();

            foreach (var element in service)
            {
                var cType = element.BlockType() ;
                if (cType == lastType)
                {
                    cBlock.Elements.Add(element);
                }
                else
                {
                    if (cBlock.Elements.Any())
                    {
                        result.Add(cBlock);
                    }
                    cBlock = new BlockedServiceElement();
                    cBlock.BlockType = cType;
                    cBlock.Elements.Add(element);
                }

                lastType = cType;
            }
            if (cBlock.Elements.Any())
            {
                result.Add(cBlock);
            }

            return result;
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
