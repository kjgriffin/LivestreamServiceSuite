using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTLiturgyImage : IXenonASTCommand
    {
        public string AssetName { get; set; }
        public void Generate(Project project)
        {
            // create a liturgy image slide
            Slide imageslide = new Slide();
            imageslide.Name = "UNNAMED_image";
            imageslide.Number = project.NewSlideNumber;
            imageslide.Lines = new List<SlideLine>();
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.RelativePath;
            }
            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            imageslide.Lines.Add(sl);
            imageslide.Format = SlideFormat.LiturgyImage;
            imageslide.Asset = assetpath;
            imageslide.MediaType = MediaType.Image;

            project.Slides.Add(imageslide);

        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTLiturgyImage>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTLiturgyImage>");
        }
    }
}
