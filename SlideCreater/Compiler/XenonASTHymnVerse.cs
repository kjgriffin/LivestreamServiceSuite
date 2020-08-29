using SlideCreater.LayoutEngine;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTHymnVerse : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            XenonASTHymnVerse verse = new XenonASTHymnVerse();
            Lexer.GobbleWhitespace();

            Lexer.Gobble("{");

            while (!Lexer.Inspect("}"))
            {
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                verse.Content.Add(content);
            }

            Lexer.Gobble("}");

            return verse;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            VerseLayoutEngine vle = new VerseLayoutEngine();
            vle.BuildLines(Content.Select(p => p.TextContent).ToList());

            XenonASTTextHymn parent = _Parent as XenonASTTextHymn;

            Slide slide = new Slide();
            slide.Name = $"texthymn_{parent.HymnName}_verse";

            slide.Number = project.NewSlideNumber;
            slide.Asset = "";
            slide.Lines = new List<SlideLine>();
            slide.Format = SlideFormat.HymnTextVerse;
            slide.MediaType = MediaType.Image;

            slide.Data["title"] = parent.HymnTitle;
            slide.Data["name"] = parent.HymnName;
            slide.Data["number"] = parent.Number;
            slide.Data["tune"] = parent.Tune;
            slide.Data["copyright"] = parent.CopyrightInfo;

            foreach (var line in vle.LayoutLines)
            {
                SlideLineContent slc = new SlideLineContent() { Data = string.Join("", line.Words).Trim() };
                SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
                slide.Lines.Add(sl);
            }

            project.Slides.Add(slide);

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTHymnVerse>");
            foreach (var c in Content)
            {
                c.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTHymnVerse>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
