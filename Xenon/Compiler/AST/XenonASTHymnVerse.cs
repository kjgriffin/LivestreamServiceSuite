using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xenon.Compiler.AST;
using Xenon.Helpers;
using Xenon.Renderer;

namespace Xenon.Compiler
{
    class XenonASTHymnVerse : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();
        public string SubName { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTHymnVerse verse = new XenonASTHymnVerse();
            Lexer.GobbleWhitespace();

            // optionally allow params for verse title. used for e.g. 'chorus'/'refrain'/'verse 1' etc.
            if (Lexer.Inspect("("))
            {
                Lexer.Consume();
                verse.SubName = Lexer.ConsumeUntil(")").tvalue.Trim();
                Lexer.Consume();
                Lexer.GobbleWhitespace();
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

            slide.Data[HymnTextVerseRenderer.DATAKEY_HCONTENT] = vle.LayoutLines.Select(x => string.Concat(x.Words).Trim()).ToList();

            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.TitledLiturgyVerse);

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
