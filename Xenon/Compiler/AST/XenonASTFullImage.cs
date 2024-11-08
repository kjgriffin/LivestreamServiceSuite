﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTFullImage : IXenonASTCommand
    {

        public string AssetName { get; set; }
        public string KeyType { get; set; }
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTFullImage fullimage = new XenonASTFullImage();
            fullimage._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "asset");
            fullimage.AssetName = args["asset"];
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("["))
                {
                    Lexer.Consume();
                    fullimage.KeyType = Lexer.ConsumeUntil("]");
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                }
            }

            fullimage.Parent = Parent;
            return fullimage;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]);
            sb.Append($"({AssetName})");
            if (!string.IsNullOrEmpty(KeyType))
            {
                sb.Append($"[{KeyType}]");
            }
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // create a full image slide
            Slide imageslide = new Slide();
            imageslide.Name = "UNNAMED_image";
            imageslide.Number = project.NewSlideNumber;
            imageslide.Lines = new List<SlideLine>();
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.CurrentPath;
            }
            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            imageslide.Lines.Add(sl);
            imageslide.Format = SlideFormat.UnscaledImage;
            imageslide.Asset = assetpath;
            imageslide.MediaType = MediaType.Image;
            imageslide.Data["key-type"] = KeyType;

            imageslide.AddPostset(_Parent, true, true);

            return imageslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTFullImage>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTFullImage>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
