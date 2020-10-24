using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xenon.Compiler.AST;
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
            if (Lexer.GobbleandLog("#"))
            {
                return CompileCommand(Lexer, Logger);
            }

            throw new XenonCompilerException();
        }

        private XenonASTExpression CompileCommand(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTExpression expr = new XenonASTExpression();
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Video]))
            {
                XenonASTVideo video = new XenonASTVideo();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Video]);
                expr.Command = (IXenonASTCommand)video.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]))
            {
                XenonASTFullImage fullimage = new XenonASTFullImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]);
                expr.Command = (IXenonASTCommand)fullimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]))
            {
                XenonASTFitImage fitimage = new XenonASTFitImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]);
                expr.Command = (IXenonASTCommand)fitimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.AutoFitImage]))
            {
                XenonASTAutoFitImage autofit = new XenonASTAutoFitImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.AutoFitImage]);
                expr.Command = (IXenonASTCommand)autofit.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]))
            {
                XenonASTLiturgyImage liturgyimage = new XenonASTLiturgyImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]);
                expr.Command = (IXenonASTCommand)liturgyimage.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Break]))
            {
                XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Break]);
                expr.Command = (IXenonASTCommand)slidebreak.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]))
            {
                XenonASTLiturgy liturgy = new XenonASTLiturgy();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]);
                expr.Command = (IXenonASTCommand)liturgy.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyVerse]))
            {
                XenonASTLiturgyVerse litverse = new XenonASTLiturgyVerse();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyVerse]);
                expr.Command = (IXenonASTCommand)litverse.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]))
            {
                XenonASTTitledLiturgyVerse tlverse = new XenonASTTitledLiturgyVerse();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]);
                expr.Command = (IXenonASTCommand)tlverse.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]))
            {
                XenonASTReading reading = new XenonASTReading();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]);
                expr.Command = (IXenonASTCommand)reading.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]))
            {
                XenonASTSermon sermon = new XenonASTSermon();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]);
                expr.Command = (IXenonASTCommand)sermon.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.AnthemTitle]))
            {
                XenonASTAnthemTitle anthem = new XenonASTAnthemTitle();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.AnthemTitle]);
                expr.Command = (IXenonASTCommand)anthem.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]))
            {
                XenonAST2PartTitle title = new XenonAST2PartTitle();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]);
                expr.Command = (IXenonASTCommand)title.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]))
            {
                XenonASTTextHymn texthymn = new XenonASTTextHymn();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]);
                expr.Command = (IXenonASTCommand)texthymn.Compile(Lexer, Logger);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]);
                expr.Command = new XenonASTPrefabCopyright();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]);
                expr.Command = new XenonASTPrefabViewServices();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]);
                expr.Command = new XenonASTPrefabViewSeries();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]);
                expr.Command = new XenonASTPrefabApostlesCreed();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]);
                expr.Command = new XenonASTPrefabLordsPrayer();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.NiceneCreed]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.NiceneCreed]);
                expr.Command = new XenonASTPrefabNiceneCreed();
                return expr;
            }
            else
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorName = "Unknown Command", ErrorMessage = $"{Lexer.Peek()} is not a recognized command", Token = Lexer.Peek(), Generator = "Compiler" });
                throw new XenonCompilerException();
            }
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
