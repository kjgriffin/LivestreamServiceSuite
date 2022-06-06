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
    class XenonASTTitledLiturgy : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public string RawContent { get; private set; } = "";

        public List<string> Titles { get; private set; } = new List<string>();
        public int OrigContentSourceLine { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTTitledLiturgy liturgy = new XenonASTTitledLiturgy();
            liturgy.Parent = Parent;


            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace at start of titledliturgy.");

            liturgy.OrigContentSourceLine = Lexer.Peek().linenum;

            Lexer.GobbleWhitespace();

            int stallcheck = 0;
            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                if (stallcheck > 10000)
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorMessage = "Compiler probably stuck parsing. Attempted 10000 times to find end of command. Aborting- this may not generate the slides you want.",
                        ErrorName = "Compiler Abort",
                        Generator = "XenonASTTitledLiturgy::Compile",
                        Inner = "Probably missing a '}' somewhere",
                        Level = XenonCompilerMessageType.Error,
                        Token = Lexer.CurrentToken
                    });
                    break;
                }

                Lexer.GobbleWhitespace();

                if (Lexer.Gobble("title"))
                {
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=");
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
                    Lexer.GobbleandLog("=");
                    Lexer.GobbleWhitespace();

                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("{", "Expected opening { at start of 'content'");
                    CaptureLiturgyContent(Lexer, liturgy);
                }
                stallcheck++;
            }

            Lexer.GobbleandLog("}", "Expected closing brace at end of titledliturgy");

            return liturgy;
        }
        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.AppendLine(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]);
            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;

            foreach (var title in Titles)
            {
                sb.AppendLine($"title={{{title}}}".PadLeft(indentDepth * indentSize));
            }

            sb.AppendLine($"content=".PadLeft(indentDepth * indentSize));

            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;

            foreach (var line in L2Parser.ToFormattedXMLLines(RawContent))
            {
                sb.AppendLine(line.PadLeft(indentDepth * indentSize));
            }

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }


        private static void CaptureLiturgyContent(Lexer Lexer, XenonASTTitledLiturgy liturgy)
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

            // borrows all the same logic as liturgy2
            // we're just adding some additional textboxs to each generated slide and filling with title text

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
                    Format = SlideFormat.ResponsiveLiturgyTitledVerse,
                    MediaType = MediaType.Image,
                };

                slide.Data[TitledResponsiveLiturgyRenderer.DATAKEY_CONTENTBLOCKS] = sblock;
                slide.Data[TitledResponsiveLiturgyRenderer.DATAKEY_TITLETEXT] = Titles;

                slide.Data["fallback-layout"] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.TitledLiturgyVerse2].defaultJsonFile;
                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.TitledLiturgyVerse2);

                slides.Add(slide);
            }

            int i = 0;
            foreach (var slide in slides)
            {
                slide.AddPostset(_Parent, i == 0, i == slides.Count - 1);
                i++;
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
