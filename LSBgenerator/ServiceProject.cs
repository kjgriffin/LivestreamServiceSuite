using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{

    [Serializable]
    public class ServiceProject
    {

        public string SourceText { get; set; } = "";
        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();

        public TextRendererLayout Layout { get; set; }


        public static ServiceProject Deserialize(string filename)
        {
            Stream s = File.OpenRead(filename);
            BinaryFormatter b = new BinaryFormatter();
            ServiceProject res = (ServiceProject)b.Deserialize(s);
            s.Close();
            return res;
        }

        public void Serialize(string filename)
        {
            Stream s = File.OpenWrite(filename);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(s, this);
            s.Close();
        }

    }
}
