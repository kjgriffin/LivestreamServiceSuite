using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater
{

    delegate bool VersionComparer(int actual, int expected);

    class BuildVersion
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int Revision { get; set; }
        public int Build { get; set; }
        public string Mode { get; set; }

        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{Revision}.{Build}-{Mode}";
        }

        /// <summary>
        /// Greater than or equal to version.
        /// </summary>
        /// <param name="minMajor"></param>
        /// <param name="minMinor"></param>
        /// <param name="minRevions"></param>
        /// <param name="minBuild"></param>
        /// <param name="matchMode"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool MeetsMinimumVersion(int minMajor, int minMinor, int minRevions, int minBuild, bool matchMode = false, string mode = "Release")
        {
            return CompareVersion((a, e) => a >= e, minMajor, minMinor, minRevions, minBuild, matchMode, mode);
        }

        private bool CompareVersion(VersionComparer comparer, int minMajor, int minMinor, int minRevions, int minBuild, bool matchMode, string mode)
        {
            int mmaj = minMajor;
            int mmin = minMinor;
            int mrev = minRevions;
            int mbuild = minBuild;

            bool mmatch = true;
            if (matchMode)
            {
                mmatch = Mode == mode;
            }

            if (comparer(mmaj, MajorVersion) && mmatch) return true;
            if (comparer(mmin, MinorVersion) && mmatch) return true;
            if (comparer(mrev, Revision) && mmatch) return true;
            if (comparer(mbuild, Build) && mmatch) return true;

            return false;
        }

        /// <summary>
        /// Strictly greater than version.
        /// </summary>
        /// <param name="minMajor"></param>
        /// <param name="minMinor"></param>
        /// <param name="minRevions"></param>
        /// <param name="minBuild"></param>
        /// <param name="matchMode"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool ExceedsMinimumVersion(int minMajor, int minMinor, int minRevions, int minBuild, bool matchMode = false, string mode = "Release")
        {
            return CompareVersion((e, a) => a > e, minMajor, minMinor, minRevions, minBuild, matchMode, mode);
        }


    }
}
