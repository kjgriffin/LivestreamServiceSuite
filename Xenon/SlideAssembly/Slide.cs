using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Xenon.SlideAssembly
{
    public class Slide
    {


        public Dictionary<string, Color> Colors { get; set; } = new Dictionary<string, Color>();

        public string Name { get; set; }
        public int Number { get; set; }
        public SlideFormat Format { get; set; }
        public MediaType MediaType {get; set;}
        public string Asset { get; set; }
        public List<SlideLine> Lines { get; set; } = new List<SlideLine>();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();


        public Slide()
        {
            Colors.Add("text", Color.White);
            Colors.Add("alttext", Color.Teal);
            Colors.Add("background", Color.Gray);
            Colors.Add("keybackground", Color.Black);
        }

    }
}
