using System.Text.RegularExpressions;

namespace CommonVersionInfo
{

    delegate bool VersionComparer(int actual, int expected);

    public class BuildVersion
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int Revision { get; set; }
        public int Build { get; set; }
        public string Mode { get; set; }


        public BuildVersion()
        {
            Mode = string.Empty;
        }


        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}.{Revision}.{Build}-{Mode}";
        }

        public static BuildVersion Parse(string version)
        {
            var match = Regex.Match(version, @"(?<mj>\d+)\.(?<mn>\d+)\.(?<rv>\d+)\.(?<bd>\d+)-(?<md>.*)");
            return new BuildVersion()
            {
                MajorVersion = int.Parse(match.Groups["mj"].Value ?? "0"),
                MinorVersion = int.Parse(match.Groups["mn"].Value ?? "0"),
                Revision = int.Parse(match.Groups["rv"].Value ?? "0"),
                Build = int.Parse(match.Groups["bd"].Value ?? "0"),
                Mode = match.Groups["md"].Value ?? ""
            };
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
            bool modematch = matchMode ? Mode == mode : true;

            if (MajorVersion > minMajor) return true && modematch;
            else if (MajorVersion == minMajor)
            {
                if (MinorVersion > minMinor) return true && modematch;
                else if (MinorVersion == minMinor)
                {
                    if (Revision > minRevions) return true && modematch;
                    else if (Revision == minRevions)
                    {
                        if (Build > minBuild) return true && modematch;
                        else if (Build == minBuild)
                        {
                            return modematch;
                        }
                    }
                }
            }
            return false;
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
        public bool ExceedsMinimumVersion(BuildVersion version)
        {
            return CompareVersion((e, a) => a < e, version.MajorVersion, version.MinorVersion, version.Revision, version.Build, matchMode: false, version.Mode);
        }



    }
}
