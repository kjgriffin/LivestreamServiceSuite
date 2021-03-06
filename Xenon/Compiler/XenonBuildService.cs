﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        public async Task<bool> BuildProject(Project proj, string inputtext, List<ProjectAsset> Assets, IProgress<int> progress)
        {
            await Task.Run(() =>
            {
                try
                {
                    Project = compiler.Compile(proj, inputtext, Assets, progress);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Master Compiler Exception");
                }
            });

            Messages.AddRange(compiler.Logger.AllErrors);
            return compiler.CompilerSucess;
        }

        public async Task<List<RenderedSlide>> RenderProject(Project project, IProgress<int> progress)
        {

            List<RenderedSlide> slides = new List<RenderedSlide>();

            try
            {

                await Task.Run(() =>
                {

                    SlideRenderer sr = new SlideRenderer(project);

                    progress.Report(0);

                    int completedslidecount = 0;

                    Parallel.ForEach(project.Slides, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, (Slide s) =>
                     {
                         slides.Add(sr.RenderSlide(s, Messages));
                         Interlocked.Increment(ref completedslidecount);
                         int prog = (int)(completedslidecount / (double)project.Slides.Count * 100);
                         progress.Report(prog);
                     });

                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Master Renderer Exception");
            }


            return slides.OrderBy(s => s.Number).ToList();

        }





    }
}
