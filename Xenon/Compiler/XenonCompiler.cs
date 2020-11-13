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

        public bool CompilerSucess { get; set; } = false;

        public Project Compile(Project proj, string input, List<ProjectAsset> assets, IProgress<int> progress)
        {
            CompilerSucess = false; 

            progress.Report(0);

            string preproc = Lexer.StripComments(input);
            Lexer.Tokenize(preproc);

            progress.Report(10);

            XenonASTProgram p = new XenonASTProgram();
            try
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Started", ErrorMessage = "Starting to compile", Generator = "Compiler", Level = XenonCompilerMessageType.Debug });
                p = (XenonASTProgram)p.Compile(Lexer, Logger);
            }
            catch (Exception ex)
            {
                Logger.Log(new XenonCompilerMessage() { ErrorName = "Compilation Failed", ErrorMessage = "Failed to compile project. Check syntax.", Generator = "Compiler", Level = XenonCompilerMessageType.Message });
                Debug.WriteLine($"Compilation Failed \n{ex}");
                return proj;
            }

            progress.Report(50);


            try
            {
                proj?.Clear();
                proj.SourceCode = input;
                p.Generate(proj, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Generation Failed \n{ex}");
                p.GenerateDebug(proj);
                return proj;
            }



            string jsonproj = JsonSerializer.Serialize<Project>(proj);
            Debug.WriteLine(jsonproj);

            progress.Report(100);

            CompilerSucess = true;
            return proj;
        }

    }
}
