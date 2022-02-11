using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Xenon.Compiler.AST;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonASTLiturgyVerse : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public string Title { get; set; }
        public string Reference { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTElementCollection litverses = new XenonASTElementCollection(Parent);
            // assume all tokens inside braces are litrugy commands
            // only excpetions are we will gobble all leading whitespace in braces, and will remove the last 
            // character of whitespace before last brace

            Logger.Log(new XenonCompilerMessage
            {
                ErrorName = "Command Deprecated",
                ErrorMessage = $"No longer supported! Consider using {LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse2]}",
                Generator = "XenonASTLiturgyVerse::Compile()",
                Inner = "",
                Level = XenonCompilerMessageType.Error,
                Token = Lexer.CurrentToken,
            });

            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(true, "title", "reference");
            Title = args["title"];
            Reference = args["reference"];
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace at start of litverse.");
            Lexer.GobbleWhitespace();
            XenonASTLiturgyVerse litverse = CompileSubContent(Lexer, Logger);
            litverse.Parent = this;
            litverses.Elements.Add(litverse);
            while (!Lexer.Inspect("}"))
            {
                Lexer.GobbleandLog("#", "Only '#break' command recognized in '#litverse' block");
                if (Lexer.Inspect("break"))
                {
                    Lexer.GobbleandLog("break");
                    Lexer.GobbleWhitespace();
                }
                else
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Expected Command 'break'", ErrorName = "Unrecognized Command", Generator = "Compiler - XenonASTLitVerse", Inner = "", Level = XenonCompilerMessageType.Error, Token = Lexer.CurrentToken });
                }
                litverse = CompileSubContent(Lexer, Logger);
                litverse.Parent = this;
                litverses.Elements.Add(litverse);
            }
            Lexer.GobbleandLog("}", "Missing closing brace for litverse.");
            return litverses;
        }

        private XenonASTLiturgyVerse CompileSubContent(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTLiturgyVerse liturgy = new XenonASTLiturgyVerse();

            while (!Lexer.Inspect("}") && !Lexer.Inspect("#"))
            {
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                liturgy.Content.Add(content);
            }

            return liturgy;
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {

            List<Slide> slides = new List<Slide>();


            LiturgyVerseLayoutEngine layoutEngine = new LiturgyVerseLayoutEngine();
            layoutEngine.BuildLines(Content.Select(p => p.TextContent).ToList());
            layoutEngine.BuildSlideLines(project.Layouts.LiturgyLayout.GetRenderInfo());


            Slide liturgyslide = new Slide
            {
                Asset = string.Empty,
                Name = "UNNAMED_liturgy",
                Number = project.NewSlideNumber,
                Format = SlideFormat.LiturgyVerse,
                MediaType = MediaType.Image
            };

            double lineheight = -project.Layouts.LiturgyLayout.InterLineSpacing;

            if (project.GetAttribute("alphatranscol").Count > 0)
            {
                liturgyslide.Colors.Add("keytrans", GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault()));
            }



            foreach (var line in layoutEngine.LayoutLines)
            {

                bool overheight = lineheight + project.Layouts.LiturgyLayout.InterLineSpacing + line.height > project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Height;
                if (overheight)
                {
                    // need to start a new slide for this one
                    //project.Slides.Add(liturgyslide);
                    slides.Add(liturgyslide);
                    // create new slide
                    liturgyslide = new Slide
                    {
                        Asset = string.Empty,
                        Name = "UNNAMED_liturgy",
                        Number = project.NewSlideNumber,
                        Format = SlideFormat.LiturgyVerse,
                        MediaType = MediaType.Image
                    };
                    lineheight = 0;
                    if (project.GetAttribute("alphatranscol").Count > 0)
                    {
                        liturgyslide.Colors.Add("keytrans", GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault()));
                    }
                }
                lineheight += project.Layouts.LiturgyLayout.InterLineSpacing + line.height;
                liturgyslide.Lines.Add(
                    new SlideLine()
                    {
                        Content = {
                            new SlideLineContent() { Data = string.Join("", line.words).Trim(), Attributes = { ["width"] = line.width, ["height"] = line.height  } }
                        }
                    }
                );
            }
            liturgyslide.Data["title"] = Title;
            liturgyslide.Data["reference"] = Reference;
            // add slide to project
            //project.Slides.Add(liturgyslide);
            slides.Add(liturgyslide);
            return slides;
        }


        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTLiturgyVerse>");
            Debug.WriteLine($"Title='{Title}'");
            Debug.WriteLine($"Reference='{Reference}'");
            foreach (var c in Content)
            {
                c.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTLiturgyVerse>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
