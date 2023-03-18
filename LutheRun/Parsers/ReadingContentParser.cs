using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun.Parsers
{
    internal static class ReadingContentParser
    {

        internal static bool ItHasResponsoryStyledContent(IEnumerable<IElement> elements)
        {
            string[] rleaders = new string[] { "A", "C", "L", "P" };
            string[] classmatch = new string[] { "lsb-responsorial" };
            string[] stylematch = new string[] { "lsb-symbol" };

            // go through stuff and see if we have anything that's got a class looking like lsb-responsorial
            // or check if it's got special characters that are P|C|L|A

            // anybody got matching class- probably responsive
            var cmatch = elements.Any(x => x.ClassList.Any(x => classmatch.Any(y => x.Contains(y))));

            // anybody got text with lsb-symbols- it's probably responsive
            var rmatch = elements.Any(x => x.ClassList.Any(x => classmatch.Any(y => x.Contains(y))) && rleaders.Contains(x.TextContent.ToLower()));

            return cmatch || rmatch;
        }

    }
}
