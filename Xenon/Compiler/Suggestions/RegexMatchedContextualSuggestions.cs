using System;
using System.Collections.Generic;

using static Xenon.Compiler.IXenonCommandSuggestionCallback;

namespace Xenon.Compiler.Suggestions
{
    public struct RegexMatchedContextualSuggestions
    {
        public string Regex;
        public bool Optional;
        public string Captureas;
        public List<(string, string)> Suggestions;
        public GetContextualSuggestionsForCommand ExternalFunc;

        public RegexMatchedContextualSuggestions(string regex, bool optional, string captureas, List<(string, string)> suggestions, GetContextualSuggestionsForCommand externalfunctionname)
        {
            Regex = regex;
            Optional = optional;
            Captureas = captureas;
            Suggestions = suggestions;
            this.ExternalFunc = externalfunctionname;
        }

        public override bool Equals(object obj)
        {
            return obj is RegexMatchedContextualSuggestions other &&
                   Regex == other.Regex &&
                   Optional == other.Optional &&
                   Captureas == other.Captureas &&
                   EqualityComparer<List<(string, string)>>.Default.Equals(Suggestions, other.Suggestions) &&
                   ExternalFunc == other.ExternalFunc;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Regex, Optional, Captureas, Suggestions, ExternalFunc);
        }

        public void Deconstruct(out string regex, out bool optional, out string captureas, out List<(string, string)> suggestions, out GetContextualSuggestionsForCommand externalfunctionname)
        {
            regex = Regex;
            optional = Optional;
            captureas = Captureas;
            suggestions = Suggestions;
            externalfunctionname = this.ExternalFunc;
        }

        public static implicit operator (string, bool, string, List<(string, string)>, GetContextualSuggestionsForCommand externalfunctionname)(RegexMatchedContextualSuggestions value)
        {
            return (value.Regex, value.Optional, value.Captureas, value.Suggestions, value.ExternalFunc);
        }

        public static implicit operator RegexMatchedContextualSuggestions((string regex, bool optional, string captureas, List<(string, string)> suggestions, GetContextualSuggestionsForCommand externalfunctionname) value)
        {
            return new RegexMatchedContextualSuggestions(value.regex, value.optional, value.captureas, value.suggestions, value.externalfunctionname);
        }
    }
}
