﻿using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTProgram : IXenonASTElement
    {

        public List<XenonASTExpression> Expressions { get; set; } = new List<XenonASTExpression>();

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            foreach (var item in Expressions)
            {
                item.Generate(project, this);
            }
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTProgram>");
            foreach (var item in Expressions)
            {
                item.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTProgram>");
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTProgram p = new XenonASTProgram();
            // gaurd against empty file
            if (Lexer.InspectEOF())
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Message, ErrorName = "Empty Project", ErrorMessage = "Program contains no symbols.", Token = Lexer.EOFText });
                return p;
            }
            do
            {
                XenonASTExpression expr = new XenonASTExpression();
                Lexer.GobbleWhitespace();
                expr = (XenonASTExpression)expr.Compile(Lexer, Logger);
                if (expr != null)
                {
                    p.Expressions.Add(expr);
                }
                Lexer.GobbleWhitespace();
            } while (!Lexer.InspectEOF());

            return p;
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
