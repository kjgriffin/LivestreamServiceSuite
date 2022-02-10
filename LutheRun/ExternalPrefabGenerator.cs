using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    return new ExternalPrefab(CopyTitleCommand(serviceTitle, serviceDate, lsback), (int)Serviceifier.Camera.Organ, options.InferPostset);

                }
            }

            return new LSBElementUnknown();
        }

        private static string CopyTitleCommand(string serviceTitle = "", string serviceDate = "", string lsback = "")
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("#scope(copytitle) {");
            sb.AppendLine();
            sb.AppendLine("#var(\"customdraw.Layout\", \"Xenon.CopyTitle::CopyTitle-Green\")");
            sb.AppendLine();

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

            sb.AppendLine("}");




            return sb.ToString();
        }



    }
}
