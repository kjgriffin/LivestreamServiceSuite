using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Helpers;

namespace Xenon.Compiler
{
    public enum PrefabSlides { 
        Copyright,
        ViewServices,
        ViewSeries,
        ApostlesCreed,
        NiceneCreed,
        LordsPrayer,
        Script_OrganIntro,
        Script_LiturgyOff,
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
                case PrefabSlides.Script_LiturgyOff:
                    return "LiturgyOff";
                case PrefabSlides.Script_OrganIntro:
                    return "OrganIntro";
                default:
                    return "?";
            }
        }
    }


    class XenonASTPrefabSlide : IXenonASTCommand
    {
        public PrefabSlides PrefabSlide { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this.Parent = Parent;
            return this;
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide();
            slide.Name = "UNNAMED_prefab";
            slide.Number = project.NewSlideNumber;
            slide.Lines = new List<SlideLine>();
            slide.Asset = "";
            slide.Data["prefabtype"] = PrefabSlide;
            slide.MediaType = MediaType.Image;
            slide.Format = SlideFormat.Prefab;

            return slide.ToList();
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
