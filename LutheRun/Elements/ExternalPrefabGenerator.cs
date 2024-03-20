using LutheRun.Elements.Interface;
using LutheRun.Elements.LSB;
using LutheRun.Generators;
using LutheRun.Parsers;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LutheRun.Elements
{
    internal static class ExternalPrefabGenerator
    {


        public static ExternalPrefab BuildScriptedIntro(string blobintro, string blobreplace, string type, BlockType btype)
        {
            return new ExternalPrefab(blobintro, type, btype);
        }


        public static ExternalPrefab BuildHymnIntroSlides(LSBElementHymn hymn, bool useUpNextForHymns)
        {
            // we can use the new up-next tabs if we have a hymn #
            var match = Regex.Match(hymn.Caption, @"(?<number>\d+)?(?<name>.*)");
            string name = match.Groups["name"]?.Value.Trim() ?? "";
            string number = match.Groups["number"]?.Value.Trim().Length > 0 ? "LSB " + match.Groups["number"]?.Value.Trim() : "";
            if (/*!string.IsNullOrWhiteSpace(number) &&*/ useUpNextForHymns)
            {
                //return new ExternalPrefab(UpNextCommand("UpNext_Numbered", name, number, ""), "upnext", BlockType.HYMN_INTRO) { IndentReplacementIndentifier = "$>" };
                return new ExternalPrefab(HTMLUpNextCommand("UpNext_HTML", name, number, ""), "upnext", BlockType.HYMN_INTRO) { IndentReplacementIndentifier = "$>" };
            }
            /*
            else if (!string.IsNullOrWhiteSpace(name) && useUpNextForHymns)
            {
                //return new ExternalPrefab(UpNextCommand("UpNext_UnNumbered", name, number, ""), "upnext", BlockType.HYMN_INTRO) { IndentReplacementIndentifier = "$>" };
                return new ExternalPrefab(UpNextCommand("UpNext_UnNumbered", name, number, ""), "upnext", BlockType.HYMN_INTRO) { IndentReplacementIndentifier = "$>" };
            }
            */
            else
            {
                return new ExternalPrefab("#organintro", "organintro", BlockType.HYMN_INTRO);
            }


        }

        public static string PrepareBlob(string blobfile)
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

            return prefabblob;
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

        private static string HTMLUpNextCommand(string blobfile, string hname, string hnumber, string postset)
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
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$HYMN"), hnumber);
            // inject number (if no-number, we just won't replace anythin)
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$ANNOTATION"), hname);
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

        public static ILSBElement GenerateCopyTitle(string serviceTitle, string serviceDate, string lsback, LSBImportOptions options)
        {
            // Since the external prefab is wrapped inside a scripted block, let the tile generate genrate the postset explicitly
            if (options.UseMk2CopyTitle)
            {
                return new ExternalPrefab(Mk2CopyTitleCommand(options, serviceTitle, serviceDate, lsback, (int)Serviceifier.Camera.Organ), "copytitle", BlockType.TITLEPAGE) { IndentReplacementIndentifier = "$>" };
            }
            return new ExternalPrefab(CopyTitleCommand(serviceTitle, serviceDate, lsback, (int)Serviceifier.Camera.Organ, options.InferPostset, options.ServiceThemeLib), "copytitle", BlockType.TITLEPAGE) { IndentReplacementIndentifier = "$>" };
        }

        internal static void ExtractTitleAndDateFuzzy(IEnumerable<ILSBElement> titleelements, out string serviceTitle, out string serviceDate)
        {
            serviceTitle = GetStringFromCaptionOrHeading(titleelements.First());
            // assume service date is second element
            serviceDate = GetStringFromCaptionOrHeading(titleelements.Last());

            // can we do better for out of order dates/titles??

            if (!DateTime.TryParse(serviceDate, out _) && DateTime.TryParse(serviceTitle, out _))
            {
                // swap them
                var tmp = serviceDate;
                serviceDate = serviceTitle;
                serviceTitle = tmp;
            }
        }

        public static ILSBElement GenerateEndPage(string serviceTitle, string serviceDate, LSBImportOptions options)
        {
            return new ExternalPrefab(EndPageCommand(serviceTitle), "endtitle", BlockType.TITLEPAGE) { IndentReplacementIndentifier = "$>" };
        }


        public static ILSBElement GenerateCreed(string creedtype, LSBImportOptions options)
        {
            const string postsetReplacementIdentifier = "$POSTSET$";

            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                           .GetManifestResourceNames()
                           .Where(n => n.StartsWith("LutheRun.PrefabBlobs") && n.EndsWith(creedtype))
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
                if (options.ThemeCreedsWithHTML)
                {
                    name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                          .GetManifestResourceNames()
                                          .FirstOrDefault(x => x.Contains("PrePIPScriptBlock_Creed-html"));
                }
                else
                {
                    name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                          .GetManifestResourceNames()
                                          .FirstOrDefault(x => x.Contains("PrePIPScriptBlock_Creed-std"));
                }

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
                                  .FirstOrDefault(x => x.Contains("Mk1CopyTitle"));

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

        private static string Mk2CopyTitleCommand(LSBImportOptions options, string serviceTitle = "", string serviceDate = "", string lsback = "", int postset = -1)
        {
            var prefabblob = ExternalPrefabGenerator.PrepareBlob("Mk2CopyTitle-std");

            // inject theme
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$LIBTHEME"), "Xenon.Titles"); // TODO: allow this override?
            // inject title
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$SERVICETITLE"), serviceTitle);
            // inject date
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$SERVICEDATE"), serviceDate);

            const string NIV = "Scripture quotations marked(NIV) are taken from the Holy Bible, New International Version®, NIV®. Copyright © 1973, 1978, 1984, 2011 by Biblica, Inc.™ Used by permission of Zondervan.All rights reserved worldwide. www.zondervan.comThe “NIV” and “New International Version” are trademarks registered in the United States Patent and Trademark Office by Biblica, Inc.™";
            //string bibleackPackage = Environment.NewLine + "#IF " + XenonGenerator.NIV_DEF + Environment.NewLine + NIV + Environment.NewLine + "#ENDIF" + Environment.NewLine;

            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$BIBLEACK"), NIV);

            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$DEFFLAGBIBLEACK"), XenonGenerator.PEW_DEF);

            // inject ack
            StringBuilder sb = new StringBuilder();
            foreach (var line in lsback.Split(Environment.NewLine))
            {
                sb.Append($"{line}<br>");
            }
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$LSBACK"), sb.ToString().TrimEnd());
            // inject postset
            string postsetstr = "";
            if (options.InferPostset && postset != -1)
            {
                postsetstr = $"::postset(last={postset})";
            }
            prefabblob = Regex.Replace(prefabblob, Regex.Escape("$POSTSET"), postsetstr);

            if (options.RunWithSubPanels)
            {
                var ctitle = prefabblob;
                var wrapper = ExternalPrefabGenerator.PrepareBlob("Mk2CopyTitle-PanelWrapper");

                var pmatch = Regex.Match(wrapper, "^(?<pre>.*)\\$COPYTITLE", RegexOptions.Multiline);

                StringBuilder insert = new StringBuilder();
                foreach (var line in ctitle.Split(Environment.NewLine))
                {
                    insert.AppendLine(line);
                    insert.Append(pmatch.Groups["pre"]?.Value ?? "");
                }

                prefabblob = Regex.Replace(wrapper, Regex.Escape("$COPYTITLE"), insert.ToString());
            }


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


        internal static string BellsCommand(int indentSpaces)
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                  .GetManifestResourceNames()
                                  .FirstOrDefault(x => x.Contains("Bells"));

            var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                .GetManifestResourceStream(name);

            // wrap it into a scripted block
            var prefabblob = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                prefabblob = sr.ReadToEnd();
            }

            // handle this here
            var cmd = Regex.Replace(prefabblob, Regex.Escape("$>"), "".PadLeft(indentSpaces));

            return cmd;
        }


    }
}
