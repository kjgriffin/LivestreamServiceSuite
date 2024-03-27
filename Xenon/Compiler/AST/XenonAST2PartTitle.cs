using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xenon.Compiler.LanguageDefinition;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    [XenonSTDCmdMetadata(LanguageKeywordCommand.TwoPartTitle, true)]
    [XenonSTDCmdParams(DefinitionRequirement.REQUIRED, true, "Part1", "Part2")]
    class XenonAST2PartTitle : IXenonASTCommand
    {

        public string Part1 { get; set; }
        public string Part2 { get; set; }
        public string Orientation { get; set; }
        private Token _badParamToken { get; set; }
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();

            //var args = Lexer.ConsumeArgList(true, "part1", "part2", "orientation");
            if (!Lexer.InspectEOF())
            {
                Lexer.GobbleandLog("(");
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("\"");
                Part1 = Lexer.ConsumeUntil("\"");
                Lexer.GobbleandLog("\"");
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog(",");
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("\"");
                Part2 = Lexer.ConsumeUntil("\"");
                Lexer.GobbleandLog("\"");
                Lexer.GobbleWhitespace();

                if (Lexer.Inspect(","))
                {
                    // We really don't want this anymore- so lets make some noise and tell users it wont work
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorName = "Parameter Deprecated",
                        ErrorMessage = $"2title is fully layout aware. Create a new layout to change the layout. Setting this value no-longer has any effect on the output.{Environment.NewLine}Consider re-writing command as: {{#2title(\"{Part1}\", \"{Part2}\")}}",
                        Generator = "XenonAST2PartTitle::Compile()",
                        Inner = "",
                        Level = XenonCompilerMessageType.Warning,
                        Token = Lexer.CurrentToken,
                    });
                    Lexer.GobbleandLog(",");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("\"");
                    _badParamToken = Lexer.CurrentToken;
                    Orientation = Lexer.ConsumeUntil("\"");
                    Lexer.GobbleandLog("\"");
                }

                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog(")");
            }

            this.Parent = Parent;
            return this;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide titleslide = new Slide
            {
                Name = "UNNAMED_2parttitle",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.ShapesAndTexts,
                MediaType = MediaType.Image
            };


            // if user has set a 'recognizable' orientation we'll add ever more logging noise to warn that it's being ignored
            if (Orientation == "horizontal" || Orientation == "vertical")
            {
                Logger.Log(new XenonCompilerMessage
                {
                    ErrorName = "Deprecated 'layout' Ignored",
                    ErrorMessage = $"2title is fully layout aware. Create a new layout to change the layout. Setting this value no-longer has any effect on the output.{Environment.NewLine}2title command requests '{Orientation}' layout, which will be ignored by renderer.",
                    Generator = "XenonAST2PartTitle::Compile()",
                    Inner = "",
                    Level = XenonCompilerMessageType.Warning,
                    Token = _badParamToken,
                });
            }

            List<string> strings = new List<string>
            {
                Part1, Part2,
            };

            titleslide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = strings;
            titleslide.Data[ShapeAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.TwoPartTitle].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, titleslide, LanguageKeywordCommand.TwoPartTitle);

            titleslide.AddPostset(_Parent, true, true);
            return titleslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonAST2PartTitle>");
            Debug.WriteLine($"Part1='{Part1}'");
            Debug.WriteLine($"Part2='{Part2}'");
            Debug.WriteLine("</XenonAST2PartTitle>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]);
            sb.Append($"(\"{Part1}\", \"{Part2}\"");
            if (!string.IsNullOrEmpty(Orientation))
            {
                sb.Append($",\"{Orientation}\"");
            }
            sb.AppendLine(")");
        }
    }
}
