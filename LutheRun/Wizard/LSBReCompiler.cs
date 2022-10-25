using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace LutheRun.Wizard
{
    public class LSBReCompiler
    {

        //public string Re_HTMLify(List<IElement> topLevelElements)
        //{
        //    LSBParser parser = new LSBParser();
        //    parser.LSBImportOptions = new LSBImportOptions();

        //    StringBuilder sb = new StringBuilder();

        //    sb.Append("<html><head></head><body>");

        //    foreach (var elem in topLevelElements)
        //    {
        //        parser._ParseLSBServiceElement(elem);

        //        // hack out elements
        //        List<ILSBElement> generated = new List<ILSBElement>(parser.ServiceElements);

        //        parser.CompileToXenon();
        //        var txt = parser.XenonText;

        //        parser.Clear();

        //        sb.Append($"<div style='border-color: red; border-style: solid'>{elem.OuterHtml}</div><div style='border-color: green; border-style: solid'>{txt}</div>");
        //        sb.AppendLine();
        //    }

        //    sb.Append("</body></html>");

        //    return sb.ToString();
        //}

        public string GenerateHTMLReport(List<ParsedLSBElement> fullservice, params string[] cssfiles)
        {
            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = new LSBImportOptions();

            StringBuilder sb = new StringBuilder();

            sb.Append("<!DOCTYPE HTML>");
            sb.Append("<html><head></head>");
            foreach (var css in cssfiles)
            {
                sb.Append($"<link href=\"{css}\" rel=\"stylesheet\">");
            }
            sb.Append("</head><body>");

            foreach (var elem in fullservice)
            {
                sb.Append("<div style='display: flex; flex-flow: row;'>");

                if (elem.SourceElements?.Any() == true)
                {
                    sb.Append($"<div style='border-color: {(elem.FilterFromOutput ? "red" : elem.AddedByInference ? "blue" : "green")}; border-style: solid; border-width: 5px; width: 650px; min-width: 650px; padding: 10px;'>");
                    foreach (var se in elem.SourceElements)
                    {
                        sb.Append($"<div>{se.OuterHtml}</div>");
                    }
                    sb.Append("</div>");
                }
                else
                {
                    sb.Append($"<div style='border-color: blue; border-style: solid; width: 650px; min-width: 650px; color: red; padding: 10px;'>NO LSB SOURCE</div>");
                }

                sb.Append($"<div style='border-color: black; border-style: solid; width: 250px; min-width: 250px; padding: 10px;'>");
                sb.Append($"<div>{EscapeXenonTextInHTML(elem.Generator, false)}</div>");
                sb.Append($"<div>{EscapeXenonTextInHTML($"Block Type: [{elem.BlockType}]", false)}</div>");
                sb.Append($"</div>");


                sb.Append($"<div style='border-color: black; border-style: solid; width: 250px; min-width: 250px; padding: 10px; white-space: break-spaces;'>");
                sb.Append($"<div>{EscapeXenonTextInHTML(elem.CameraUse.Format(), false)}</div>");
                sb.Append($"</div>");

                sb.Append($"<div style='line-height: 1px; padding-top: 10px; white-space: nowrap;'>{EscapeXenonTextInHTML(elem.XenonCode)}</div>");

                sb.Append("</div>");
                sb.AppendLine();
            }

            sb.Append("</body></html>");

            return sb.ToString();

        }

        public void CompileToXenonMappedToSource(string serviceFileName, LSBImportOptions options, List<ParsedLSBElement> fullservice)
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
                    macros = LSBParser.ApplySeasonalMacroText(fullservice, macros);
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


        public string EscapeXenonTextInHTML(string source, bool fixWhitespace = true)
        {
            // whitespace gets destroyed
            // replace new-lines with <p>
            // replace spaces with nbsp
            var res = source.Replace($"<", "&lt;");
            res = res.Replace($">", "&gt;");
            if (fixWhitespace)
            {
                res = res.Replace($" ", "&nbsp;");
                res = res.Replace($"{Environment.NewLine}", "</p>");
            }

            return res;
        }




    }


    public class XenonChunkMappedToSource
    {
        public IElement Source { get; set; }
        public string XenonCode { get; set; }
        public string Generator { get; set; }

    }

}
