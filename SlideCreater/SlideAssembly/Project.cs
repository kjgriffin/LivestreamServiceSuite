using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.SlideAssembly
{
    public class Project
    {
        public SlideLayout Layouts { get; set; } = new SlideLayout();
        public List<Slide> Slides { get; set; } = new List<Slide>();
    }
}
