using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

    [Serializable]
    public static class AssetListSerilizer
    {
        public static  void Serialize(string filename, List<ProjectAsset> assets)
        {
            Stream s = File.OpenWrite(filename);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(s, assets);
            s.Close();
        }

        public static List<ProjectAsset> Deserialize(string filename)
        {
            Stream s = File.OpenRead(filename);
            BinaryFormatter b = new BinaryFormatter();
            List<ProjectAsset> res = (List<ProjectAsset>)b.Deserialize(s);
            s.Close();
            return res;
        }
    }

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
