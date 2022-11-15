﻿using AngleSharp.Text;

using LutheRun.Elements.LSB;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Xenon.Helpers;

namespace LutheRun.Generators
{
    class SeasonKeyDefinition
    {
        public string start { get; set; } = "";
        public string note { get; set; } = "";
        public Dictionary<string, string> macros { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public DateTime? StartDate
        {
            get
            {
                if (DateTime.TryParse(start, out var date))
                {
                    return date;
                }
                return null;
            }
        }
    }

    internal static class XenonGenerator
    {
        internal static (Dictionary<string, string> macros, DateTime extractedDate, string season, bool success) ApplySeasonalMacroText(List<ParsedLSBElement> service, Dictionary<string, string> macros)
        {
            bool success = false;
            string season = "unknown";
            Dictionary<string, string> newmacros = new Dictionary<string, string>(macros);
            // determine service date

            // try and find a caption/heading the looks like a date (this may not be the strongest way to do it...)
            var candidateStrings = service.Select(x => (x.LSBElement as LSBElementCaption)?.Caption).Where(x => !string.IsNullOrEmpty(x)).ToList();

            // clean up 'date' strings to remove times
            // it seems that Pastor likes to include '-- time' after for some services...
            // this doesn't parse, so we'll remove that. I think this will also give us the time too (bonus!)
            candidateStrings = candidateStrings.Select(x => x.Replace("-", "")).ToList();

            var candidateDates = new List<DateTime>();

            foreach (var cdstring in candidateStrings)
            {
                if (DateTime.TryParse(cdstring, out var date))
                {
                    candidateDates.Add(date);
                }
            }

            var ldate = candidateDates.FirstOrDefault();
            if (ldate != null)
            {
                var keydates = GetKeySeasonDatesAsync();


                // try and find the 'most relevant' key date
                // this will be the earliest date that's equal to or later than the service
                var sdate = keydates.OrderByDescending(x => x.StartDate).FirstOrDefault(x => x.StartDate <= ldate);

                if (sdate != null)
                {
                    success = true;
                    season = sdate.note;
                    // add all macros
                    foreach (var macro in sdate.macros)
                    {
                        newmacros[macro.Key] = macro.Value;
                    }
                }

            }

            return (newmacros, ldate, season, success);
        }

        private static List<SeasonKeyDefinition> GetKeySeasonDatesAsync()
        {
            // try and find info for this date
            const string dateLookupURL = "https://raw.githubusercontent.com/kjgriffin/LivestreamServiceSuite/seasons/BlobData/seasons.json";

            List<SeasonKeyDefinition> res = new List<SeasonKeyDefinition>();

            try
            {
                Debug.WriteLine($"Fetching image {dateLookupURL} from web.");
                var resp = WebHelpers.httpClient.GetAsync(dateLookupURL).Result?.Content;
                var json = resp.ReadAsStringAsync().Result;
                res = JsonSerializer.Deserialize<List<SeasonKeyDefinition>>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed trying to download: {dateLookupURL}\r\n{ex}");
            }

            return res;
        }

        public static string CompileToXenon(string ServiceFileName, LSBImportOptions options, List<ParsedLSBElement> fullservice)
        {
            CompileToXenonMappedToSource(ServiceFileName, options, fullservice);
            // extract all the source code
            StringBuilder sb = new StringBuilder();
            foreach (var elem in fullservice)
            {
                sb.Append(elem.XenonCode);
            }
            return sb.ToString();
        }

        internal static void CompileToXenonMappedToSource(string serviceFileName, LSBImportOptions options, List<ParsedLSBElement> fullservice)
        {
            StringBuilder sb = new StringBuilder();

            int indentDepth = 0;
            int indentSpace = 4;


            sb.Append($"\r\n////////////////////////////////////\r\n// XENON AUTO GEN: From Service File '{System.IO.Path.GetFileName(serviceFileName)}'\r\n////////////////////////////////////\r\n\r\n");

            if (options.UseThemedHymns || options.UseThemedCreeds)
            {
                sb.AppendLine();
                sb.AppendLine("#scope(LSBService)".Indent(indentDepth, indentSpace));
                sb.AppendLine("{".Indent(indentDepth, indentSpace));
                indentDepth++;
                sb.AppendLine();
                sb.AppendLine($"#var(\"stitchedimage.Layout\", \"{options.ServiceThemeLib}::SideBar\")".Indent(indentDepth, indentSpace));
                sb.AppendLine($"#var(\"texthymn.Layout\", \"{options.ServiceThemeLib}::SideBar\")".Indent(indentDepth, indentSpace));
                sb.AppendLine();


                sb.AppendLine($"/// </MANUAL_UPDATE name='Theme Colors'>".Indent(indentDepth, indentSpace));
                sb.AppendLine($"// See: https://github.com/kjgriffin/LivestreamServiceSuite/wiki/Themes".Indent(indentDepth, indentSpace));

                Dictionary<string, string> macros = options.Macros;

                if (options.InferSeason)
                {
                    var smreq = XenonGenerator.ApplySeasonalMacroText(fullservice, macros);
                    macros = smreq.macros;
                    sb.AppendLine($"// Seasonal Extraction {(smreq.success ? "succeded" : "failed")} finding date [{(smreq.success ? smreq.extractedDate.ToShortDateString() : "")}] with theme for season: [{smreq.season}]".Indent(indentDepth, indentSpace));
                }

                // macros!
                foreach (var macro in macros)
                {
                    sb.AppendLine($"#var(\"{options.ServiceThemeLib}@{macro.Key}\", ```{macro.Value}```)".Indent(indentDepth, indentSpace));
                }
                sb.AppendLine();

            }

            fullservice.Insert(0, new ParsedLSBElement
            {
                Generator = "XenonAutoGen",
                AddedByInference = true,
                XenonCode = sb.ToString(),
                FilterFromOutput = false,
            });
            sb.Clear();

            foreach (var se in fullservice)
            {
                if (!se.FilterFromOutput)
                {
                    sb.AppendLine(se.LSBElement?.XenonAutoGen(options, ref indentDepth, indentSpace) ?? se.XenonCode ?? "");

                    // here we can attach pilot
                    GeneratePilotCommand(sb, ref indentDepth, indentSpace, options, se);

                    se.XenonCode = sb.ToString();
                }

                sb.Clear();
            }

            if (options.UseThemedHymns || options.UseThemedCreeds)
            {
                indentDepth--;
                sb.AppendLine("}");

                fullservice.Add(new ParsedLSBElement
                {
                    Generator = "XenonAutoGen",
                    AddedByInference = true,
                    XenonCode = sb.ToString(),
                });
            }
            sb.Clear();
        }

        private static void GeneratePilotCommand(StringBuilder sb, ref int indentDepth, int indentSpace, LSBImportOptions options, ParsedLSBElement se)
        {
            HashSet<CameraID> plannedCameras = new HashSet<CameraID>();
            foreach (var pcopt in options.PlannedCameras.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(PilotEnumMap)) && Attribute.IsDefined(x, typeof(BoolSettingAttribute))))
            {
                var camid = pcopt.GetCustomAttribute<PilotEnumMap>().ID;
                if ((bool)pcopt.GetValue(options.PlannedCameras))
                {
                    plannedCameras.Add(pcopt.GetCustomAttributes<PilotEnumMap>().First().ID);
                }

            }

            if (se.CameraUse.SafeActions.Any(x => plannedCameras.Contains(x.Key)) || se.CameraUse.RiskyActions.Any(x => plannedCameras.Contains(x.Key)))
            {
                int i = sb.Length - 1;
                while (i >= 0)
                {
                    if (sb[i].IsWhiteSpaceCharacter())
                    {
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
                sb.Length = i + 1;

                sb.AppendLine();

                sb.AppendLine(">>pilot{".Indent(indentDepth, indentSpace));
                indentDepth++;
                sb.AppendLine("<0>{".Indent(indentDepth, indentSpace));
                indentDepth++;
                foreach (var action in se.CameraUse.SafeActions.Where(x => plannedCameras.Contains(x.Key)))
                {
                    string camID;
                    string posPST;
                    string zoomPST;

                    if (!options.PilotCamIdMap.TryGetValue(action.Key, out camID))
                    {
                        camID = action.Key.ToString();
                    }
                    if (!options.PilotPresetMap.TryGetValue($"{action.Key.ToString().ToLower()}:{action.Value.Preset}", out posPST))
                    {
                        posPST = action.Value.Preset;
                    }
                    if (!options.PilotZoomMap.TryGetValue($"{action.Key.ToString().ToLower()}:{action.Value.Preset}", out zoomPST))
                    {
                        zoomPST = action.Value.Preset;
                    }


                    sb.AppendLine($"run[{camID}]({posPST})@15:{zoomPST};".Indent(indentDepth, indentSpace));
                }

                foreach (var action in se.CameraUse.RiskyActions.Where(x => plannedCameras.Contains(x.Key)))
                {
                    string camID;
                    string posPST;
                    string zoomPST;

                    if (!options.PilotCamIdMap.TryGetValue(action.Key, out camID))
                    {
                        camID = action.Key.ToString();
                    }
                    if (!options.PilotPresetMap.TryGetValue($"{action.Key.ToString().ToLower()}:{action.Value.Preset}", out posPST))
                    {
                        posPST = action.Value.Preset;
                    }
                    if (!options.PilotZoomMap.TryGetValue($"{action.Key.ToString().ToLower()}:{action.Value.Preset}", out zoomPST))
                    {
                        zoomPST = action.Value.Preset;
                    }


                    sb.AppendLine($"/// </MANUAL_UPDATE name='Flight Planner Stalled'>".Indent(indentDepth, indentSpace));
                    sb.AppendLine("// Flight planner: Camera movement required to resolve a future slide dependancy,".Indent(indentDepth, indentSpace));
                    sb.AppendLine("// but it conflicts with an expectation for the camera to be actively in use- preventing safe motion.".Indent(indentDepth, indentSpace));
                    sb.AppendLine($"//> run[{camID}]({posPST})@15:{zoomPST};".Indent(indentDepth, indentSpace));
                }


                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpace));
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpace));
                sb.AppendLine();
            }
        }
    }
}