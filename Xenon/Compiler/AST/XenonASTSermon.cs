using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTSermon : IXenonASTCommand
    {

        public string Title { get; set; }
        public string Reference { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTSermon sermon = new XenonASTSermon();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            sermon.Title = Lexer.ConsumeUntil("\"");
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(",");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            sermon.Reference = Lexer.ConsumeUntil("\"");
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(")");
            return sermon;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Slide sermonslide = new Slide
            {
                Name = "UNNAMED_sermon",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.SermonTitle,
                MediaType = MediaType.Image
            };

            SlideLineContent slcref = new SlideLineContent() { Data = Reference };
            SlideLineContent slctitle = new SlideLineContent() { Data = Title };

            SlideLine slref = new SlideLine() { Content = new List<SlideLineContent>() { slcref } };
            SlideLine sltitle = new SlideLine() { Content = new List<SlideLineContent>() { slctitle } };

            sermonslide.Lines.Add(sltitle);
            sermonslide.Lines.Add(slref);

            project.Slides.Add(sermonslide);


        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTSermon>");
            Debug.WriteLine($"Title='{Title}'");
            Debug.WriteLine($"Reference='{Reference}'");
            Debug.WriteLine("</XenonASTSermon>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
