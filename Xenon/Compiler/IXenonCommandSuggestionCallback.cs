using System;
using System.Collections.Generic;
using System.Text;

namespace Xenon.Compiler
{

    public interface IXenonCommandSuggestionCallback
    {
        public delegate List<(string suggestion, string description)> GetContextualSuggestionsForCommand(Dictionary<string, string> priorcaptures, string sourcesnippet, string remainingsnippet);

        //public (bool complete, List<(string suggestion, string description)> suggestinos) GetContextualSuggestionsFromOption(string sourcecode);
    }
}
