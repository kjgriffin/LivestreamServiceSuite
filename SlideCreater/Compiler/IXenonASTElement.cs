using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Compiler
{
    public interface IXenonASTElement
    {

        public void Generate(Project project);

        public void GenerateDebug(Project project);
        
    }
}
