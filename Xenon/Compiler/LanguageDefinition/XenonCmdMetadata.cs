using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Compiler.LanguageDefinition
{
    internal class XenonSTDCmdMetadataAttribute : Attribute
    {
        public string XenonCmdText { get; private set; } = "";
        public LanguageKeywordMetadata Metadata { get; private set; }
        public LanguageKeywordCommand Command { get; private set; }
        public bool Deprecated { get; private set; } = false;

        public XenonSTDCmdMetadataAttribute(LanguageKeywordCommand command, bool deprecated = false)
        {
            Command = command;
            Deprecated = deprecated;
            if (LanguageKeywords.Commands.TryGetValue(command, out var str))
            {
                XenonCmdText = str;
            }

            if (LanguageKeywords.LanguageKeywordMetadata.TryGetValue(command, out var val))
            {
                Metadata = val;
            }
            else
            {
                Metadata = null;
            }
        }
    }

    internal class XenonSTDCmdParamsAttribute : Attribute
    {
        public List<string> ParamNames { get; private set; }
        public bool ParamsAreEnclosed { get; private set; }
        public DefinitionRequirement Requirement { get; private set; }

        public XenonSTDCmdParamsAttribute(DefinitionRequirement required, bool paramsAreEnclosed, params string[] paramNames)
        {
            ParamNames = paramNames.ToList() ?? new List<string>();
            ParamsAreEnclosed = paramsAreEnclosed;
            Requirement = required;
        }
    }

    internal class XenonSTDBodyAttribute : Attribute
    {
        public DefinitionRequirement Requirement { get; private set; }

        public bool BodyNestsExpression { get; private set; }
        public bool CustomBodySuggester { get; private set; }
        //public Func<string, int, int, List<(string suggestion, string description, int replaceindex)>> BodySuggestionProvider;

        public XenonSTDBodyAttribute(DefinitionRequirement requirement, bool bodyNestsExpression = false)
        {
            Requirement = requirement;
            BodyNestsExpression = bodyNestsExpression;
        }
    }

}
