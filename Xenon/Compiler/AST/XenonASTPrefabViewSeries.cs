using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabViewSeries : IXenonASTCommand
    {
        IXenonASTElement IXenonASTElement.Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            return this;
        }

        void IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // add 1 slides for each image we have to render
            for (int i = 1; i <= 1; i++)
            {
                Slide slide = new Slide();
                slide.Name = "UNNAMED_prefab";
                slide.Number = project.NewSlideNumber;
                slide.Lines = new List<SlideLine>();
                slide.Asset = "";
                slide.Data["prefabtype"] = PrefabSlides.ViewSeries;
                slide.Data["layoutnum"] = i;
                slide.MediaType = MediaType.Image;
                slide.Format = SlideFormat.Prefab;
                slide.AddPostset(_Parent, true, true);
                project.Slides.Add(slide);
            }
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.ViewSeries);
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
