using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater
{
    class AutoSave
    {
        public string SourceCode { get; set; } = "";
        public List<string> SourceAssets { get; set; } = new List<string>();
        public Dictionary<string, string> AssetMap { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> AssetRenames { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> AssetExtensions { get; set; } = new Dictionary<string, string>();
    }
}
