using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonASTLiturgy : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTLiturgy liturgy = new XenonASTLiturgy();
            // assume all tokens inside braces are litrugy commands
            // only excpetions are we will gobble all leading whitespace in braces, and will remove the last 
            // character of whitespace before last brace


            Lexer.GobbleWhitespace();
            Lexer.Gobble("{");
            Lexer.GobbleWhitespace();
            while (!Lexer.InspectEOF() && !Lexer.Inspect("\\/\\/") && !Lexer.Inspect("}"))
            {
                if (Lexer.PeekNext() == "}")
                {
                    Lexer.GobbleWhitespace();
                    continue;
                }
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                liturgy.Content.Add(content);
            }
            Lexer.Gobble("}");
            return liturgy;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {

            //liturgyslide.Lines = Content.Select(p => new SlideLine { Content = new List<string> { p.TextContent } }).ToList();

            LiturgyLayoutEngine layoutEngine = new LiturgyLayoutEngine();
            layoutEngine.BuildLines(Content.Select(p => p.TextContent).ToList());
            layoutEngine.BuildSlideLines(project.Layouts.LiturgyLayout.GetRenderInfo());



            //liturgyslide.Lines = layoutEngine.LayoutLines.Select(l => new SlideLine() { Content = new List<string>() { l.speaker, string.Join("", l.words) } }).ToList();


            // turn lines into slides
            /*
                We start by computing the height of each line
                Add this to the running total height of the slide's lines + min interline spacing
                Once we can't fit any more declare a slide, figure out the slide 

                Also must follow the 3 golden rules of slide layout

                1. If the starting speaker is [C] Congregation, no other speaker allowed on the slide
                2. There may be no more than 2 speakers per slide
                3. If a logical-line requires wrapping the line must be the first line of the slide
             */

            Slide liturgyslide = new Slide
            {
                Asset = string.Empty,
                Name = "UNNAMED_liturgy",
                Number = project.NewSlideNumber,
                Format = SlideFormat.Liturgy,
                MediaType = MediaType.Image
            };

            double lineheight = -project.Layouts.LiturgyLayout.InterLineSpacing;

            string lastspeaker = "";
            int speakers = 0;
            string startspeaker = layoutEngine.LayoutLines.FirstOrDefault().speaker ?? "";

            foreach (var line in layoutEngine.LayoutLines)
            {
                if (lastspeaker != line.speaker)
                {
                    speakers++;
                }

                bool overheight = lineheight + project.Layouts.LiturgyLayout.InterLineSpacing + line.height > project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Height;
                bool overspeakerswitch = speakers > 2; // Rule 2
                bool incorrrectspeakerorder = startspeaker == "C" && line.speaker != "C"; // Rule 1
                bool paragraphwrapissue = speakers > 1 && !line.fulltextonline; // Rule 3
                if (overheight || overspeakerswitch || incorrrectspeakerorder || paragraphwrapissue)
                {
                    // need to start a new slide for this one
                    project.Slides.Add(liturgyslide);
                    // create new slide
                    liturgyslide = new Slide
                    {
                        Asset = string.Empty,
                        Name = "UNNAMED_liturgy",
                        Number = project.NewSlideNumber,
                        Format = SlideFormat.Liturgy,
                        MediaType = MediaType.Image
                    };
                    lineheight = 0;
                    startspeaker = line.speaker;
                    lastspeaker = line.speaker;
                    speakers = 1;
                }
                lastspeaker = line.speaker;
                lineheight += project.Layouts.LiturgyLayout.InterLineSpacing + line.height;
                liturgyslide.Lines.Add(
                    new SlideLine()
                    {
                        Content = {
                            new SlideLineContent() { Data = line.speaker, Attributes = { ["width"] = line.width, ["height"] = line.height } },
                            new SlideLineContent() { Data = string.Join("", line.words).Trim(), Attributes = { ["width"] = line.width, ["height"] = line.height  } }
                        }
                    }
                );
            }
            // add slide to project
            project.Slides.Add(liturgyslide);
        }


        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTLiturgy>");
            foreach (var c in Content)
            {
                c.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTLiturgy>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
