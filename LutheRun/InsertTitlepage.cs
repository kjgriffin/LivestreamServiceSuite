using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class InsertTitlepage : ExternalElement
    {

        public override string XenonAutoGen(LSBImportOptions lSBImportOptions)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/// </MANUAL_UPDATE name='titlepage'>");
            sb.AppendLine("//> INSERTION POINT: titlepage");
            return sb.ToString();
        }

    }
}
