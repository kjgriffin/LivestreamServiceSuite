using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp.Dom;
using System.Linq;

namespace LutheRun
{
    class LSBElementCaption : ILSBElement
    {
        public string PostsetCmd { get; set; }

        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";

        public static ILSBElement Parse(IElement element)
        {
            var res = new LSBElementCaption();
            res.Caption = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent ?? "";
            res.SubCaption = element.QuerySelectorAll(".subcaption-text").FirstOrDefault()?.TextContent ?? "";
            return res;

        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_CAPTION. Caption:'{Caption}' SubCaption:'{SubCaption}'";
        }

        public string XenonAutoGen()
        {
            // for now just make a title slide:: and flag it as optional
            //return $"/// <XENON_AUTO_GEN optional=\"true\">\r\n#2title(\"{Caption.Replace('\"', '\'')}\", \"{SubCaption.Replace('\"', '\'')}\", \"horizontal\")\r\n/// </XENON_AUTO_GEN>";
            // Only insert captions that might have value...
            /*
            Captions with value:
            Prelude/Postlude/Anthem/Bells/Sermon
             */
            StringBuilder sb = new StringBuilder();

            string ctest = $"{Caption.ToLower()} {SubCaption.ToLower()}";

            if (ctest.Contains("bells"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='bells'>");
                sb.AppendLine("//> INSERTION POINT: bells");
            }
            else if (ctest.Contains("prelude"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='prelude'>");
                sb.AppendLine("//> INSERTION POINT: prelude");
                sb.AppendLine($"#2title(\"{Caption}\", \"{SubCaption}\", \"horizontal\")");
            }
            else if (ctest.Contains("postlude"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='postlude'>");
                sb.AppendLine("//> INSERTION POINT: postlude");
                sb.AppendLine($"#2title(\"{Caption}\", \"{SubCaption}\", \"horizontal\")");
            }
            else if (ctest.Contains("anthem"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='anthem'>");
                sb.AppendLine("//> INSERTION POINT: anthem");
                sb.AppendLine($"#anthemtitle(\"{Caption}\", \"{SubCaption}\", \"\", \"\")");
            }
            else if (ctest.Contains("sermon"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='sermon'>");
                sb.AppendLine("//> INSERTION POINT: sermon");
                sb.AppendLine($"#sermon(\"TITLE\", \"REFERENCE\", \"PREACHER\")");
            }
            return sb.ToString();
        }
    }
}
