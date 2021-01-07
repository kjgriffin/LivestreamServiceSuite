using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTTitledLiturgyVerse : IXenonASTCommand
    {

        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public string Title { get; set; }
        public string Reference { get; set; }
        public string DrawSpeaker { get; set; }
        public List<string> Text { get; set; } = new List<string>();

        public IXenonASTElement Compile(Lexer lexer, XenonErrorLogger Logger)
        {
            lexer.GobbleWhitespace();
            var args = lexer.ConsumeArgList(true, "title", "reference", "drawspeaker");
            Title = args["title"];
            Reference = args["reference"];
            DrawSpeaker = args["drawspeaker"];


            lexer.GobbleWhitespace();
            lexer.GobbleandLog("{", "Expected opening brace to start content.");

            while (!lexer.Inspect("}"))
            {
                Text.Add(lexer.Consume());
            }
            lexer.GobbleandLog("}", "Missing closing brace for liturgy.");

            return this;
        }


        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Slide slide = new Slide
            {
                Name = "UNNAMED_titledliturgyverse",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.LiturgyTitledVerse,
                MediaType = MediaType.Image
            };


            Dictionary<string, string> otherspeakers = new Dictionary<string, string>();
            var s = project.GetAttribute("otherspeakers");
            foreach (var item in s)
            {
                var match = Regex.Match(item, "(?<speaker>(.*)-(?<text>.*))").Groups;
                otherspeakers.Add(match["speaker"].Value, match["text"].Value);
            }


            LiturgyLayoutEngine layoutEngine = new LiturgyLayoutEngine();
            layoutEngine.BuildLines(Text, otherspeakers);
            layoutEngine.BuildTextLines(project.Layouts.TitleLiturgyVerseLayout.Textbox);

            slide.Data["lines"] = layoutEngine.LiturgyTextLines;
            slide.Data["title"] = Title;
            slide.Data["reference"] = Reference;
            slide.Data["drawspeaker"] = DrawSpeaker;

            if (project.GetAttribute("alphatranscol").Count > 0)
            {
                slide.Colors.Add("keytrans", GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault()));
            }


            project.Slides.Add(slide);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTTitledLiturgyVerse>");
            Debug.WriteLine($"Title='{Title}'");
            Debug.WriteLine($"Reference='{Reference}'");
            Debug.WriteLine($"DrawSpeaker='{DrawSpeaker}'");
            Debug.WriteLine($"Content=[");
            foreach (var word in Text)
            {
                Debug.Write($"'{word}',");
            }
            Debug.WriteLine("]");
            Debug.WriteLine("</XenonASTTitledLiturgyVerse>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
