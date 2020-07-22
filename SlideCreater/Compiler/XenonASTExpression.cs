using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTExpression : IXenonASTElement
    {
        public IXenonASTCommand Command { get; set; }

        public void Generate(Project project)
        {
            Command.Generate(project);
        }
    }
}
