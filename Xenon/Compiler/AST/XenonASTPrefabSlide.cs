﻿using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    public enum PrefabSlides { 
        Copyright,
        ViewServices,
        ViewSeries,
        ApostlesCreed,
        NiceneCreed,
        LordsPrayer,
    }

    internal static class PrefabSlideConverter
    {
        internal static string Convert(this PrefabSlides slide)
        {
            switch (slide)
            {
                case PrefabSlides.Copyright:
                    return "Copyright";
                case PrefabSlides.ViewServices:
                    return "ViewServices";
                case PrefabSlides.ViewSeries:
                    return "ViewSeries";
                case PrefabSlides.ApostlesCreed:
                    return "ApostlesCreed";
                case PrefabSlides.NiceneCreed:
                    return "NiceneCreed";
                case PrefabSlides.LordsPrayer:
                    return "LordsPrayer";
                default:
                    return "?";
            }
        }
    }


    class XenonASTPrefabSlide : IXenonASTCommand
    {
        public PrefabSlides PrefabSlide { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            return this;
        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Slide slide = new Slide();
            slide.Name = "UNNAMED_prefab";
            slide.Number = project.NewSlideNumber;
            slide.Lines = new List<SlideLine>();
            slide.Asset = "";
            slide.Data["prefabtype"] = PrefabSlide;
            slide.MediaType = MediaType.Image;
            slide.Format = SlideFormat.Prefab;

            project.Slides.Add(slide);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlide.ToString());
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
