using System;
using System.Collections.Generic;
using System.Text;
using LutheRun.Parsers;

namespace LutheRun.Elements
{
    class InsertTitlepage : ExternalElement
    {

        public override string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/// </MANUAL_UPDATE name='titlepage'>".Indent(indentDepth, indentSpaces));
            sb.AppendLine("//> INSERTION POINT: titlepage".Indent(indentDepth, indentSpaces));
            return sb.ToString();
        }

    }
}
