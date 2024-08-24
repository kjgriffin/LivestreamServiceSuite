using AngleSharp.Dom;

using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun.Wizard
{
    public class LSBReCompiler
    {

        public string GenerateHTMLReport(List<ParsedLSBElement> fullservice, string[] cssfiles, LSBImportOptions opts = null)
        {
            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = opts ?? new LSBImportOptions();

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
                sb.Append($"<div style='word-break: break-word;'>{EscapeXenonTextInHTML(elem.Generator, false)}</div>");
                sb.Append($"<div>{EscapeXenonTextInHTML($"Block Type: [{elem.BlockType}] id<{elem.ElementOrder}>", false)}</div>");
                sb.Append($"<div>{EscapeXenonTextInHTML("Out of Band Info:" + string.Join(';', elem.OutOfBandInfo.Select(v => v.Key + " : " + v.Value.ToString())), false)}</div>");
                sb.Append($"</div>");


                sb.Append($"<div style='border-color: black; border-style: solid; width: 250px; min-width: 250px; padding: 10px; white-space: break-spaces;'>");
                sb.Append($"<div>{EscapeXenonTextInHTML(elem.CameraUse.FormatForHTML(), false)}</div>");
                sb.Append($"</div>");

                sb.Append($"<div style='line-height: 1px; padding-top: 10px; white-space: nowrap;'>{EscapeXenonTextInHTML(elem.XenonCode)}</div>");

                sb.Append("</div>");
                sb.AppendLine();
            }

            sb.Append("</body></html>");

            return sb.ToString();

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
