using SlideCreater.AssetManagment;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SlideCreater.SlideAssembly
{
    public class Project
    {
        public SlideLayout Layouts { get; set; } = new SlideLayout();
        public List<Slide> Slides { get; set; } = new List<Slide>();

        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();


        private int slidenum = 0;
        public int NewSlideNumber => slidenum++;
    }
}
