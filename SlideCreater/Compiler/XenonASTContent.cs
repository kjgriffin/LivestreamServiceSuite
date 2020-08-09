using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTContent : IXenonASTElement
    {
        public string TextContent { get; set; }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            throw new NotImplementedException();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTContent>");
            Debug.WriteLine(TextContent);
            Debug.WriteLine("</XenonASTContent>");
        }
    }
}
