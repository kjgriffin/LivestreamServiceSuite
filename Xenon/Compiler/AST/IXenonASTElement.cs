using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler.AST
{
    internal interface IXenonASTElement
    {

        public IXenonASTElement Parent { get; }

        public List<Slide> Generate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger);
        public virtual void PreGenerate(Project project, IXenonASTElement _Parent, XenonErrorLogger Logger)
        {
            return;
        }



        public void GenerateDebug(Project project);
        public XenonCompilerSyntaxReport Recognize(Lexer Lexer);
        public IXenonASTElement Compile(Lexer Lexer, XenonErrorLogger Logger, IXenonASTElement Parent);

        public void DecompileFormatted(StringBuilder sb, ref int indentDepth, int indentSize);

        public (bool found, string scopename) TryGetScopedVariable(string vname, out string value)
        {
            var scope = this as IXenonASTScope;
            if (scope != null)
            {
                var sc = scope.GetScopedVariableValue(vname, out value);
                if (sc.found)
                {
                    return sc;
                }
            }
            if (Parent != null)
            {
                return Parent.TryGetScopedVariable(vname, out value);
            }
            value = "";
            return (false, "");
        }


        public (bool found, string scopename) CheckAnsestorScopeFornameConflict(string vname)
        {
            IXenonASTElement e = Parent;
            while (e != null)
            {
                var sc = e as IXenonASTScope;
                if (sc != null)
                {
                    var x = sc.GetScopedVariableValue(vname, out _);
                    if (x.found)
                    {
                        return x;
                    }
                }

                e = e.Parent;
            }
            return (false, "");
        }


    }
}
