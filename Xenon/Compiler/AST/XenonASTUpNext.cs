using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTUpNext : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public string Title { get; set; }
        public string MainText { get; set; }
        public string InfoText { get; set; }


        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTUpNext upnext = new XenonASTUpNext();
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "maintext", "infotext");

            upnext.Title = args["title"];
            upnext.MainText = args["maintext"];
            upnext.InfoText = args["infotext"];
            upnext.Parent = Parent;
            return upnext;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {

            Slide slide = new Slide
            {
                Name = "UNNAMED_upnext",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ShapesAndTexts,
                MediaType = MediaType.Image,
            };

            List<string> strings = new List<string>
            {
                Title, MainText, InfoText,
            };

            slide.Data["shape-and-text-strings"] = strings;
            slide.Data["fallback-layout"] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.UpNext].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.UpNext);

            slide.AddPostset(_Parent, true, true);

            project.Slides.Add(slide);
        }

        public void GenerateDebug(Project project)
        {
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
