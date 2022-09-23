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


        public static ExternalPrefab BuildHymnIntroSlides(LSBElementHymn hymn, bool useUpNextForHymns)
        {
            // we can use the new up-next tabs if we have a hymn #
            var match = Regex.Match(hymn.Caption, @"(?<number>\d+)?(?<name>.*)");
            string name = match.Groups["name"]?.Value.Trim() ?? "";
            string number = match.Groups["number"]?.Value.Trim().Length > 0 ? ("LSB " + match.Groups["number"]?.Value.Trim()) : "";
            if (!string.IsNullOrWhiteSpace(number) && useUpNextForHymns)
            {
                return new ExternalPrefab(UpNextCommand("UpNext_Numbered", name, number, ""), "upnext", BlockType.HYMN) { IndentReplacementIndentifier = "$>" };
            }
            else if (!string.IsNullOrWhiteSpace(name) && useUpNextForHymns)
            {
                return new ExternalPrefab(UpNextCommand("UpNext_UnNumbered", name, number, ""), "upnext", BlockType.HYMN) { IndentReplacementIndentifier = "$>" };
            }
            else
            {
                return new ExternalPrefab("#organintro", "organintro", BlockType.HYMN);
            }


        }

        private static string UpNextCommand(string blobfile, string hname, string hnumber, string postset)
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

            // inject name
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$HYMNNAME"), hname);
            // inject number (if no-number, we just won't replace anythin)
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$HYMNNUMBER"), hnumber);
            // inject postset
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$POSTSET"), postset);

            return prefabblob;
        }


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
                    return new ExternalPrefab(CopyTitleCommand(serviceTitle, serviceDate, lsback, (int)Serviceifier.Camera.Organ, options.InferPostset, options.ServiceThemeLib), "copytitle", BlockType.TITLEPAGE) { IndentReplacementIndentifier = "$>" };

                }
            }

            return new LSBElementUnknown();
        }

        public static ILSBElement GenerateEndPage(IEnumerable<ILSBElement> titleelements, LSBImportOptions options)
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
                    return new ExternalPrefab(EndPageCommand(serviceTitle), "endtitle", BlockType.TITLEPAGE) { IndentReplacementIndentifier = "$>" };

                }
            }

            return new LSBElementUnknown();
        }


        public static ILSBElement GenerateCreed(string creedtype, LSBImportOptions options)
        {
            const string postsetReplacementIdentifier = "$POSTSET$";

            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                           .GetManifestResourceNames()
                           .Where(n => n.StartsWith("LutheRun.PrefabBlobs") && n.Contains(creedtype))
                           .FirstOrDefault();

            string txtcmd = "";

            if (!string.IsNullOrEmpty(name))
            {
                var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator)).GetManifestResourceStream(name);
                using (StreamReader sr = new StreamReader(stream))
                {
                    txtcmd = sr.ReadToEnd();
                }
            }
            txtcmd = Regex.Replace(txtcmd, @"\$SERVICETHEME\$", options.ServiceThemeLib);

            if (options.UsePIPCreeds)
            {
                StringBuilder sb = new StringBuilder();
                name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                      .GetManifestResourceNames()
                                      .FirstOrDefault(x => x.Contains("PrePIPScriptBlock_Creed"));

                var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                    .GetManifestResourceStream(name);

                // wrap it into a scripted block
                using (StreamReader sr = new StreamReader(stream))
                {
                    sb.AppendLine(sr.ReadToEnd());
                }

                sb.AppendLine();

                // need to further indent the command...
                var cmdlines = txtcmd.Split(Environment.NewLine);
                foreach (var cmdline in cmdlines)
                {
                    sb.AppendLine($"$>{cmdline}");
                }

                sb.AppendLine("}");

                txtcmd = sb.ToString();
            }

            return new ExternalPrefab(txtcmd, "creed", BlockType.CREED) { PostsetReplacementIdentifier = postsetReplacementIdentifier, IndentReplacementIndentifier = "$>" };

        }



        private static string CopyTitleCommand(string serviceTitle = "", string serviceDate = "", string lsback = "", int postset = -1, bool inferPostset = true, string libtheme = "Xenon.Green")
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                  .GetManifestResourceNames()
                                  .FirstOrDefault(x => x.Contains("CopyTitle"));

            var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                .GetManifestResourceStream(name);

            // wrap it into a scripted block
            var prefabblob = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                prefabblob = sr.ReadToEnd();
            }

            // inject theme
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$LIBTHEME"), libtheme);
            // inject title
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$SERVICETITLE"), serviceTitle);
            // inject date
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$SERVICEDATE"), serviceDate);
            // inject ack
            StringBuilder sb = new StringBuilder();
            foreach (var line in lsback.Split(Environment.NewLine))
            {
                sb.AppendLine($"$>$>`{line}");
            }
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$LSBACK"), sb.ToString().TrimEnd());
            // inject postset
            string postsetstr = "";
            if (inferPostset && postset != -1)
            {
                postsetstr = $"::postset(last={postset})";
            }
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$POSTSET"), postsetstr);

            return prefabblob;
        }

        private static string EndPageCommand(string serviceTitle = "")
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                  .GetManifestResourceNames()
                                  .FirstOrDefault(x => x.Contains("EndTitle"));

            var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                .GetManifestResourceStream(name);

            // wrap it into a scripted block
            var prefabblob = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                prefabblob = sr.ReadToEnd();
            }

            // inject title
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$SERVICETITLE"), serviceTitle);

            return prefabblob;
        }



    }
}
