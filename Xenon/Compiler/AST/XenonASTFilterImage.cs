using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Xenon.Renderer.ImageFilters;
using Xenon.Helpers;
using Xenon.Compiler.AST;

namespace Xenon.Compiler
{
    class XenonASTFilterImage : IXenonASTCommand
    {
        public List<(ImageFilter Type, ImageFilterParams FParams)> Filters { get; set; } = new List<(ImageFilter Type, ImageFilterParams)>();
        public IXenonASTElement Parent { get; private set; }

        private Dictionary<int, string> assetstoresolve = new Dictionary<int, string>();
        private int assetids = 0;

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
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
                if (filtername == "centerassetfill")
                {
                    CompileFilterCommand_centerassetfill(Lexer, Logger);
                }
                if (filtername == "uniformstretch")
                {
                    CompileFilterCommand_uniformstetch(Lexer, Logger);
                }
                if (filtername == "centeronbackground")
                {
                    CompileFilterCommand_centeronbackground(Lexer, Logger);
                }
                if (filtername == "coloredit")
                {
                    CompileFilterCommand_coloredit(Lexer, Logger, ImageFilter.ColorEditRGB);
                }
                if (filtername == "coloredithsv")
                {
                    CompileFilterCommand_coloredit(Lexer, Logger, ImageFilter.ColorEditHSV);
                }
                if (filtername == "colorshifthsv")
                {
                    CompileFilterCommand_colorshifthsv(Lexer, Logger);
                }
                if (filtername == "colortint")
                {
                    CompileFilterCommand_colortint(Lexer, Logger);
                }
                if (filtername == "coloruntint")
                {
                    CompileFilterCommand_coloruntint(Lexer, Logger);
                }

                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleWhitespace();

            Lexer.GobbleandLog("}", "Expecting closing '}' after filter chain.");

            this.Parent = Parent;

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

        private void CompileFilterCommand_centerassetfill(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            CenterAssetFillFilterParams fparams = new CenterAssetFillFilterParams();
            // parse params

            Lexer.GobbleandLog("asset", "Expected 'asset' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting '(' to enclose asset name");
            string pname = Lexer.ConsumeUntil(")", "Expecting paramater: asset");
            Lexer.GobbleandLog(")", "Expecting ')' to enclose asset name");
            fparams.AssetPath = assetids.ToString();
            assetstoresolve[assetids] = pname;
            assetids++;

            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.CenterAssetFill, fparams));

        }

        private void CompileFilterCommand_uniformstetch(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            UniformStretchFilterParams fparams = new UniformStretchFilterParams();
            // parse params

            Lexer.GobbleandLog("width", "Expected 'width' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pwidth = Lexer.ConsumeUntil(",");
            Lexer.GobbleandLog(",", "Expecting ',' before height parameter");
            int.TryParse(pwidth, out int width);
            fparams.Width = width;

            Lexer.GobbleandLog("height", "Expected 'height' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pheight = Lexer.ConsumeUntil(",");
            Lexer.GobbleandLog(",", "Expecting ',' before fill parameter");
            int.TryParse(pheight, out int height);
            fparams.Height = height;

            Lexer.GobbleandLog("fill", "Expecting 'fill' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pcolorfill = Lexer.ConsumeUntil(")", "Expecting parameter: fill (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            Lexer.GobbleandLog(",", "Expecting , before next parameter");
            fparams.Fill = GraphicsHelper.ColorFromRGB(pcolorfill);

            Lexer.GobbleandLog("kfill", "Expecting 'kfill' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pkeyfill = Lexer.ConsumeUntil(")", "Expecting parameter: kfill (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            fparams.KFill = GraphicsHelper.ColorFromRGB(pkeyfill);


            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.UniformStretch, fparams));
        }

        private void CompileFilterCommand_centeronbackground(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            CenterOnBackgroundFilterParams fparams = new CenterOnBackgroundFilterParams();
            // parse params

            Lexer.GobbleandLog("width", "Expected 'width' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pwidth = Lexer.ConsumeUntil(",");
            Lexer.GobbleandLog(",", "Expecting ',' before height parameter");
            int.TryParse(pwidth, out int width);
            fparams.Width = width;

            Lexer.GobbleandLog("height", "Expected 'height' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pheight = Lexer.ConsumeUntil(",");
            Lexer.GobbleandLog(",", "Expecting ',' before fill parameter");
            int.TryParse(pheight, out int height);
            fparams.Height = height;

            Lexer.GobbleandLog("fill", "Expecting 'fill' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pcolorfill = Lexer.ConsumeUntil(")", "Expecting parameter: fill (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            Lexer.GobbleandLog(",", "Expecting , before next parameter");
            fparams.Fill = GraphicsHelper.ColorFromRGB(pcolorfill);

            Lexer.GobbleandLog("kfill", "Expecting 'kfill' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pkeyfill = Lexer.ConsumeUntil(")", "Expecting parameter: kfill (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            fparams.KFill = GraphicsHelper.ColorFromRGB(pkeyfill);


            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.CenterOnBackground, fparams));
        }

        private void CompileFilterCommand_coloredit(Lexer Lexer, XenonErrorLogger Logger, ImageFilter mode)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            ColorEditFilterParams fparams = new ColorEditFilterParams();
            // parse params

            Lexer.GobbleandLog("identifier", "Expecting 'identifier' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string pcolorrgb = Lexer.ConsumeUntil(")", "Expecting parameter: identifier (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            Lexer.GobbleandLog(",", "Expecting , before next parameter");
            fparams.Identifier = GraphicsHelper.ColorFromRGB(pcolorrgb);

            Lexer.GobbleandLog("replace", "Expecting 'replace' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            Lexer.GobbleandLog("(", "Expecting open '('");
            string preplacergb = Lexer.ConsumeUntil(")", "Expecting parameter: replace (r,g,b) e.g. (0, 132, 39)");
            Lexer.GobbleandLog(")", "Expecting closing ')'");
            Lexer.GobbleandLog(",", "Expecting , before next parameter");
            fparams.Replace = GraphicsHelper.ColorFromRGB(preplacergb);

            Lexer.GobbleandLog("exclude", "Expected 'exclude' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pexclude = Lexer.ConsumeUntil(",", "Expecting paramater: exclude (True, False)");
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            bool.TryParse(pexclude, out bool exclude);
            fparams.IsExcludeMatch = exclude;

            Lexer.GobbleandLog("forkey", "Expected 'exclude' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to start parameter value");
            string pkey = Lexer.ConsumeUntil(",", "Expecting paramater: exclude (True, False)");
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            bool.TryParse(pkey, out bool key);
            fparams.ForKey = key;

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

            Filters.Add((mode, fparams));

        }

        private void CompileFilterCommand_colorshifthsv(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            ColorShiftFilterParams fparams = new ColorShiftFilterParams();
            // parse params

            Lexer.GobbleandLog("hue", "Expecting 'hue' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Hue = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting hue value e.g. 0-240"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("sat", "Expecting 'sat' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Saturation = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting saturation value e.g. 0-240"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("lum", "Expecting 'lum' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Luminance = Convert.ToInt32(Lexer.ConsumeUntil(";", "Expecting luminance value e.g. 0-240"));
            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.ColorShiftHSV, fparams));

        }

        private void CompileFilterCommand_colortint(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            ColorTintFilterParams fparams = new ColorTintFilterParams();
            // parse params

            Lexer.GobbleandLog("r", "Expecting 'r' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Red = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting red value e.g. 0-255"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("g", "Expecting 'g' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Green = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting green value e.g. 0-255"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("b", "Expecting 'b' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Blue = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting blue value e.g. 0-255"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("mix", "Expecting 'mix' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Mix = Convert.ToDouble(Lexer.ConsumeUntil(",", "Expecting mix value e.g. 0..1"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("gamma", "Expecting 'gamma' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Gammma = Convert.ToDouble(Lexer.ConsumeUntil(";", "Expecting mix value e.g. 0..1"));
            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.ColorTint, fparams));

        }

        private void CompileFilterCommand_coloruntint(Lexer Lexer, XenonErrorLogger Logger)
        {
            Lexer.GobbleandLog("::", "Expected '::' after filtername and before filter params.");
            ColorUnTintFilterParams fparams = new ColorUnTintFilterParams();
            // parse params

            Lexer.GobbleandLog("r", "Expecting 'r' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Red = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting red value e.g. 0-255"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("g", "Expecting 'g' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Green = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting green value e.g. 0-255"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("b", "Expecting 'b' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Blue = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting blue value e.g. 0-255"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("mix", "Expecting 'mix' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Mix = Convert.ToDouble(Lexer.ConsumeUntil(",", "Expecting mix value e.g. 0..1"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("gamma", "Expecting 'gamma' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.Gammma = Convert.ToDouble(Lexer.ConsumeUntil(",", "Expecting mix value e.g. 0..1"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("hmin", "Expecting 'hmin' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.HueMin = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting hue min value e.g. 0-240"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("hmax", "Expecting 'hmax' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.HueMax = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting hue max value e.g. 0-240"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("lmin", "Expecting 'lmin' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.LumMin = Convert.ToInt32(Lexer.ConsumeUntil(",", "Expecting lum min value e.g. 0-240"));
            Lexer.GobbleandLog(",", "Expecting ',' before next parameter");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("lmax", "Expecting 'lmax' parameter");
            Lexer.GobbleandLog("=", "Expecting '=' to starrt parameter value");
            fparams.LumMax = Convert.ToInt32(Lexer.ConsumeUntil(";", "Expecting lum max value e.g. 0-240"));


            Lexer.GobbleandLog(";", "Expecting ';' at end of filter");

            Filters.Add((ImageFilter.ColorUnTint, fparams));

        }




        public List<Slide> Generate(Project project, IXenonASTElement _parent, XenonErrorLogger Logger)
        {
            // create a full image slide
            Slide imageslide = new Slide();
            imageslide.Name = "UNNAMED_image";
            imageslide.Number = project.NewSlideNumber;
            imageslide.Lines = new List<SlideLine>();
            imageslide.Asset = "";


            // resolve assets used in filters
            foreach (var f in Filters)
            {
                if (f.Type == ImageFilter.CenterAssetFill)
                {
                    int id = int.Parse((f.FParams as CenterAssetFillFilterParams).AssetPath);
                    var asset = project.Assets.Find(p => p.Name == assetstoresolve[id]);
                    if (asset != null)
                    {
                        (f.FParams as CenterAssetFillFilterParams).AssetPath = asset.CurrentPath;
                    }
                }
            }

            imageslide.Format = SlideFormat.FilterImage;
            imageslide.MediaType = MediaType.Image;

            // set filter data
            imageslide.Data["filter-chain"] = Filters;

            imageslide.AddPostset(_parent, true, true);

            return imageslide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTFilterImage>");
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
