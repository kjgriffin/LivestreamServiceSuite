﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal interface ISlideRendertimeInfoProvider
    {
        public int FindSlideNumber(string reference);
        public int FindCameraID(string camName);
    }
    internal interface ISlideRenderer
    {
        public Task<RenderedSlide> VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide operand);
    }
    internal interface ISlideRenderer<out LayoutInfoType> where LayoutInfoType : ALayoutInfo
    {
        public ILayoutInfoResolver<LayoutInfoType> LayoutResolver { get; }
    }

    internal interface ISlideLayoutPrototypePreviewer<out LayoutInfoType> where LayoutInfoType : ALayoutInfo
    {
        public Task<(Image<Bgra32> main, Image<Bgra32> key)> GetPreviewForLayout(string layoutInfo);

        public bool IsValidLayoutJson(string json);

        public static bool _InternalDefaultIsValidLayoutJson(string json)
        {
            try
            {
                JsonSerializer.Deserialize<LayoutInfoType>(json);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }

    public interface IAssetResolver
    {
        ProjectAsset GetProjectAssetByName(string assetName);
    }

    internal interface ILayoutInfoResolver<out LayoutType> where LayoutType : ALayoutInfo
    {
        static LayoutType _InternalDefault_GetLayoutInfo(Slide slide)
        {
            string v = "";
            if (slide.Data.TryGetValue("fallback-layout", out object value))
            {
                // try using this one
                v = value as string ?? "";
            }
            if (slide.Data.TryGetValue(Slide.LAYOUT_INFO_KEY, out object info))
            {
                if (TryParseJson(info, out LayoutType layout))
                {
                    return layout;
                }
            }
            return GetDefaultInfo(v);
        }

        public static bool TryParseJson(object info, out LayoutType layout)
        {
            layout = null;
            try
            {
                layout = JsonSerializer.Deserialize<LayoutType>((string)info);
                return true;
            }
            catch (Exception ex)
            {
                //return GetDefaultInfo(v);
                return false;
            }
        }

        LayoutType Deserialize(string json)
        {
            return JsonSerializer.Deserialize<LayoutType>(json);
        }
        string Serialize(object layoutType)
        {
            return JsonSerializer.Serialize<LayoutType>(layoutType as LayoutType, new JsonSerializerOptions() { WriteIndented = true });
        }
        LayoutType GetLayoutInfo(Slide slide);
        static LayoutType GetDefaultInfo(string overrideDefault = "")
        {
            var typeinfo = typeof(LayoutType);
            var name = string.IsNullOrEmpty(overrideDefault) ? $"{typeinfo.Namespace}.Defaults.{typeinfo.Name}_Default.json" : $"{typeinfo.Namespace}.Defaults.{overrideDefault}";

            //var assembly = System.Reflection.Assembly.GetAssembly(typeof(ALayoutInfo)).GetManifestResourceNames();
            var stream = System.Reflection.Assembly.GetAssembly(typeof(ASlideLayoutInfo)).GetManifestResourceStream(name);

            if (stream != null)
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    string json = sr.ReadToEnd();
                    return JsonSerializer.Deserialize<LayoutType>(json);
                }
            }
            else if (typeof(LayoutType).GetConstructor(Type.EmptyTypes) != null)
            { 
                var constructor = typeof(LayoutType).GetConstructor(Type.EmptyTypes);
                return (LayoutType)constructor.Invoke(null);
            }
            // hope we don't get here
            return null;
        }
        LayoutType _Internal_GetDefaultInfo(string overrideDefault = "");
        string _Internal_GetDefaultJson(string overrideDefault = "")
        {
            LayoutType layout = _Internal_GetDefaultInfo(overrideDefault);
            return JsonSerializer.Serialize<LayoutType>(layout, new JsonSerializerOptions() { WriteIndented = true });
        }
        System.Type GetType => typeof(LayoutType);
    }



}
