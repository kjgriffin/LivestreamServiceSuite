using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.AssetManagment
{
    public class ProjectAsset
    {
        public string RelativePath { get; set; }
        public Guid Id { get; set; }  
        public string Name { get; set; }
        public AssetType Type { get; set; }
    }
}
