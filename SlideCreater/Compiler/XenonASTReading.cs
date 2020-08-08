using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{


    class XenonASTReading : IXenonASTCommand
    {
        public string Name { get; set; }
        public string Reference { get; set; }

        public void Generate(Project project)
        {
            Slide readingslide = new Slide();
            readingslide.Name = "UNNAMED_reading";
            readingslide.Number = project.NewSlideNumber;
            readingslide.Lines = new List<SlideLine>();
            readingslide.Asset = "";
            readingslide.Format = SlideFormat.Reading;
            readingslide.MediaType = MediaType.Image;

            SlideLineContent slcref = new SlideLineContent() { Data = Reference };
            SlideLineContent slcname = new SlideLineContent() { Data = Name };

            SlideLine slref = new SlideLine() { Content = new List<SlideLineContent>() { slcref } };
            SlideLine slname = new SlideLine() { Content = new List<SlideLineContent>() { slcname } };

            readingslide.Lines.Add(slname);
            readingslide.Lines.Add(slref);

            project.Slides.Add(readingslide);

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTReading>");
            Debug.WriteLine($"Name='{Name}'");
            Debug.WriteLine($"Reference='{Reference}'");
            Debug.WriteLine("</XenonASTReading>");
        }
    }
}
