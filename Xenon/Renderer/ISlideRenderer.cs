using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using Xenon.AssetManagment;
using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    public interface ISlideRenderer
    {
        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result);
    }
    public interface ISlideRenderer<out LayoutInfoType> where LayoutInfoType : ALayoutInfo
    {
        public ILayoutInfoResolver<LayoutInfoType> LayoutResolver { get; }
    }

    public interface IAssetResolver
    {
        ProjectAsset GetProjectAssetByName(string assetName);
    }

    public interface ILayoutInfoResolver<out LayoutType> where LayoutType : ALayoutInfo
    {
        static LayoutType _InternalDefault_GetLayoutInfo(Slide slide)
        {
            if (slide.Data.TryGetValue(Slide.LAYOUT_INFO_KEY, out object info))
            {
                if (info is LayoutType)
                {
                    return info as LayoutType;
                }
            }
            return GetDefaultInfo();
        }
        LayoutType GetLayoutInfo(Slide slide);
        static LayoutType GetDefaultInfo()
        {
            var typeinfo = typeof(LayoutType);
            var name = $"{typeinfo.Namespace}.Defaults.{typeinfo.Name}_Default.json";

            //var assembly = System.Reflection.Assembly.GetAssembly(typeof(ALayoutInfo)).GetManifestResourceNames();
            var stream = System.Reflection.Assembly.GetAssembly(typeof(ALayoutInfo)).GetManifestResourceStream(name);

            using (StreamReader sr = new StreamReader(stream))
            {
                string json = sr.ReadToEnd();
                return JsonSerializer.Deserialize<LayoutType>(json);
            }

        }
        Type GetType => typeof(LayoutType);
    }



}
