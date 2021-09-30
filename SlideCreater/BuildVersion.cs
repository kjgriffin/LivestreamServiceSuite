using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater
{
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

    }
}
