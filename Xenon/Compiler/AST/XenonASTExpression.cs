using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonASTExpression : IXenonASTElement
    {
        public IXenonASTCommand Command { get; set; }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Command?.Generate(project, _Parent);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTExperession>");
            Command?.GenerateDebug(project);
            Debug.WriteLine("</XenonASTExperession>");
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            if (Lexer.Inspect("#"))
            {
                Lexer.Gobble("#");
                return CompileCommand(Lexer, Logger);
            }

            // eat all inter-command whitespace
            Lexer.GobbleWhitespace();

            return null;
        }

        private XenonASTExpression CompileCommand(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTExpression expr = new XenonASTExpression();
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Video]))
            {
                XenonASTVideo video = new XenonASTVideo();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Video]);
                expr.Command = (IXenonASTCommand)video.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]))
            {
                XenonASTFullImage fullimage = new XenonASTFullImage();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]);
                expr.Command = (IXenonASTCommand)fullimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]))
            {
                XenonASTFitImage fitimage = new XenonASTFitImage();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]);
                expr.Command = (IXenonASTCommand)fitimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]))
            {
                XenonASTLiturgyImage liturgyimage = new XenonASTLiturgyImage();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]);
                expr.Command = (IXenonASTCommand)liturgyimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Break]))
            {
                XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Break]);
                expr.Command = (IXenonASTCommand)slidebreak.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]))
            {
                XenonASTLiturgy liturgy = new XenonASTLiturgy();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]);
                expr.Command = (IXenonASTCommand)liturgy.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]))
            {
                XenonASTReading reading = new XenonASTReading();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]);
                expr.Command = (IXenonASTCommand)reading.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]))
            {
                XenonASTSermon sermon = new XenonASTSermon();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]);
                expr.Command = (IXenonASTCommand)sermon.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]))
            {
                XenonASTTextHymn texthymn = new XenonASTTextHymn();
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]);
                expr.Command = (IXenonASTCommand)texthymn.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]);
                expr.Command = new XenonASTPrefabSlide() { PrefabSlide = PrefabSlides.Copyright };
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]);
                expr.Command = new XenonASTPrefabSlide() { PrefabSlide = PrefabSlides.ViewServices };
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]);
                expr.Command = new XenonASTPrefabSlide() { PrefabSlide = PrefabSlides.ViewSeries };
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]);
                expr.Command = new XenonASTPrefabSlide() { PrefabSlide = PrefabSlides.ApostlesCreed };
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]);
                expr.Command = new XenonASTPrefabSlide() { PrefabSlide = PrefabSlides.LordsPrayer };
                return expr;
            }
            else
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorName = "Unknown Command", ErrorMessage = $"{Lexer.Peek()} is not a recognized command", Token = Lexer.Peek() });
                throw new ArgumentException($"Unexpected Command. Symbol: '{Lexer.Peek()}' is not a recognized command");
            }
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
