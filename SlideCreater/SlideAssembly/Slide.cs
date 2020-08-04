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
        public string Format { get; set; }
        public MediaType MediaType {get; set;}
        public string Asset { get; set; }
        public List<SlideLine> Lines { get; set; } = new List<SlideLine>();
    }
}
