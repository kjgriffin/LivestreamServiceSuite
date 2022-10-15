using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    abstract class ExternalElement : ILSBElement
    {
        public virtual string PostsetCmd { get; set; }

        public IElement SourceHTML { get; private set; }

        public virtual string DebugString()
        {
            return $"/// XENON DEBUG::Added External Element";
        }

        public virtual string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace)
        {
            return "";
        }

        public virtual BlockType BlockType()
        {
            return LutheRun.BlockType.UNKNOWN;
        }
    }
}
