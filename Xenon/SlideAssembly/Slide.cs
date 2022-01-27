using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Xenon.SlideAssembly
{
    public class Slide
    {

        public const string LAYOUT_INFO_KEY = "slide.layout.info";

        public Dictionary<string, Color> Colors { get; set; } = new Dictionary<string, Color>();

        public string Name { get; set; }
        public int Number { get; set; }
        public SlideFormat Format { get; set; }
        public MediaType MediaType { get; set; }
        public string Asset { get; set; }
        public List<SlideLine> Lines { get; set; } = new List<SlideLine>();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public SlideOverridingBehaviour OverridingBehaviour { get; set; } = new SlideOverridingBehaviour();


        public Slide()
        {
            Colors.Add("text", Color.White);
            Colors.Add("alttext", Color.Teal);
            Colors.Add("background", Color.Gray);
            Colors.Add("keybackground", Color.Black);
            Colors.Add("keytrans", Color.Gray);
        }

    }

    public class SlideOverridingBehaviour
    {
        public bool ForceOverrideExport { get; set; } = false;
        public string OverrideExportName { get; set; } = "";
        public string OverrideExportKeyName { get; set; } = "";

    }

}
