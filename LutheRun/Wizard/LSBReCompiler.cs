using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun.Wizard
{
    public class LSBReCompiler
    {

        public string Re_HTMLify(List<IElement> topLevelElements)
        {
            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = new LSBImportOptions();

            StringBuilder sb = new StringBuilder();

            sb.Append("<html><head></head><body>");

            foreach (var elem in topLevelElements)
            {
                parser._ParseLSBServiceElement(elem);

                // hack out elements
                List<ILSBElement> generated = new List<ILSBElement>(parser.ServiceElements);

                parser.CompileToXenon();
                var txt = parser.XenonText;

                parser.Clear();






                sb.Append($"<div style='border-color: red; border-style: solid'>{elem.OuterHtml}</div><div style='border-color: green; border-style: solid'>{txt}</div>");
                sb.AppendLine();
            }



            sb.Append("</body></html>");


            return sb.ToString();
        }


    }
}
