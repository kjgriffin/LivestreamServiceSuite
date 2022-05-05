using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler.AST
{
    class XenonASTTextHymn : IXenonASTCommand
    {

        public List<XenonASTHymnVerse> Verses { get; set; } = new List<XenonASTHymnVerse>();
        public string HymnTitle { get; set; }
        public string HymnName { get; set; }
        public string Tune { get; set; }
        public string Number { get; set; }
        public string CopyrightInfo { get; set; }
        public bool IsOverlay { get; set; } = false;
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement _localParent;
        public int _localVNum;
        public int _localVerses;

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> vslides = new List<Slide>();
            _localVNum = 0;
            _localParent = _Parent;
            _localVerses = Verses.Count - 1;
            foreach (var verse in Verses)
            {
                vslides.AddRange(verse.Generate(project, this, Logger));
                _localVNum += 1;
            }
            return vslides;
        }

        void IXenonASTElement.PreGenerate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            foreach (var verse in Verses)
            {
                ((IXenonASTElement)verse).PreGenerate(project, this, Logger);
            }
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTTextHymn>");
            Debug.WriteLine($"HymnTitle='{HymnTitle}'");
            Debug.WriteLine($"HymnName='{HymnName}'");
            Debug.WriteLine($"Tune='{Tune}'");
            Debug.WriteLine($"Number='{Number}'");
            Debug.WriteLine($"CopyrightInfo='{CopyrightInfo}'");
            Debug.WriteLine($"IsOverlay='{IsOverlay}'");
            foreach (var verse in Verses)
            {
                verse.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTTextHymn>");
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTTextHymn textHymn = new XenonASTTextHymn();

            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "name", "tune", "number", "copyright");

            textHymn.HymnTitle = args["title"];
            textHymn.HymnName = args["name"];
            textHymn.Tune = args["tune"];
            textHymn.Number = args["number"];
            textHymn.CopyrightInfo = args["copyright"];

            if (Lexer.Inspect("("))
            {
                Lexer.Consume();
                string sisoverlay = Lexer.ConsumeUntil(")");
                bool isoverlay;
                bool.TryParse(sisoverlay, out isoverlay);
                textHymn.IsOverlay = isoverlay;
                Lexer.Consume();

                Logger.Log(new XenonCompilerMessage
                {
                    ErrorName = "Deprecated Feature!",
                    ErrorMessage = "This has been deprecated. To achieve the same results, apply a layout override, and in the layout define the SlideType as 'Full.nodrive'",
                    Generator = "XenonASTTextHymn::Compile",
                    Inner = "",
                    Level = XenonCompilerMessageType.Warning,
                    Token = Lexer.CurrentToken,
                });
            }

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expect opening brace for body of hymn. Verses go here");
            Lexer.GobbleWhitespace();

            while (Lexer.Inspect("#"))
            {
                Lexer.GobbleandLog("#", "Expecting '#verse' command");
                // only valid command at this point (so far) is a verse
                if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Verse]))
                {
                    Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Verse], "Only verse command is valid here");
                    XenonASTHymnVerse verse = new XenonASTHymnVerse();
                    textHymn.Verses.Add((XenonASTHymnVerse)verse.Compile(Lexer, Logger, textHymn));
                }
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("}", "Missing closing brace on hymn command.");

            textHymn.Parent = Parent;
            return textHymn;

        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]);
            sb.AppendLine($"(\"{HymnTitle}\", \"{HymnName}\", \"{Tune}\" \"{Number}\", \"{CopyrightInfo}\")");
            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;

            foreach (var verse in Verses)
            {
                verse.DecompileFormatted(sb, ref indentDepth, indentSize);
            }

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));

        }


        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
