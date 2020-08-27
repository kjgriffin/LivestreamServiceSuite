using SlideCreater.AssetManagment;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public Project Compile(string input, List<ProjectAsset> assets, out bool success, out List<string> errormsg)
        {

            errormsg = new List<string>();
            success = false;
            
            Project proj = new Project();
            proj.Assets = assets;

            Lexer.Tokenize(input);

            XenonASTProgram p;
            try
            {
                p = Program(errormsg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Compilation Failed");
                errormsg.Add($"Failed to compile project.");
                success = false;
                return proj;
            }



            try
            {
                p.Generate(proj, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Generation Failed");
                errormsg.Add("Failed to build project.");
                p.GenerateDebug(proj);
                success = false;
                return proj;
            }



            string jsonproj = JsonSerializer.Serialize<Project>(proj);
            Debug.WriteLine(jsonproj);

            errormsg.Add($"Lexer Status::Max inspections: {Lexer.MaxChecks}");
            success = true;
            return proj;
        }

        private XenonASTProgram Program(List<string> errormsg)
        {
            errormsg.Add("[XenonCompiler]: Trying to build program.");
            XenonASTProgram p = new XenonASTProgram();
            // gaurd against empty file
            if (Lexer.InspectEOF())
            {
                return p;
            }
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
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]);
                expr.Command = TextHymn();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]);
                expr.Command = PrefabSlide(PrefabSlides.Copyright);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]);
                expr.Command = PrefabSlide(PrefabSlides.ViewServices);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]);
                expr.Command = PrefabSlide(PrefabSlides.ViewSeries);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]);
                expr.Command = PrefabSlide(PrefabSlides.ApostlesCreed);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]))
            {
                Lexer.Gobble(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]);
                expr.Command = PrefabSlide(PrefabSlides.LordsPrayer);
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

        private IXenonASTCommand PrefabSlide(PrefabSlides type)
        {
            return new XenonASTPrefabSlide() { PrefabSlide = type };
        }

        private IXenonASTCommand TextHymn()
        {
            XenonASTTextHymn textHymn = new XenonASTTextHymn();
            Lexer.GobbleWhitespace();
            Lexer.Gobble("(");
            Lexer.GobbleWhitespace();
            textHymn.HymnTitle = Lexer.ConsumeUntil(",").Trim();
            Lexer.Gobble(",");
            textHymn.HymnName = Lexer.ConsumeUntil(",");
            Lexer.Gobble(",");
            textHymn.Tune = Lexer.ConsumeUntil(",");
            Lexer.Gobble(",");
            textHymn.Number = Lexer.ConsumeUntil(",");
            Lexer.Gobble(",");
            textHymn.CopyrightInfo = Lexer.ConsumeUntil("\\)");
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
                    textHymn.Verses.Add(Verse());
                }
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleWhitespace();
            Lexer.Gobble("}");


            return textHymn;
        }

        private XenonASTHymnVerse Verse()
        {
            XenonASTHymnVerse verse = new XenonASTHymnVerse();
            Lexer.GobbleWhitespace();

            Lexer.Gobble("{");

            while (!Lexer.Inspect("}"))
            {
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                verse.Content.Add(content);
            }

            Lexer.Gobble("}");

            return verse;
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
