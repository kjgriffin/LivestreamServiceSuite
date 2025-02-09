﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTHymnVerse : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();
        public string SubName { get; set; } = "";
        public bool Doxological { get; set; } = false;
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTHymnVerse verse = new XenonASTHymnVerse();
            verse._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();

            // optionally allow params for verse title. used for e.g. 'chorus'/'refrain'/'verse 1' etc.
            if (Lexer.Inspect("("))
            {
                Lexer.Consume();
                verse.SubName = Lexer.ConsumeUntil(")").tvalue.Trim();
                Lexer.Consume();
                Lexer.GobbleWhitespace();
            }

            if (Lexer.Inspect("["))
            {
                Lexer.Consume();
                var flags = Lexer.ConsumeUntil("]").tvalue.Trim().Split(",");
                Lexer.GobbleandLog("]");
                Lexer.GobbleWhitespace();
                if (flags.Contains("doxological"))
                {
                    verse.Doxological = true;
                }
            }

            Lexer.GobbleandLog("{", "Expect opening brace at start of verse.");

            while (!Lexer.Inspect("}"))
            {
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                verse.Content.Add(content);
            }

            Lexer.GobbleandLog("}", "Missing closing brace for verse.");

            verse.Parent = Parent;
            return verse;

        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.Verse]);

            if (!string.IsNullOrEmpty(SubName))
            {
                sb.Append($"({SubName})");
            }
            sb.AppendLine();

            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;

            // rather do it like this than spit out raw content
            VerseLayoutEngine vle = new VerseLayoutEngine();
            vle.BuildLines(Content.Select(p => p.TextContent).ToList());
            var lines = vle.LayoutLines.Select(x => string.Concat(x.Words).Trim()).ToList();

            foreach (var line in lines)
            {
                sb.Append(line.PadLeft(indentDepth * indentSize));
            }

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));

        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            VerseLayoutEngine vle = new VerseLayoutEngine();
            vle.BuildLines(Content.Select(p => p.TextContent).ToList());

            XenonASTTextHymn parent = _Parent as XenonASTTextHymn;

            Slide slide = new Slide();
            slide.Name = $"texthymn_{parent.HymnName}_verse";

            slide.Number = project.NewSlideNumber;
            slide.Asset = "";
            slide.Lines = new List<SlideLine>();
            slide.Format = SlideFormat.HymnTextVerse;
            slide.MediaType = MediaType.Image;

            slide.Data[HymnTextVerseRenderer.DATAKEY_HTITLE] = parent.HymnTitle;
            slide.Data[HymnTextVerseRenderer.DATAKEY_HNAME] = parent.HymnName;
            slide.Data[HymnTextVerseRenderer.DATAKEY_HNUMBER] = parent.Number;
            slide.Data[HymnTextVerseRenderer.DATAKEY_HTUNE] = parent.Tune;
            slide.Data[HymnTextVerseRenderer.DATAKEY_COPYRIGHT] = parent.CopyrightInfo;
            slide.Data[HymnTextVerseRenderer.DATAKEY_VINFO] = SubName;
            slide.Data[HymnTextVerseRenderer.DATAKEY_DOXOLOGICAL] = Doxological;

            slide.Data[HymnTextVerseRenderer.DATAKEY_HCONTENT] = vle.LayoutLines.Select(x => string.Concat(x.Words).Trim()).ToList();

            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.TextHymn);

            slide.AddPostset(parent._localParent, parent._localVNum == 0, parent._localVNum == parent._localVerses);

            return slide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTHymnVerse>");
            Debug.WriteLine($"<SubName='{SubName}'/>");
            foreach (var c in Content)
            {
                c.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTHymnVerse>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
