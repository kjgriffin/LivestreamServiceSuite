using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    public interface ILSBElement
    {

        public string PostsetCmd { get; set; }
        public string DebugString();
        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace);


        public IElement SourceHTML { get; }


    }
}
