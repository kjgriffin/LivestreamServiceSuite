using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTSlideBreak : IXenonASTCommand
    {
        public void Generate(Project project)
        {

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("</XenonASTSlideBreak>");
        }
    }
}
