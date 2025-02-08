using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls.Ribbon.Primitives;

using Xenon.Compiler.LanguageDefinition;
using Xenon.Compiler.Meta;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{

    internal class ScriptSolution
    {
        XenonASTCalledScript _invokedScript { get; set; }
        XenonASTScript _backingScript { get; set; }
        internal string Source { get => IsBacked ? _backingScript?.Source : ""; }
        internal bool IsBacked { get; private set; }

        internal void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            if (IsBacked)
            {
                _backingScript?.DecompileFormatted(sb, ref indentDepth, indentSize);
            }
            else
            {
                _invokedScript.DecompileFormatted(sb, ref indentDepth, indentSize);
            }
        }

        internal static ScriptSolution Create(XenonASTCalledScript script)
        {
            return new ScriptSolution
            {
                IsBacked = false,
                _invokedScript = script,
            };
        }

        internal static ScriptSolution Create(XenonASTScript script)
        {
            return new ScriptSolution
            {
                IsBacked = true,
                _backingScript = script,
            };
        }

        internal SlideVariableSubstituter.UnresolvedScript BuildUnresolvedScrip()
        {
            if (IsBacked)
                return null;

            return new SlideVariableSubstituter.UnresolvedScript
            {
                InvokedScriptID = _invokedScript.InvokedScript,
                Arguments = _invokedScript.InvokedArguments,
                DKEY = SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT,
            };
        }
    }

    [XenonSTDCmdMetadata(LanguageKeywordCommand.Scripted)]
    [XenonSTDBody(DefinitionRequirement.REQUIRED, true)]
    internal class XenonASTAsScripted : IXenonASTCommand
    {
        public static string DATAKEY_DISPLAYSRC { get => "slide-display-src"; }
        public static string DATAKEY_KEYSRC { get => "slide-key-src"; }

        public IXenonASTElement Parent { get; private set; }

        public XenonASTElementCollection Children { get; private set; }

        public ScriptSolution AllScript { get; private set; }
        public ScriptSolution FirstScript { get; private set; }
        public ScriptSolution LastScript { get; private set; }

        public ScriptSolution DupLastScript { get; private set; }
        public ScriptSolution DupFirstScript { get; private set; }

        public bool HasAll { get; private set; }
        public bool HasFirst { get; private set; }
        public bool HasLast { get; private set; }
        public bool HasDupLast { get; private set; }
        public bool HasDupFirst { get; private set; }
        public int _SourceLine { get; set; }


        private ScriptSolution CompileScriptImplementation(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement element)
        {
            Lexer.GobbleandLog("=", "expected = to assign all script");
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("#", "expected #");

            ScriptSolution solution = null;

            if (Lexer.Inspect("script"))
            {
                Lexer.GobbleandLog("script", "expected script command");
                Lexer.GobbleWhitespace();
                XenonASTScript script = new XenonASTScript();
                script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                solution = ScriptSolution.Create(script);
            }
            else if (Lexer.Inspect("callscript"))
            {
                Lexer.Gobble("callscript");
                Lexer.GobbleWhitespace();
                XenonASTCalledScript script = new XenonASTCalledScript();
                script = (XenonASTCalledScript)script.Compile(Lexer, Logger, element);
                solution = ScriptSolution.Create(script);
            }

            return solution;
        }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTAsScripted element = new XenonASTAsScripted();

            element._SourceLine = Lexer.Peek().linenum;

            element.Children = new XenonASTElementCollection(element);
            element.Children.Elements = new List<IXenonASTElement>();
            element.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expecting opening brace { to mark start of scripted");

            Lexer.GobbleWhitespace();

            // we allow a single script definiton for:
            // first, last, all
            do
            {
                if (Lexer.InspectEOF())
                {
                    Logger.Log(new XenonCompilerMessage
                    {
                        ErrorName = "scripted: body not closed",
                        ErrorMessage = "missing '}' to close body",
                        Generator = "XenonAstAsScripted::Compile",
                        Inner = "",
                        Level = XenonCompilerMessageType.Info,
                        Token = Lexer.CurrentToken
                    });
                }
                if (Lexer.Inspect("all"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();

                    element.AllScript = CompileScriptImplementation(Lexer, Logger, element);
                    /*
                    Lexer.GobbleandLog("=", "expected = to assign all script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.AllScript = script;
                    */
                    element.HasAll = true;
                }
                else if (Lexer.Inspect("first"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    /*
                    Lexer.GobbleandLog("=", "expected = to assign first script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.FirstScript = script;
                    */
                    element.FirstScript = CompileScriptImplementation(Lexer, Logger, element);
                    element.HasFirst = true;
                }
                else if (Lexer.Inspect("last"))
                {
                    if (element.HasDupLast)
                    {
                        Logger.Log(new XenonCompilerMessage
                        {
                            ErrorMessage = "duplast already defined for scripted block. They will conflict with undefined behaviour",
                            ErrorName = "Script Conflict: last conflicts with duplast",
                            Generator = "XenonASTASScripted::Compile()",
                            Inner = "",
                            Level = XenonCompilerMessageType.Error,
                            Token = Lexer.CurrentToken,
                        });
                    }
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    /*
                    Lexer.GobbleandLog("=", "expected = to assign last script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.LastScript = script;
                    */
                    element.LastScript = CompileScriptImplementation(Lexer, Logger, element);
                    element.HasLast = true;
                }
                else if (Lexer.Inspect("duplast"))
                {
                    if (element.HasLast)
                    {
                        Logger.Log(new XenonCompilerMessage
                        {
                            ErrorMessage = "last already defined for scripted block. They will conflict with undefined behaviour",
                            ErrorName = "Script Conflict: last conflicts with duplast",
                            Generator = "XenonASTASScripted::Compile()",
                            Inner = "",
                            Level = XenonCompilerMessageType.Error,
                            Token = Lexer.CurrentToken,
                        });
                    }
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    /*
                    Lexer.GobbleandLog("=", "expected = to assign duplicating last script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.DupLastScript = script;
                    */
                    element.DupLastScript = CompileScriptImplementation(Lexer, Logger, element);
                    element.HasDupLast = true;
                }
                else if (Lexer.Inspect("dupfirst"))
                {
                    if (element.HasFirst)
                    {
                        Logger.Log(new XenonCompilerMessage
                        {
                            ErrorMessage = "first already defined for scripted block. They will conflict with undefined behaviour",
                            ErrorName = "Script Conflict: last conflicts with duplast",
                            Generator = "XenonASTASScripted::Compile()",
                            Inner = "",
                            Level = XenonCompilerMessageType.Error,
                            Token = Lexer.CurrentToken,
                        });
                    }
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    /*
                    Lexer.GobbleandLog("=", "expected = to assign duplicating first script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.DupFirstScript = script;
                    */
                    element.DupFirstScript = CompileScriptImplementation(Lexer, Logger, element);
                    element.HasDupFirst = true;
                }

                else
                {
                    // or allow nested expressions
                    XenonASTExpression expr = new XenonASTExpression();
                    expr = (XenonASTExpression)expr.Compile(Lexer, Logger, element);
                    if (expr != null)
                    {
                        element.Children.Elements.Add(expr);
                    }
                }
                Lexer.GobbleWhitespace();
            }
            while (!Lexer.Inspect("}"));
            Lexer.Consume();

            // might need to throw error/warnings here if we don't find stuff...

            return element;
        }
        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.AppendLine(LanguageKeywords.Commands[LanguageKeywordCommand.Scripted]);

            sb.AppendLine("{".PadLeft(indentDepth * indentSize));
            indentDepth++;


            if (HasFirst)
            {
                sb.Append("first=".PadLeft(indentDepth * indentSize));
                FirstScript.DecompileFormatted(sb, ref indentDepth, indentSize);
            }

            if (HasLast)
            {
                sb.Append("last=".PadLeft(indentDepth * indentSize));
                LastScript.DecompileFormatted(sb, ref indentDepth, indentSize);
            }

            if (HasDupLast)
            {
                sb.Append("duplast=".PadLeft(indentDepth * indentSize));
                DupLastScript.DecompileFormatted(sb, ref indentDepth, indentSize);
            }

            if (HasDupFirst)
            {
                sb.Append("dupfirst=".PadLeft(indentDepth * indentSize));
                DupFirstScript.DecompileFormatted(sb, ref indentDepth, indentSize);
            }


            if (HasAll)
            {
                sb.Append("all=".PadLeft(indentDepth * indentSize));
                AllScript.DecompileFormatted(sb, ref indentDepth, indentSize);
            }

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }

        void IXenonASTElement.PreGenerate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            ((IXenonASTElement)Children).PreGenerate(project, _Parent, Logger);
            // TODO: do we care about the scripts...
            // like- NO for now, but in future?
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            List<Slide> childslides = (Children as IXenonASTElement).Generate(project, _Parent, Logger);

            List<Slide> modifiedslides = new List<Slide>();

            // we can now go through and poke around on the children to edit them

            // TODO: the approach here is to take every slide that was produced by the child expressions
            //       and 'scriptify' it
            //       we'll find the appropriate script to apply (if any) and place a script in it's place
            //       we'll then need to kick the slide out of the 'regular' presentation and make it a resource
            //       we can steal the slide's number to use for the script we're hacking in
            //       NOTE: we may need to consider a more graceful way to handle slide numbering if we ever see the
            //              need to have multiple scripts being applied/slide... ??? (probably not right now)

            // TODO: need a defined way to make a slide a 'resource'
            // NOTE: while I'm thinking it may be worth putting in some mechanism to mark slides as generated for the pool
            //          heck- Integrated Presenter could even auto import the first 4

            foreach (var slide in childslides)
            {
                var scopy = slide.Clone();
                if (slide == childslides.Last() && HasLast)
                {
                    var swaped = SwapForScript(slide, LastScript, project, Logger);
                    modifiedslides.Add(swaped.scripted);
                    modifiedslides.Add(swaped.resource);
                }
                else if (slide == childslides.First() && HasFirst)
                {
                    var swaped = SwapForScript(slide, FirstScript, project, Logger);
                    modifiedslides.Add(swaped.scripted);
                    modifiedslides.Add(swaped.resource);
                }
                else if (HasAll)
                {
                    var swaped = SwapForScript(slide, AllScript, project, Logger);
                    modifiedslides.Add(swaped.scripted);
                    modifiedslides.Add(swaped.resource);
                }
                // Add slides untouches if we don't script them
                else if (!(slide == childslides.Last() && HasDupLast) && !(slide == childslides.First() && HasDupFirst))
                {
                    modifiedslides.Add(slide);
                }

                // handle duplicates
                // last beats first
                if (slide == childslides.Last() && HasDupLast)
                {
                    if (slide == childslides.First() && HasFirst)
                    {
                        // its a duplicating last, and we've already mangled it because the first script has used it
                        // so we'll borrow the original slide, and add a new one with the dup-last
                        // need to increase the number of scopy though

                        // we also need to get down 'n dirty and steal the postset and add it to the duplast
                        var swappedfirst = modifiedslides[modifiedslides.Count - 2];

                        scopy.Number = project.NewSlideNumber;
                        var swaped = SwapForScript(scopy, DupLastScript, project, Logger);

                        // swap postset
                        if (swappedfirst.TryGetPostset(out var postset))
                        {
                            swaped.scripted.Data[XenonASTHelpers.DATAKEY_POSTSET] = postset;
                            // techincally can kill the postset on the swapped slide
                            swappedfirst.Data.Remove(XenonASTHelpers.DATAKEY_POSTSET);
                        }

                        // swap pilot
                        if (swappedfirst.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
                        {
                            swaped.scripted.Data[XenonASTExpression.DATAKEY_PILOT] = pilot;
                            // techincally can kill the postset on the swapped slide
                            swappedfirst.Data.Remove(XenonASTExpression.DATAKEY_PILOT);
                        }


                        modifiedslides.Add(swaped.scripted);
                        modifiedslides.Add(swaped.resource);
                    }
                    else
                    {
                        // still need to duplicate the slide, but it's not mangled by any other scriptification already

                        // add original
                        modifiedslides.Add(slide);

                        // add duplicated scriptified slide
                        scopy.Number = project.NewSlideNumber;
                        var swaped = SwapForScript(scopy, DupLastScript, project, Logger);

                        // extract posteset
                        if (slide.TryGetPostset(out var postset))
                        {
                            swaped.scripted.Data[XenonASTHelpers.DATAKEY_POSTSET] = postset;
                            slide.Data.Remove(XenonASTHelpers.DATAKEY_POSTSET);
                        }

                        modifiedslides.Add(swaped.scripted);
                        modifiedslides.Add(swaped.resource);
                    }
                }
                else if (slide == childslides.First() && HasDupFirst)
                {
                    if (slide == childslides.Last() && HasLast)
                    {
                        // its a duplicating first, and we've already mangled it because the last script has used it
                        // so we'll borrow the original slide, and add a new one with the dup-first
                        // need to increase the number of scopy though

                        // we also need to get down 'n dirty and steal the postset and add it to the duplast
                        var swappedfirst = modifiedslides[0];

                        scopy.Number = project.NewSlideNumber;
                        var swaped = SwapForScript(scopy, DupFirstScript, project, Logger);

                        // swap postset
                        if (swappedfirst.TryGetPostset(out var postset))
                        {
                            swaped.scripted.Data[XenonASTHelpers.DATAKEY_POSTSET] = postset;
                            // techincally can kill the postset on the swapped slide
                            swappedfirst.Data.Remove(XenonASTHelpers.DATAKEY_POSTSET);
                        }

                        // swap pilot
                        if (swappedfirst.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
                        {
                            swaped.scripted.Data[XenonASTExpression.DATAKEY_PILOT] = pilot;
                            // techincally can kill the postset on the swapped slide
                            swappedfirst.Data.Remove(XenonASTExpression.DATAKEY_PILOT);
                        }


                        modifiedslides.Add(swaped.scripted);
                        modifiedslides.Add(swaped.resource);
                    }
                    else
                    {
                        // still need to duplicate the slide, but it's not mangled by any other scriptification already

                        // add original
                        modifiedslides.Add(slide);

                        // add duplicated scriptified slide
                        scopy.Number = project.NewSlideNumber;
                        var swaped = SwapForScript(scopy, DupLastScript, project, Logger);

                        // extract posteset
                        if (slide.TryGetPostset(out var postset))
                        {
                            swaped.scripted.Data[XenonASTHelpers.DATAKEY_POSTSET] = postset;
                            slide.Data.Remove(XenonASTHelpers.DATAKEY_POSTSET);
                        }

                        modifiedslides.Add(swaped.scripted);
                        modifiedslides.Add(swaped.resource);
                    }
                }



            }


            return modifiedslides;
        }

        private (Slide scripted, Slide resource) SwapForScript(Slide slide, ScriptSolution script, Project proj, XenonErrorLogger log)
        {
            int place = slide.Number;


            // directly create the script slide.... (I would rather have had the script do this itself
            // but then it would be forced to use the number
            Slide scriptslide = new Slide
            {
                Name = "UNNAMED_scriptified",
                Number = place,
                Lines = new List<SlideLine>()
            };
            scriptslide.Format = SlideFormat.Script;
            scriptslide.Asset = "";
            scriptslide.MediaType = MediaType.Text;
            // somehow need to dive into the source and set the appropriate overrides

            int rnum = proj.NewResourceSlideNumber;
            SlideOverridingBehaviour behaviour = new SlideOverridingBehaviour
            {
                ForceOverrideExport = true,
                OverrideExportName = $"Resource_{rnum}_forslide_{place}",
                OverrideExportKeyName = $"Resource_{rnum}_forkey_{place}",
            };

            slide.OverridingBehaviour = behaviour;
            slide.Number = -1;

            // swap postset
            if (slide.TryGetPostset(out var postset))
            {
                scriptslide.Data[XenonASTHelpers.DATAKEY_POSTSET] = postset;
                // techincally can kill the postset on the swapped slide
                slide.Data.Remove(XenonASTHelpers.DATAKEY_POSTSET);
            }

            // swap pilot
            if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
            {
                scriptslide.Data[XenonASTExpression.DATAKEY_PILOT] = pilot;
                slide.Data.Remove(XenonASTExpression.DATAKEY_PILOT);
            }

            // swap labeling
            if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_CMD_SOURCESLIDENUM_LABELS, out var labels))
            {
                scriptslide.Data[XenonASTExpression.DATAKEY_CMD_SOURCESLIDENUM_LABELS] = labels;
                slide.Data.Remove(XenonASTExpression.DATAKEY_CMD_SOURCESLIDENUM_LABELS);
            }


            // NOTE: only supports images for now- make huge noise if we are trying to do this for any other type of slide!
            if (slide.MediaType != MediaType.Image && slide.MediaType != MediaType.Video)
            {
                log.Log(new XenonCompilerMessage
                {
                    ErrorName = "Invalid Scriptification!",
                    ErrorMessage = "Trying to replace a slide with a scripted slide. Only Availbe for slides that generate Images",
                    Generator = "XenonASTScripted::SwapForScript()",
                    Inner = $"Slide had type {slide.MediaType}",
                    Level = XenonCompilerMessageType.Error,
                    Token = "",
                });
            }

            string ftype = "png";
            string ktype = "png";

            if (slide.MediaType == MediaType.Video)
            {
                ftype = "mp4";
                // actually look up the asset type
                var match = proj.Assets.FirstOrDefault(x => x.DisplayName == slide.Asset);
                if (match != null)
                {
                    ftype = Regex.Replace(match.Extension, @"\.", "");
                }
            }

            string srcFile = $"!displaysrc='{behaviour.OverrideExportName}.{ftype}';";
            string keyFile = $"!keysrc='{behaviour.OverrideExportKeyName}.{ktype}';";



            scriptslide.Data[DATAKEY_DISPLAYSRC] = srcFile;
            scriptslide.Data[DATAKEY_KEYSRC] = keyFile;

            if (script.IsBacked)
            {
                // if we can resolve now, do so
                InjectScriptData(scriptslide, script.Source);
            }
            else
            {
                // otherwise we'll need to let the substituter handle everything
                scriptslide.Data[SlideVariableSubstituter.UnresolvedScript.DATAKEY_UNRESOLVEDSCRIPT] = script.BuildUnresolvedScrip();
            }

            /*
            // extract the PostScript slide's commands and run overwrite/inject the appropriate src/key file overrides 
            var lines = script.Source.Split(';').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
            List<string> newlines = new List<string>();

            bool setsrc = false;
            bool setkey = false;

            string titleline = "";

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    titleline = line + ";";
                }
                else if (line.StartsWith("!displaysrc="))
                {
                    // make some noise we're overriding it?
                    newlines.Add(srcFile);
                    setsrc = true;
                }
                else if (line.StartsWith("!keysrc="))
                {
                    // make some noise we're overriding it?
                    newlines.Add(keyFile);
                    setkey = true;
                }
                else
                {
                    newlines.Add(line + ";");
                }
            }

            if (!setkey)
            {
                newlines.Insert(0, keyFile);
            }
            if (!setsrc)
            {
                newlines.Insert(0, srcFile);
            }

            //if (slide.MediaType == MediaType.Video)
            //{
            //    // need to point it here
            //    string vsrcFile = $"!videosrc='{behaviour.OverrideExportName}.mp4';";
            //    newlines.Insert(0, vsrcFile);
            //}

            newlines.Insert(0, titleline);

            SlideVariableSubstituter.UnresolvedText unresolved = new SlideVariableSubstituter.UnresolvedText
            {
                DKEY = ScriptRenderer.DATAKEY_SCRIPTSOURCE_TARGET,
                Raw = string.Join(Environment.NewLine, newlines),
            };
            scriptslide.Data[SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;
*/

            return (scriptslide, slide);
        }

        public static void InjectScriptData(Slide target, string scriptSource)
        {
            // extract the PostScript slide's commands and run overwrite/inject the appropriate src/key file overrides 
            var lines = scriptSource.Split(';').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
            List<string> newlines = new List<string>();


            string srcFile = string.Empty;
            string keyFile = string.Empty;
            if (target.Data.TryGetValue(DATAKEY_DISPLAYSRC, out var dfile))
            {
                srcFile = dfile as string;
            }
            if (target.Data.TryGetValue(DATAKEY_KEYSRC, out var kfile))
            {
                keyFile = kfile as string;
            }

            bool setsrc = false;
            bool setkey = false;

            string titleline = "";

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    titleline = line + ";";
                }
                else if (line.StartsWith("!displaysrc="))
                {
                    // make some noise we're overriding it?
                    newlines.Add(srcFile);
                    setsrc = true;
                }
                else if (line.StartsWith("!keysrc="))
                {
                    // make some noise we're overriding it?
                    newlines.Add(keyFile);
                    setkey = true;
                }
                else
                {
                    newlines.Add(line + ";");
                }
            }

            if (!setkey)
            {
                newlines.Insert(0, keyFile);
            }
            if (!setsrc)
            {
                newlines.Insert(0, srcFile);
            }

            //if (slide.MediaType == MediaType.Video)
            //{
            //    // need to point it here
            //    string vsrcFile = $"!videosrc='{behaviour.OverrideExportName}.mp4';";
            //    newlines.Insert(0, vsrcFile);
            //}

            newlines.Insert(0, titleline);

            SlideVariableSubstituter.UnresolvedText unresolved = new SlideVariableSubstituter.UnresolvedText
            {
                DKEY = ScriptRenderer.DATAKEY_SCRIPTSOURCE_TARGET,
                Raw = string.Join(Environment.NewLine, newlines),
            };
            target.Data[SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;
        }

        public void GenerateDebug(Project project)
        {
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }

    }
}
