using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    public interface IXenonASTElement
    {

        public void Generate(Project project, IXenonASTElement _Parent);

        public void GenerateDebug(Project project);
        
    }
}
