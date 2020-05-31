using System;
using System.Drawing;

namespace LSBgenerator
{
    [Serializable]
    public class ProjectAsset
    {

        public string ResourcePath { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Bitmap Image { get; set; }

        public Guid guid { get; } = Guid.NewGuid();
    }
}
