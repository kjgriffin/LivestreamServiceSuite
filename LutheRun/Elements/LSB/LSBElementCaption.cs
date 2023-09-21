using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LutheRun.Elements.LSB
{
    class LSBElementCaption : ILSBElement
    {
        public string PostsetCmd { get; set; }

        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";

        public IElement SourceHTML { get; private set; }

        public static Random rnd = new Random(DateTime.Now.Millisecond);

        public static ILSBElement Parse(IElement element)
        {
            var res = new LSBElementCaption();
            res.SourceHTML = element;
            res.Caption = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent ?? "";
            res.SubCaption = element.QuerySelectorAll(".subcaption-text").FirstOrDefault()?.TextContent ?? "";
            return res;

        }

        internal LSBElementCaption()
        {

        }

        internal static LSBElementCaption FromHeading(LSBElementHeading hdg)
        {
            return new LSBElementCaption
            {
                Caption = hdg.Heading,
                PostsetCmd = hdg.PostsetCmd,
                SourceHTML = hdg.SourceHTML,
                SubCaption = "",
            };
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_CAPTION. Caption:'{Caption}' SubCaption:'{SubCaption}'";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace, ParsedLSBElement fullInfo)
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
                foreach (var line in ExternalPrefabGenerator.BellsCommand(indentSpace).Split(Environment.NewLine))
                {
                    sb.AppendLine(line.Indent(indentDepth, indentSpace));
                }
            }
            else if (ctest.Contains("prelude"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='prelude'>".Indent(indentDepth, indentSpace));
                sb.AppendLine("//> INSERTION POINT: prelude".Indent(indentDepth, indentSpace));
                sb.AppendLine($"#2title(\"{Caption}\", \"{SubCaption}\"){PostsetCmd}".Indent(indentDepth, indentSpace));
            }
            else if (ctest.Contains("postlude"))
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='postlude'>".Indent(indentDepth, indentSpace));
                sb.AppendLine("//> INSERTION POINT: postlude".Indent(indentDepth, indentSpace));
                sb.AppendLine($"#2title(\"{Caption}\", \"{SubCaption}\"){PostsetCmd}".Indent(indentDepth, indentSpace));
            }
            else if (ctest.Contains("anthem") || ctest.Contains("chorus"))
            {
                if (lSBImportOptions.RunWithSubPanels)
                {
                    string blobtext = ExternalPrefabGenerator.PrepareBlob("SubPaneledAnthemTemplate");

                    int anthemID = -1;
                    if (fullInfo.OutOfBandInfo.TryGetValue("anthemUID", out object oanthemid))
                    {
                        anthemID = (int)oanthemid;
                    }

                    blobtext = Regex.Replace(blobtext, Regex.Escape("$>"), "".PadLeft(indentSpace));
                    blobtext = Regex.Replace(blobtext, Regex.Escape("$ANTHEMID"), anthemID.ToString());

                    sb.AppendLine(blobtext.IndentBlock(indentDepth, indentSpace));
                }
                else if (lSBImportOptions.ExpandAnthemsForAutomation)
                {
                    // anthem was pre-expanded, so do something different here....

                    // dump in a scope and title generating slide

                    // this will allow pilot to still attach here if needed

                    // previous 'expander' intro will setup/configure a panel that all anthem operations will use
                    string blobtext = ExternalPrefabGenerator.PrepareBlob("PaneledAnthemTemplate");

                    int anthemID = -1;
                    if (fullInfo.OutOfBandInfo.TryGetValue("anthemUID", out object oanthemid))
                    {
                        anthemID = (int)oanthemid;
                    }

                    blobtext = Regex.Replace(blobtext, Regex.Escape("$>"), "".PadLeft(indentSpace));
                    blobtext = Regex.Replace(blobtext, Regex.Escape("$ANTHEMID"), anthemID.ToString());

                    sb.AppendLine(blobtext.IndentBlock(indentDepth, indentSpace));
                }
                else
                {
                    sb.AppendLine("/// </MANUAL_UPDATE name='anthem'>".Indent(indentDepth, indentSpace));
                    sb.AppendLine("//> INSERTION POINT: anthem".Indent(indentDepth, indentSpace));
                    sb.AppendLine($"#anthemtitle(\"{Caption}\", \"{SubCaption}\", \"\", \"\"){PostsetCmd}".Indent(indentDepth, indentSpace));
                }
            }
            else if (ctest.Contains("sermon"))
            {
                if (lSBImportOptions.PreachWithHTMLTitles)
                {
                    string blobtext = ExternalPrefabGenerator.PrepareBlob("SermonHTML");
                    blobtext = Regex.Replace(blobtext, Regex.Escape("$>"), "".PadLeft(indentSpace));
                    sb.AppendLine(blobtext.IndentBlock(indentDepth, indentSpace));
                }
                else
                {
                    sb.AppendLine("/// </MANUAL_UPDATE name='sermon'>".Indent(indentDepth, indentSpace));
                    sb.AppendLine("//> INSERTION POINT: sermon".Indent(indentDepth, indentSpace));
                    sb.AppendLine($"#sermon(\"TITLE\", \"REFERENCE\", \"PREACHER\"){PostsetCmd}".Indent(indentDepth, indentSpace));
                }
            }
            else if (ctest.Contains("announcements") || ctest.Contains("offering"))
            {
                string blobtext = ExternalPrefabGenerator.PrepareBlob("PaneledAnnouncementsTemplate");

                blobtext = Regex.Replace(blobtext, Regex.Escape("$>"), "".PadLeft(indentSpace));
                //blobtext = Regex.Replace(blobtext, Regex.Escape("$ANTHEMID"), fullInfo.ElementOrder.ToString());

                sb.AppendLine(blobtext.IndentBlock(indentDepth, indentSpace));
            }

            else if (!lSBImportOptions.OnlyKnownCaptions)
            {
                sb.AppendLine("/// </MANUAL_UPDATE name='unknown caption'>".Indent(indentDepth, indentSpace));
                sb.AppendLine($"#2title(\"{Caption}\", \"{SubCaption}\")".Indent(indentDepth, indentSpace));
            }

            return sb.ToString();
        }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            string ctest = $"{Caption.ToLower()} {SubCaption.ToLower()}";

            if (ctest.Contains("bells"))
            {
                return Pilot.BlockType.OPENING;
            }
            else if (ctest.Contains("prelude"))
            {
                return Pilot.BlockType.PRELUDE;
            }
            else if (ctest.Contains("postlude"))
            {
                return Pilot.BlockType.POSTLUDE;
            }
            else if (ctest.Contains("anthem"))
            {
                if (importOptions.ExpandAnthemsForAutomation)
                {
                    return Pilot.BlockType.ANTHEM_RESOLVED;
                }
                return Pilot.BlockType.ANTHEM;
            }
            else if (ctest.Contains("sermon"))
            {
                return Pilot.BlockType.SERMON;
            }
            else if (ctest.Contains("announcements") || ctest.Contains("offering"))
            {
                return Pilot.BlockType.ANNOUNCEMENTS;
            }

            return Pilot.BlockType.UNKNOWN;
        }

    }
}
