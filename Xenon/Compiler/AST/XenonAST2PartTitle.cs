﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonAST2PartTitle : IXenonASTCommand
    {

        public string Part1 { get; set; }
        public string Part2 { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonAST2PartTitle title = new XenonAST2PartTitle();
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "part1", "part2");
            title.Part1 = args["part1"];
            title.Part2 = args["part2"];
            return title;

        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Slide titleslide = new Slide
            {
                Name = "UNNAMED_2parttitle",
                Number = project.NewSlideNumber,
                Lines = new List<SlideLine>(),
                Asset = "",
                Format = SlideFormat.TwoPartTitle,
                MediaType = MediaType.Image
            };

            SlideLineContent slcpart1 = new SlideLineContent() { Data = Part1 };
            SlideLineContent slcpart2 = new SlideLineContent() { Data = Part2 };

            SlideLine slpart1 = new SlideLine() { Content = new List<SlideLineContent>() { slcpart1 } };
            SlideLine slpart2 = new SlideLine() { Content = new List<SlideLineContent>() { slcpart2} };

            titleslide.Lines.Add(slpart1);
            titleslide.Lines.Add(slpart2);

            project.Slides.Add(titleslide);


        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonAST2PartTitle>");
            Debug.WriteLine($"Part1='{Part1}'");
            Debug.WriteLine($"Part2='{Part2}'");
            Debug.WriteLine("</XenonAST2PartTitle>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}