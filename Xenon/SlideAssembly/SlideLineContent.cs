using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.SlideAssembly
{
    public class SlideLineContent
    {
        public string Data { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }
}
