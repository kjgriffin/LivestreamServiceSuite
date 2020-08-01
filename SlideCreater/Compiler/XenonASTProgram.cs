using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTProgram : IXenonASTElement
    {

        public List<XenonASTExpression> Expressions { get; set; } = new List<XenonASTExpression>();

        public void Generate(Project project)
        {
            foreach (var item in Expressions)
            {
                item.Generate(project);    
            }
        }

    }
}
