using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.AssetManagment;
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

                if (Lexer.Inspect("text"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("{");
                    Lexer.GobbleWhitespace();

                    StringBuilder sb = new StringBuilder();
                    while (!Lexer.Inspect("}"))
                    {
                        sb.Append(Lexer.Consume());
                        // allow string escaping
                        if (Lexer.Peek() == "}" && Lexer.PeekNext() == "}")
                        {
                            Lexer.Consume();
                            sb.Append(Lexer.Consume());
                        }
                    }
                    Lexer.Consume();
                    slide.Texts.Add(sb.ToString());
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

            slide.Data["shape-and-text-strings"] = Texts;

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
            slide.Data["fallback-layout"] = LanguageKeywords.LayoutForType[LanguageKeywordCommand.CustomDraw].defaultJsonFile;
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
