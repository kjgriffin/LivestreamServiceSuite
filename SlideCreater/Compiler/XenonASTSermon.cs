using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTSermon : IXenonASTCommand
    {

        public string Title { get; set; }
        public string Reference { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            XenonASTSermon sermon = new XenonASTSermon();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect(","))
            {
                sb.Append(Lexer.Consume());
            }
            sermon.Title = sb.ToString().Trim();
            sb.Clear();
            Lexer.Gobble(",");
            while (!Lexer.Inspect("\\)"))
            {
                sb.Append(Lexer.Consume());
            }
            sermon.Reference = sb.ToString().Trim();
            Lexer.Gobble(")");
            return sermon;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Slide sermonslide = new Slide();
            sermonslide.Name = "UNNAMED_sermon";
            sermonslide.Number = project.NewSlideNumber;
            sermonslide.Lines = new List<SlideLine>();
            sermonslide.Asset = "";
            sermonslide.Format = SlideFormat.SermonTitle;
            sermonslide.MediaType = MediaType.Image;

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
