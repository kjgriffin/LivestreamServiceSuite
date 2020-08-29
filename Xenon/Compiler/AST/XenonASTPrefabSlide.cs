using Xenon.SlideAssembly;
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
        LordsPrayer,
    }

    class XenonASTPrefabSlide : IXenonASTCommand
    {
        public PrefabSlides PrefabSlide { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
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
