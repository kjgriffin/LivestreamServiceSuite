using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class LSBElementIsPrefab : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public string Prefab { get; private set; }
        public string SourceText { get; private set; }

        public IElement SourceHTML { get; private set; }

        public LSBElementIsPrefab(string command, string elementtext, IElement source)
        {
            Prefab = command;
            SourceText = elementtext;
            SourceHTML = source;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_PREFAB[{Prefab}]. For Source Element: {SourceText}";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions)
        {
            return $"#{Prefab}";
        }
    }
}
