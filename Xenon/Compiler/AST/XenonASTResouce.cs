using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTResource : IXenonASTCommand
    {

        public string AssetName { get; set; } = "";
        public string Assettype { get; set; } = "";
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTResource resource = new XenonASTResource();
            resource._SourceLine = Lexer.Peek().linenum;
            var args = Lexer.ConsumeArgList(true, "assetname", "type");
            resource.AssetName = args["assetname"];
            resource.Assettype = args["type"];
            resource.Parent = Parent;
            return resource;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.Resource]);
            sb.AppendLine($"(\"{AssetName}\", \"{Assettype}\")");
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide res = new Slide
            {
                Name = "UNNAMED_resource",
                Number = -1, // Slide is not given a number since is not an ordered slide
                Lines = new List<SlideLine>()
            };
            res.Format = SlideFormat.ResourceCopy;
            res.Asset = project.Assets.Find(p => p.Name == AssetName).CurrentPath;
            res.Data["resource.name"] = AssetName;
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

            return res.ToList();
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