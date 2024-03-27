using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Compiler.LanguageDefinition;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabLordsPrayer : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.AppendLine(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]);
        }

        IXenonASTElement IXenonASTElement.Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this.Parent = Parent;
            this._SourceLine = Lexer.Peek().linenum;
            return this;
        }

        List<Slide> IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // add 1 slides for each image we have to render
            List<Slide> slides = new List<Slide>();
            for (int i = 1; i <= 1; i++)
            {
                Slide slide = new Slide();
                slide.Name = "UNNAMED_prefab";
                slide.Number = project.NewSlideNumber;
                slide.Lines = new List<SlideLine>();
                slide.Asset = "";
                slide.Data["prefabtype"] = PrefabSlides.LordsPrayer;
                slide.Data["layoutnum"] = i;
                slide.MediaType = MediaType.Image;
                slide.Format = SlideFormat.Prefab;
                slide.AddPostset(_Parent, true, true);
                slides.Add(slide);
            }
            return slides;
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.LordsPrayer);
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
