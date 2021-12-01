using System;
using System.Collections.Generic;
using System.Text;

using Xenon.Compiler.Suggestions;

namespace Xenon.Compiler
{

    public interface IXenonCommandSuggestionCallback
    {
        public delegate List<(string suggestion, string description)> GetContextualSuggestionsForCommand(Dictionary<string, string> priorcaptures, string sourcesnippet, string remainingsnippet);

        List<RegexMatchedContextualSuggestions> contextualsuggestions { get; }
        public (bool complete, List<(string suggestion, string description)> suggestions) GetContextualSuggestionsFromOption(XenonSuggestionService service, string sourcecode, IXenonCommandSuggestionCallback elem)
        {
            return service.GetSuggestionsByRegexMatchedSequence(contextualsuggestions, sourcecode, elem);
        }
    }
}
