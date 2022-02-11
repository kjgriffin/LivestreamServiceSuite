using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler.SubParsers;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonAstTitledLiturgy : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public string RawContent { get; private set; } = "";

        public List<string> Titles { get; private set; } = new List<string>();
        public int OrigContentSourceLine { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonAstTitledLiturgy liturgy = new XenonAstTitledLiturgy();
            liturgy.Parent = Parent;


            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace at start of titledliturgy.");

            liturgy.OrigContentSourceLine = Lexer.Peek().linenum;


            while (!Lexer.InspectEOF())
            {

                if (Lexer.Gobble("title"))
                {
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("{", "expected opening '{' before title");
                    liturgy.Titles.Add(Lexer.ConsumeUntil("}").tvalue.Trim());
                    Lexer.GobbleandLog("}", "expected closing '}' after title");
                }

                else if (Lexer.Gobble("content"))
                {
                    if (!string.IsNullOrWhiteSpace(liturgy.RawContent))
                    {
                        Logger.Log(new XenonCompilerMessage
                        {
                            ErrorMessage = "Content set more than once. Will overwrite and use only the last definition.",
                            ErrorName = "Duplicate Content",
                            Generator = "XenonASTTitledLiturgy::Compile()",
                            Level = XenonCompilerMessageType.Warning,
                            Inner = "",
                            Token = Lexer.CurrentToken,
                        });
                    }
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("{", "Expected opening { at start of 'content'");
                    CaptureLiturgyContent(Lexer, liturgy);
                }
            }

            Lexer.GobbleandLog("}", "Expected closing brace at end of titledliturgy");

            return liturgy;
        }

        private static void CaptureLiturgyContent(Lexer Lexer, XenonAstTitledLiturgy liturgy)
        {
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
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
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


            var linfo = (this as IXenonASTCommand).GetLayoutOverrideFromProj(project, Logger, LanguageKeywordCommand.TitledLiturgyVerse2);
            TitledResponsiveLiturgySlideLayoutInfo layout = (TitledResponsiveLiturgySlideLayoutInfo)LanguageKeywords.LayoutForType[LanguageKeywordCommand.TitledLiturgyVerse2].layoutResolver._Internal_GetDefaultInfo();
            if (linfo.found)
            {
                layout = JsonSerializer.Deserialize<TitledResponsiveLiturgySlideLayoutInfo>(linfo.json);
            }

            ResponsiveLiturgyLayoutEngine engine = new ResponsiveLiturgyLayoutEngine();
            var slideblocks = engine.GenerateSlides(statements, layout.ContentBox);

            List<Slide> slides = new List<Slide>();
            foreach (var sblock in slideblocks)
            {
                // attach to a slide
                int slidenum = project.NewSlideNumber;

                Slide slide = new Slide
                {
                    Name = "UNNAMED_titledliturgy2",
                    Number = slidenum,
                    Lines = new List<SlideLine>(),
                    Asset = "",
                    Format = SlideFormat.ResponsiveLiturgy,
                    MediaType = MediaType.Image,
                };

                slide.Data[ResponsiveLiturgyRenderer.DATAKEY] = sblock;

                slide.Data["fallback-layout"] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.Liturgy2].defaultJsonFile;
                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.Liturgy2);

                slides.Add(slide);
            }

            int i = 0;
            foreach (var slide in slides)
            {
                slide.AddPostset(_Parent, i == 0, i == slides.Count - 1);
            }

            return slides;
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
