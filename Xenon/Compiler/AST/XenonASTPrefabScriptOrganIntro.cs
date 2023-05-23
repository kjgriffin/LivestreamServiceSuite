using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Xenon.Compiler.Meta;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabScriptOrganIntro : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            this.Parent = Parent;
            this._SourceLine = Lexer.Peek().linenum;
            return this;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.Script_OrganIntro]);
            sb.AppendLine();
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide();
            slide.Name = "UNNAMED_prefab_script";
            slide.Number = project.NewSlideNumber;
            slide.Lines = new List<SlideLine>();
            slide.Asset = "";
            // put script here
            SlideNumberVariableSubstituter.UnresolvedText unresolved = new SlideNumberVariableSubstituter.UnresolvedText
            {
                DKEY = ScriptRenderer.DATAKEY_SCRIPTSOURCE_TARGET,
                Raw = "#Organ Intro;" + Environment.NewLine
                + "@arg1:PresetSelect(5)[Preset Organ];" + Environment.NewLine
                + "@arg1:DelayMs(100);" + Environment.NewLine
                + "@arg0:AutoTrans[Take Organ];",
            };
            slide.Data[SlideNumberVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;
            slide.Data["prefabtype"] = PrefabSlides.Script_OrganIntro;
            slide.MediaType = MediaType.Empty;
            slide.Format = SlideFormat.Prefab;
            slide.AddPostset(_Parent, true, true);
            return slide.ToList();
        }

        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.Script_OrganIntro);
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        public XenonCompilerSyntaxReport Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
