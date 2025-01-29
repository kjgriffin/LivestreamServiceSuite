using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using CoreHtmlToImage;

using Microsoft.Extensions.Logging.Abstractions;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.DevTools.V130.Runtime;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

using PuppeteerSharp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
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
        PUPPETEER,
    }

    static class BROWSER_RENDER_ENGINE_MK2
    {
        static volatile BROWSER _browserPref = BROWSER.PUPPETEER;
        static volatile Dictionary<BROWSER, WebDriver> _drivers = new Dictionary<BROWSER, WebDriver>();

        static volatile BrowserFetcher _fetcher;
        static volatile IBrowser _puppet;
        static volatile SemaphoreSlim _puppetSem = new SemaphoreSlim(1);

        static volatile object _renderLock = new object();
        static volatile object _initLock = new object();

        public static void Change_Driver_Preference(BROWSER type)
        {
            _browserPref = type;
        }


        #region Driver Spinup

        private static WebDriver Spinup_Chrome()
        {
            ChromeOptions opts = new ChromeOptions();
            //opts.AddArgument("window-size=1920x1080");
            opts.AddArgument("window-size=1280x720");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbox");
            opts.AddArgument("--headless=old");
            //opts.AddArgument("--remote-debugging-pipe");
            //opts.AddArgument("--remote-debugging-port=9222");
            //opts.AddArgument("--disable-dev-shm-usage");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var _driver = new ChromeDriver(service, opts, TimeSpan.FromSeconds(5));
            return _driver;
        }
        private static WebDriver Spinup_Edge()
        {
            EdgeOptions opts = new EdgeOptions();
            opts.AddArgument("window-size=1920x1080");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbox");
            opts.AddArgument("--headless=old");
            EdgeDriverService service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var _driver = new EdgeDriver(service, opts);
            return _driver;
        }
        private static WebDriver Spinup_Firefox()
        {
            FirefoxOptions opts = new FirefoxOptions();
            opts.AddArgument("--window-size=1920,1080");
            opts.AddArgument("--width=1920");
            opts.AddArgument("--height=1080");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbox");
            opts.AddArgument("--headless");
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var _driver = new FirefoxDriver(service, opts);
            _driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
            _driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);

            return _driver;
        }
        #endregion

        static Task<WebDriver> InitTask(BROWSER type)
        {
            WebDriver driver;
            lock (_initLock)
            {
                if (_drivers.ContainsKey(type))
                {
                    return Task.FromResult(_drivers[type]);
                }
                switch (_browserPref)
                {
                    case BROWSER.Edge:
                        driver = Spinup_Edge();
                        break;
                    case BROWSER.Firefox:
                        driver = Spinup_Firefox();
                        break;
                    case BROWSER.Chrome:
                    default:
                        driver = Spinup_Chrome();
                        break;
                }
                _drivers[_browserPref] = driver;
            }
            return Task.FromResult(driver);
        }

        public static async Task<Image<Bgra32>> PerformRenderWithHeadlessBrowser(string html)
        {
            if (_browserPref == BROWSER.PUPPETEER)
            {
                // concurrent access is probably OK
                if (_puppet != null)
                {
                    var res = await Task.Run(() => RenderWithHeadlessPuppeteer(html, _puppet));
                    return res;
                }

                // only need to manage if not
                await _puppetSem.WaitAsync();
                if (_puppet == null)
                {
                    await Task.Run(async () =>
                    {
                        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string cachePath = Path.GetFullPath(Path.Combine(programFiles, "chrome-headless-browser"));
                        var opts = new BrowserFetcherOptions
                        {
                            Browser = SupportedBrowser.ChromeHeadlessShell,
                        };
                        _fetcher = new BrowserFetcher(opts);
                        _fetcher.CacheDir = cachePath;
                        try
                        {
                            await _fetcher.DownloadAsync();
                            var installed = _fetcher.GetInstalledBrowsers().Where(x => x.Browser == SupportedBrowser.ChromeHeadlessShell).FirstOrDefault();
                            var exePath = _fetcher.GetExecutablePath(installed.BuildId);
                            _puppet = await Puppeteer.LaunchAsync(new LaunchOptions
                            {
                                Headless = true,
                                Args = new string[] { "--single-process" },
                                ExecutablePath = exePath,
                            });
                            _puppet.Closed += _puppet_Closed;
                        }
                        catch (Exception ex)
                        {
                            // geee, what should we do?
                            if (_puppet != null)
                            {
                                _puppet.Closed -= _puppet_Closed;
                                _puppet?.Dispose();
                                _puppet = null;
                            }
                        }
                    });
                }
                _puppetSem.Release();

                var res2 = await Task.Run(() => RenderWithHeadlessPuppeteer(html, _puppet));
                return res2;
            }

            WebDriver driver;
            if (!_drivers.TryGetValue(_browserPref, out driver))
            {
                // get a new one spun up
                await Task.Run(async () =>
                {
                    driver = await InitTask(_browserPref);
                });
            }


            return await Task.Run(() => RenderWithHeadlessBrowser(html, driver));
        }

        private static void _puppet_Closed(object sender, EventArgs e)
        {
            _puppet.Dispose();
            _puppet = null;
        }

        private static Image<Bgra32> RenderWithHeadlessBrowser(string html, WebDriver driver)
        {

            lock (_renderLock)
            {
                var htmlFile = Path.GetTempFileName() + ".html";
                using (StreamWriter sw = new StreamWriter(htmlFile))
                {
                    sw.Write(html);
                }

                // hmmmm, looks like we need a file on disk for this.
                // get it out of the project's tmp folder

                string content = "file:///" + htmlFile;
                driver.Navigate().GoToUrl(content);
                Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                var img = Image.Load<Bgra32>(ss.AsByteArray);

                return img;
            }
        }

        private static async Task<Image<Bgra32>> RenderWithHeadlessPuppeteer(string html, IBrowser driver)
        {
            // Big improvements:
            // 1- we can render each request in parallel, by using a new page
            // 2- we don't need to write tmp files to disk to load, but can directly navigate to a string html page
            await using var page = await driver.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Height = 1080, Width = 1920 });
            await page.SetContentAsync(html);
            var stream = await page.ScreenshotStreamAsync(new ScreenshotOptions { Type = ScreenshotType.Png });
            var img = Image.Load<Bgra32>(stream);
            return img;
        }

        internal static async Task SpindownPUPPET()
        {
            await _puppetSem.WaitAsync();
            await _puppet.CloseAsync();
        }
    }


    static class WEB_RENDER_ENGINE
    {
        private static WebDriver driver;
        private static volatile bool initialized = false;
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
                    EngineSpinup();
                }
                return driver;
            }
        }

        private static object _singleInitLock = new object();
        private static void EngineSpinup()
        {
            lock (_singleInitLock)
            {
                if (!initialized)
                {
                    switch (engineType)
                    {
                        case BROWSER.Firefox:
                            driver = Spinup_Firefox();
                            break;
                        case BROWSER.Edge:
                            driver = Spinup_Edge();
                            break;
                        case BROWSER.Chrome:
                        default:
                            driver = Spinup_Chrome();
                            break;
                    }
                    initialized = true;
                }
            }
        }

        private static void Shutdown_Driver()
        {
            if (initialized)
            {
                // cancel all pending work
                _rendered.Clear();
                _workAvailable.Reset();
                jobs.Clear();
                _doneReady.Reset();


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

                if (_workerThread.ThreadState.HasFlag(System.Threading.ThreadState.Unstarted))
                {
                    _workerThread.Start();
                }
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
        public static string DATAKEY_SETIMGS { get => "set-content-images"; }

        ILayoutInfoResolver<HTMLLayoutInfo> ISlideRenderer<HTMLLayoutInfo>.LayoutResolver { get => new HTMLLayoutInfo(); }

        async Task<(Image<Bgra32> main, Image<Bgra32> key)> ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.GetPreviewForLayout(string layoutInfo)
        {
            HTMLLayoutInfo layout = JsonSerializer.Deserialize<HTMLLayoutInfo>(layoutInfo);

            var html_main = layout.HTMLSrc;
            var html_key = layout.HTMLSrc_KEY;
            var css = layout.CSSSrc;

            html_main = HTML_INJECT_STYLE(html_main, css);
            html_key = HTML_INJECT_STYLE(html_key, css);

            var itask = await BROWSER_RENDER_ENGINE_MK2.PerformRenderWithHeadlessBrowser(html_main).ConfigureAwait(false);
            var iktask = await BROWSER_RENDER_ENGINE_MK2.PerformRenderWithHeadlessBrowser(html_key).ConfigureAwait(false);

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

            // let this gen prior to regular text's
            if (slide.Data.TryGetValue(DATAKEY_SETIMGS, out var simgs))
            {
                html_slide = SET_ASSET_INSERT(html_slide, simgs as Dictionary<string, List<string>>, assetResolver);
                html_key = SET_ASSET_INSERT(html_key, simgs as Dictionary<string, List<string>>, assetResolver);
            }


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

            Image<Bgra32> ibmp = await BROWSER_RENDER_ENGINE_MK2.PerformRenderWithHeadlessBrowser(html_slide).ConfigureAwait(false);
            Image<Bgra32> ikbmp = await BROWSER_RENDER_ENGINE_MK2.PerformRenderWithHeadlessBrowser(html_key).ConfigureAwait(false);

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
                            if (asset != null)
                            {
                                (tag as IHtmlImageElement).Source = asset.CurrentPath;
                            }
                            else
                            {
                                (tag as IHtmlImageElement).Source = "";
                            }
                        }
                    }
                }
            }

            return document.ToHtml();
        }

        private string SET_ASSET_INSERT(string src, Dictionary<string, List<string>> assetRefs, IAssetResolver assetResolver)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(src);

            // find all elements that are tagged
            foreach (var tagid in assetRefs.Keys)
            {
                var tags = document.QuerySelectorAll($"*[data-set-img='{tagid}']");
                foreach (var tag in tags)
                {
                    // allow non 'img' tags be used as the root?

                    var par = tag.Parent;

                    // remove itself from parent
                    par.RemoveChild(tag);

                    if (assetRefs.TryGetValue(tagid, out var aref))
                    {
                        int dynid = 0;
                        foreach (var aid in aref)
                        {
                            var copy = tag.Clone() as IElement;

                            // find either itself or first child as the 'img' tag

                            // load in dynamic id's
                            foreach (var dynode in copy.QuerySelectorAll("*[data-dynamic-id]"))
                            {
                                var did = dynode.GetAttribute("data-dynamic-id");
                                dynode.SetAttribute("data-id", $"{did}-{dynid}");
                            }
                            dynid++;

                            IHtmlImageElement imgtag;
                            if (copy.LocalName == "img")
                            {
                                imgtag = (IHtmlImageElement)copy;
                            }
                            else
                            {
                                //imgtag = copy.Children.FirstOrDefault(x => x.LocalName == "img") as IHtmlImageElement;
                                imgtag = copy.QuerySelectorAll("img").FirstOrDefault() as IHtmlImageElement;
                            }

                            if (imgtag == null)
                            {
                                break;
                            }

                            var asset = assetResolver.GetProjectAssetByName(aid);
                            if (asset != null)
                            {
                                imgtag.Source = asset.CurrentPath;
                            }
                            else
                            {
                                imgtag.Source = "";
                            }

                            par.AppendChild(copy);
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
