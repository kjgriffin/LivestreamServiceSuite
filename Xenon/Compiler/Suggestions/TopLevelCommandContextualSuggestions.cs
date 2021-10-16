using System;
using System.Collections.Generic;

namespace Xenon.Compiler.Suggestions
{
    public struct TopLevelCommandContextualSuggestions
    {
        public bool Complete;
        public List<(string suggestion, string description)> Suggestions;

        public TopLevelCommandContextualSuggestions(bool complete, List<(string suggestion, string description)> suggestions)
        {
            this.Complete = complete;
            this.Suggestions = suggestions;
        }

        public override bool Equals(object obj)
        {
            return obj is TopLevelCommandContextualSuggestions other &&
                   Complete == other.Complete &&
                   EqualityComparer<List<(string suggestion, string description)>>.Default.Equals(Suggestions, other.Suggestions);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Complete, Suggestions);
        }

        public void Deconstruct(out bool complete, out List<(string suggestion, string description)> suggestions)
        {
            complete = this.Complete;
            suggestions = this.Suggestions;
        }

        public static implicit operator (bool complete, List<(string suggestion, string description)> suggestions)(TopLevelCommandContextualSuggestions value)
        {
            return (value.Complete, value.Suggestions);
        }

        public static implicit operator TopLevelCommandContextualSuggestions((bool complete, List<(string suggestion, string description)> suggestions) value)
        {
            return new TopLevelCommandContextualSuggestions(value.complete, value.suggestions);
        }
    }
}
