using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xenon.Compiler.LanguageDefinition;
using Xenon.Compiler.SubParsers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTExpression : IXenonASTElement
    {
        public static string DATAKEY_PILOT { get => "pilot-data"; }
        public static string DATAKEY_PILOT_SOURCECODE_LOOKUP { get => "source-code-lookup-pilot"; }
        public static string DATAKEY_CMD_SOURCECODE_LOOKUP { get => "source-code-lookup-generating-command"; }
        public static string DATAKEY_CMD_SOURCESLIDENUM_LABELS { get => "source-code-slide-num-lookup-label"; }
        public static string DATAKEY_CMD_SOURCEFILE_LOOKUP { get => "source-code-sourcefile-label"; }

        public int _SourceLine { get; private set; } = -1;
        public int _PilotSourceLine { get; private set; } = -1;
        public IXenonASTCommand Command { get; set; }
        public bool Postset { get; set; } = false;
        public bool Postset_forAll { get => Postset_All >= 0; }
        public bool Postset_forFirst { get => Postset_First >= 0; }
        public bool Postset_forLast { get => Postset_Last >= 0; }
        public int Postset_All { get; set; } = -1;
        public int Postset_First { get; set; } = -1;
        public int Postset_Last { get; set; } = -1;

        public Dictionary<string, string> PilotCommands = new Dictionary<string, string>();
        public List<string> Attributes = new List<string>();

        public IXenonASTElement Parent { get; private set; }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            var slides = Command?.Generate(project, this, Logger) ?? new List<Slide>();

            // apply anything on the scope
            if (Command.TryGetScopedVariable(SlideRenderer.VARNAME_SLIDE_PREMULTIPLEY_OVERRIDE, out string pval).found)
            {
                if (bool.TryParse(pval, out var premultiply))
                {
                    slides.ForEach(x =>
                    {
                        x.Data[SlideRenderer.DATAKEY_SLIDE_PREMULTIPLY_OVERRIDE] = premultiply;
                    });
                }
            }

            foreach (var s in slides)
            {
                if (!s.NonRenderedMetadata.TryGetValue(DATAKEY_PILOT_SOURCECODE_LOOKUP, out var oldline))
                {
                    s.NonRenderedMetadata[DATAKEY_PILOT_SOURCECODE_LOOKUP] = _PilotSourceLine;
                }
                else
                {
                    // allow overwrite if we have better data, or crappy existing data
                    if (_PilotSourceLine != -1 || (int)oldline == -1)
                    {
                        s.NonRenderedMetadata[DATAKEY_PILOT_SOURCECODE_LOOKUP] = _PilotSourceLine;
                    }
                }
                if (!s.NonRenderedMetadata.TryGetValue(DATAKEY_CMD_SOURCECODE_LOOKUP, out _))
                {
                    if (Command?._SourceLine > -1)
                    {
                        s.NonRenderedMetadata[DATAKEY_CMD_SOURCECODE_LOOKUP] = Command._SourceLine;
                    }
                }
                // process labeling
                var label = Attributes.FirstOrDefault(x => x.StartsWith("@label::"));
                if (label != null)
                {
                    var lname = label.Remove(0, "@label::".Length);
                    if (s.Data.TryGetValue(DATAKEY_CMD_SOURCESLIDENUM_LABELS, out var d))
                    {
                        var dl = d as List<string>;
                        dl.Add(lname);
                    }
                    else
                    {
                        s.Data[DATAKEY_CMD_SOURCESLIDENUM_LABELS] = new List<string> { lname };
                    }
                }
            }

            var flightworthyslides = slides.Where(x => x.Number >= 0).OrderBy(x => x.Number).ToList();

            // add pilot here
            Dictionary<string, int> mappedKeys = new Dictionary<string, int>();
            Dictionary<int, string> sanatizedCommands = new Dictionary<int, string>();
            foreach (var key in PilotCommands.Keys)
            {
                int slideid = -1;
                // compute it's refrenced slide
                if (key.StartsWith("!"))
                {
                    if (int.TryParse(key.Substring(1), out int sid))
                    {
                        var rs = slides.Count - 1 - sid;
                        if (rs < slides.Count && rs > 0)
                        {
                            slideid = rs;
                        }
                    }
                }
                else
                {
                    if (int.TryParse(key, out int sid))
                    {
                        if (sid < slides.Count)
                        {
                            slideid = sid;
                        }
                    }
                }
                if (slideid != -1)
                {
                    mappedKeys[key] = slideid;
                }
            }

            foreach (var val in mappedKeys)
            {
                var rawSrc = PilotCommands[val.Key];
                var src = PilotParser.SanatizePilotCommands(rawSrc);
                sanatizedCommands[val.Value] = src;
            }

            foreach (var pilotSet in sanatizedCommands)
            {
                //slides[pilotSet.Key].Data[DATAKEY_PILOT] = pilotSet.Value;
                flightworthyslides[pilotSet.Key].Data[DATAKEY_PILOT] = pilotSet.Value;
            }

            return slides;
        }

        void IXenonASTElement.PreGenerate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Command.PreGenerate(project, this, Logger);
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTExperession>");
            Command?.GenerateDebug(project);
            Debug.WriteLine("</XenonASTExperession>");
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTExpression expr;
            List<string> attrs = new List<string>();

            Lexer.GobbleWhitespace();
            while (!Lexer.InspectEOF() && !Lexer.Inspect("#"))
            {
                // allow optional 'attribute' style on any valid command
                if (Lexer.Inspect("["))
                {
                    Lexer.Consume();
                    attrs.Add(Lexer.ConsumeUntil("]"));
                    Lexer.GobbleandLog("]");
                    Lexer.GobbleWhitespace();
                }
            }

            // parse expressions command
            if (Lexer.GobbleandLog("#"))
            {
                int sline = Lexer.Peek().linenum;
                expr = CompileCommand(Lexer, Logger);
                if (expr._SourceLine == -1)
                {
                    expr._SourceLine = sline;
                }
                expr.Attributes = attrs;
            }
            else
            {
                throw new XenonCompilerException();
            }

            Lexer.GobbleWhitespace();

            // parse optional expression postshot
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect("::"))
                {
                    CompilePostset(expr, Lexer, Logger);
                }
            }

            Lexer.GobbleWhitespace();

            // parse optional pilot
            if (!Lexer.InspectEOF())
            {
                if (Lexer.Inspect(">>"))
                {
                    CompilePilot(expr, Lexer, Logger);
                }
            }

            expr.Parent = Parent;
            return expr;
        }

        private void CompilePilot(XenonASTExpression expr, Lexer lexer, XenonErrorLogger logger)
        {
            lexer.Gobble(">>");
            lexer.GobbleandLog("pilot", "Expected 'pilot' tag.");

            lexer.GobbleWhitespace();

            lexer.GobbleandLog("{");
            lexer.GobbleWhitespace();

            var stok = lexer.Peek();
            expr._PilotSourceLine = stok.linenum;

            int inspections = 0;
            while (inspections++ < 10000 && !lexer.InspectEOF() && !lexer.Inspect("}"))
            {
                lexer.GobbleandLog("<");
                var id = lexer.ConsumeUntil(">");
                lexer.GobbleandLog(">");
                lexer.GobbleWhitespace();
                lexer.GobbleandLog("{");
                var src = lexer.ConsumeUntil("}");
                lexer.GobbleandLog("}");
                lexer.GobbleWhitespace();
                expr.PilotCommands[id] = src;
            }
            if (inspections >= 10000)
            {
                logger.Log(new XenonCompilerMessage
                {
                    ErrorMessage = "Lexer possibly stuck in a loop trying to parse malformed pilot sequence",
                    ErrorName = "Lexer stuck",
                    Generator = "XenonASTExpression::CompilePilot()",
                    Inner = "",
                    Level = XenonCompilerMessageType.Error,
                    Token = lexer.CurrentToken,
                });
            }
            lexer.GobbleandLog("}");
        }

        private void CompilePostset(XenonASTExpression expr, Lexer lexer, XenonErrorLogger logger)
        {
            lexer.Gobble("::");

            lexer.GobbleandLog("postset", "Expected 'postset' tag.");

            var args = lexer.ConsumeOptionalNamedArgsUnenclosed("all", "first", "last");

            if (args.ContainsKey("all"))
            {
                if (int.TryParse(args["all"], out int val))
                {
                    expr.Postset_All = val;
                }
            }
            if (args.ContainsKey("first"))
            {
                if (int.TryParse(args["first"], out int val))
                {
                    expr.Postset_First = val;
                }
            }
            if (args.ContainsKey("last"))
            {
                if (int.TryParse(args["last"], out int val))
                {
                    expr.Postset_Last = val;
                }
            }

            // handle missing params

            expr.Postset = true;

            if (!expr.Postset_forAll && !expr.Postset_forFirst && !expr.Postset_forLast)
            {
                // bad params
                // let it compile still
                expr.Postset = false;
                // log error
                logger.Log(new XenonCompilerMessage() { ErrorName = "Missing 'postset' parameters", ErrorMessage = $"Expression marked to have 'postset' but no parameters were provided. Use any of 'all', 'first', 'last'.", Generator = "XenonASTExpression.CompilePostset", Inner = "Will ignore postset.", Level = XenonCompilerMessageType.Error });
            }

        }

        private XenonASTExpression CompileCommand(Lexer Lexer, XenonErrorLogger Logger)
        {
            XenonASTExpression expr = new XenonASTExpression();
            IXenonASTElement parent = expr;

            // warn for deprecated
            var cmd = Lexer.Peek();

            var CMeta = LanguageKeywords.Commands.OrderByDescending(x => x.Value.Length).FirstOrDefault(x => x.Value == cmd);
            if (XenonAPIConstructor.APIMetadata.TryGetValue(CMeta.Key, out var xapi))
            {
                if (xapi.STDMetadata.Deprecated)
                {
                    Logger.Log(new XenonCompilerMessage()
                    {
                        ErrorName = "Command Deprecated",
                        ErrorMessage = $"{CMeta.Value} has been deprecated",
                        Generator = "Compiler::XenonASTExpression::XenonAPIConstructor",
                        Inner = "",
                        Level = XenonCompilerMessageType.Error,
                        Token = cmd,
                        SrcFile = Logger.File,
                    });
                }
            }

            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Resource]))
            {
                XenonASTResource resource = new XenonASTResource();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Resource]);
                expr.Command = (IXenonASTCommand)resource.Compile(Lexer, Logger, parent);
                return expr;
            }
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.DynamicControllerDef]))
            {
                XenonASTDynamicController resource = new XenonASTDynamicController();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.DynamicControllerDef]);
                expr.Command = (IXenonASTCommand)resource.Compile(Lexer, Logger, parent);
                return expr;
            }

            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Script]))
            {
                XenonASTScript script = new XenonASTScript();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Script]);
                expr.Command = (IXenonASTCommand)script.Compile(Lexer, Logger, parent);
                return expr;
            }

            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.HTML]))
            {
                XenonASTHTML html = new XenonASTHTML();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.HTML]);
                expr.Command = (IXenonASTCommand)html.Compile(Lexer, Logger, parent);
                return expr;
            }
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.HTML2]))
            {
                XenonASTHtml2 html = new XenonASTHtml2();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.HTML2]);
                expr.Command = (IXenonASTCommand)html.Compile(Lexer, Logger, parent);
                return expr;
            }




            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.SetVar]))
            {
                XenonASTSetVariable xenonASTSetVariable = new XenonASTSetVariable();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.SetVar]);
                expr.Command = (IXenonASTCommand)xenonASTSetVariable.Compile(Lexer, Logger, parent);
                return expr;
            }
            if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Video]))
            {
                XenonASTVideo video = new XenonASTVideo();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Video]);
                expr.Command = (IXenonASTCommand)video.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FilterImage]))
            {
                XenonASTFilterImage fimage = new XenonASTFilterImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FilterImage]);
                expr.Command = (IXenonASTCommand)fimage.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]))
            {
                XenonASTFullImage fullimage = new XenonASTFullImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FullImage]);
                expr.Command = (IXenonASTCommand)fullimage.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]))
            {
                XenonASTFitImage fitimage = new XenonASTFitImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.FitImage]);
                expr.Command = (IXenonASTCommand)fitimage.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.AutoFitImage]))
            {
                XenonASTAutoFitImage autofit = new XenonASTAutoFitImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.AutoFitImage]);
                expr.Command = (IXenonASTCommand)autofit.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]))
            {
                XenonASTStitchedHymn hymn = new XenonASTStitchedHymn();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]);
                expr.Command = (IXenonASTCommand)hymn.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ReStitchedHymn]))
            {
                XenonASTReStitchedHymn hymn = new XenonASTReStitchedHymn();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ReStitchedHymn]);
                expr.Command = (IXenonASTCommand)hymn.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]))
            {
                XenonASTLiturgyImage liturgyimage = new XenonASTLiturgyImage();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyImage]);
                expr.Command = (IXenonASTCommand)liturgyimage.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Break]))
            {
                XenonASTSlideBreak slidebreak = new XenonASTSlideBreak();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Break]);
                expr.Command = (IXenonASTCommand)slidebreak.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]))
            {
                XenonASTLiturgy liturgy = new XenonASTLiturgy();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy]);
                expr.Command = (IXenonASTCommand)liturgy.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy2]))
            {
                XenonASTLiturgy2 liturgy = new XenonASTLiturgy2();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Liturgy2]);
                expr.Command = (IXenonASTCommand)liturgy.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyVerse]))
            {
                XenonASTLiturgyVerse litverse = new XenonASTLiturgyVerse();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LiturgyVerse]);
                expr.Command = (IXenonASTCommand)litverse.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]))
            {
                XenonASTTitledLiturgyVerse tlverse = new XenonASTTitledLiturgyVerse();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse]);
                expr.Command = (IXenonASTCommand)tlverse.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse2]))
            {
                XenonASTTitledLiturgy tlverse = new XenonASTTitledLiturgy();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TitledLiturgyVerse2]);
                expr.Command = (IXenonASTCommand)tlverse.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]))
            {
                XenonASTReading reading = new XenonASTReading();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Reading]);
                expr.Command = (IXenonASTCommand)reading.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]))
            {
                XenonASTSermon sermon = new XenonASTSermon();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Sermon]);
                expr.Command = (IXenonASTCommand)sermon.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.AnthemTitle]))
            {
                XenonASTAnthemTitle anthem = new XenonASTAnthemTitle();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.AnthemTitle]);
                expr.Command = (IXenonASTCommand)anthem.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]))
            {
                XenonAST2PartTitle title = new XenonAST2PartTitle();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle]);
                expr.Command = (IXenonASTCommand)title.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]))
            {
                XenonASTTextHymn texthymn = new XenonASTTextHymn();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.TextHymn]);
                expr.Command = (IXenonASTCommand)texthymn.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Copyright]);
                expr.Command = new XenonASTPrefabCopyright();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ViewServices]);
                expr.Command = new XenonASTPrefabViewServices();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ViewSeries]);
                expr.Command = new XenonASTPrefabViewSeries();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ApostlesCreed]);
                expr.Command = new XenonASTPrefabApostlesCreed();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.LordsPrayer]);
                expr.Command = new XenonASTPrefabLordsPrayer();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.NiceneCreed]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.NiceneCreed]);
                expr.Command = new XenonASTPrefabNiceneCreed();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Script_LiturgyOff]))
            {
                var command = new XenonASTPrefabScriptLiturgyOff();
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Script_LiturgyOff]);
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Script_OrganIntro]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Script_OrganIntro]);
                expr.Command = new XenonASTPrefabScriptOrganIntro();
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.VariableScope]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.VariableScope]);
                var command = new XenonASTVariableScope();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ScopedVariable]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ScopedVariable]);
                var command = new XenonASTScopedVariable();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.UpNext]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.UpNext]);
                var command = new XenonASTUpNext();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.CustomText]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.CustomText]);
                var command = new XenonASTShapesAndText();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.CustomDraw]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.CustomDraw]);
                var command = new XenonASTShapesImagesAndText();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.ComplexText]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.ComplexText]);
                var command = new XenonASTShapesImagesAndTextComplex();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.Scripted]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.Scripted]);
                var command = new XenonASTAsScripted();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.NamedScript]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.NamedScript]);
                var command = new XenonASTNamedScript();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }
            else if (Lexer.Inspect(LanguageKeywords.Commands[LanguageKeywordCommand.CalledScript]))
            {
                Lexer.GobbleandLog(LanguageKeywords.Commands[LanguageKeywordCommand.CalledScript]);
                var command = new XenonASTCalledScript();
                expr.Command = (IXenonASTCommand)command.Compile(Lexer, Logger, parent);
                return expr;
            }

            else
            {
                Logger.Log(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorName = "Unknown Command", ErrorMessage = $"{Lexer.Peek()} is not a recognized command", Token = Lexer.Peek(), Generator = "Compiler" });
                throw new XenonCompilerException();
            }
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            // postset is handled at this level,
            // may need to refactor the newline/command logic to here

            // new line between all commands?
            sb.AppendLine();

            // let the command generate itself
            Command.DecompileFormatted(sb, ref indentDepth, indentSize);

            // add its postset
            if (Postset)
            {
                sb.Append("::postset(");
                bool sep = false;

                if (Postset_forFirst)
                {
                    sb.Append($"first={Postset_First}");
                    sep = true;
                }

                if (Postset_forLast)
                {
                    if (sep)
                    {
                        sb.Append(", ");
                    }
                    sb.Append($"last={Postset_Last}");
                    sep = true;
                }

                if (Postset_forAll)
                {
                    if (sep)
                    {
                        sb.Append(", ");
                    }
                    sb.Append($"all={Postset_All}");
                }

                sb.AppendLine(")");
            }
        }
    }
}
