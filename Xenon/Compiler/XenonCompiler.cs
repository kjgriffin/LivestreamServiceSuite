using Xenon.AssetManagment;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
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
            Lexer = new Lexer();
        }

        public List<XenonCompilerMessage> Messages { get; set; } = new List<XenonCompilerMessage>();

        public bool CompilerSucess { get; set; } = false;

        public Project Compile(string input, List<ProjectAsset> assets, IProgress<int> progress)
        {
            CompilerSucess = false; 
            Project proj = new Project();
            proj.Assets = assets;

            progress.Report(0);

            string preproc = Lexer.StripComments(input);
            Lexer.Tokenize(preproc);

            progress.Report(10);

            XenonASTProgram p = new XenonASTProgram();
            try
            {
                p = (XenonASTProgram)p.Compile(Lexer, Messages);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Compilation Failed \n{ex}");
                return proj;
            }

            progress.Report(50);


            try
            {
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
