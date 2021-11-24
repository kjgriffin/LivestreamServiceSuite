using System;
using System.Collections.Generic;

using Xenon.Compiler.Suggestions;

namespace Xenon.Compiler
{
    internal interface IXenonASTCommand : IXenonASTElement
    {
        public TopLevelCommandContextualSuggestions GetContextualSuggestions(string sourcecode)
        {
            return (false, new List<(string suggestion, string description)>());
        }

        public static XenonASTExpression GetParentExpression(IXenonASTCommand cmd)
        {
            IXenonASTElement parent = cmd.Parent;
            while (parent != null)
            {
                if (parent is XenonASTExpression)
                {
                    return parent as XenonASTExpression;
                }
                parent = parent.Parent;
            }
            return null;
        }

        public static T GetInstance<T>() where T : new()
        {
            return new T();
        }
    }
}