using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeepSixGUI.Templates
{
    public static class TemplateHelper
    {
        public static string PrepareBlob(string blobfile)
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(TemplateHelper))
                                             .GetManifestResourceNames()
                                             .FirstOrDefault(x => x.Contains(blobfile));

            var stream = System.Reflection.Assembly.GetAssembly(typeof(TemplateHelper))
                .GetManifestResourceStream(name);

            var prefabblob = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                prefabblob = sr.ReadToEnd();
            }

            return prefabblob;
        }

        public static string ReplaceBlob(this string blob, string key, string value)
        {
            return Regex.Replace(blob, Regex.Escape(key), value);
        }
    }
}
