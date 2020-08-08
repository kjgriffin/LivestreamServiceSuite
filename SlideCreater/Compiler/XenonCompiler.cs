using SlideCreater.AssetManagment;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SlideCreater.Compiler
{
    class XenonCompiler
    {

        /// <summary>
        /// Unescapes characters and removes comments.
        /// </summary>
        /// <param name="input">Input strings - joined on empty string ""</param>
        /// <returns></returns>
        public static string Sanatize(params string[] input)
        {
            // join into one string
            string text = string.Join("", input);

            // unescape characters

            // not sure we want to do this right now
            //string unescapedText = Regex.Unescape(text);

            // remove comments


            return text;
        }


        Lexer Lexer;

        public XenonCompiler()
        {
            Lexer = new Lexer();
        }

        public Project Compile(string input, List<ProjectAsset> assets)
        {
            Lexer.Tokenize(input);

            XenonASTProgram p = Program();

            Project proj = new Project();
            proj.Assets = assets;


            p.GenerateDebug(proj);


            p.Generate(proj);



            string jsonproj = JsonSerializer.Serialize<Project>(proj);
            Debug.WriteLine(jsonproj);

            return proj;
        }

        private XenonASTProgram Program()
        {
            XenonASTProgram p = new XenonASTProgram();
            do
            {
                p.Expressions.Add(Expression());
            } while (!Lexer.InspectEOF());

            return p;
        }

        private XenonASTExpression Expression()
        {
            XenonASTExpression expr = new XenonASTExpression();
            if (Lexer.Inspect("#"))
            {
                Lexer.Gobble("#");
                return Command();
            }

            // eat all inter-command whitespace
            Lexer.GobbleWhitespace();

            return expr;
        }

        private XenonASTExpression Command()
        {
            XenonASTExpression expr = new XenonASTExpression();
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Video]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Video]);
                expr.Command = Video();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]);
                expr.Command = FullImage();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]);
                expr.Command = FitImage();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]))
            {
                throw new NotImplementedException("litimage command not implemented");
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Break]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Break]);
                expr.Command = SlideBreak();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]);
                expr.Command = Liturgy();
                return expr;
            }
            else if (Lexer.Inspect("//"))
            {
                Lexer.Gobble("//");
                expr.Command = Comment();
                return expr;
            }
            else
            {
                throw new ArgumentException($"Unexpected Command. Symbol: '{Lexer.Peek()}' is not a recognized command");
            }
        }

        private XenonASTFullImage FullImage()
        {
            XenonASTFullImage fullimage = new XenonASTFullImage();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            fullimage.AssetName = Lexer.Consume();
            Lexer.GobbleWhitespace();
            Lexer.Gobble(")");
            return fullimage;
        }

        private XenonASTFitImage FitImage()
        {
            XenonASTFitImage fullimage = new XenonASTFitImage();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            fullimage.AssetName = Lexer.Consume();
            Lexer.GobbleWhitespace();
            Lexer.Gobble(")");
            return fullimage;
        }



        private XenonAstComment Comment()
        {
            XenonAstComment comment = new XenonAstComment();
            while (!Lexer.Inspect("\r\n"))
            {
                comment.AppendCommentText(Lexer.Consume());
            }
            comment.AppendCommentText(Lexer.Consume());
            return comment;
        }

        private XenonASTVideo Video()
        {
            XenonASTVideo video = new XenonASTVideo();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            video.AssetName = Lexer.Consume();
            Lexer.GobbleWhitespace();
            Lexer.Gobble(")");
            return video;
        }

        private XenonASTSlideBreak SlideBreak()
        {
            XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
            Lexer.GobbleWhitespace();
            return slidebreak;
        }


        public XenonASTLiturgy Liturgy()
        {
            XenonASTLiturgy liturgy = new XenonASTLiturgy();
            // assume all tokens inside braces are litrugy commands
            // only excpetions are we will gobble all leading whitespace in braces, and will remove the last 
            // character of whitespace before last brace


            Lexer.GobbleWhitespace();
            Lexer.Gobble("{");
            Lexer.GobbleWhitespace();
            while (!Lexer.InspectEOF() && !Lexer.Inspect("\\/\\/") && !Lexer.Inspect("}"))
            {
                if (Lexer.PeekNext() == "}")
                {
                    Lexer.GobbleWhitespace();
                    continue;
                }
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                liturgy.Content.Add(content);
            }
            Lexer.Gobble("}");
            return liturgy;
        }




    }
}
