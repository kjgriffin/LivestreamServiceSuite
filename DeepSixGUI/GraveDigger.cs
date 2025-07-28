using Concord;

using DeepSixGUI.Templates;

using LutheRun;
using LutheRun.Elements.Interface;
using LutheRun.Elements.LSB;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;

using OpenQA.Selenium.DevTools.V130.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSixGUI
{

    public static class GraveDigger
    {
        public async static Task<Dictionary<string, string>> Bury(GravePlot template, Action<System.Drawing.Bitmap, string, string, string> addAsset)
        {
            Dictionary<string, string> files = new Dictionary<string, string>
            {
                ["deep-six-elements"] = TemplateHelper.PrepareBlob("deep-six-elements"),
                ["deep-six-hymn-rdg"] = TemplateHelper.PrepareBlob("deep-six-hymn-rdg"),
                ["HymnPanel"] = TemplateHelper.PrepareBlob("HymnPanel"),
                ["SermonPanel"] = TemplateHelper.PrepareBlob("SermonPanel"),
                ["AnthemPanel"] = TemplateHelper.PrepareBlob("AnthemPanel"),

                ["CommonScripts"] = TemplateHelper.PrepareBlob("CommonScripts"),
                ["MasterPanel"] = TemplateHelper.PrepareBlob("MasterPanel"),
            };

            var std = TemplateHelper.PrepareBlob("STDElement")
                                    .ReplaceBlob("$$TITLE$$", template.ServiceName)
                                    .ReplaceBlob("$$NAME$$", template.DeceasedName)
                                    .ReplaceBlob("$$DEATH$$", template.Lifespan)
                                    .ReplaceBlob("$$TIME$$", template.ServiceTime)
                                    .ReplaceBlob("$$DATE$$", template.ServiceDate.ToString("dddd MMMM d, yyyy"));

            files.Add("STDElement", std);


            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = new LSBImportOptions();
            await parser.ParseHTML(template.ServicePath);

            // use alternate generator
            var hymns = parser.ExtractOnlyHymns();
            // generate just hymns in standalone mode
            await parser.LoadWebAssets(addAsset);
            string hymnFile = ExtractHymnsFromImport(template, hymns);
            files.Add("FuneralHymns", hymnFile);

            // now generate readings with tool
            string readingFile = BuildReadings(template);
            files.Add("FuneralReadings", readingFile);





            // check the option to import into new file?
            // otherwise we'll overwrite the main file
            // update views

            // either way, add all 'extra' files

            return files;
        }

        public static string BuildReadings(GravePlot plot)
        {
            StringBuilder sb = new StringBuilder();

            int indentDepth = 0;
            int indentSpace = 4;

            StringBuilder sbmacro = new StringBuilder();
            for (int i = 0; i < plot.Readings.Length; i++)
            {
                if (plot.Readings[i].Use)
                {
                    sb.AppendLine($"#DEFINE RDG_{i + 1}");
                }
            }

            // create file template with scope & theme
            sb.AppendLine(TemplateHelper.PrepareBlob("FuneralTheme")
                                        .ReplaceBlob("$$SCOPE$$", "FuneralReadings")
                                        .ReplaceBlob("$$DEF$$", sbmacro.ToString())
                                        .IndentBlock(indentDepth, indentSpace));
            sb.AppendLine();
            indentDepth++;

            // dump in the readings
            for (int i = 0; i < plot.Readings.Length; i++)
            {
                if (plot.Readings[i].Use)
                {
                    var translation = plot.Translation == "niv" ? BibleTranslations.NIV : BibleTranslations.ESV;
                    var rblock = TemplateHelper.PrepareBlob("ReadingTemplate")
                                               .ReplaceBlob("$$READING_NUM$$", plot.Readings[i].ID.ToString())
                                               .ReplaceBlob("$$TRANSLATION$$", plot.Translation)
                                               .ReplaceBlob("$$REFERENCE$$", plot.Readings[i].Reference)
                                               .IndentBlock(indentDepth, indentSpace);

                    var text = LutheRun.Generators.ReadingTextGenerator.GenerateXenonComplexReading(translation, plot.Readings[i].Reference, 4, 4);

                    rblock = rblock.ReplaceBlob("$$CONTENT$$", text);

                    sb.AppendLine(rblock);
                }
            }

            indentDepth--;
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string ExtractHymnsFromImport(GravePlot plot, List<ParsedLSBElement> elements)
        {
            // find each hymn
            StringBuilder sb = new StringBuilder();
            LSBImportOptions options = new LSBImportOptions();
            // override all options because we'll do it ourselves
            options.WrapConsecuitivePackages = false;
            options.UsePIPHymns = false;
            options.ImSoProICanRunPIPHymsWithoutStuttersEvenDuringCommunion = false;
            options.RunPIPHymnsLikeAProWithoutStutters = false;

            int indentDepth = 0;
            int indentSpace = 4;

            Dictionary<string, string> extraFileContents = new Dictionary<string, string>();

            StringBuilder sbmacro = new StringBuilder();
            for (int i = 0; i < plot.Hymns.Length; i++)
            {
                if (plot.Hymns[i].Use)
                {
                    sb.AppendLine($"#DEFINE HYMN_{i + 1}");
                }
            }

            // create file template with scope & theme
            sb.AppendLine(TemplateHelper.PrepareBlob("FuneralTheme")
                                        .ReplaceBlob("$$SCOPE$$", "FuneralHymns")
                                        .ReplaceBlob("$$DEF$$", sbmacro.ToString())
                                        .IndentBlock(indentDepth, indentSpace));
            sb.AppendLine();

            // indent
            indentDepth++;


            foreach (ParsedLSBElement se in elements)
            {
                var hymnCaption = se.LSBElement as ICaptionElement;
                // snoop the hymn to see what's inside
                HymnCaptionExtractor.ExtractAsHymnInfo(hymnCaption, out var title, out var name, out var number);
                var intro = TemplateHelper.PrepareBlob("HymnIntro")
                                                            .ReplaceBlob("$$TITLE$$", name)
                                                            .ReplaceBlob("$$NUMBER$$", number);
                // if number is referenced, then use it
                var hdef = plot.Hymns.FirstOrDefault(h => h.Number == number);
                if (hdef != null)
                {
                    intro = intro.ReplaceBlob("$$LABEL$$", $"[@label::hymn{hdef.ID}]");
                }
                else
                {
                    intro = intro.ReplaceBlob("$$LABEL$$", "");
                }

                sb.AppendLine(intro.IndentBlock(indentDepth, indentSpace));

                // wrap in scripted
                sb.AppendLine("#scripted".Indent(indentDepth, indentSpace));
                sb.AppendLine("{".Indent(indentDepth, indentSpace));
                indentDepth++;

                sb.AppendLine("first=#callscript{scriptname(pip-setup)parameter(PIPFill){%cam.BACK%}}".Indent(indentDepth, indentSpace));
                sb.AppendLine("duplast=#callscript{scriptname(pip-teardown)parameter(PostCam){%cam.CENTER%}}".Indent(indentDepth, indentSpace));
                sb.AppendLine(se.LSBElement?.XenonAutoGen(options, ref indentDepth, indentSpace, se, extraFileContents) ?? se.XenonCode ?? "".Indent(indentDepth, indentSpace));

                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpace));
                sb.AppendLine();
            }

            indentDepth--;
            sb.AppendLine("}".Indent(indentDepth, indentSpace));

            // unindent

            return sb.ToString();
        }

        public static string GetLayoutLib()
        {
            return TemplateHelper.PrepareBlob("DeepSix.json");
        }
    }
}
