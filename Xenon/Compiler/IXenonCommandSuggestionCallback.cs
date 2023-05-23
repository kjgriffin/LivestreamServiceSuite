using System.Collections.Generic;

using Xenon.AssetManagment;
using Xenon.Compiler.Suggestions;

namespace Xenon.Compiler
{

    public interface IXenonCommandSuggestionCallback
    {
        public delegate (bool complete, List<(string suggestion, string description)> suggestions) GetContextualSuggestionsForCommand(
            Dictionary<string, string> priorcaptures, string sourcesnippet, string remainingsnippet, List<(string assetName, AssetType type)> KnownAssets, List<(string libname, LanguageKeywordCommand group, string name)> KnownLayouts);

        List<RegexMatchedContextualSuggestions> contextualsuggestions { get; }
        public (bool complete, List<(string suggestion, string description)> suggestions) GetContextualSuggestionsFromOption(XenonSuggestionService service, string sourcecode, IXenonCommandSuggestionCallback elem)
        {
            return service.GetSuggestionsByRegexMatchedSequence(contextualsuggestions, sourcecode, elem);
        }
    }
}
