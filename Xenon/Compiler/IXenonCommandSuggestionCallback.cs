using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{

    public interface IXenonCommandSuggestionCallback
    {
        public delegate List<(string suggestion, string description)> GetContextualSuggestionsForCommand(Dictionary<string, string> priorcaptures, string sourcesnippet, string remainingsnippet);

        //List<(string suggestion, string description)> GetContextualSuggestionsFromOption((string regex, List<(string, string)> suggestions, string externalfunctionname) option, string sourcesnippet);
    }
}
