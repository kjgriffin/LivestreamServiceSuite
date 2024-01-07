using System.Collections.Generic;

namespace Xenon.Compiler
{
    internal class XenonErrorLogger : ILexerLogger
    {
        public string File { get; set; } = "";

        List<XenonCompilerMessage> messages = new List<XenonCompilerMessage>();

        public List<XenonCompilerMessage> AllErrors => messages;

        public void Log(XenonCompilerMessage message)
        {
            message.SrcFile = File;
            messages.Add(message);
        }

        public void ClearErrors()
        {
            messages.Clear();
        }

    }

    interface ILexerLogger
    {
        public void Log(XenonCompilerMessage message);
    }
}
