using Xenon.AssetManagment;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Xenon.Compiler.AST;

namespace Xenon.Compiler
{
    class XenonCompiler
    {

        /// <summary>
        /// Unescapes characters and removes comments.
        /// </summary>
        /// <param name="input">Input strings - joined on empty string ""</param>
        /// <returns></returns>
        public static string Sanatize(params string[] input)
        {
            // join into one string
            string text = string.Join("", input);

            // unescape characters

            // not sure we want to do this right now
            //string unescapedText = Regex.Unescape(text);

            // remove comments


            return text;
        }


        Lexer Lexer;

        public XenonCompiler()
        {
            Lexer = new Lexer(Logger);
        }

        public XenonErrorLogger Logger { get; set; } = new XenonErrorLogger();
        public XMLErrorGenerator XMLMessageGenerator = new XMLErrorGenerator();

        public bool CompilerSucess { get; set; } = false;

        public Task<Project> Compile(Project proj, IProgress<int> progress = null)
        {
            CompilerSucess = false;

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
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Failed", ErrorMessage = "Failed to compile project. Check syntax.", Generator = "Compiler", Level = XenonCompilerMessageType.Error });
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



            string jsonproj = JsonSerializer.Serialize<Project>(proj, new JsonSerializerOptions() { MaxDepth = 256, ReferenceHandler = ReferenceHandler.Preserve });
            Debug.WriteLine(jsonproj);

            progress?.Report(100);

            CompilerSucess = true;
            return Task.FromResult(proj);

        }

        public Task<Project> Compile(Project proj, string input, List<ProjectAsset> assets, IProgress<int> progress)
        {
            CompilerSucess = false;

            progress.Report(0);

            var preproc = Lexer.StripComments(input);
            Lexer.Tokenize(preproc);

            progress.Report(10);

            XenonASTProgram p = new XenonASTProgram();
            try
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Started", ErrorMessage = "Starting to compile", Generator = "Compiler", Level = XenonCompilerMessageType.Debug });
                XMLMessageGenerator.AddXMLNotes(input, Logger);
                p = (XenonASTProgram)p.Compile(Lexer, Logger, null);
            }
            catch (Exception ex)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Failed", ErrorMessage = "Failed to compile project. Check syntax.", Generator = "Compiler", Level = XenonCompilerMessageType.Error });
                Debug.WriteLine($"Compilation Failed \n{ex}");
                return Task.FromResult(proj);
            }

            progress.Report(50);


            try
            {
                proj?.Clear();
                proj.SourceCode = input;
                p.Generate(proj, null, Logger);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Generation Failed \n{ex}");
                p.GenerateDebug(proj);
                return Task.FromResult(proj);
            }



            string jsonproj = JsonSerializer.Serialize<Project>(proj);
            Debug.WriteLine(jsonproj);

            progress.Report(100);

            CompilerSucess = true;
            return Task.FromResult(proj);
        }

    }
}
