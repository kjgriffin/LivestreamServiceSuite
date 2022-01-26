using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Helpers
{
    internal static class BoolHelpers
    {
        public static bool Optional(this bool val, bool enabled)
        {
            if (enabled)
            {
                return val;
            }
            return true;
        }
    }
}
