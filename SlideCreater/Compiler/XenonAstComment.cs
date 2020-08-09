using SlideCreater.SlideAssembly;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SlideCreater.Compiler
{
    public class XenonAstComment : IXenonASTCommand
    {
        public string CommentText { get => sb.ToString(); set => sb.Clear().Append(value); }

        StringBuilder sb = new StringBuilder();
        
        public void AppendCommentText(string text)
        {
            sb.Append(text); 
        }
        public void Generate(Project project, IXenonASTElement _Parent)
        {
            
        }
        public void GenerateDebug(Project project)
        {
            Debug.WriteLine("<XenonASTComment>");
            Debug.WriteLine(CommentText);
            Debug.WriteLine("</XenonASTComment>");
        }


    }
}