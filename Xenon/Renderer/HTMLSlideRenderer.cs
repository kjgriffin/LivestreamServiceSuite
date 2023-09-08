using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using CoreHtmlToImage;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    public enum BROWSER
    {
        Chrome,
        Firefox,
        Edge,
    }

    static class WEB_RENDER_ENGINE
    {
        private static WebDriver driver;
        private static bool initialized = false;
        private static BROWSER engineType = BROWSER.Chrome;

        public static void Change_Driver_Preference(BROWSER type)
        {
            Shutdown_Driver();
            engineType = type;
        }

        static WebDriver PREFBROWSER
        {
            get
            {
                if (!initialized || driver == null)
                {
                    driver = EngineSpinup();
                }
                return driver;
            }
        }

        private static WebDriver EngineSpinup()
        {
            initialized = true;
            switch (engineType)
            {
                case BROWSER.Firefox:
                case BROWSER.Edge:
                    return Spinup_Edge();
                case BROWSER.Chrome:
                default:
                    return Spinup_Chrome();
            }
        }

        private static void Shutdown_Driver()
        {
            if (initialized)
            {
                driver?.Quit();
                driver = null;
                initialized = false;
            }
        }

        private static WebDriver Spinup_Chrome()
        {
            ChromeOptions opts = new ChromeOptions();
            opts.AddArgument("window-size=1920x1080");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbox");
            opts.AddArgument("--headless");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var _driver = new ChromeDriver(service, opts);

            StartDriverThread();

            _workerThread.Name = "CHROME_RENDER_THREAD";

            return _driver;
        }
        private static WebDriver Spinup_Edge()
        {
            EdgeOptions opts = new EdgeOptions();
            opts.AddArgument("window-size=1920x1080");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbox");
            opts.AddArgument("--headless");
            EdgeDriverService service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var _driver = new EdgeDriver(service, opts);

            StartDriverThread();

            _workerThread.Name = "EDGE_RENDER_THREAD";

            return _driver;
        }
        private static WebDriver Spinup_Firefox()
        {
            FirefoxOptions opts = new FirefoxOptions();
            opts.AddArgument("window-size=1920x1080");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbox");
            opts.AddArgument("--headless");
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var _driver = new FirefoxDriver(service, opts);

            StartDriverThread();

            _workerThread.Name = "FIREFOX_RENDER_THREAD";

            return _driver;
        }


        private static void StartDriverThread()
        {
            if (!(_workerThread?.IsAlive == true))
            {
                _workerThread = new Thread(RunJobs);
                _workerThread.IsBackground = true;
                _workerThread.Start();
            }
        }



        class WorkItem
        {
            public string HTML { get; set; }
            public Guid ReqID { get; set; }
        }

        static ConcurrentQueue<WorkItem> jobs = new ConcurrentQueue<WorkItem>();
        static Thread _workerThread;
        static ManualResetEvent _workAvailable = new ManualResetEvent(false);
        static ManualResetEvent _doneReady = new ManualResetEvent(false);
        static ConcurrentDictionary<Guid, Image<Bgra32>> _rendered = new ConcurrentDictionary<Guid, Image<Bgra32>>();
        static long Completed = 0;
        static readonly TimeSpan timeout = TimeSpan.FromSeconds(3);


        private static void RunJobs()
        {
            while (true)
            {
                _workAvailable.WaitOne(timeout);
                // perform all work available
                while (jobs.TryDequeue(out var job))
                {
                    var img = RenderWithHeadlessBrowser(job.HTML);
                    _rendered[job.ReqID] = img;
                    Interlocked.Increment(ref Completed);
                }
                // signal that we've finished
                // work was requested by someone, so they can reset this
                if (Interlocked.Add(ref Completed, 0) > 0)
                {
                    _doneReady.Set();
                }
            }
        }

        public static async Task<Image<Bgra32>> PerformRenderWithHeadlessBrowser(string html)
        {
            if (!initialized)
            {
                EngineSpinup();
            }

            var _internal = await Task.Run(async () =>
            {

                // spinup a job here
                Guid id = Guid.NewGuid();
                jobs.Enqueue(new WorkItem
                {
                    HTML = html,
                    ReqID = id,
                });
                _workAvailable.Set();

                // wait for completion
                bool look = true;
                while (look)
                {
                    _doneReady.WaitOne(timeout);

                    // check if we've rendered this job
                    if (_rendered.TryGetValue(id, out var res))
                    {
                        _rendered.Remove(id, out _);
                        // keep blocking stuff again because
                        var finished = Interlocked.Decrement(ref Completed);
                        if (finished == 0)
                        {
                            _doneReady.Reset();
                        }
                        return res;
                    }
                    else
                    {
                        // perhaps??
                        await Task.Delay(500).ConfigureAwait(false);
                    }
                }
#if DEBUG
                Debugger.Break();
#endif
                return default(Image<Bgra32>);
            });

            return _internal;
        }

        private static Image<Bgra32> RenderWithHeadlessBrowser(string html)
        {

            var htmlFile = Path.GetTempFileName() + ".html";
            using (StreamWriter sw = new StreamWriter(htmlFile))
            {
                sw.Write(html);
            }

            // hmmmm, looks like we need a file on disk for this.
            // get it out of the project's tmp folder

            string content = "file:///" + htmlFile;
            PREFBROWSER.Navigate().GoToUrl(content);
            Screenshot ss = ((ITakesScreenshot)PREFBROWSER).GetScreenshot();
            var img = Image.Load<Bgra32>(ss.AsByteArray);

            // cleanup up
            // close browser?
            //driver.Quit();
            //File.Delete(htmlFile);

            return img;
        }
    }

    internal class HTMLSlideRenderer : ISlideRenderer, ISlideRenderer<HTMLLayoutInfo>, ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>
    {
        public static string DATAKEY_TEXTS { get => "content-replacements"; }
        public static string DATAKEY_IMGS { get => "content-images"; }

        ILayoutInfoResolver<HTMLLayoutInfo> ISlideRenderer<HTMLLayoutInfo>.LayoutResolver { get => new HTMLLayoutInfo(); }

        async Task<(Image<Bgra32> main, Image<Bgra32> key)> ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.GetPreviewForLayout(string layoutInfo)
        {
            HTMLLayoutInfo layout = JsonSerializer.Deserialize<HTMLLayoutInfo>(layoutInfo);

            var html_main = layout.HTMLSrc;
            var html_key = layout.HTMLSrc_KEY;
            var css = layout.CSSSrc;

            html_main = HTML_INJECT_STYLE(html_main, css);
            html_key = HTML_INJECT_STYLE(html_key, css);

            var itask = await WEB_RENDER_ENGINE.PerformRenderWithHeadlessBrowser(html_main).ConfigureAwait(false);
            var iktask = await WEB_RENDER_ENGINE.PerformRenderWithHeadlessBrowser(html_key).ConfigureAwait(false);

            Image<Bgra32> ibmp = itask;
            Image<Bgra32> ikbmp = iktask;

            return (ibmp, ikbmp);
        }

        bool ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        async Task<RenderedSlide> ISlideRenderer.VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide result)
        {
            if (slide.Format == SlideFormat.HTML)
            {
                HTMLLayoutInfo layout = (this as ISlideRenderer<HTMLLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                var render = await RenderSlide(slide, Messages, assetResolver, layout);
                return render;
            }
            return result;
        }
        public async Task<RenderedSlide> RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, IAssetResolver assetResolver, HTMLLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            //res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Liturgy" : layout.SlideType;

            var html_slide = layout.HTMLSrc;
            var html_key = layout.HTMLSrc_KEY;
            var css = layout.CSSSrc;

            if (slide.Data.TryGetValue(DATAKEY_TEXTS, out var texts))
            {
                html_slide = HTML_INSERT(html_slide, texts as Dictionary<string, string>);
                html_key = HTML_INSERT(html_key, texts as Dictionary<string, string>);
            }

            if (slide.Data.TryGetValue(DATAKEY_IMGS, out var imgs))
            {
                html_slide = ASSET_INSERT(html_slide, imgs as Dictionary<string, string>, assetResolver);
                html_key = ASSET_INSERT(html_key, imgs as Dictionary<string, string>, assetResolver);
            }

            html_slide = HTML_INJECT_STYLE(html_slide, css);
            html_key = HTML_INJECT_STYLE(html_key, css);

            res.RenderedAs = EXTRACT_SLIDE_TYPE(html_slide, "Liturgy");

            Image<Bgra32> ibmp = await WEB_RENDER_ENGINE.PerformRenderWithHeadlessBrowser(html_slide).ConfigureAwait(false);
            Image<Bgra32> ikbmp = await WEB_RENDER_ENGINE.PerformRenderWithHeadlessBrowser(html_key).ConfigureAwait(false);

            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;
        }


        private string HTML_INJECT_STYLE(string src, string css)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(src);

            // find all elements that are tagged
            var style = document.CreateElement<IHtmlStyleElement>();
            style.TextContent = css;
            document.Head.AppendChild(style);

            return document.ToHtml();

        }

        private string HTML_INSERT(string src, Dictionary<string, string> textReplace)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(src);

            // find all elements that are tagged
            foreach (var tagid in textReplace.Keys)
            {
                var tags = document.QuerySelectorAll($"*[data-id='{tagid}']");
                foreach (var tag in tags)
                {
                    tag.InnerHtml = textReplace[tagid];
                }
            }

            return document.ToHtml();
        }

        private string ASSET_INSERT(string src, Dictionary<string, string> assetRefs, IAssetResolver assetResolver)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(src);

            // find all elements that are tagged
            foreach (var tagid in assetRefs.Keys)
            {
                var tags = document.QuerySelectorAll($"*[data-img='{tagid}']");
                foreach (var tag in tags)
                {
                    if (tag.LocalName == "img")
                    {
                        if (assetRefs.TryGetValue(tagid, out var aref))
                        {
                            var asset = assetResolver.GetProjectAssetByName(aref);
                            (tag as IHtmlImageElement).Source = asset.CurrentPath;
                        }
                    }
                }
            }

            return document.ToHtml();
        }

        private string EXTRACT_SLIDE_TYPE(string src, string defaultValue)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(src);

            // find all elements that are tagged
            var tag = document.Head.QuerySelectorAll($"meta[data-slide-type]").FirstOrDefault();

            if (tag != null)
            {
                var att = tag.GetAttribute("data-slide-type");
                if (att != null)
                {
                    return att;
                }
            }
            return defaultValue;
        }

    }
}
