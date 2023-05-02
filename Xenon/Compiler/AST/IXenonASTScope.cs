using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    interface IXenonASTScope
    {

        public string ScopeName { get; }

        public (bool found, string scopename) GetScopedVariableValue(string vname, out string value);
        public bool SetScopedVariableValue(string vname, string value, Project proj);


    }
}
