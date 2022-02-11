using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class TitledResponsiveLiturgyRenderer : ISlideRenderer, ISlideRenderer<TitledResponsiveLiturgySlideLayoutInfo>, ISlideLayoutPrototypePreviewer<TitledResponsiveLiturgySlideLayoutInfo>
    {
        public ILayoutInfoResolver<TitledResponsiveLiturgySlideLayoutInfo> LayoutResolver { get; }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            throw new NotImplementedException();
        }

        public bool IsValidLayoutJson(string json)
        {
            throw new NotImplementedException();
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            throw new NotImplementedException();
        }
    }
}
