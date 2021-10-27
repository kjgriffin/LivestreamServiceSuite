using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabScriptOrganIntro : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this.Parent = Parent;
            return this;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide();
            slide.Name = "UNNAMED_prefab_script";
            slide.Number = project.NewSlideNumber;
            slide.Lines = new List<SlideLine>();
            slide.Asset = "";
            // put script here
            slide.Data["source"] = "#Organ Intro;" + Environment.NewLine
                + "@arg1:PresetSelect(5)[Preset Organ];" + Environment.NewLine
                + "@arg1:DelayMs(100);" + Environment.NewLine
                + "@arg0:AutoTrans[Take Organ];";
            slide.Data["prefabtype"] = PrefabSlides.Script_OrganIntro;
            slide.MediaType = MediaType.Empty;
            slide.Format = SlideFormat.Prefab;
            slide.AddPostset(_Parent, true, true);
            project.Slides.Add(slide);

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.Script_OrganIntro);
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
