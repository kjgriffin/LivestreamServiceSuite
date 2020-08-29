using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xenon.AssetManagment;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    public class XenonBuildService
    {


        public Project Project { get; private set; }

        public List<XenonCompilerMessage> Messages { get; private set; } = new List<XenonCompilerMessage>();

        XenonCompiler compiler = new XenonCompiler();

        public async Task<bool> BuildProject(string inputtext, List<ProjectAsset> Assets, IProgress<int> progress)
        {

            await Task.Run(() =>
            {
                Project = compiler.Compile(inputtext, Assets, progress);
            });

            return compiler.CompilerSucess;
        }





    }
}
