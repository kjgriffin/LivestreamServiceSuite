using System;
using System.Collections.Concurrent;
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
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        Project = await compiler.Compile(proj, inputtext, Assets, progress);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Master Compiler Exception");
            }

            Messages.AddRange(compiler.Logger.AllErrors);
            return compiler.CompilerSucess;
        }

        public async Task<(bool success, Project project)> BuildProjectAsync(Project proj, IProgress<int> progress = null)
        {
            try
            {
                var res = await Task.Run(() => compiler.Compile(proj, progress));
                Messages.AddRange(compiler.Logger.AllErrors);
                return (true, res);
            }
            catch (Exception ex)
            {
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Building Project", Level = XenonCompilerMessageType.Error });
                return (false, null);
            }
        }

        public async Task<List<RenderedSlide>> RenderProjectAsync(Project proj, IProgress<int> progress = null, bool doparallel = false)
        {
            if (doparallel)
            {
                return await RenderProjectAsync_Parallel(proj, progress);
            }
            return await RenderProjectAsync_Synchronous(proj, progress);
        }

        private async Task<List<RenderedSlide>> RenderProjectAsync_Synchronous(Project proj, IProgress<int> progress = null)
        {
            int failedslides = 0;
            try
            {
                if (progress != null)
                {
                    progress.Report(0);
                }

                SlideRenderer sr = new SlideRenderer(proj);
                List<RenderedSlide> slides = new List<RenderedSlide>();
                int completedslidecount = 0;

                foreach (var slide in proj.Slides)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            slides.Add(sr.RenderSlide(slide, Messages));
                            Messages.Add(new XenonCompilerMessage() { ErrorMessage = $"Slide number ({slide.Number}) of type [{slide.Name}] was rendered.", ErrorName = "Render Slide Debug", Generator = $"renderer for slide number ({slide.Number})", Level = XenonCompilerMessageType.Debug });
                        }
                        catch (Exception ex)
                        {
                            Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Slide", Generator = $"renderer for slide number ({slide.Number})", Level = XenonCompilerMessageType.Error });
                            failedslides++;
                        }
                    });
                    if (progress != null)
                    {
                        Interlocked.Increment(ref completedslidecount);
                        int prog = (int)(completedslidecount / (double)proj.Slides.Count * 100);
                        progress.Report(prog);
                    }
                }

                if (failedslides > 0)
                {
                    Messages.Add(new XenonCompilerMessage() { ErrorMessage = $"Rendering failed to render {failedslides} slides.", ErrorName = "Failed to Render Slides", Level = XenonCompilerMessageType.Error });
                }
                return slides;
            }
            catch (Exception ex)
            {
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Project", Level = XenonCompilerMessageType.Error });
                return new List<RenderedSlide>();
            }
        }


        private async Task<List<RenderedSlide>> RenderProjectAsync_Parallel(Project proj, IProgress<int> progress = null)
        {
            int failedslides = 0;
            try
            {
                ConcurrentBag<RenderedSlide> slides = new ConcurrentBag<RenderedSlide>();
                SlideRenderer sr = new SlideRenderer(proj);
                var slidetasks = new List<Task>();

                if (progress != null)
                {
                    progress.Report(0);
                }

                int completedslidecount = 0;

                foreach (var slide in proj.Slides)
                {
                    Action a = new Action(() =>
                    {
                        try
                        {
                            slides.Add(sr.RenderSlide(slide, Messages));
                            Messages.Add(new XenonCompilerMessage() { ErrorMessage = $"Slide number ({slide.Number}) of type [{slide.Name}] was rendered.", ErrorName = "Render Slide Debug", Generator = $"renderer for slide number ({slide.Number})", Level = XenonCompilerMessageType.Debug });
                            if (progress != null)
                            {
                                Interlocked.Increment(ref completedslidecount);
                                int prog = (int)(completedslidecount / (double)proj.Slides.Count * 100);
                                progress.Report(prog);
                            }
                        }
                        catch (Exception ex)
                        {
                            Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Slide", Generator = $"renderer for slide number ({slide.Number})", Level = XenonCompilerMessageType.Error });
                            failedslides++;
                        }
                    });
                    slidetasks.Add(Task.Run(a));
                }

                await Task.WhenAll(slidetasks);
                
                if (failedslides > 0)
                {
                    Messages.Add(new XenonCompilerMessage() { ErrorMessage = $"Rendering failed to render {failedslides} slides.", ErrorName = "Failed to Render Slides", Level = XenonCompilerMessageType.Error });
                }
                
                return slides.ToList();
            }
            catch (Exception ex)
            {
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Project", Level = XenonCompilerMessageType.Error });
                return new List<RenderedSlide>();
            }

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
