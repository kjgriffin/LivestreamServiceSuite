using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTReStitchedHymn : IXenonASTCommand
    {

        public List<string> ImageAssets { get; set; } = new List<string>();
        public string Title { get; set; }
        public string HymnName { get; set; }
        public string Number { get; set; }
        public string CopyrightInfo { get; set; }

        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        private string CopyrightTune
        {
            get
            {
                var split = CopyrightInfo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3)
                {
                    return "Tune: " + split[0];
                }
                else
                {
                    return CopyrightInfo;
                }
            }
        }

        private string CopyrightText
        {
            get
            {
                var split = CopyrightInfo.Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 3)
                {
                    return "Text: " + split[2];
                }
                else
                {
                    return CopyrightInfo;
                }
            }
        }



        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTReStitchedHymn hymn = new XenonASTReStitchedHymn();
            hymn._SourceLine = Lexer.Peek().linenum;

            Lexer.GobbleWhitespace();

            var args = Lexer.ConsumeArgList(true, "title", "name", "number", "copyright");

            hymn.Title = args["title"];
            hymn.HymnName = args["name"];
            hymn.Number = args["number"];
            hymn.CopyrightInfo = args["copyright"];

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("{", "Expected opening '{'");

            Lexer.GobbleWhitespace();

            var inputToken = new Token();
            if (!Lexer.InspectEOF() && Lexer.Inspect("input"))
            {
                Lexer.Consume();
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("=");
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("{");
                inputToken = Lexer.CurrentToken;
                hymn.InputFormat = Lexer.ConsumeUntil("}");
                // VALIDATE input:
                Lexer.GobbleandLog("}");
                Lexer.GobbleWhitespace();
            }

            var outputToken = new Token();
            if (!Lexer.InspectEOF() && Lexer.Inspect("output"))
            {
                if (string.IsNullOrWhiteSpace(hymn.InputFormat))
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorName = "Illegal specification",
                        ErrorMessage = "Can't declare an output format without also providing a concrete input format",
                        Generator = "XenonASTReStitchedHymn::Compile",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = Lexer.CurrentToken,
                    });
                }
                Lexer.Consume();
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("=");
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("{");
                outputToken = Lexer.CurrentToken;
                hymn.OutputFormat = Lexer.ConsumeUntil("}");
                // VALIDATE output:
                Lexer.GobbleandLog("}");
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleandLog("assets");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("=");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{");
            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                Lexer.GobbleWhitespace();
                string assetline = Lexer.ConsumeUntil(";");
                hymn.ImageAssets.Add(assetline);
                Lexer.GobbleandLog(";", "Expected ';' at end of asset dependency");
                Lexer.GobbleWhitespace();
            }
            Lexer.GobbleandLog("}", "Expected closing '}'");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("}", "Expected closing '}'");


            // once we have all assets we can validate/compile formatting
            var x = XenonReStitchedInputFormat.Compile(hymn.InputFormat, hymn.ImageAssets);

            hymn.Parent = Parent;
            return hymn;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]);
            sb.Append($"(\"{Title}\", \"{HymnName}\", \"{Number}\", \"{CopyrightInfo}\")");

            sb.AppendLine();
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("{");

            if (!string.IsNullOrWhiteSpace(InputFormat))
            {
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine($"input={{{InputFormat}}}");
            }
            if (!string.IsNullOrWhiteSpace(OutputFormat))
            {
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.AppendLine($"input={{{OutputFormat}}}");
            }

            sb.AppendLine();
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.AppendLine("{");
            indentDepth++;
            foreach (var asset in ImageAssets)
            {
                sb.Append("".PadLeft(indentDepth * indentSize));
                sb.Append(asset);
                sb.AppendLine(";");
            }
            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }


        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> slides = new List<Slide>();
            Logger.Log(new XenonCompilerMessage() { ErrorMessage = $"Generating StitchedHymn {HymnName}", ErrorName = "Generation Debug Log", Generator = "XenonASTStitchedHymn:Generate()", Inner = "", Level = XenonCompilerMessageType.Debug, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });

            Dictionary<string, Size> ImageSizes = new Dictionary<string, Size>();

            foreach (var item in ImageAssets)
            {
                string assetpath = project.Assets.Find(a => a.Name == item)?.CurrentPath ?? "";
                try
                {
                    if (!string.IsNullOrEmpty(assetpath))
                    {

                        ImageInfo metadata = Image.Identify(assetpath);
                        ImageSizes[item] = new Size(metadata.Width, metadata.Height);
                    }
                    else
                    {
                        Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Generating StitchedHymn", ErrorName = "Failed to load image asset", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"{item}", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
                        Debug.WriteLine($"Error opening image to check size: {item}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(new XenonCompilerMessage() { ErrorMessage = "Generating StitchedHymn", ErrorName = "Failed to load image asset", Generator = "XenonASTStitchedHymn:Generate()", Inner = $"{ex}", Level = XenonCompilerMessageType.Warning, Token = ("", IXenonASTCommand.GetParentExpression(this)._SourceLine) });
                    Debug.WriteLine($"Error opening image to check size: {ex}");
                    // hmmm....
                }
            }


            // do it all on one slide
            Slide slide = new Slide();
            slide.Name = $"restitchedhymn";
            slide.Number = project.NewSlideNumber;
            slide.Asset = "";
            slide.Lines = new List<SlideLine>();
            slide.Format = SlideFormat.StitchedImage;
            slide.MediaType = MediaType.Image;

            slide.Data[StitchedImageRenderer.DATAKEY_TITLE] = Title;
            slide.Data[StitchedImageRenderer.DATAKEY_NAME] = HymnName;
            slide.Data[StitchedImageRenderer.DATAKEY_NUMBER] = Number;
            slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT] = CopyrightInfo;
            if (CopyrightText != CopyrightTune)
            {
                slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TEXT] = CopyrightText;
                slide.Data[StitchedImageRenderer.DATAKEY_COPYRIGHT_TUNE] = CopyrightTune;
            }

            slide.Data[StitchedImageRenderer.DATAKEY_IMAGES] = ImageSizes.Select(i => new LSBImageResource(i.Key, i.Value)).ToList();
            slide.AddPostset(_Parent, true, true);
            (this as IXenonASTCommand).ApplyLayoutOverride(project, Logger, slide, LanguageKeywordCommand.StitchedImage);
            slides.Add(slide);

            return slides;
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTStitchedHymn>");
            Debug.WriteLine($"Title={Title}");
            Debug.WriteLine($"HymnName={HymnName}");
            Debug.WriteLine($"Number={Number}");
            Debug.WriteLine($"Copyright={CopyrightInfo}");
            foreach (var asset in ImageAssets)
            {
                Debug.WriteLine($"ImageAsset={asset}");
            }
            Debug.WriteLine("</XenonASTStitchedHymn>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }



    class XenonReStitchedInputFormat
    {
        public List<XenonReStitchedLine> Lines { get; set; } = new List<XenonReStitchedLine>();


        internal static bool Validate(string format)
        {
            if (!format.EndsWith(";"))
            {
                return false;
            }
            var lines = format.Split(";");
            foreach (var line in lines)
            {
                if (!XenonReStitchedLine.Validate(line))
                {
                    return false;
                }
            }
            return true;
        }

        internal static XenonReStitchedInputFormat Compile(string input, List<string> assets)
        {
            List<string> _assets = new List<string>(assets);
            List<XenonReStitchedLine> blocks = new List<XenonReStitchedLine>();
            var lines = input.Split(";");
            foreach (var line in lines)
            {
                blocks.Add(XenonReStitchedLine.Compile(line, _assets));
            }
            return new XenonReStitchedInputFormat { Lines = blocks };
        }
    }

    class XenonReStitchedLine
    {
        public string ExpandedOrder { get; set; }
        public List<string> Lines { get; set; }
        public string Name { get; set; }

        public int Repeats { get; set; }
        public int VLines { get; set; }

        internal static bool Validate(string format)
        {
            return true;
        }

        internal static XenonReStitchedLine Compile(string input, List<string> assets)
        {
            // parse tag
            var m = Regex.Match(input, @"(?<bname>\w+)<(?<block>[^;]+)>(?<repeats>\d+)\*(?<lines>\d+)");
            string tagName = m.Groups["bname"].Value;
            string val = m.Groups["block"].Value;

            int.TryParse(m.Groups["repeats"].Value, out int repeats);
            int.TryParse(m.Groups["lines"].Value, out int vlines);

            // begin expanding
            string expanded = Expand(val);

            // with expanded value, just eat 'em up
            // warn if we run-out
            var lines = expanded.Split(",").ToList();
            if (lines.Count > assets.Count)
            {
                throw new Exception("Can't parse input format");
            }

            var res = new XenonReStitchedLine
            {
                Lines = assets.Take(lines.Count).ToList(),
                ExpandedOrder = expanded,
                Name = tagName,
                Repeats = repeats,
                VLines = vlines,
            };
            assets.RemoveRange(0, lines.Count);
            return res;
        }


        static string Expand(string input)
        {

            string str = input;
            Regex regex = new Regex(@"\((?<group>[^()]+)\)\[(?<qty>\d+)\]");

            bool more = true;
            while (more)
            {
                var match = regex.Match(str);
                if (match.Success)
                {
                    more = true;
                    int.TryParse(match.Groups["qty"].Value, out int times);
                    string val = match.Groups["group"].Value;
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < times; i++)
                    {
                        sb.Append(val);
                        if (i < times - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    str = regex.Replace(str, sb.ToString(), 1);
                }
                else
                {
                    more = false;
                }
            }

            return str;
        }

    }












}
