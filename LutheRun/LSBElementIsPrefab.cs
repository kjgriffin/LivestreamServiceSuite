using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class LSBElementIsPrefab : ILSBElement
    {
        public string Prefab { get; private set; }
        public string SourceText { get; private set; }

        public LSBElementIsPrefab(string command, string elementtext)
        {
            Prefab = command;
            SourceText = elementtext;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_PREFAB[{Prefab}]. For Source Element: {SourceText}";
        }

        public string XenonAutoGen()
        {
            return $"/// <XENON_AUTO_GEN>\r\n#{Prefab}\r\n/// </XENON_AUTO_GEN>";
        }
    }
}
