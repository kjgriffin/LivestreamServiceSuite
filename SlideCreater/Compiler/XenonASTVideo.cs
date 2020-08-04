using SlideCreater.SlideAssembly;
using System.Diagnostics;
using System.Runtime;

namespace SlideCreater.Compiler
{
    class XenonASTVideo : IXenonASTCommand
    {

        public string AssetName;
        public void Generate(Project project)
        {
            // create a video slide
            Slide videoslide = new Slide();
            videoslide.Name = "UNNAMED_video";
            videoslide.Number = 0;
            videoslide.Lines = new System.Collections.Generic.List<SlideLine>();
            videoslide.Format = "VIDEO";
            videoslide.Asset = AssetName;

            project.Slides.Add(videoslide);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTVideo>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTVideo>");
        }
    }
}