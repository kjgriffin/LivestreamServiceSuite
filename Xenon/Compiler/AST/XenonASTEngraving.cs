using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler.SubParsers;
using Xenon.Engraver.Layout;
using Xenon.Engraver.Parser;
using Xenon.Helpers;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTEngraving : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public string RawMusic { get; private set; } = "";
        public string RawWords { get; private set; } = "";
        public string RawPackage { get; private set; } = "";

        public List<MusicPart> MusicParts { get; set; } = new List<MusicPart>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTEngraving slide = new XenonASTEngraving();
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            do
            {
                if (Lexer.InspectEOF())
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorMessage = "Expecting ending '}' for #engraving command",
                        ErrorName = "Unexpected EOF",
                        Generator = "XenonASTEngraving::Compile()",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = Lexer.CurrentToken,
                    });
                }

                Lexer.GobbleWhitespace();

                // parse music
                if (Lexer.Inspect("music"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    slide.RawMusic = Lexer.ConsumeNestedBlockAsRaw();
                }
                // parse words
                if (Lexer.Inspect("words"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    slide.RawWords = Lexer.ConsumeNestedBlockAsRaw();
                }
                // parse package
                if (Lexer.Inspect("package"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    slide.RawPackage = Lexer.ConsumeNestedBlockAsRaw();
                }

            } while (!Lexer.Inspect("}"));
            Lexer.Consume();


            // Run all sub-compilers
            slide.MusicParts = MusicParser.ExtractMusicParts(slide.RawMusic);


            slide.Parent = Parent;
            return slide;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.AppendLine(LanguageKeywords.Commands[LanguageKeywordCommand.Engraving]);
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("{");
            indentDepth++;

            sb.AppendLine("// TODO:".PadLeft(indentDepth * indentSize));


            indentDepth--;
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("}");

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {

            var linfo = (this as IXenonASTCommand).GetLayoutOverrideFromProj(project, Logger, LanguageKeywordCommand.Engraving);
            EngravingLayoutInfo layout = (EngravingLayoutInfo)LanguageKeywords.LayoutForType[LanguageKeywordCommand.Engraving].layoutResolver._Internal_GetDefaultInfo();
            if (linfo.found)
            {
                layout = JsonSerializer.Deserialize<EngravingLayoutInfo>(linfo.json);
            }


            // perform layout
            var visobjs = EngravingLayoutEngine.TestLayout(MusicParts, layout);


            Slide slide = new Slide
            {
                Name = "UNNAMED_engraving",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.Engraving,
                MediaType = MediaType.Image,
            };

            slide.Data[EngravingRenderer.DATAKEY_VISUALS] = visobjs;


            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.Engraving);

            slide.AddPostset(_Parent, true, true);


            return slide.ToList();
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
