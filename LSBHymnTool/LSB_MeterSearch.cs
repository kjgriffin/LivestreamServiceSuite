using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBHymnTool.MeterSearch
{

    public class LSBMeterSearch
    {
        public LSBMeterResults[] data { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public int searchid { get; set; }
    }

    public class LSBMeterResults
    {
        public string type { get; set; }
        public string id { get; set; }
        public LSBMeterSearchAttributes attributes { get; set; }
    }

    public class LSBMeterSearchAttributes
    {
        public string name { get; set; }
        public string number { get; set; }
        public string category { get; set; }
        public string section { get; set; }
        public int rank { get; set; }
        public object[] hits { get; set; }
        public bool autoselect { get; set; }
        public object context { get; set; }
        public Usage[] usage { get; set; }
        public VersionUsage versionusage { get; set; }
        public object[] contentnames { get; set; }
        public object[] stanzas { get; set; }
    }

    public class VersionUsage
    {
    }

    public class Usage
    {
        public string id { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public string color { get; set; }
        public bool planned { get; set; }
    }
}
