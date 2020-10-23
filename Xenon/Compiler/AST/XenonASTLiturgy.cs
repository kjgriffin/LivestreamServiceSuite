using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xenon.Compiler.AST;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonASTLiturgy : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTElementCollection liturgys = new XenonASTElementCollection();
            // assume all tokens inside braces are litrugy commands
            // only excpetions are we will gobble all leading whitespace in braces, and will remove the last 
            // character of whitespace before last brace


            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace at start of liturgy.");
            Lexer.GobbleWhitespace();
            XenonASTLiturgy liturgy = CompileSubContent(Lexer, Logger);
            liturgys.Elements.Add(liturgy);
            while (!Lexer.Inspect("}"))
            {
                Lexer.GobbleandLog("#", "Only '#break' command recognized in '#liturgy' block");
                if (Lexer.Inspect("break"))
                {
                    Lexer.GobbleandLog("break");
                    Lexer.GobbleWhitespace();
                }
                else
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Expected Command 'break'", ErrorName = "Unrecognized Command", Generator = "Compiler - XenonASTLiturgy", Inner = "", Level = XenonCompilerMessageType.Error, Token = Lexer.CurrentToken });
                }
                liturgy = CompileSubContent(Lexer, Logger);
                liturgys.Elements.Add(liturgy);
            }
            Lexer.GobbleandLog("}", "Missing closing brace for liturgy.");
            return liturgys;
        }

        private XenonASTLiturgy CompileSubContent(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTLiturgy liturgy = new XenonASTLiturgy();

            while (!Lexer.Inspect("}") && !Lexer.Inspect("#"))
            {
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                liturgy.Content.Add(content);
            }

            return liturgy;
        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {

            //liturgyslide.Lines = Content.Select(p => new SlideLine { Content = new List<string> { p.TextContent } }).ToList();

            LiturgyLayoutEngine layoutEngine = new LiturgyLayoutEngine();
            layoutEngine.BuildLines(Content.Select(p => p.TextContent).ToList());
            layoutEngine.BuildSlideLines(project.Layouts.LiturgyLayout.GetRenderInfo());
            layoutEngine.BuildTextLines(project.Layouts.LiturgyLayout.GetRenderInfo());



            //liturgyslide.Lines = layoutEngine.LayoutLines.Select(l => new SlideLine() { Content = new List<string>() { l.speaker, string.Join("", l.words) } }).ToList();


            // turn lines into slides
            /*
                We start by computing the height of each line
                Add this to the running total height of the slide's lines + min interline spacing
                Once we can't fit any more declare a slide, figure out the slide 

                Also must follow the 3 golden rules of slide layout

                1. If the starting speaker is [C] Congregation, no other speaker allowed on the slide (or if starting speaker is [R] respondant
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
            string startspeaker = layoutEngine.LiturgyTextLines.FirstOrDefault().Speaker ?? "";

            foreach (var line in layoutEngine.LiturgyTextLines)
            {
                if (lastspeaker != line.Speaker)
                {
                    speakers++;
                }

                bool overheight = lineheight + project.Layouts.LiturgyLayout.InterLineSpacing + line.Height > project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Height;
                bool overspeakerswitch = speakers > 2; // Rule 2
                bool incorrrectspeakerorder = (startspeaker == "C" && line.Speaker != "C") || (startspeaker == "R" && line.Speaker != "R"); // Rule 1
                bool paragraphwrapissue = speakers > 1 && line.MultilineParagraph; // Rule 3
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
                        MediaType = MediaType.Image,
                    };
                    lineheight = 0;
                    startspeaker = line.Speaker;
                    lastspeaker = line.Speaker;
                    speakers = 1;
                }
                lastspeaker = line.Speaker;
                lineheight += project.Layouts.LiturgyLayout.InterLineSpacing + line.Height;
                liturgyslide.Lines.Add(
                    new SlideLine()
                    {
                        Content = {
                            new SlideLineContent() { Data = line.Speaker, Attributes = { ["width"] = line.Width, ["height"] = line.Height } },
                            new SlideLineContent()
                            {
                                Data = string.Join("", line.Words.Select(w => w.Value)).Trim(),
                                Attributes =
                                {
                                    ["textline"] = line,
                                    ["width"] = line.Width,
                                    ["height"] = line.Height
                                }
                            }
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
