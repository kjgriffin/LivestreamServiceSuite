using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

using Xenon.Compiler.AST;
using Xenon.Compiler.Meta;
using Xenon.SlideAssembly;

namespace Xenon.Compiler
{
    class XenonCompiler
    {

        //Lexer Lexer;

        public XenonCompiler()
        {
            //Lexer = new Lexer(Logger);
        }

        //public XenonErrorLogger Logger { get; set; } = new XenonErrorLogger();
        //public XMLErrorGenerator XMLMessageGenerator = new XMLErrorGenerator();

        internal List<XenonCompilerMessage> Messages = new List<XenonCompilerMessage>();

        public bool CompilerSucess { get; set; } = false;

        /*
        public Task<Project> Compile(Project proj, IProgress<int> progress = null)
        {
            CompilerSucess = false;

            Lexer.ClearInspectionState();

            try
            {
                proj.BMDSwitcherConfig = JsonSerializer.Deserialize<IntegratedPresenter.BMDSwitcher.Config.BMDSwitcherConfigSettings>(proj.SourceConfig);
            }
            catch (Exception ex)
            {
                // log error, but otherwise ignore
                Logger.Log(new XenonCompilerMessage()
                {
                    ErrorName = "Invalid Switcher Config",
                    ErrorMessage = "Something went wrong parsing the switcher config file",
                    Generator = "RenderSlides",
                    Inner = ex.ToString(),
                    Level = XenonCompilerMessageType.Error,
                    Token = "",
                });
                Debug.WriteLine($"Compilation Failed \n{ex}");
                return Task.FromResult(proj);
            }


            progress?.Report(0);

            var preproc = Lexer.StripComments(proj.SourceCode);
            Lexer.Tokenize(preproc);
            //Lexer.Tokenize(new List<(string inputblock, int startline)> { (proj.SourceCode, 9) });

            progress?.Report(10);

            XenonASTProgram p = new XenonASTProgram();
            try
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Started", ErrorMessage = "Starting to compile", Generator = "Compiler", Level = XenonCompilerMessageType.Debug });
                XMLMessageGenerator.AddXMLNotes(proj.SourceCode, Logger);
                p = (XenonASTProgram)p.Compile(Lexer, Logger, null);
            }
            catch (Exception ex)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Failed", ErrorMessage = "Failed to compile project. Check syntax.", Generator = "Compiler", Level = XenonCompilerMessageType.Error, Inner = ex.ToString(), Token = Lexer.CurrentToken });
                Debug.WriteLine($"Compilation Failed \n{ex}");
                return Task.FromResult(proj);
            }

            progress?.Report(50);

            try
            {
                proj?.ClearSlidesAndVariables();
                p?.Generate(proj, null, Logger, progress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Generation Failed \n{ex}");
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Failed", ErrorMessage = "Failed to generate project. Something went wrong after the project was compiled.", Generator = "Project.Generate()", Inner = $"Generate threw exception {ex} at callstack {Environment.StackTrace}", Level = XenonCompilerMessageType.Error });
                p.GenerateDebug(proj);
                return Task.FromResult(proj);
            }

            progress?.Report(90);



            //string jsonproj = JsonSerializer.Serialize<Project>(proj, new JsonSerializerOptions() { MaxDepth = 256, ReferenceHandler = ReferenceHandler.Preserve });
            //Debug.WriteLine(jsonproj);

            progress?.Report(100);

            CompilerSucess = true;
            return Task.FromResult(proj);

        }
        */

        public (HashSet<string> defs, Dictionary<string, int> order) ExtractAllPreProcDefinitions(Project proj)
        {
            HashSet<string> defs = new HashSet<string>();
            Dictionary<string, int> order = new Dictionary<string, int>();

            var main = ExtractPreProcInfoForFile(proj.SourceCode, defs, 0);
            order["main.xenon"] = main;

            foreach (var file in proj.ExtraSourceFiles)
            {
                var forder = ExtractPreProcInfoForFile(file.Value, defs);
                order[file.Key] = forder;
            }

            return (defs, order);
        }

        private int ExtractPreProcInfoForFile(string text, HashSet<string> defs, int orderOverride = 1)
        {
            int order = orderOverride;

            var o = Regex.Match(text, @"^#ORDER\s+(?<num>-?\d+)");
            if (o.Success && int.TryParse(o.Groups["num"].Value, out int i))
            {
                order = i;
            }

            var m = Regex.Matches(text, @"^#DEFINE\s+(?<def>[^\s]+)", RegexOptions.Multiline);
            m.ToImmutableList().ForEach(x =>
            {
                if (x.Success)
                {
                    defs.Add(x.Groups["def"].Value);
                }
            });

            return order;
        }

        public async Task<Project> MultiCompile(Project proj, IProgress<int> progress = null)
        {
            Messages.Clear();
            CompilerSucess = false;

            XenonErrorLogger configlog = new XenonErrorLogger();
            configlog.File = "BMDSwitcherConfig.json";

            XenonErrorLogger masterlog = new XenonErrorLogger();

            try
            {
                proj.BMDSwitcherConfig = JsonSerializer.Deserialize<IntegratedPresenter.BMDSwitcher.Config.BMDSwitcherConfigSettings>(proj.SourceConfig);
            }
            catch (Exception ex)
            {
                // log error, but otherwise ignore
                configlog.Log(new XenonCompilerMessage()
                {
                    ErrorName = "Invalid Switcher Config",
                    ErrorMessage = "Something went wrong parsing the switcher config file",
                    Generator = "RenderSlides",
                    Inner = ex.ToString(),
                    Level = XenonCompilerMessageType.Error,
                    Token = "",
                });
                Debug.WriteLine($"Compilation Failed \n{ex}");
                return proj;
            }
            Messages.AddRange(configlog.AllErrors);

            progress?.Report(10);

            // compile all source in parallel

            // TODO: can we do this with less passes
            var dirs = ExtractAllPreProcDefinitions(proj);

            var compilefiletasks = new List<Task<(XenonASTProgram project, XenonErrorLogger log, bool success)>>();
            compilefiletasks.Add(CompileFile(proj.SourceCode, "main.xenon", dirs.defs));

            foreach (var extrafile in proj.ExtraSourceFiles)
            {
                compilefiletasks.Add(CompileFile(extrafile.Value, extrafile.Key, dirs.defs));
            }
            var compiledFiles = await Task.WhenAll(compilefiletasks);

            // order compiled files
            var orderedFiles = compiledFiles.OrderBy(x => dirs.order[x.log.File]);

            progress?.Report(50);

            // fow now
            CompilerSucess = true;

            List<Slide> compiledSlides = new List<Slide>();

            foreach (var p in orderedFiles)
            {
                if (!p.success)
                {
                    CompilerSucess = false;
                }
                try
                {
                    var slides = p.project?.Generate(proj, null, p.log, progress);
                    if (slides?.Any() == true)
                    {
                        compiledSlides.AddRange(slides);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Generation Failed \n{ex}");
                    masterlog.Log(new XenonCompilerMessage() { ErrorName = "Compilation Failed", ErrorMessage = "Failed to generate project. Something went wrong after the project was compiled.", Generator = "Project.Generate()", Inner = $"Generate threw exception {ex} at callstack {Environment.StackTrace}", Level = XenonCompilerMessageType.Error });
                    p.project.GenerateDebug(proj);
                    CompilerSucess = false;
                    Messages.AddRange(masterlog.AllErrors);
                    return proj;
                }
                // marshal all logs
                Messages.AddRange(p.log.AllErrors);
                Messages.AddRange(masterlog.AllErrors);
            }

            // reset project
            proj?.ClearSlidesAndVariables();
            // analyze all slides at once
            SlideVariableSubstituter subengine = new SlideVariableSubstituter(compiledSlides, proj.BMDSwitcherConfig);
            // at this point we can do this
            proj.Slides.AddRange(subengine.ApplyNesscarySubstitutions());


            progress?.Report(100);

            return proj;
        }


        private Task<(XenonASTProgram, XenonErrorLogger, bool)> CompileFile(string source, string file, HashSet<string> preProcDefs)
        {
            return Task.Run(() =>
            {
                bool success = false;
                XenonErrorLogger logger = new XenonErrorLogger();
                logger.File = file;
                Lexer lexer = new Lexer(logger);

                var preproc = lexer.StripComments(source);
                preproc = lexer.StripPreProc(preproc, preProcDefs);
                lexer.Tokenize(preproc);
                lexer.ClearInspectionState();

                XenonASTProgram p = new XenonASTProgram();
                try
                {
                    logger.Log(new XenonCompilerMessage() { ErrorName = $"Compilation Started on {file}", ErrorMessage = "Starting to compile", Generator = "Compiler", Level = XenonCompilerMessageType.Debug });
                    XMLErrorGenerator.AddXMLNotes(source, logger);
                    p = (XenonASTProgram)p.Compile(lexer, logger, null);

                    // perhaps?
                    success = true;
                    if (logger.AllErrors.Any(x => x.Level == XenonCompilerMessageType.Error))
                    {
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(new XenonCompilerMessage() { ErrorName = $"Compilation Failed on {file}", ErrorMessage = "Failed to compile project. Check syntax.", Generator = "Compiler", Level = XenonCompilerMessageType.Error, Inner = ex.ToString(), Token = lexer.CurrentToken });
                    Debug.WriteLine($"Compilation Failed \n{ex}");
                }

                return Task.FromResult((p, logger, success));
            });
        }


    }
}
