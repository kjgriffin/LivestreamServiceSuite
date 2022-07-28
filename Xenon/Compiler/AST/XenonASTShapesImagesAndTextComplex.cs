using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler.SubParsers;
using Xenon.Helpers;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTShapesImagesAndTextComplex : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public string ComplexText { get; private set; } = "";
        public List<string> Texts { get; private set; } = new List<string>();
        public List<string> FGAssetNames { get; private set; } = new List<string>();
        public List<string> BGAssetNames { get; private set; } = new List<string>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTShapesImagesAndTextComplex slide = new XenonASTShapesImagesAndTextComplex();
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            Lexer.GobbleWhitespace();

            do
            {
                if (Lexer.InspectEOF())
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorMessage = "Expecting ending '}' for #complextext command",
                        ErrorName = "Unexpected EOF",
                        Generator = "XenonASTShapesImagesAndTextComplex::Compile()",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = Lexer.CurrentToken,
                    });
                }

                if (Lexer.Inspect("ctext"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("{");
                    Lexer.GobbleWhitespace();

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
                    slide.ComplexText = sb.ToString();

                    Lexer.GobbleWhitespace();
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

            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("ctext={");
            indentDepth++;
            sb.AppendLine(ComplexText);
            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));

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

            var linfo = (this as IXenonASTCommand).GetLayoutOverrideFromProj(project, Logger, LanguageKeywordCommand.ComplexText);
            ComplexShapeImageAndTextLayoutInfo layout = (ComplexShapeImageAndTextLayoutInfo)LanguageKeywords.LayoutForType[LanguageKeywordCommand.ComplexText].layoutResolver._Internal_GetDefaultInfo();
            if (linfo.found)
            {
                layout = JsonSerializer.Deserialize<ComplexShapeImageAndTextLayoutInfo>(linfo.json);
            }

            // perform layout of the complex text to determine number of slides produced
            var stextdata = ComplexTextLayoutEngine.GenerateSlides(XParser.ParseXText(ComplexText), layout.ComplexBoxes.First());

            List<Slide> slides = new List<Slide>();

            if (!stextdata.Any()) // should still generate even without complex text
            {
                stextdata.Add(new List<SizedTextBlurb>());
            }

            // then dump all the other content on each slide
            foreach (var sblock in stextdata)
            {
                Slide slide = new Slide
                {
                    Name = "UNNAMED_customdrawcomplex",
                    Number = project.NewSlideNumber,
                    Lines = new List<SlideLine>(),
                    Asset = "",
                    Format = SlideFormat.ComplexText,
                    MediaType = MediaType.Image,
                };

                slide.Data[ComplexShapeImageAndTextRenderer.DATAKEY_COMPLEX_TEXT] = sblock;

                slide.Data[ComplexShapeImageAndTextRenderer.DATAKEY_TEXTS] = Texts;

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
                            Generator = "XenonASTShapesImagesAndTextComplex::Generate()",
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
                            Generator = "XenonASTShapesImagesAndTextComplex::Generate()",
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

                slide.Data[ComplexShapeImageAndTextRenderer.DATAKEY_FGDIMAGES] = FGAssets;
                slide.Data[ComplexShapeImageAndTextRenderer.DATAKEY_BKGDIMAGES] = BGAssets;
                slide.Data[ComplexShapeImageAndTextRenderer.DATAKEY_FALLBACKLAYOUT] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.ComplexText].defaultJsonFile;
                (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.ComplexText);

                slide.AddPostset(_Parent, true, true);

                slides.Add(slide);

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
