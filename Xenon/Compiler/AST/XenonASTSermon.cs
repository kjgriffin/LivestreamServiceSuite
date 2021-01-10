using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Helpers;
using System.Linq;

namespace Xenon.Compiler
{
    class XenonASTSermon : IXenonASTCommand
    {

        public string Title { get; set; }
        public string Reference { get; set; }
        public string Preacher { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTSermon sermon = new XenonASTSermon();
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "reference", "preacher");
            sermon.Title = args["title"];
            sermon.Reference = args["reference"];
            sermon.Preacher = args["preacher"];
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
            SlideLineContent slcpreacher = new SlideLineContent() { Data = Preacher };

            SlideLine slref = new SlideLine() { Content = new List<SlideLineContent>() { slcref } };
            SlideLine sltitle = new SlideLine() { Content = new List<SlideLineContent>() { slctitle } };
            SlideLine slpreacher = new SlideLine() { Content = new List<SlideLineContent>() { slcpreacher } };

            sermonslide.Lines.Add(sltitle);
            sermonslide.Lines.Add(slref);
            sermonslide.Lines.Add(slpreacher);

            if (project.GetAttribute("alphatranscol").Count > 0)
            {
                sermonslide.Colors.Add("keytrans", GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault()));
            }


            project.Slides.Add(sermonslide);


        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTSermon>");
            Debug.WriteLine($"Title='{Title}'");
            Debug.WriteLine($"Reference='{Reference}'");
            Debug.WriteLine($"Preacher='{Preacher}'");
            Debug.WriteLine("</XenonASTSermon>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
