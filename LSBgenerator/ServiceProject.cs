using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{

    public class ProjectAsset
    {

        public string ResourcePath { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Bitmap Image { get; set; }

    }

    public class ServiceProject
    {

        public string SourceText { get; set; } = "";
        public TextRenderer Renderer { get; set; } = new TextRenderer(1920, 1080, 1, 1, 1, 1, 1, 1, 1, Control.DefaultFont);
        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();



    }
}
