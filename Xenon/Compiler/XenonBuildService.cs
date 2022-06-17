using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Xenon.Analyzers;
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

        private ConcurrentDictionary<int, RenderedSlide> hashedOldSlides = new ConcurrentDictionary<int, RenderedSlide>();
        private ConcurrentDictionary<int, RenderedSlide> hashedNewSlides = new ConcurrentDictionary<int, RenderedSlide>();

        public async Task<(bool success, Project project)> CompileProjectAsync(Project proj, IProgress<int> progress = null)
        {
            compiler.Logger.ClearErrors();
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

        public async Task<List<RenderedSlide>> RenderProjectAsync(Project proj, IProgress<int> progress = null, bool doparallel = false, bool fromclean = true)
        {
            if (doparallel)
            {
                return await RenderProjectAsync_Parallel(proj, progress, fromclean);
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

                var report = PilotReportGenerator.GeneratePilotPresetReport(proj);
                Messages.Add(new XenonCompilerMessage()
                {
                    ErrorName = "Pilot Preset Use Report",
                    ErrorMessage = report,
                    Generator = "PilotReportGenerator",
                    Inner = "",
                    Level = XenonCompilerMessageType.Message,
                    Token = "",
                });

                return slides;
            }
            catch (Exception ex)
            {
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Project", Level = XenonCompilerMessageType.Error });
                return new List<RenderedSlide>();
            }
        }


        private async Task<List<RenderedSlide>> RenderProjectAsync_Parallel(Project proj, IProgress<int> progress = null, bool fromclean = true)
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
                int reusedslides = 0;

                foreach (var slide in proj.Slides)
                {
                    slidetasks.Add(Task.Run(() => RenderSlide(proj, progress, ref failedslides, slides, sr, ref completedslidecount, slide, ref reusedslides, fromclean)));
                }

                await Task.WhenAll(slidetasks);

                hashedOldSlides.Clear();
                foreach (var hs in hashedNewSlides)
                {
                    hashedOldSlides[hs.Key] = hs.Value;
                }
                hashedNewSlides.Clear();

                if (!fromclean)
                {
                    Messages.Add(new XenonCompilerMessage
                    {
                        ErrorName = $"{completedslidecount - reusedslides} Rendered. {reusedslides} Up to date.",
                        ErrorMessage = "",
                        Generator = "XenonBuildService-Summary",
                        Inner = "",
                        Level = XenonCompilerMessageType.Message,
                        Token = "",
                    });
                }

                var report = PilotReportGenerator.GeneratePilotPresetReport(proj);
                Messages.Add(new XenonCompilerMessage()
                {
                    ErrorName = "Pilot Preset Use Report",
                    ErrorMessage = report,
                    Generator = "PilotReportGenerator",
                    Inner = "",
                    Level = XenonCompilerMessageType.Message,
                    Token = "",
                });


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

        private void RenderSlide(Project proj, IProgress<int> progress, ref int failedslides, ConcurrentBag<RenderedSlide> slides, SlideRenderer sr, ref int completedslidecount, Slide slide, ref int reusedslides, bool fromclean = true)
        {
            try
            {

                RenderedSlide rs;

                if (fromclean)
                {
                    // re-render it even if we don't strictly need to
                    rs = sr.RenderSlide(slide, Messages);
                }
                else
                {
                    // if slide hasn't changed, we can just use the same result from the previous rendering
                    if (hashedOldSlides.TryGetValue(slide.Hash(), out rs))
                    {
                        // create a shallow copy, since we're going to modify the number
                        // but we're ok (and want) to have them all read the same underlying bitmap/streams/ other data
                        rs = hashedOldSlides[slide.Hash()].Clone();

                        Interlocked.Increment(ref reusedslides);
                        // make sure to update the slide's number if it has changed though
                        rs.Number = slide.Number;
                        Messages.Add(new XenonCompilerMessage
                        {
                            ErrorName = "Re-using slide",
                            ErrorMessage = "Slide generated identically. Use previously rendered copy.",
                            Level = XenonCompilerMessageType.Debug,
                            Generator = "XenonBuildService::RenderSlide",
                            Inner = "",
                            Token = "",
                        });
                    }
                    else
                    {
                        rs = sr.RenderSlide(slide, Messages);
                    }
                }
                hashedNewSlides[slide.Hash()] = rs;
                slides.Add(rs);
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
        }


    }
}
