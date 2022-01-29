﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal class XenonASTAsScripted : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }

        public XenonASTElementCollection Children { get; private set; }

        public XenonASTScript AllScript { get; private set; }
        public XenonASTScript FirstScript { get; private set; }
        public XenonASTScript LastScript { get; private set; }

        public bool HasAll { get; private set; }
        public bool HasFirst { get; private set; }
        public bool HasLast { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTAsScripted element = new XenonASTAsScripted();
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
                    Lexer.GobbleandLog("=", "expected = to assign all script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.AllScript = script;
                    element.HasAll = true;
                }
                else if (Lexer.Inspect("first"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=", "expected = to assign first script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.FirstScript = script;
                    element.HasFirst = true;
                }
                else if (Lexer.Inspect("last"))
                {
                    Lexer.Consume();
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("=", "expected = to assign last script");
                    Lexer.GobbleWhitespace();
                    Lexer.GobbleandLog("#", "expected #");
                    Lexer.GobbleandLog("script", "expected script command");
                    Lexer.GobbleWhitespace();
                    XenonASTScript script = new XenonASTScript();
                    script = (XenonASTScript)script.Compile(Lexer, Logger, element);
                    element.LastScript = script;
                    element.HasLast = true;
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
                else
                {
                    modifiedslides.Add(slide);
                }
            }

            return modifiedslides;
        }

        private (Slide scripted, Slide resource) SwapForScript(Slide slide, XenonASTScript script, Project proj, XenonErrorLogger log)
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

            // NOTE: only supports images for now- make huge noise if we are trying to do this for any other type of slide!
            if (slide.MediaType != MediaType.Image)
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

            string srcFile = $"!displaysrc='{behaviour.OverrideExportName}.png';";
            string keyFile = $"!keysrc='{behaviour.OverrideExportKeyName}.png';";

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
            newlines.Insert(0, titleline);

            scriptslide.Data["source"] = string.Join(Environment.NewLine, newlines);

            return (scriptslide, slide);
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