using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Compiler.LanguageDefinition;
using Xenon.Compiler.Meta;
using Xenon.Helpers;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTPrefabScriptLiturgyOff : IXenonASTCommand
    {

        public string SlideTitleMessage { get; set; } = "Liturgy Off";
        public IXenonASTElement Parent { get; private set; }
        public int _SourceLine { get; set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            // allow optional title parameter
            this._SourceLine = Lexer.Peek().linenum;
            Lexer.GobbleWhitespace();
            if (!Lexer.InspectEOF() && Lexer.Inspect("("))
            {
                Lexer.GobbleandLog("(", "Expecting '('");
                SlideTitleMessage = Lexer.ConsumeUntil(")");
                Lexer.GobbleandLog(")", "Expecting closing ')'");
            }
            this.Parent = Parent;
            return this;
        }

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize)
        {
            sb.Append("".PadLeft(indentDepth * indentSize));
            sb.Append("#");
            sb.Append(LanguageKeywords.Commands[LanguageKeywordCommand.Script_LiturgyOff]);
            if (SlideTitleMessage != "Liturgy Off")
            {
                sb.Append($"({SlideTitleMessage})");
            }
            sb.AppendLine();
        }

        List<Slide> IXenonASTElement.Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            Slide slide = new Slide();
            slide.Name = "UNNAMED_prefab_script";
            slide.Number = project.NewSlideNumber;
            slide.Lines = new List<SlideLine>();
            slide.Asset = "";
            // put script here
            SlideVariableSubstituter.UnresolvedText unresolved = new SlideVariableSubstituter.UnresolvedText
            {
                DKEY = ScriptRenderer.DATAKEY_SCRIPTSOURCE_TARGET,
                Raw = $"#{SlideTitleMessage};" + Environment.NewLine
                + "@arg0:DSK1FadeOff[Kill Liturgy];" + Environment.NewLine
                + "@arg1:DelayMs(1000);",
            };

            slide.Data[SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT] = unresolved;
            slide.Data["prefabtype"] = PrefabSlides.Script_LiturgyOff;
            slide.MediaType = MediaType.Empty;
            slide.Format = SlideFormat.Prefab;
            slide.AddPostset(_Parent, true, true);
            return slide.ToList();
        }

        void IXenonASTElement.GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTPrefabSlide>");
            Debug.WriteLine(PrefabSlides.Script_LiturgyOff);
            Debug.WriteLine($"<Title='{SlideTitleMessage}'/>");
            Debug.WriteLine("</XenonASTPrefabSlide>");
        }

        XenonCompilerSyntaxReport IXenonASTElement.Recognize(Lexer Lexer)
        {
            throw new NotImplementedException();
        }
    }
}
