using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Xenon.Compiler.LanguageDefinition;
using Xenon.Compiler.Meta;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    [XenonSTDCmdMetadata(LanguageKeywordCommand.NamedScript)]
    [XenonSTDBody(DefinitionRequirement.REQUIRED, true)]
    internal class XenonASTNamedScript : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public XenonASTScript ScriptTemplate { get; private set; }
        public int _SourceLine { get; set; }

        public string NameID { get; private set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTNamedScript element = new XenonASTNamedScript();

            element._SourceLine = Lexer.Peek().linenum;

            element.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expecting opening brace { to mark start of scripted");

            Lexer.GobbleWhitespace();

            Lexer.Gobble("name");
            Lexer.Gobble("=");
            Lexer.Gobble("(");
            element.NameID = Lexer.ConsumeUntil(")");
            Lexer.Gobble(")");

            while (!Lexer.Inspect("script"))
            {
                Lexer.GobbleWhitespace();

                if (Lexer.Inspect("parameter"))
                {
                    Lexer.Gobble("parameter");
                    Lexer.Gobble("(");
                    var pname = Lexer.ConsumeUntil(")");
                    Lexer.Gobble(")");
                    Lexer.Gobble("{");
                    var pdefault = Lexer.ConsumeUntil("}");
                    Lexer.Gobble("}");

                    element.Parameters[pname] = pdefault;
                }
            }

            Lexer.Gobble("script");
            Lexer.Gobble("=");
            Lexer.Gobble("#");
            Lexer.Gobble("script");
            element.ScriptTemplate = new XenonASTScript().Compile(Lexer, Logger, Parent) as XenonASTScript;

            Lexer.GobbleWhitespace();

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

            Lexer.Gobble("}");

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

            // too lazy to implement

            indentDepth--;
            sb.AppendLine("}".PadLeft(indentDepth * indentSize));
        }

        void IXenonASTElement.PreGenerate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // Can we fill in parameters at this point?
        }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            // this probably doesn't do much?
            Slide slide = new Slide
            {
                Asset = "",
                Data = new Dictionary<string, object>(),
                Format = SlideFormat.NamedScript, // own type, so no renderers visit it
                Name = $"NAMED_SCRIPT_{NameID}",
                MediaType = MediaType.Empty,
                Number = -1, // ??
                NonRenderedMetadata = new Dictionary<string, object>(),
            };

            slide.NonRenderedMetadata[SlideVariableSubstituter.UnresolvedScript.DATAKEY_NAMEDSCRIPT_ID] = NameID;
            slide.NonRenderedMetadata[SlideVariableSubstituter.UnresolvedScript.DATAKEY_NAMEDSCRIPT_SOURCE] = ScriptTemplate.Source;
            slide.NonRenderedMetadata[SlideVariableSubstituter.UnresolvedScript.DATAKEY_DEFAULT_ARGUMENTS] = Parameters;

            return new List<Slide> { slide };
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
