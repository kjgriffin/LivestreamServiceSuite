using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBHymnTool.GeneralSearch
{
    public class LSBSearchResults
    {
        public LSBSearchResult[] data { get; set; }
        public LSBSearchMeta meta { get; set; }
    }

    public class LSBSearchMeta
    {
        public int searchid { get; set; }
    }

    public class LSBSearchResult
    {
        public string type { get; set; }
        public string id { get; set; }
        public GeneralSearchAttributes attributes { get; set; }
    }

    public class GeneralSearchAttributes
    {
        public string name { get; set; }
        public string number { get; set; }
        public bool category { get; set; }
        public string section { get; set; }
        public float rank { get; set; }
        public Hit[] hits { get; set; }
        public bool autoselect { get; set; }
        public object context { get; set; }
        public object[] usage { get; set; }
        public VersionUsage versionusage { get; set; }
        public object[] contentnames { get; set; }
        public object[] stanzas { get; set; }
    }

    public class VersionUsage
    {
    }

    public class Hit
    {
        public string field { get; set; }
        public float rank { get; set; }
        public string headline { get; set; }
    }


}
