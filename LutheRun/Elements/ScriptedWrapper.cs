using LutheRun.Parsers;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun.Elements
{
    internal class ScriptedWrapper : ExternalElement
    {
        BlockType btype { get; set; }
        public override BlockType BlockType()
        {
            return btype;
        }

        string ScriptedContents { get; set; }
        string IndentExpr { get; set; }
        bool Closing { get; set; } = false;


        public static ScriptedWrapper ClosingWrapper(BlockType type)
        {
            return new ScriptedWrapper
            {
                btype = type,
                Closing = true,
                ScriptedContents = "}"
            };
        }

        public static ScriptedWrapper FromRaw(BlockType btype, string rawScriptedContents)
        {
            return new ScriptedWrapper
            {
                btype = btype,
                ScriptedContents = rawScriptedContents,
            };
        }

        public static ScriptedWrapper FromBlob(BlockType btype, string blobfile, string indentExpr = "$>", Dictionary<string, string> blobReplace = null)
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                             .GetManifestResourceNames()
                                             .FirstOrDefault(x => x.Contains(blobfile));

            var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                .GetManifestResourceStream(name);

            var prefabblob = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                prefabblob = sr.ReadToEnd();
            }

            if (blobReplace != null)
            {
                foreach (var rep in blobReplace)
                {
                    prefabblob = Regex.Replace(prefabblob, Regex.Escape(rep.Key), rep.Value);
                }
            }

            return new ScriptedWrapper
            {
                ScriptedContents = prefabblob,
                btype = btype,
                IndentExpr = indentExpr,
            };
        }



        public override string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace)
        {
            if (Closing)
            {
                indentDepth--;
                return "}".Indent(indentDepth, indentSpace);
            }

            StringBuilder sb = new StringBuilder();
            string cblob = Regex.Replace(ScriptedContents, Regex.Escape(IndentExpr), "".PadLeft(indentSpace));

            sb.AppendLine("#scripted".Indent(indentDepth, indentSpace));
            sb.AppendLine("{".Indent(indentDepth, indentSpace));

            indentDepth++;
            sb.AppendLine(cblob.IndentBlock(indentDepth, indentSpace));

            return sb.ToString();
        }
    }
}
