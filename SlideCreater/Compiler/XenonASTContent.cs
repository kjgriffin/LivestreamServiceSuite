using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTContent : IXenonASTElement
    {
        public string TextContent { get; set; }

        public void Generate(Project project)
        {
            throw new NotImplementedException();
        }
    }
}
