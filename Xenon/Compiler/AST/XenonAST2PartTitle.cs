using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonAST2PartTitle : IXenonASTCommand
    {

        public string Part1 { get; set; }
        public string Part2 { get; set; }
        public string Orientation { get; set; }
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "part1", "part2", "orientation");
            Part1 = args["part1"];
            Part2 = args["part2"];
            Orientation = args["orientation"];

            this.Parent = Parent;
            return this;

        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
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


            titleslide.Data["orientation"] = Orientation;
            titleslide.Data["maintext"] = Part1;
            titleslide.Data["subtext"] = Part2;

            var layoutfromproj = (this as IXenonASTElement).TryGetScopedVariable(LanguageKeywords.LayoutVarName(LanguageKeywordCommand.TwoPartTitle), out string layoutoverridefromproj);
            var layoutfromcode = (this as IXenonASTElement).TryGetScopedVariable(LanguageKeywords.LayoutJsonVarName(LanguageKeywordCommand.TwoPartTitle), out string layoutoverridefromcode);

            if (layoutfromproj.found && layoutfromcode.found)
            {
                Logger.Log(new XenonCompilerMessage { ErrorName = "Conflicting Layouts", ErrorMessage = $"A layout for #2Title was defined on the scope:{{{layoutfromproj.scopename}}} as well as on the project with the name{{{layoutoverridefromproj}}}", Generator = "XenonAST2PartTitle::Generate()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
            }

            // TODO: warn if we don't actualy find it on the project
            if (layoutfromproj.found)
            {
                if (project.ProjectLayouts.AllLayouts.TryGetValue(LanguageKeywordCommand.TwoPartTitle, out var layouts))
                {
                    if (layouts.TryGetValue(layoutoverridefromproj, out var layoutjson))
                    {
                        titleslide.Data[Slide.LAYOUT_INFO_KEY] = layoutjson;
                    }
                }
            }
            if (layoutfromcode.found)
            {
                titleslide.Data[Slide.LAYOUT_INFO_KEY] = layoutoverridefromcode;
            }


            titleslide.AddPostset(_Parent, true, true);

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
