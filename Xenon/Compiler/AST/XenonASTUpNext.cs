using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public XenonASTScript PostScript { get; set; }
        public bool HasPostScript { get; private set; } = false;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTUpNext upnext = new XenonASTUpNext();
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "maintext", "infotext");

            // allow for optional post-script
            Lexer.GobbleWhitespace();

            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("{"))
                {
                    // borrow the compiler for action slides
                    upnext.HasPostScript = true;

                    XenonASTScript script = new XenonASTScript();
                    upnext.PostScript = (XenonASTScript)script.Compile(Lexer, Logger, upnext);
                }
            }


            upnext.Title = args["title"];
            upnext.MainText = args["maintext"];
            upnext.InfoText = args["infotext"];
            upnext.Parent = Parent;
            return upnext;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {

            int slidenum = project.NewSlideNumber;

            Slide slide = new Slide
            {
                Name = "UNNAMED_upnext",
                Number = slidenum,
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

            if (HasPostScript)
            {
                // create post script slide
                // TODO: perhaps there's a better way to infer the slide name of what will be generated
                string srcFile = $"#_Liturgy.png";
                string keyFile = $"Key_#.png";

                // extract the PostScript slide's commands and run compilation-time resolution on special characters for source ('#')
                var lines = PostScript.Source.Split(';').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                List<string> newlines = new List<string>();
                foreach (var line in lines)
                {
                    if (line.StartsWith("!"))
                    {
                        newlines.Add(Regex.Replace(line, @"#", slidenum.ToString()) + ";");
                    }
                    else
                    {
                        newlines.Add(line + ";");
                    }
                }
                PostScript.Source = string.Join(Environment.NewLine, newlines);

                PostScript.Generate(project, this, Logger);
            }
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
