using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{
    internal class XenonErrorLogger : ILexerLogger
    {

        List<XenonCompilerMessage> messages = new List<XenonCompilerMessage>();

        public List<XenonCompilerMessage> AllErrors => messages;

        public void Log(XenonCompilerMessage message)
        {
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
