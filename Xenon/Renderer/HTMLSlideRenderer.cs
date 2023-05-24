using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using CoreHtmlToImage;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
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

            var pngbytes_main = converter.FromHtmlString(html_main, format: ImageFormat.Png);
            var pngbytes_key = converter.FromHtmlString(html_key, format: ImageFormat.Png);

            Image<Bgra32> ibmp = Image.Load<Bgra32>(pngbytes_main);
            Image<Bgra32> ikbmp = Image.Load<Bgra32>(pngbytes_key);

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
            // TODO: fix this!
            res.RenderedAs = "Liturgy";

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

            var converter = new HtmlConverter();
            var pngbytes_slide = converter.FromHtmlString(html_slide, format: ImageFormat.Png);
            var pngbytes_key = converter.FromHtmlString(html_key, format: ImageFormat.Png);


            Image<Bgra32> ibmp = Image.Load<Bgra32>(pngbytes_slide);
            Image<Bgra32> ikbmp = Image.Load<Bgra32>(pngbytes_key);

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


    }
}
