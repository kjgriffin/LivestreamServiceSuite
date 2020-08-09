using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class Slide
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public SlideFormat Format { get; set; }
        public MediaType MediaType {get; set;}
        public string Asset { get; set; }
        public List<SlideLine> Lines { get; set; } = new List<SlideLine>();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
