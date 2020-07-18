using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTLiturgy : IXenonASTCommand
    {
        public List<string> Content { get; set; } = new List<string>();

        public void Generate()
        {
            throw new NotImplementedException();
        }
    }
}
