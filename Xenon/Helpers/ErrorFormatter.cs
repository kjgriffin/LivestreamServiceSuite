using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler;

namespace Xenon.Helpers
{
    public static class ErrorFormatter
    {
        public static string Format(List<XenonCompilerMessage> logs)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("<table>");

            stringBuilder.Append("<th>Level</th>");
            stringBuilder.Append("<th>Error</th>");
            stringBuilder.Append("<th>Message</th>");
            stringBuilder.Append("<th>Source File</th>");
            stringBuilder.Append("<th>Token</th>");
            stringBuilder.Append("<th>Generator</th>");
            stringBuilder.Append("<th>Inner</th>");

            foreach (XenonCompilerMessage message in logs)
            {
                Format(message, stringBuilder);
            }

            stringBuilder.Append("</table>");

            return stringBuilder.ToString();
        }

        static string Format(XenonCompilerMessage message, StringBuilder sb)
        {
            sb.Append("<tr>");

            sb.Append($"<td>{message.Level}</td>");
            sb.Append($"<td>{message.ErrorName}</td>");
            sb.Append($"<td>{message.ErrorMessage}</td>");
            sb.Append($"<td>{message.SrcFile}</td>");
            sb.Append($"<td>{message.Token}</td>");
            sb.Append($"<td>{message.Generator}</td>");
            sb.Append($"<td>{message.Inner}</td>");

            sb.Append("</tr>");

            return sb.ToString();
        }


    }
}
