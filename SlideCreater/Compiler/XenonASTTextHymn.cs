using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Compiler
{
    class XenonASTTextHymn : IXenonASTCommand
    {

        public List<XenonASTHymnVerse> Verses { get; set; } = new List<XenonASTHymnVerse>();
        public string HymnTitle { get; set; }
        public string HymnName { get; set; }
        public string Tune { get; set; }
        public string Number { get; set; }
        public string CopyrightInfo { get; set; }

        public void Generate(Project project, IXenonASTElement _Parent)
        {
            foreach (var verse in Verses)
            {
                verse.Generate(project, this);
            }
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTTextHymn>");
            Debug.WriteLine($"HymnTitle='{HymnTitle}'");
            Debug.WriteLine($"HymnName='{HymnName}'");
            Debug.WriteLine($"Tune='{Tune}'");
            Debug.WriteLine($"Number='{Number}'");
            Debug.WriteLine($"CopyrightInfo='{CopyrightInfo}'");
            foreach (var verse in Verses)
            {
                verse.GenerateDebug(project);
            }
            Debug.WriteLine("</XenonASTTextHymn>");
        }
    }
}
