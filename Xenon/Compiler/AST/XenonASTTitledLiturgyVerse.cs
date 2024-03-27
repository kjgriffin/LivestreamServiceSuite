using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTTitledLiturgyVerse : IXenonASTCommand
    {

        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public string Title { get; set; }
        public string Reference { get; set; }
        public string DrawSpeaker { get; set; }
        public string Mode { get; set; }
        public List<string> Text { get; set; } = new List<string>();
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this._SourceLine = lexer.Peek().linenum;
            lexer.GobbleWhitespace();
            var args = lexer.ConsumeArgList(true, "title", "reference", "drawspeaker");
            Title = args["title"];
            Reference = args["reference"];
            DrawSpeaker = args["drawspeaker"];
            lexer.GobbleWhitespace();
            if (lexer.Inspect("<"))
            {
                lexer.Consume();
                lexer.GobbleWhitespace();
                Mode = lexer.ConsumeUntil(">");
                lexer.GobbleandLog(">", "Expected closing '>' for mode (e.g. '<legacy>')");
            }


            lexer.GobbleWhitespace();
            lexer.GobbleandLog("{", "Expected opening brace to start content.");

            while (!lexer.Inspect("}"))
            {
                Text.Add(lexer.Consume());
            }
            lexer.GobbleandLog("}", "Missing closing brace for liturgy.");

            this.Parent = Parent;
            return this;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]);
            sb.Append($"(\"{Title}\", \"{Reference}\", \"{DrawSpeaker}\")");
            if (!string.IsNullOrEmpty(Mode))
            {
                sb.Append($"<{Mode}>");
            }
            sb.AppendLine();
            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            //indentDepth++;
            //sb.Append("".PadLeft(indentDepth * indentSize));

            // so here we just ignore indentation??
            sb.AppendLine(string.Join("", Text));

            //indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
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

            slide.Data[TitledLiturgyVerseSlideRenderer.DATAKEY_LINES] = layoutEngine.LiturgyTextLines;
            slide.Data[TitledLiturgyVerseSlideRenderer.DATAKEY_TITLE] = Title;
            slide.Data[TitledLiturgyVerseSlideRenderer.DATAKEY_REF] = Reference;
            slide.Data[TitledLiturgyVerseSlideRenderer.DATAKEY_DRAWSPEAKER] = DrawSpeaker;
            if (!string.IsNullOrWhiteSpace(Mode))
            {
                slide.Data[TitledLiturgyVerseSlideRenderer.DATAKEY_MODE] = Mode;
            }

            // This is no-longer relevatn
            /*
            if (project.GetAttribute("alphatranscol").Count > 0)
            {
                slide.Colors.Add("keytrans", GraphicsHelper.ColorFromRGB(project.GetAttribute("alphatranscol").FirstOrDefault()));
            }
            */

            /* this was technically an un-released feature, so we won't make it be backwards compatible. Old projects depending on this wil ljust have to be dissapointed
            if (!string.IsNullOrWhiteSpace(project.GetAttribute("global.tlverse.layout").FirstOrDefault()))
            {
                try
                {
                    slide.Data[Slide.LAYOUT_INFO_KEY] = TitledLiturgyVerseLayout.FromJSON(project.GetAttribute("global.tlverse.layout").FirstOrDefault());
                }
                catch (Exception ex)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Error Parsing Layout for #tlverse. Threw exception parsing json. Layout Override will not be used. Using default instead.", ErrorName = "Error Parsing Layout", Generator = "XenonASTTitledLiturgyVerse::Generate", Inner = ex.ToString(), Level = XenonCompilerMessageType.Warning });
                }
            }
            */

            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.TitledLiturgyVerse);

            slide.AddPostset(_Parent, true, true);

            return slide.ToList();
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
