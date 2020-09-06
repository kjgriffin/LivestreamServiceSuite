using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenon.AssetManagment;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    public class XenonBuildService
    {


        public Project Project { get; private set; }

        public List<XenonCompilerMessage> Messages { get; private set; } = new List<XenonCompilerMessage>();

        readonly XenonCompiler compiler = new XenonCompiler();

        public async Task<bool> BuildProject(string inputtext, List<ProjectAsset> Assets, IProgress<int> progress)
        {

            await Task.Run(() =>
            {
                Project = compiler.Compile(inputtext, Assets, progress);
            });

            return compiler.CompilerSucess;
        }

        public async Task<List<RenderedSlide>> RenderProject(Project project, IProgress<int> progress)
        {

            List<RenderedSlide> slides = new List<RenderedSlide>();

            await Task.Run(() =>
            {

                SlideRenderer sr = new SlideRenderer(project);

                progress.Report(0);

                int completedslidecount = 0;

                Parallel.ForEach(project.Slides, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (Slide s) =>
                 {
                     slides.Add(sr.RenderSlide(s.Number, Messages));
                     Interlocked.Increment(ref completedslidecount);
                     int prog = (int)(completedslidecount / (double)project.Slides.Count * 100);
                     progress.Report(prog);
                 });

            });

            return slides.OrderBy(s => s.Number).ToList();

        }





    }
}
