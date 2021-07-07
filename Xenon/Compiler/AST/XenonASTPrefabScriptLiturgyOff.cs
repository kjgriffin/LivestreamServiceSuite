using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabScriptLiturgyOff : IXenonASTCommand
    {

        public string SlideTitleMessage { get; set; } = "Litury Off";

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            // allow optional title parameter
            Lexer.GobbleWhitespace();
            if (!Lexer.InspectEOF() && Lexer.Inspect("("))
            {
                Lexer.GobbleandLog("(", "Expecting '('");
                SlideTitleMessage = Lexer.ConsumeUntil(")");
                Lexer.GobbleandLog(")", "Expecting closing ')'");
            }
            return this;
        }

        void IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide();
            slide.Name = "UNNAMED_prefab_script";
            slide.Number = project.NewSlideNumber;
            slide.Lines = new List<SlideLine>();
            slide.Asset = "";
            // put script here
            slide.Data["source"] = $"#{SlideTitleMessage};" + Environment.NewLine
                + "@arg0:DSK1FadeOff[Kill Liturgy];" + Environment.NewLine
                + "@arg1:DelayMs(1000);";
            slide.Data["prefabtype"] = PrefabSlides.Script_LiturgyOff;
            slide.MediaType = MediaType.Empty;
            slide.Format = SlideFormat.Prefab;
            slide.AddPostset(_Parent, true, true);
            project.Slides.Add(slide);
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.Script_LiturgyOff);
            Debug.WriteLine($"<Title='{SlideTitleMessage}'/>");
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
