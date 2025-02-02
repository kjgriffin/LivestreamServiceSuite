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
    internal class XenonASTCalledScript : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public XenonASTScript ScriptTemplate { get; private set; }
        public int _SourceLine { get; set; }

        public string InvokedScript { get; private set; } = string.Empty;
        public Dictionary<string, string> InvokedArguments { get; private set; } = new Dictionary<string, string>();

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTCalledScript element = new XenonASTCalledScript();

            element._SourceLine = Lexer.Peek().linenum;

            element.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expecting opening brace { to mark start of callscript");
            Lexer.GobbleWhitespace();
            do
            {
                if (Lexer.Inspect("scriptname"))
                {
                    Lexer.Gobble("scriptname");
                    Lexer.Gobble("(");
                    element.InvokedScript = Lexer.ConsumeUntil(")", escapewithbackslash: true);
                    Lexer.Gobble(")");
                }
                if (Lexer.Inspect("parameter"))
                {
                    Lexer.Gobble("parameter");
                    Lexer.Gobble("(");
                    string pname = Lexer.ConsumeUntil(")", escapewithbackslash: true);
                    Lexer.Gobble(")");
                    Lexer.Gobble("{");
                    string pval = Lexer.ConsumeUntil("}", escapewithbackslash: true);
                    Lexer.Gobble("}");
                    element.InvokedArguments[pname] = pval;
                }
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
            // if this is a top-level command, it would need to generate itself

            var slide = new Slide
            {
                Asset = "",
                Data = new Dictionary<string, object>(),
                NonRenderedMetadata = new Dictionary<string, object>(),
                Name = $"CalledScript_{this.InvokedScript}",
                Number = project.NewSlideNumber,
                MediaType = MediaType.Text,
                Format = SlideFormat.Script,
            };

            slide.Data[SlideVariableSubstituter.UnresolvedScript.DATAKEY_UNRESOLVEDSCRIPT] = new SlideVariableSubstituter.UnresolvedScript
            {
                InvokedScriptID = this.InvokedScript,
                Arguments = this.InvokedArguments,
                DKEY = SlideVariableSubstituter.UnresolvedText.DATAKEY_UNRESOLVEDTEXT,
            };

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
