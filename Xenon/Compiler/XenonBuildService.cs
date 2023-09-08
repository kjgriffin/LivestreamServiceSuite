using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xenon.Analyzers;
using Xenon.Compiler.AST;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

using static Xenon.Renderer.WEB_RENDER_ENGINE;

namespace Xenon.Compiler
{

    public class XenonBuildService
    {

        long _failedslides;
        long _completedslidecount;
        long _reusedslides;


        public Project Project { get; private set; }

        public List<XenonCompilerMessage> Messages { get; private set; } = new List<XenonCompilerMessage>();

        readonly XenonCompiler compiler = new XenonCompiler();

        private ConcurrentDictionary<int, RenderedSlide> hashedOldSlides = new ConcurrentDictionary<int, RenderedSlide>();
        private ConcurrentDictionary<int, RenderedSlide> hashedNewSlides = new ConcurrentDictionary<int, RenderedSlide>();

        public void CleanSlides()
        {
            hashedNewSlides.Clear();
            hashedOldSlides.Clear();
        }

        public async Task<(bool success, Project project)> CompileProjectAsync(Project proj, IProgress<int> progress = null)
        {
            compiler.Logger.ClearErrors();
            try
            {
                var res = await Task.Run(() => compiler.Compile(proj, progress));
                PilotReportGenerator.YellAboutUnkownPilotCommands(proj, compiler.Logger);
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
                    await Task.Run(async () =>
                    {
                        try
                        {
                            slides.Add(await sr.RenderSlide(slide, Messages));
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

                /*
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
                */

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
            _failedslides = 0;
            _reusedslides = 0;
            _completedslidecount = 0;
            try
            {
                ConcurrentBag<RenderedSlide> slides = new ConcurrentBag<RenderedSlide>();
                SlideRenderer sr = new SlideRenderer(proj);
                var slidetasks = new List<Task>();

                if (progress != null)
                {
                    progress.Report(0);
                }

                foreach (var slide in proj.Slides)
                {
                    slidetasks.Add(Task.Run(() => RenderSlide(proj, progress, slides, sr, slide, fromclean)));
                }

                await Task.WhenAll(slidetasks).ConfigureAwait(false);

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
                        ErrorName = $"{_completedslidecount - _reusedslides} Rendered. {_reusedslides} Up to date.",
                        ErrorMessage = "",
                        Generator = "XenonBuildService-Summary",
                        Inner = "",
                        Level = XenonCompilerMessageType.Message,
                        Token = "",
                    });
                }

                if (_failedslides > 0)
                {
                    Messages.Add(new XenonCompilerMessage() { ErrorMessage = $"Rendering failed to render {_failedslides} slides.", ErrorName = "Failed to Render Slides", Level = XenonCompilerMessageType.Error });
                }

                return slides.ToList();
            }
            catch (Exception ex)
            {
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Project", Level = XenonCompilerMessageType.Error });
                return new List<RenderedSlide>();
            }

        }

        private async Task RenderSlide(Project proj, IProgress<int> progress, ConcurrentBag<RenderedSlide> slides, SlideRenderer sr, Slide slide, bool fromclean = true)
        {
            try
            {

                RenderedSlide rs;

                if (fromclean)
                {
                    // re-render it even if we don't strictly need to
                    rs = await sr.RenderSlide(slide, Messages);
                }
                else
                {
                    // if slide hasn't changed, we can just use the same result from the previous rendering
                    if (hashedOldSlides.TryGetValue(slide.Hash(), out rs))
                    {
                        // create a shallow copy, since we're going to modify the number
                        // but we're ok (and want) to have them all read the same underlying bitmap/streams/ other data
                        rs = hashedOldSlides[slide.Hash()].Clone();

                        Interlocked.Increment(ref _reusedslides);
                        // make sure to update the slide's number if it has changed though
                        rs.Number = slide.Number;
                        if (slide.NonRenderedMetadata.TryGetValue(XenonASTExpression.DATAKEY_CMD_SOURCECODE_LOOKUP, out var sourceCodeLookup))
                        {
                            rs.SourceLineRef = (int)sourceCodeLookup;
                        }

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
                        rs = await sr.RenderSlide(slide, Messages);
                    }
                }
                hashedNewSlides[slide.Hash()] = rs;
                slides.Add(rs);
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = $"Slide number ({slide.Number}) of type [{slide.Name}] was rendered.", ErrorName = "Render Slide Debug", Generator = $"renderer for slide number ({slide.Number})", Level = XenonCompilerMessageType.Debug });
                if (progress != null)
                {
                    Interlocked.Increment(ref _completedslidecount);
                    int prog = (int)(_completedslidecount / (double)proj.Slides.Count * 100);
                    progress.Report(prog);
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new XenonCompilerMessage() { ErrorMessage = ex.ToString(), ErrorName = "Error Rendering Slide", Generator = $"renderer for slide number ({slide.Number})", Level = XenonCompilerMessageType.Error });
                _failedslides++;
            }
        }


        public void Configure_WebRenderEngine(BROWSER type)
        {
            // this is a blocking call, so use a new task/thread so we don't deadlock a UI by accident
            CleanSlides();
            Task.Run(() => WEB_RENDER_ENGINE.Change_Driver_Preference(type));
        }

    }
}
