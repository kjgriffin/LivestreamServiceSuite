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

        public Project Compile(string input)
        {
            Lexer.Tokenize(input);

            XenonASTProgram p = Program();

            Project proj = new Project();
            p.Generate(proj);

            p.GenerateDebug(proj);
            

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
            if (Lexer.Inspect("#video"))
            {
                Lexer.Gobble("#video");
                expr.Command = Video();
                return expr;
            }
            if (Lexer.Inspect("#break"))
            {
                Lexer.Gobble("#break");
                expr.Command = SlideBreak();
                return expr;
            }
            if (Lexer.Inspect("//"))
            {
                Lexer.Gobble("//");
                expr.Command = Comment();
                return expr;
            }



            
            // else assume its liturgy
            expr.Command = Liturgy();
            return expr;
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
            // assume all tokens until we find one starting with # are to be considred the content
            while(!Lexer.InspectEOF() && !Lexer.Inspect("#", false) && !Lexer.Inspect("\\/\\/"))
            {
                XenonASTContent content = new XenonASTContent() { TextContent = Lexer.Consume() };
                liturgy.Content.Add(content);
            }
            return liturgy;
        }
    

        

    }
}
