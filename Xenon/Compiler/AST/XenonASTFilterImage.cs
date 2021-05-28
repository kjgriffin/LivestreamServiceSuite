using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Xenon.Renderer.ImageFilters;
using Xenon.Helpers;

namespace Xenon.Compiler
{
    class XenonASTFilterImage : IXenonASTCommand
    {
        public string AssetName { get; set; }
        public List<(ImageFilter Type, ImageFilterParams FParams)> Filters { get; set; } = new List<(ImageFilter Type, ImageFilterParams)>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTFilterImage filterimage = new XenonASTFilterImage();
            Lexer.GobbleWhitespace();
            //var args = Lexer.ConsumeArgList(false, "asset");
            //filterimage.AssetName = args["asset"];
            Lexer.GobbleWhitespace();

            if (!Lexer.InspectEOF())
            {
                Lexer.GobbleandLog("{", "Expected opening '{' for start of filter chain.");
            }

            // parse each filter
            while (!Lexer.InspectEOF() && !Lexer.Inspect("}"))
            {
                Lexer.GobbleWhitespace();

                // parse filters
                string filtername = Lexer.Consume();
                if (filtername == "solidcolorcanvas")
                {
                    CompileFilterCommand_solidcolorcanvas(Lexer, Logger);
                }
                if (filtername == "crop")
                {
                    CompileFilterCommand_crop(Lexer, Logger);
                }

            }

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("}", "Expecting closing '}' after filter chain.");

            return this;
        }

        private void CompileFilterCommand_crop(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            CropFilterParams fparams = new CropFilterParams();
            // parse params
            Lexer.GobbleandLog("bound", "Expected 'bound' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pbound = Lexer.ConsumeUntil(",", "Expecting paramater: bound (Top, Left, Bottom, Right)");
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            CropFilterParams.CropBound bound;
            if (!Enum.TryParse(pbound, out bound))
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Invalid Parameter", ErrorMessage = $"{bound} invalid value for bound parameter of crop filter. Expected (top, left, bottom, right)", Generator = "FilterImage.Compile", Inner = "Parsing Parameters", Level = XenonCompilerMessageType.Error, Token = pbound });
            }
            fparams.Bound = bound;

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("exclude", "Expecting 'exclude' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pexclude = Lexer.ConsumeUntil(",", "Expecting parameter: exclude (True, False)");
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            bool isexclude;
            if (!bool.TryParse(pexclude, out isexclude))
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Invalid Parameter", ErrorMessage = $"{bound} invalid value for exclude parameter of crop filter. Expected (true, false)", Generator = "FilterImage.Compile", Inner = "Parsing Parameters", Level = XenonCompilerMessageType.Error, Token = pbound });
            }
            fparams.IsExcludeMatch = isexclude;

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("icolor", "Expecting 'icolor' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pcolorrgb = Lexer.ConsumeUntil(")", "Expecting parameter: icolor (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            Lexer.GobbleandLog(",", "Expecting , before next parameter");
            fparams.Identifier = GraphicsHelper.ColorFromRGB(pcolorrgb);

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("rtol", "Expecting 'rtol' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.RTolerance = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting tolerance value e.g. 123"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("gtol", "Expecting 'gtol' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.GTolerance = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting tolerance value e.g. 123"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("btol", "Expecting 'btol' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.BTolerance = Convert.ToInt32(Lexer.ConsumeUntil(";", "Expecting tolerance value e.g. 123"));
            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.Crop, fparams));
        }

        private void CompileFilterCommand_solidcolorcanvas(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            SolidColorCanvasFilterParams fparams = new SolidColorCanvasFilterParams();
            // parse params

            Lexer.GobbleandLog("width", "Expected 'width' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pwidth = Lexer.ConsumeUntil(",", "Expecting paramater: width <int> eg. 1920");
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            int width = 1920;
            int.TryParse(pwidth, out width);
            fparams.Width = width;

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("height", "Expected 'height' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pheight = Lexer.ConsumeUntil(",", "Expecting paramater: height <int> eg. 1080");
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            int height = 1080;
            int.TryParse(pheight, out height);
            fparams.Height = height;


            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("color", "Expecting 'color' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pcolorrgb = Lexer.ConsumeUntil(")", "Expecting parameter: icolor (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            Lexer.GobbleandLog(",", "Expecting , before next parameter");
            fparams.Background = GraphicsHelper.ColorFromRGB(pcolorrgb);

            Lexer.GobbleWhitespace();



            Lexer.GobbleandLog("kcolor", "Expecting 'kcolor' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pcolorrgk = Lexer.ConsumeUntil(")", "Expecting parameter: icolor (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            fparams.KBackground = GraphicsHelper.ColorFromRGB(pcolorrgk);

            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.SolidColorCanvas, fparams));

        }


        public void Generate(Project project, IXenonASTElement _Project)
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

            imageslide.Format = SlideFormat.FilterImage;
            imageslide.Asset = assetpath;
            imageslide.MediaType = MediaType.Image;

            // set filter data
            imageslide.Data["filter-chain"] = Filters;


            project.Slides.Add(imageslide);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTFilterImage>");
            Debug.WriteLine(AssetName);
            Debug.WriteLine("<Filters>");
            Debug.WriteLine(Filters);
            Debug.WriteLine("</Filters>");
            Debug.WriteLine("</XenonASTFilterImage>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
