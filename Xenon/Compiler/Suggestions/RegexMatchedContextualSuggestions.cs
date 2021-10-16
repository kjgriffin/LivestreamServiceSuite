using System;
using System.Collections.Generic;

namespace Xenon.Compiler.Suggestions
{
    public struct RegexMatchedContextualSuggestions
    {
        public string Regex;
        public bool Optional;
        public string Captureas;
        public List<(string, string)> Suggestions;
        public string ExternalFunctionName;

        public RegexMatchedContextualSuggestions(string regex, bool optional, string captureas, List<(string, string)> suggestions, string externalfunctionname)
        {
            Regex = regex;
            Optional = optional;
            Captureas = captureas;
            Suggestions = suggestions;
            this.ExternalFunctionName = externalfunctionname;
        }

        public override bool Equals(object obj)
        {
            return obj is RegexMatchedContextualSuggestions other &&
                   Regex == other.Regex &&
                   Optional == other.Optional &&
                   Captureas == other.Captureas &&
                   EqualityComparer<List<(string, string)>>.Default.Equals(Suggestions, other.Suggestions) &&
                   ExternalFunctionName == other.ExternalFunctionName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Regex, Optional, Captureas, Suggestions, ExternalFunctionName);
        }

        public void Deconstruct(out string regex, out bool optional, out string captureas, out List<(string, string)> suggestions, out string externalfunctionname)
        {
            regex = Regex;
            optional = Optional;
            captureas = Captureas;
            suggestions = Suggestions;
            externalfunctionname = this.ExternalFunctionName;
        }

        public static implicit operator (string, bool, string, List<(string, string)>, string externalfunctionname)(RegexMatchedContextualSuggestions value)
        {
            return (value.Regex, value.Optional, value.Captureas, value.Suggestions, value.ExternalFunctionName);
        }

        public static implicit operator RegexMatchedContextualSuggestions((string regex, bool optional, string captureas, List<(string, string)> suggestions, string externalfunctionname) value)
        {
            return new RegexMatchedContextualSuggestions(value.regex, value.optional, value.captureas, value.suggestions, value.externalfunctionname);
        }
    }
}
