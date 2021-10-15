using System;
using System.Collections.Generic;

namespace Xenon.Compiler
{
    internal interface IXenonASTCommand : IXenonASTElement
    {
        public (bool complete, List<(string suggestion, string description)> suggestions) GetContextualSuggestions(string sourcecode)
        {
            return (false, new List<(string suggestion, string description)>());
        }

        public static T GetInstance<T>() where T : new()
        {
            return new T();
        }
    }
}