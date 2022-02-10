using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    internal class LSBElementResponsiveLiturgy : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public IElement SourceHTML { get; private set; } // this will be original root element containing all the liturgy that created this
        private List<IElement> _litPTags { get; set; } = new List<IElement>();

        public static ILSBElement Create(IElement root, List<IElement> ptags)
        {
            return new LSBElementResponsiveLiturgy()
            {
                _litPTags = new List<IElement>(ptags),
                SourceHTML = root
            };

        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_RESPONSIVE_LITURGY.";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions)
        {
            string litcontent = LSBResponsorialExtractor.ExtractResponsiveLiturgy(_litPTags);
            return $"#liturgyresponsive{Environment.NewLine}{{{Environment.NewLine}{litcontent}{Environment.NewLine}}}{PostsetCmd}{Environment.NewLine}";
        }
    }
}
