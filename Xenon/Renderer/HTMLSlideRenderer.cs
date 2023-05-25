﻿using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using CoreHtmlToImage;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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
    static class CHROME_RENDER_ENGINE
    {
        static WebDriver driver;

        static CHROME_RENDER_ENGINE()
        {
            ChromeOptions opts = new ChromeOptions();
            opts.AddArgument("window-size=1920x1080");
            opts.AddArgument("--hide-scrollbars");
            opts.AddArgument("--no--sandbos");
            opts.AddArgument("--headless");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            driver = new ChromeDriver(service, opts);

            _workerThread = new Thread(RunJobs);
            _workerThread.Name = "CHROME_RENDER_THREAD";
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        class WorkItem
        {
            public string HTML { get; set; }
            public Guid ReqID { get; set; }
        }

        static ConcurrentQueue<WorkItem> jobs = new ConcurrentQueue<WorkItem>();
        static Thread _workerThread;
        static ManualResetEvent _workAvailable = new ManualResetEvent(false);
        static ConcurrentDictionary<Guid, Image<Bgra32>> _rendered = new ConcurrentDictionary<Guid, Image<Bgra32>>();
        static Semaphore signalReady = new Semaphore(0, 10000); // probably enough??
        static long Waiters = 0;

        public static void RunJobs()
        {
            while (true)
            {
                _workAvailable.WaitOne();
                // perform all work available
                while (jobs.TryDequeue(out var job))
                {
                    var img = RenderWithHeadlessChrome(job.HTML);
                    _rendered[job.ReqID] = img;
                }
                // signal only when all are done?
                var waiters = (int)Interlocked.Read(ref Waiters);
                if (waiters > 0)
                {
                    signalReady.Release(waiters);
                }
            }
        }

        public static Task<Image<Bgra32>> PerformRenderWithHeadlessChrome(string html)
        {
            Interlocked.Increment(ref Waiters);
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
                signalReady.WaitOne();

                // check if we've rendered this job
                if (_rendered.TryGetValue(id, out var res))
                {
                    _rendered.Remove(id, out _);
                    Interlocked.Decrement(ref Waiters);
                    return Task.FromResult(res);
                }
            }
#if DEBUG
            Debugger.Break();
#endif
            return Task.FromResult(default(Image<Bgra32>));
        }

        private static Image<Bgra32> RenderWithHeadlessChrome(string html)
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

        (Image<Bgra32> main, Image<Bgra32> key) ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.GetPreviewForLayout(string layoutInfo)
        {
            HTMLLayoutInfo layout = JsonSerializer.Deserialize<HTMLLayoutInfo>(layoutInfo);

            var html_main = layout.HTMLSrc;
            var html_key = layout.HTMLSrc_KEY;
            var css = layout.CSSSrc;

            var converter = new HtmlConverter();

            html_main = HTML_INJECT_STYLE(html_main, css);
            html_key = HTML_INJECT_STYLE(html_key, css);

            //var pngbytes_main = converter.FromHtmlString(html_main, format: ImageFormat.Png);
            //var pngbytes_key = converter.FromHtmlString(html_key, format: ImageFormat.Png);

            //Image<Bgra32> ibmp = Image.Load<Bgra32>(pngbytes_main);
            //Image<Bgra32> ikbmp = Image.Load<Bgra32>(pngbytes_key);


            Image<Bgra32> ibmp = CHROME_RENDER_ENGINE.PerformRenderWithHeadlessChrome(html_main).Result;
            Image<Bgra32> ikbmp = CHROME_RENDER_ENGINE.PerformRenderWithHeadlessChrome(html_key).Result;

            return (ibmp, ikbmp);
        }

        bool ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        void ISlideRenderer.VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.HTML)
            {
                HTMLLayoutInfo layout = (this as ISlideRenderer<HTMLLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }

        }
        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, IAssetResolver assetResolver, HTMLLayoutInfo layout)
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

            //var converter = new HtmlConverter();
            //var pngbytes_slide = converter.FromHtmlString(html_slide, format: ImageFormat.Png);
            //var pngbytes_key = converter.FromHtmlString(html_key, format: ImageFormat.Png);


            //Image<Bgra32> ibmp = Image.Load<Bgra32>(pngbytes_slide);
            //Image<Bgra32> ikbmp = Image.Load<Bgra32>(pngbytes_key);

            Image<Bgra32> ibmp = CHROME_RENDER_ENGINE.PerformRenderWithHeadlessChrome(html_slide).Result;
            Image<Bgra32> ikbmp = CHROME_RENDER_ENGINE.PerformRenderWithHeadlessChrome(html_key).Result;

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