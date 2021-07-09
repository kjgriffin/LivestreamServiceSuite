using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Xenon.Compiler.AST;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonASTLiturgy : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public bool ForceSpeakerStartOnNewline = false;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTElementCollection liturgys = new XenonASTElementCollection();
            // assume all tokens inside braces are litrugy commands
            // only excpetions are we will gobble all leading whitespace in braces, and will remove the last 
            // character of whitespace before last brace


            Lexer.GobbleWhitespace();

            // optional params
            if (Lexer.Inspect("("))
            {
                var args = Lexer.ConsumeArgList(false, "startnewline");
                if (args["startnewline"] == "true")
                {
                    ForceSpeakerStartOnNewline = true;
                }
                Lexer.GobbleWhitespace();
            }

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

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {

            Dictionary<string, string> otherspeakers = new Dictionary<string, string>();
            // get otherspeakers from project

            var s = project.GetAttribute("otherspeakers");
            foreach (var item in s)
            {
                var match = Regex.Match(item, "(?<speaker>(.*))-(?<text>.*)").Groups;
                otherspeakers.Add(match["speaker"].Value, match["text"].Value);
            }




            LiturgyLayoutEngine layoutEngine = new LiturgyLayoutEngine();
            layoutEngine.BuildLines(Content.Select(p => p.TextContent).ToList(), otherspeakers, ForceSpeakerStartOnNewline);
            layoutEngine.BuildSlideLines(project.Layouts.LiturgyLayout.GetRenderInfo());
            layoutEngine.BuildTextLines(project.Layouts.LiturgyLayout.Text);


            // override colors if requried on slide

            bool overridespeakercolor = false;
            bool overridetextcolor = false;
            bool overridebackgroundcolor = false;

            System.Drawing.Color liturgyspeakercolor = new System.Drawing.Color();
            System.Drawing.Color liturgytextcolor = new System.Drawing.Color();
            System.Drawing.Color liturgybackgroundcolor = new System.Drawing.Color();
            System.Drawing.Color liturgytransppcolor = System.Drawing.Color.Gray;

            if (project.GetAttribute("litspeakertextcol").Count > 0)
            {
                liturgyspeakercolor = GraphicsHelper.ColorFromRGB(project.GetAttribute("litspeakertextcol").FirstOrDefault());
                overridespeakercolor = true;
            }
            if (project.GetAttribute("littextcol").Count > 0)
            {
                liturgytextcolor = GraphicsHelper.ColorFromRGB(project.GetAttribute("littextcol").FirstOrDefault());
                overridetextcolor = true;
            }
            if (project.GetAttribute("litbackgroundcol").Count > 0)
            {
                liturgybackgroundcolor = GraphicsHelper.ColorFromRGB(project.GetAttribute("litbackgroundcol").FirstOrDefault());
                overridebackgroundcolor = true;
            }
            if (project.GetAttribute("alphatranscol").Count > 0)
            {
                liturgytransppcolor = GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault());
            }




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
                MediaType = MediaType.Image,
            };

            if (overridespeakercolor)
            {
                liturgyslide.Colors["alttext"] = liturgyspeakercolor;
            }
            if (overridetextcolor)
            {
                liturgyslide.Colors["text"] = liturgytextcolor;
            }
            if (overridebackgroundcolor)
            {
                liturgyslide.Colors["keybackground"] = liturgybackgroundcolor;
            }
            liturgyslide.Colors["keytrans"] = liturgytransppcolor;


            double lineheight = -project.Layouts.LiturgyLayout.InterLineSpacing;

            string lastspeaker = "";
            int speakers = 0;
            string startspeaker = layoutEngine.LiturgyTextLines.FirstOrDefault().Speaker ?? "";

            bool first = true;

            int lineindexnum = 0;
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

                // Asethetic rule: if next line is continuation of speaker, and is super short -> if we're nearind end of slide, push it all to the next slide since it will look better there?
                var nextwidth = lineindexnum + 1 < layoutEngine.LiturgyTextLines.Count ? layoutEngine.LiturgyTextLines[lineindexnum + 1].Width : project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Width;
                bool looksbettertopushtonextslide = lastspeaker == line.Speaker && nextwidth < 0.7 * project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Width && lineheight > project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Height / 2;

                if (overheight || overspeakerswitch || incorrrectspeakerorder || paragraphwrapissue || looksbettertopushtonextslide)
                {
                    // need to start a new slide for this one
                    liturgyslide.AddPostset(_Parent, first, false);
                    first = false;
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
                    if (overridespeakercolor)
                    {
                        liturgyslide.Colors["alttext"] = liturgyspeakercolor;
                    }
                    if (overridetextcolor)
                    {
                        liturgyslide.Colors["text"] = liturgytextcolor;
                    }
                    if (overridebackgroundcolor)
                    {
                        liturgyslide.Colors["keybackground"] = liturgybackgroundcolor;
                    }
                    liturgyslide.Colors["keytrans"] = liturgytransppcolor;

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
                lineindexnum += 1;
            }
            // add slide to project
            liturgyslide.AddPostset(_Parent, first, true);
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
