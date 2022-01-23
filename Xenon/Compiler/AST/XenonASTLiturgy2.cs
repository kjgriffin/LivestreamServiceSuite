﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler.SubParsers;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTLiturgy2 : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public string RawContent { get; private set; } = "";
        public int OrigContentSourceLine { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTLiturgy2 liturgy = new XenonASTLiturgy2();
            liturgy.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace at start of liturgy.");

            liturgy.OrigContentSourceLine = Lexer.Peek().linenum;


            // re-assemble all tokens until end of liturgy. // this will allow us to re-parse/tokenize with a custom liturgy lexer that will be better at what we're trying to do.
            StringBuilder sb = new StringBuilder();
            bool keepgoing = true;
            while (!Lexer.InspectEOF() && keepgoing)
            {
                if (Lexer.Peek() == "}") // check if it was escaped by doubbling it
                {
                    if (Lexer.PeekNext() == "}")
                    {
                        Lexer.Consume();
                        sb.Append(Lexer.Consume().tvalue);
                    }
                    else
                    {
                        // this is the end of the command
                        keepgoing = false;
                        Lexer.Consume();
                    }
                }
                else
                {
                    sb.Append(Lexer.Consume().tvalue);
                }
            }
            liturgy.RawContent = sb.ToString();

            // use a custom/conextual parser to re-parse the content


            // we're done! (already captured the ending token '}')
            Lexer.GobbleWhitespace();

            return liturgy;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            var statements = L2Parser.ParseLiturgyStatements(RawContent);

            // need to further process statements- split into 'words' (whatever that means)
            // then need to get the layout info for the slide
            // using the layout info (perhaps get crazy here and define all the weight constants [easier to debug for sure this way]) we can resolve the fonts that should be used to measure each hunk of text
            // once we know how big everything is we can try to stuff it where it should go as defined by the layout info
            // we can at this point begin scoring various attempts, keeping in mind the golden rules of responsive liturgy
            // need to figure out where to place it
            // as for data to send to renderer- we'll build a custom renderer that does what we want
            // it will do any background stuff
            // then it will just print out text
            // so the text that needs to go onto the slide will be just a bunch of 'textbox' objects containing the absolute position, font, text, styles, color etc.
            // renderer should be pretty simple since it just needs to draw it- we've done the work for layout already. No need to make it do so twice
            // as an architecture decision- perhaps we can have a common-layout class that can be called here, and also used in the renderer.
            // that ways we won't have to do graphics stuff here, and can re-use it later (if we ever do other fancy stuff with liturgy-like things)


            int slidenum = project.NewSlideNumber;

            Slide slide = new Slide
            {
                Name = "UNNAMED_liturgy2",
                Number = slidenum,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ResponsiveLiturgy,
                MediaType = MediaType.Image,
            };

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