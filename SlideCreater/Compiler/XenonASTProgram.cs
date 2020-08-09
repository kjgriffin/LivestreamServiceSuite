using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTProgram : IXenonASTElement
    {

        public List<XenonASTExpression> Expressions { get; set; } = new List<XenonASTExpression>();

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            foreach (var item in Expressions)
            {
                item.Generate(project, this);    
            }
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTProgram>");
            foreach (var item in Expressions)
            {
                item.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTProgram>");
        }

    }
}
