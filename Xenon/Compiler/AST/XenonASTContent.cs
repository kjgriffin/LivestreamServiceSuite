﻿using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTContent : IXenonASTElement
    {
        public string TextContent { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            throw new NotImplementedException();
        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            throw new NotImplementedException();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTContent>");
            Debug.WriteLine(TextContent);
            Debug.WriteLine("</XenonASTContent>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
