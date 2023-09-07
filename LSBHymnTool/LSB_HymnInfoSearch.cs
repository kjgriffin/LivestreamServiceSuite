using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBHymnTool.HymnInfoSearch
{

    public class HymnInfoSearchResult
    {
        public HymnInfoSearchData data { get; set; }
    }

    public class HymnInfoSearchData
    {
        public string id { get; set; }
        public string type { get; set; }
        public HymnInfoAttributes attributes { get; set; }
    }

    public class HymnInfoAttributes
    {
        public string name { get; set; }
        public string number { get; set; }
        public ContentNames[] contentnames { get; set; }
        public string selectedcontentname { get; set; }
        public bool hasinfo { get; set; }
        public bool iscopyrighted { get; set; }
        public Stanza[] stanzas { get; set; }
        public object selectedstanzas { get; set; }
        public string html { get; set; }
        public Author[][] authors { get; set; }
        public Copyright[][] copyrights { get; set; }
        public Tune[] tunes { get; set; }
        public Holiday[] holidays { get; set; }
        public string[] topics { get; set; }
        public Pericope[] pericopes { get; set; }
        public object[] requiredpermissions { get; set; }
    }

    public class ContentNames
    {
        public string name { get; set; }
        public string type { get; set; }
        public string format { get; set; }
    }

    public class Stanza
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool isSelected { get; set; }
    }

    public class Author
    {
        public string text { get; set; }
        public string entity { get; set; }
        public bool book { get; set; }
    }

    public class Copyright
    {
        public string text { get; set; }
        public string entity { get; set; }
    }

    public class Tune
    {
        public string contentname { get; set; }
        public string name { get; set; }
        public string meter { get; set; }
        public string key { get; set; }
        public string url { get; set; }
    }

    public class Holiday
    {
        public string name { get; set; }
        public string lectionary { get; set; }
    }

    public class Pericope
    {
        public string stanza { get; set; }
        public string[] pericopes { get; set; }
    }

}
