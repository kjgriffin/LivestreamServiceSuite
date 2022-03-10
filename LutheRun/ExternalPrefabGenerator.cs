using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun
{
    internal static class ExternalPrefabGenerator
    {

        private static string GetStringFromCaptionOrHeading(ILSBElement element)
        {
            if (element is LSBElementCaption)
            {
                return (element as LSBElementCaption).Caption;
            }
            if (element is LSBElementHeading)
            {
                return (element as LSBElementHeading).Heading;
            }
            return "";
        }

        public static ILSBElement GenerateCopyTitle(IEnumerable<ILSBElement> titleelements, string lsback, LSBImportOptions options)
        {

            if (titleelements.Count() == 2)
            {
                // assumes service title is first element
                string serviceTitle = GetStringFromCaptionOrHeading(titleelements.First());
                // assume service date is second element
                string serviceDate = GetStringFromCaptionOrHeading(titleelements.Last());

                if (!string.IsNullOrWhiteSpace(serviceTitle) || !string.IsNullOrWhiteSpace(serviceDate))
                {
                    // Since the external prefab is wrapped inside a scripted block, let the tile generate genrate the postset explicitly
                    return new ExternalPrefab(CopyTitleCommand(serviceTitle, serviceDate, lsback, (int)Serviceifier.Camera.Organ, options.InferPostset, options.ServiceThemeLib), "copytitle");

                }
            }

            return new LSBElementUnknown();
        }

        public static ILSBElement GenerateCreed(string creedtype, LSBImportOptions options)
        {

            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                           .GetManifestResourceNames()
                           .Where(n => n.StartsWith("LutheRun.PrefabBlobs") && n.Contains(creedtype))
                           .FirstOrDefault();

            string txtcmd = "";

            // TODO: do theme injection/replacement
            if (!string.IsNullOrEmpty(name))
            {
                var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator)).GetManifestResourceStream(name);
                using (StreamReader sr = new StreamReader(stream))
                {
                    txtcmd = sr.ReadToEnd();
                }
            }
            txtcmd = Regex.Replace(txtcmd, @"\$SERVICETHEME\$", options.ServiceThemeLib);

            // TODO: figure out how to get postset in the right place...
            return new ExternalPrefab(txtcmd, "creed");

        }

        private static string CopyTitleCommand(string serviceTitle = "", string serviceDate = "", string lsback = "", int postset = -1, bool inferPostset = true, string libtheme = "Xenon.Green")
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("#scope(copytitle) {");
            sb.AppendLine();
            sb.AppendLine($"#var(\"customdraw.Layout\", \"{libtheme}::CopyTitle\")");
            sb.AppendLine();

            sb.AppendLine("/// </MANUAL_UPDATE name='TitlePage Details'>");
            sb.AppendLine("#customdraw {");
            sb.AppendLine("asset=(\"HCLogo-white\", \"fg\")");
            sb.AppendLine("// Service Title");
            sb.AppendLine($"text={{{serviceTitle}}}");
            sb.AppendLine("// Service Type");
            sb.AppendLine("text={Worship Service}"); // TODO: perhaps we can infer this based on the date?? (ie Lent)
            sb.AppendLine("// Service Date");
            sb.AppendLine($"text={{{serviceDate}}}");
            sb.AppendLine("// Service Time");
            sb.AppendLine("text={11:00 a.m.}");
            sb.AppendLine("// LSB Acknowledgements");
            sb.AppendLine($"text={{{lsback}}}");
            sb.AppendLine("// Holy Cross Licences");
            sb.AppendLine("text={CCLI License # 524846; CSPL127841}");
            sb.AppendLine("// Church Name");
            sb.AppendLine("text={HOLY CROSS LUTHERAN CHURCH}");
            sb.AppendLine("// Church Location");
            sb.AppendLine("text={KITCHENER, ON}");

            sb.AppendLine("}");

            // manually add postset here
            if (inferPostset && postset != -1)
            {
                sb.AppendLine($"::postset(last={postset})");
            }

            sb.AppendLine("}");




            return sb.ToString();
        }



    }
}
