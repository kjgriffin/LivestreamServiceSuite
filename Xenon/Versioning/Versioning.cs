using CommonVersionInfo;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Versioning
{
    public static class Versioning
    {
        private static BuildVersion _version;
        public static BuildVersion Version { get => _version; }

        public static void SetVersion(BuildVersion version)
        {
            _version = version;
        }
    }
}
