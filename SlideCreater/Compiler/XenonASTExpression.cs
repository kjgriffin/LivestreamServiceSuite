using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTExpression : IXenonASTElement
    {
        public IXenonASTCommand Command { get; set; }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Command?.Generate(project, _Parent);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTExperession>");
            Command?.GenerateDebug(project);
            Debug.WriteLine("</XenonASTExperession>");
        }
    }
}
