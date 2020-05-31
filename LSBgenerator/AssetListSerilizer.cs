using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LSBgenerator
{
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
}
