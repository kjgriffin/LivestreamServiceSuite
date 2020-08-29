using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Xenon.Compiler
{
    public enum XenonCompilerMessageType
    {
        Debug,
        Message,
        Info,
        Warning,
        Error,
    }

    static class XenonCompilerMessageTypeConverter
    {
        public static string ToString(this XenonCompilerMessageType type)
        {
            switch (type)
            {
                case XenonCompilerMessageType.Debug:
                    return "Debug";
                case XenonCompilerMessageType.Message:
                    return "Message";
                case XenonCompilerMessageType.Info:
                    return "Info";
                case XenonCompilerMessageType.Warning:
                    return "Warning";
                case XenonCompilerMessageType.Error:
                    return "Error";
                default:
                    return "Default";
            }
        }
    }


    public class XenonCompilerMessage
    {
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
        public XenonCompilerMessageType Level { get; set; }

        public override string ToString()
        {
            return $"[{Level}]\t{ErrorName}\t{ErrorMessage}\t<on token '{Token}'>";
        }
    }
}
