﻿using SlideCreater.AssetManagment;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Permissions;
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


            try
            {
                p.Generate(proj);
            }
            catch (Exception)
            {
                Debug.WriteLine("Generation Failed");
                p.GenerateDebug(proj);
            }



            string jsonproj = JsonSerializer.Serialize<Project>(proj);
            Debug.WriteLine(jsonproj);

            return proj;
        }

        private XenonASTProgram Program()
        {
            XenonASTProgram p = new XenonASTProgram();
            do
            {
                XenonASTExpression expr = Expression();
                if (expr != null)
                {
                    p.Expressions.Add(expr);
                }
            } while (!Lexer.InspectEOF());

            return p;
        }

        private XenonASTExpression Expression()
        {
            if (Lexer.Inspect("#"))
            {
                Lexer.Gobble("#");
                return Command();
            }
            else if (Lexer.Inspect("\\/\\/"))
            {
                Lexer.Gobble("//");
                return new XenonASTExpression() { Command = Comment() };
            }

            // eat all inter-command whitespace
            Lexer.GobbleWhitespace();

            return null;
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
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]);
                expr.Command = LiturgyImage();
                return expr;
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
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]);
                expr.Command = Reading();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]);
                expr.Command = Sermon();
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

        private IXenonASTCommand Sermon()
        {
            XenonASTSermon sermon = new XenonASTSermon();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect(","))
            {
                sb.Append(Lexer.Consume());
            }
            sermon.Title = sb.ToString().Trim();
            sb.Clear();
            Lexer.Gobble(",");
            while (!Lexer.Inspect("\\)"))
            {
                sb.Append(Lexer.Consume());
            }
            sermon.Reference = sb.ToString().Trim();
            Lexer.Gobble(")");
            return sermon;
        }

        private IXenonASTCommand Reading()
        {
            XenonASTReading reading = new XenonASTReading();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect(","))
            {
                sb.Append(Lexer.Consume());
            }
            reading.Name = sb.ToString().Trim();
            Lexer.Gobble(",");
            sb.Clear();
            while (!Lexer.Inspect("\\)"))
            {
                sb.Append(Lexer.Consume());
            }
            reading.Reference = sb.ToString().Trim();
            Lexer.Gobble(")");
            return reading;
        }

        private IXenonASTCommand LiturgyImage()
        {
            XenonASTLiturgyImage litimage = new XenonASTLiturgyImage();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            litimage.AssetName = Lexer.Consume();
            Lexer.GobbleWhitespace();
            Lexer.Gobble(")");
            return litimage;
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
            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect("\\)"))
            {
                sb.Append(Lexer.Consume());
            }
            video.AssetName = sb.ToString().Trim();
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