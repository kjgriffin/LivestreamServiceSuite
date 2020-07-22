using SlideCreater.LayoutEngine;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTLiturgy : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public void Generate(Project project)
        {
            Slide liturgyslide = new Slide();

            liturgyslide.Asset = string.Empty;

            liturgyslide.Name = "UNNAMED_liturgy";
            liturgyslide.Number = 0;
            liturgyslide.Format = "LITURGY";
            //liturgyslide.Lines = Content.Select(p => new SlideLine { Content = new List<string> { p.TextContent } }).ToList();

            LiturgyLayoutEngine layoutEngine = new LiturgyLayoutEngine();
            layoutEngine.BuildLines(Content.Select(p => p.TextContent).ToList());

            liturgyslide.Lines = layoutEngine.Lines.Select(l => new SlideLine() { Content = new List<string>() { l.speaker, string.Join("", l.words) } }).ToList();

            project.Slides.Add(liturgyslide);
        }
    }
}
