using SlideCreater.LayoutEngine;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace SlideCreater.Compiler
{
    class XenonASTLiturgy : IXenonASTCommand
    {
        public List<XenonASTContent> Content { get; set; } = new List<XenonASTContent>();

        public void Generate(Project project)
        {

            //liturgyslide.Lines = Content.Select(p => new SlideLine { Content = new List<string> { p.TextContent } }).ToList();

            LiturgyLayoutEngine layoutEngine = new LiturgyLayoutEngine();
            layoutEngine.BuildLines(Content.Select(p => p.TextContent).ToList());
            layoutEngine.BuildSlideLines(project.Layouts.LiturgyLayout.GetRenderInfo());



            //liturgyslide.Lines = layoutEngine.LayoutLines.Select(l => new SlideLine() { Content = new List<string>() { l.speaker, string.Join("", l.words) } }).ToList();


            // turn lines into slides
            /*
                We start by computing the height of each line
                Add this to the running total height of the slide's lines + min interline spacing
                Once we can't fit any more declare a slide, figure out the slide 

                Also must follow the 3 golden rules of slide layout

                1. If the starting speaker is [C] Congregation, no other speaker allowed on the slide
                2. There may be no more than 2 speakers per slide
                3. If a logical-line requires wrapping the line must be the first line of the slide
             */

            Slide liturgyslide = new Slide();
            liturgyslide.Asset = string.Empty;
            liturgyslide.Name = "UNNAMED_liturgy";
            liturgyslide.Number = project.GetNewSlideNumber();
            liturgyslide.Format = "LITURGY";

            double lineheight = -project.Layouts.LiturgyLayout.InterLineSpacing;

            string lastspeaker = "";
            int speakers = 0;
            string startspeaker = layoutEngine.LayoutLines.FirstOrDefault().speaker ?? "";

            foreach (var line in layoutEngine.LayoutLines)
            {
                if (lastspeaker != line.speaker)
                {
                    speakers++;
                }

                bool overheight = lineheight + project.Layouts.LiturgyLayout.InterLineSpacing + line.height > project.Layouts.LiturgyLayout.GetRenderInfo().TextBox.Height;
                bool overspeakerswitch = speakers > 2; // Rule 2
                bool incorrrectspeakerorder = startspeaker == "C" && line.speaker != "C"; // Rule 1
                bool paragraphwrapissue = speakers > 1 && !line.fulltextonline; // Rule 3
                if (overheight || overspeakerswitch || incorrrectspeakerorder || paragraphwrapissue)
                {
                    // need to start a new slide for this one
                    project.Slides.Add(liturgyslide);
                    // create new slide
                    liturgyslide = new Slide();
                    liturgyslide.Asset = string.Empty;
                    liturgyslide.Name = "UNNAMED_liturgy";
                    liturgyslide.Number = project.GetNewSlideNumber();
                    liturgyslide.Format = "LITURGY";
                    lineheight = 0;
                    startspeaker = line.speaker;
                    lastspeaker = line.speaker;
                    speakers = 1;
                }
                lastspeaker = line.speaker;
                lineheight += project.Layouts.LiturgyLayout.InterLineSpacing + line.height;
                liturgyslide.Lines.Add(
                    new SlideLine()
                    {
                        Content = {
                            new SlideLineContent() { Data = line.speaker, Attributes = { ["width"] = line.width, ["height"] = line.height } },
                            new SlideLineContent() { Data = string.Join("", line.words).Trim(), Attributes = { ["width"] = line.width, ["height"] = line.height  } }
                        }
                    }
                );
            }
            // add slide to project
            project.Slides.Add(liturgyslide);
        }


    }
}
