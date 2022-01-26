using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Xenon.Compiler.AST;
using Xenon.Helpers;

namespace Xenon.Compiler
{
    class XenonASTAutoFitImage : IXenonASTCommand
    {
        public string AssetName { get; set; }
        public bool InvertColor { get; set; }
        public string KeyType { get; set; }
        public string Options { get; set; } = "";
        public IXenonASTElement Parent { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTAutoFitImage fullimage = new XenonASTAutoFitImage();
            Lexer.GobbleWhitespace();
            var args = Lexer.ConsumeArgList(false, "asset");
            fullimage.AssetName = args["asset"];

            Lexer.GobbleWhitespace();

            InvertColor = false;
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("("))
                {
                    Lexer.Consume();
                    string val = Lexer.ConsumeUntil(")");
                    if (val == "true")
                    {
                        fullimage.InvertColor = true;
                    }
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                }
            }
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("["))
                {
                    Lexer.Consume();
                    if (Lexer.Inspect("("))
                    {
                        Lexer.Consume();
                        fullimage.Options = Lexer.ConsumeUntil(")");
                        Lexer.Consume();
                        Lexer.Consume();
                        Lexer.GobbleWhitespace();
                    }
                    else
                    {
                        fullimage.KeyType = Lexer.ConsumeUntil("]");
                        Lexer.Consume();
                        Lexer.GobbleWhitespace();
                    }
                }
            }
            fullimage.Parent = Parent; 
            return fullimage;

        }

        public List<Slide> Generate(Project project, IXenonASTElement _parent, XenonErrorLogger Logger)
        {
            // create a full image slide
            Slide imageslide = new Slide();
            imageslide.Name = "UNNAMED_image";
            imageslide.Number = project.NewSlideNumber;
            imageslide.Lines = new List<SlideLine>();
            string assetpath = "";
            var asset = project.Assets.Find(p => p.Name == AssetName);
            if (asset != null)
            {
                assetpath = asset.CurrentPath;
            }
            SlideLineContent slc = new SlideLineContent() { Data = assetpath };
            SlideLine sl = new SlideLine() { Content = new List<SlideLineContent>() { slc } };
            imageslide.Lines.Add(sl);
            imageslide.Format = SlideFormat.AutoscaledImage;
            imageslide.Asset = assetpath;
            imageslide.MediaType = MediaType.Image;
            imageslide.Data["key-type"] = KeyType;

            if (project.ProjectVariables.ContainsKey("invert-autofit"))
            {
                bool val = Convert.ToBoolean(project.GetAttribute("invert-autofit")[0]);
                imageslide.Data["invert-color"] = val | InvertColor;
            }
            else
            {
                imageslide.Data["invert-color"] = InvertColor;
            }

            var match = Regex.Match(Options, @"cc-bw-(?<val>\d{3})");

            if (match.Success)
            {
                imageslide.Data["color-correct-black-white"] = Convert.ToInt32(match.Groups["val"].Value);
            }

            imageslide.AddPostset(_parent, true, true);

            return imageslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTAutoFullImage>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine($"Invert-Color: {InvertColor}");
            Debug.WriteLine($"Options: {Options}");
            Debug.WriteLine("</XenonASTAutoFullImage>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
