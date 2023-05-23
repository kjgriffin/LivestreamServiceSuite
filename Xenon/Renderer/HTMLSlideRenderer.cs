using CoreHtmlToImage;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
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
        ILayoutInfoResolver<HTMLLayoutInfo> ISlideRenderer<HTMLLayoutInfo>.LayoutResolver { get => new HTMLLayoutInfo(); }

        (Image<Bgra32> main, Image<Bgra32> key) ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.GetPreviewForLayout(string layoutInfo)
        {
            HTMLLayoutInfo layout = JsonSerializer.Deserialize<HTMLLayoutInfo>(layoutInfo);

            var html = layout.HTMLSrc;
            var converter = new HtmlConverter();

            var pngbytes= converter.FromHtmlString(html, 1920, ImageFormat.Png);

            // not sure about this here??
            ASlideLayoutInfo template = new ASlideLayoutInfo();
            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, template);

            Image<Bgra32> slide = Image.Load<Bgra32>(pngbytes);

            return (slide, ikbmp);
        }

        bool ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>.IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<HTMLLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        void ISlideRenderer.VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            return;
        }
    }
}
