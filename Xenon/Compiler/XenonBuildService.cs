using System;
using System.Collections.Generic;
using System.Text;
using Xenon.AssetManagment;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    public class XenonBuildService
    {


        public Project Project { get; private set; }

        public List<XenonCompilerMessage> Messages { get; private set; } = new List<XenonCompilerMessage>();

        XenonCompiler compiler = new XenonCompiler();

        public bool BuildProject(string inputtext, List<ProjectAsset> Assets)
        {

            Project = compiler.Compile(inputtext, Assets);



            return compiler.CompilerSucess;
        }





    }
}
