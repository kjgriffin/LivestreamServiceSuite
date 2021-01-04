using Xenon.SlideAssembly;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    class XenonASTResource : IXenonASTCommand
    {

        public string AssetName { get; set; } = "";
        public string Assettype { get; set; } = "";

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTResource resource = new XenonASTResource();
            var args = Lexer.ConsumeArgList(true, "assetname", "type");
            resource.AssetName = args["assetname"];
            resource.Assettype = args["type"];
            return resource;
        }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            Slide res = new Slide
            {
                Name = "UNNAMED_resource",
                Number = -1, // Slide is not given a number since is not an ordered slide
                Lines = new List<SlideLine>()
            };
            res.Format = SlideFormat.ResourceCopy;
            res.Asset = project.Assets.Find(p => p.Name == AssetName).CurrentPath;
            if (Assettype == "audio")
            {
                res.MediaType = MediaType.Audio;
            }
            if (Assettype == "video")
            {
                res.MediaType = MediaType.Video;
            }
            if (Assettype == "image")
            {
                res.MediaType = MediaType.Image;
            }

            project.Slides.Add(res);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTResource>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("</XenonASTResource>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}