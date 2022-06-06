using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler.SubParsers;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTShapesImagesAndText : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public List<string> Texts { get; private set; } = new List<string>();
        public List<string> FGAssetNames { get; private set; } = new List<string>();
        public List<string> BGAssetNames { get; private set; } = new List<string>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTShapesImagesAndText slide = new XenonASTShapesImagesAndText();
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            do
            {
                if (Lexer.InspectEOF())
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorMessage = "Expecting ending '}' for #customdraw command",
                        ErrorName = "Unexpected EOF",
                        Generator = "XenonASTShapesImagesAndText::Compile()",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = Lexer.CurrentToken,
                    });
                }


                if (TextBlockParser.TryParseTextBlock(Lexer, out var text))
                {
                    slide.Texts.Add(text);
                }

                if (Lexer.Inspect("asset"))
                {
                    var name = "";
                    var type = "";
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("(");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("\"");
                    name = Lexer.ConsumeUntil("\"");
                    Lexer.GobbleandLog("\"");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog(",");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("\"");
                    type = Lexer.ConsumeUntil("\"");
                    Lexer.GobbleandLog("\"");

                    if (type == "fg")
                    {
                        slide.FGAssetNames.Add(name);
                    }
                    else if (type == "bg")
                    {
                        slide.BGAssetNames.Add(name);
                    }


                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog(")");
                }

                Lexer.GobbleWhitespace();
            } while (!Lexer.Inspect("}"));
            Lexer.Consume();

            slide.Parent = Parent;
            return slide;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.AppendLine(LanguageKeywords.Commands[LanguageKeywordCommand.CustomDraw]);
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("{");
            indentDepth++;

            foreach (var asset in BGAssetNames)
            {
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine($"asset=(\"{asset}\", \"bg\")");
            }
            foreach (var asset in FGAssetNames)
            {
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine($"asset=(\"{asset}\", \"fg\")");
            }

            foreach (var text in Texts)
            {
                var refmt = TextBlockParser.ReformatTextBlock(text);
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine($"text({string.Join(',', refmt.modes)}{(refmt.modes.Any() ? "," : "")}trimat:`)=");

                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine("{");
                indentDepth++;
                foreach (var line in refmt.lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        sb.Append("".PadLeft(indentDepth * indentSize));
                        sb.AppendLine($"`{line}");
                    }
                }
                indentDepth--;
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine("}");
            }


            indentDepth--;
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("}");

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide
            {
                Name = "UNNAMED_customdraw",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.CustomDraw,
                MediaType = MediaType.Image,
            };

            slide.Data[ShapeAndTextRenderer.DATAKEY_TEXTS] = Texts;

            List<ProjectAsset> FGAssets = new List<ProjectAsset>();
            List<ProjectAsset> BGAssets = new List<ProjectAsset>();

            foreach (var asset in FGAssetNames)
            {
                var a = project.Assets.FirstOrDefault(x => x.Name == asset);
                if (a == null)
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorName = "Missing Asset",
                        ErrorMessage = $"Can't find asset {asset} in project",
                        Generator = "XenonASTShapesImagesAndText::Generate()",
                        Inner = asset,
                        Level = XenonCompilerMessageType.Error,
                        Token = ""
                    });
                }
                else
                {
                    FGAssets.Add(a);
                }
            }

            foreach (var asset in BGAssetNames)
            {
                var a = project.Assets.FirstOrDefault(x => x.Name == asset);
                if (a == null)
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorName = "Missing Asset",
                        ErrorMessage = $"Can't find asset {asset} in project",
                        Generator = "XenonASTShapesImagesAndText::Generate()",
                        Inner = asset,
                        Level = XenonCompilerMessageType.Error,
                        Token = ""
                    });
                }
                else
                {
                    BGAssets.Add(a);
                }
            }

            slide.Data[ShapeImageAndTextRenderer.DATAKEY_FGDIMAGES] = FGAssets;
            slide.Data[ShapeImageAndTextRenderer.DATAKEY_BKGDIMAGES] = BGAssets;
            slide.Data[ShapeImageAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.CustomDraw].defaultJsonFile;
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.CustomDraw);

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
