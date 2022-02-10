using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    internal class LSBElementAcknowledments : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public IElement SourceHTML { get; private set; }

        public string Text { get; private set; }

        public string DebugString()
        {
            return "";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions)
        {
            return $"acknowledments={{{Text}}}";
        }

        public static LSBElementAcknowledments Parse(IElement elem)
        {
            return new LSBElementAcknowledments()
            {
                PostsetCmd = "",
                SourceHTML = elem,
                Text = elem?.StrippedText(),
            };
        }
    }
}
