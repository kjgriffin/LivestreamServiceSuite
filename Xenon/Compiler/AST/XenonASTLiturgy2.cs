using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    class XenonASTLiturgy2 : IXenonASTCommand
    {
        public IXenonASTElement Parent { get; private set; }
        public string RawContent { get; private set; } = "";
        public int OrigContentSourceLine { get; private set; }

        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent)
        {
            XenonASTLiturgy2 liturgy = new XenonASTLiturgy2();
            liturgy.Parent = Parent;

            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("{", "Expected opening brace at start of liturgy.");

            liturgy.OrigContentSourceLine = Lexer.Peek().linenum;


            // re-assemble all tokens until end of liturgy. // this will allow us to re-parse/tokenize with a custom liturgy lexer that will be better at what we're trying to do.
            StringBuilder sb = new StringBuilder();
            bool keepgoing = true;
            while (!Lexer.InspectEOF() && keepgoing)
            {
                if (Lexer.Peek() == "}") // check if it was escaped by doubbling it
                {
                    if (Lexer.PeekNext() == "}")
                    {
                        Lexer.Consume();
                        sb.Append(Lexer.Consume().tvalue);
                    }
                    else
                    { 
                        // this is the end of the command
                        keepgoing = false;
                        Lexer.Consume(); 
                    }
                }
                else
                {
                    sb.Append(Lexer.Consume().tvalue);
                }
            }
            liturgy.RawContent = sb.ToString();

            // use a custom/conextual parser to re-parse the content


            // we're done! (already captured the ending token '}')
            Lexer.GobbleWhitespace();

            return liturgy;
        }

        public void Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
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
