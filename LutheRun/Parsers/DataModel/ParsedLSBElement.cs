using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Pilot;
using System.Collections.Generic;

namespace LutheRun.Parsers.DataModel
{
    public class ParsedLSBElement
    {
        public ILSBElement LSBElement { get; set; } = null;
        public string Generator { get; set; } = "";
        public string XenonCode { get; set; } = "";
        public IEnumerable<IElement> SourceElements { get; set; } = null;
        public IElement ParentSourceElement { get; set; } = null;
        public bool FilterFromOutput { get; set; } = false;
        public bool AddedByInference { get; set; } = false;
        public bool ConsiderForServicification { get; set; } = true;
        internal BlockType BlockType { get; set; } = BlockType.UNKNOWN;
        internal CameraUsage CameraUse { get; set; } = new CameraUsage();
    }
}
