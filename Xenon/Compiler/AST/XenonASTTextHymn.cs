using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTTextHymn : IXenonASTCommand
    {

        public List<XenonASTHymnVerse> Verses { get; set; } = new List<XenonASTHymnVerse>();
        public string HymnTitle { get; set; }
        public string HymnName { get; set; }
        public string Tune { get; set; }
        public string Number { get; set; }
        public string CopyrightInfo { get; set; }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            foreach (var verse in Verses)
            {
                verse.Generate(project, this);
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
            foreach (var verse in Verses)
            {
                verse.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTTextHymn>");
        }

        public IXenonASTElement Compile(Lexer Lexer, List<XenonCompilerMessage> Messages)
        {
            XenonASTTextHymn textHymn = new XenonASTTextHymn();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            textHymn.HymnTitle = Lexer.ConsumeUntil("\"").Trim();
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(",");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            textHymn.HymnName = Lexer.ConsumeUntil("\"");
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(",");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            textHymn.Tune = Lexer.ConsumeUntil("\"");
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(",");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            textHymn.Number = Lexer.ConsumeUntil("\"");
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(",");
            Lexer.GobbleWhitespace();
            Lexer.Gobble("\"");
            textHymn.CopyrightInfo = Lexer.ConsumeUntil("\"");
            Lexer.Gobble("\"");
            Lexer.GobbleWhitespace();
            Lexer.Gobble(")");

            Lexer.GobbleWhitespace();
            Lexer.Gobble("{");
            Lexer.GobbleWhitespace();

            while (Lexer.Inspect("#"))
            {
                Lexer.Gobble("#");
                // only valid command at this point (so far) is a verse
                if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Verse]))
                {
                    Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Verse]);
                    XenonASTHymnVerse verse = new XenonASTHymnVerse();
                    textHymn.Verses.Add((XenonASTHymnVerse)verse.Compile(Lexer, Messages));
                }
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleWhitespace();
            Lexer.Gobble("}");


            return textHymn;

        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
