using OpenQA.Selenium.DevTools.V120.DOMStorage;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Compiler.LanguageDefinition;

namespace Xenon.Compiler.Suggestions
{
    internal static class XAPISuggestionHelper
    {

        internal static bool IsCommandComplete(XenonCommandAPIMetadata cmdMeta, string sourcecode)
        {
            // we expect to have captured at LEAST the whole command
            // trim that off and see what's happening
            var remainder = sourcecode.Remove(0, "#".Length).Remove(0, cmdMeta.STDMetadata.XenonCmdText.Length);

            // check for params
            if (cmdMeta.HasParams == DefinitionRequirement.REQUIRED)
            {
                if (!AreParamsComplete(cmdMeta, remainder, out remainder))
                {
                    return false;
                }
            }
            else if (remainder.StartsWith("(") && cmdMeta.HasParams == DefinitionRequirement.OPTIONAL)
            {
                if (!AreParamsComplete(cmdMeta, remainder, out remainder))
                {
                    return false;
                }
            }

            // TODO: check for flags

            // TODO: check for body


            return true;
        }

        internal static List<(string suggestion, string description, int captureIndex)> GetCommandContextualSuggestions(XenonCommandAPIMetadata cmdMeta, string sourcecode)
        {
            // we expect to have captured at LEAST the whole command
            // trim that off and see what's happening
            var remainder = sourcecode.Remove(0, "#".Length).Remove(0, cmdMeta.STDMetadata.XenonCmdText.Length);

            string premainder = remainder;

            // check for params
            if (cmdMeta.HasParams == DefinitionRequirement.REQUIRED)
            {
                if (!AreParamsComplete(cmdMeta, remainder, out premainder))
                {
                    return GetParameterSuggestions(cmdMeta, remainder);
                }
            }
            else if (remainder.StartsWith("(") && cmdMeta.HasParams == DefinitionRequirement.OPTIONAL)
            {
                if (!AreParamsComplete(cmdMeta, remainder, out premainder))
                {
                    return GetParameterSuggestions(cmdMeta, remainder);
                }
            }

            // TODO: check for flags

            // TODO: check for body

            // return empty??
            // shouldn't really get here since cmd is complete
            return new List<(string suggestion, string description, int captureIndex)>();
        }

        private static List<(string suggestion, string description, int captureIndex)> GetParameterSuggestions(XenonCommandAPIMetadata cmdMeta, string sourcecode)
        {
            string remainder = sourcecode;

            // check open brace
            if (!CheckUntilAndRemove(remainder, "(", out remainder))
                return new List<(string suggestion, string description, int captureIndex)> { ("(", "", 0) };

            // determine if all params are completed
            for (int i = 0; i < cmdMeta.STDParams.ParamNames.Count; i++)
            {
                if (cmdMeta.STDParams.ParamsAreEnclosed)
                {
                    if (!CheckUntilAndRemove(remainder, "\"", out remainder))
                        return new List<(string suggestion, string description, int captureIndex)> { ("\"", "", 0) };
                }

                var cmdSuggestion = BuildSuggestionsFromParamsAPI(cmdMeta.STDParams, i);

                string delimiter = ",";
                if (i == cmdMeta.STDParams.ParamNames.Count - 1)
                {
                    delimiter = ")";
                }

                if (cmdMeta.STDParams.ParamsAreEnclosed)
                {
                    if (!CheckUntilAndRemove(remainder, "\"", out remainder))
                        return new List<(string suggestion, string description, int captureIndex)> { ("\"", "", 0) }.Concat(cmdSuggestion).ToList();
                }

                // expect delimiter
                if (!CheckUntilAndRemove(remainder, delimiter, out remainder))
                    return new List<(string suggestion, string description, int captureIndex)> { (delimiter, "", 0) }.Concat(cmdSuggestion).ToList();

            }


            // shouldn't get here since we wouldn't then be in a parameter
            return new List<(string suggestion, string description, int captureIndex)>();
        }

        private static List<(string suggestion, string description, int captureIndex)> BuildSuggestionsFromParamsAPI(XenonSTDCmdParamsAttribute pinfo, int pindex)
        {
            return new List<(string suggestion, string description, int captureIndex)> { ($"<{pinfo.ParamNames[pindex]}>", $"[API] parameter (TODO: description)", 0) };
        }

        private static bool AreParamsComplete(XenonCommandAPIMetadata cmdMeta, string sourcecode, out string remaining)
        {
            remaining = sourcecode;
            string remainder = sourcecode;

            // check open brace
            if (!CheckUntilAndRemove(remainder, "(", out remainder))
                return false;

            // determine if all params are completed
            for (int i = 0; i < cmdMeta.STDParams.ParamNames.Count; i++)
            {
                if (cmdMeta.STDParams.ParamsAreEnclosed)
                {
                    if (!CheckUntilAndRemove(remainder, "\"", out remainder))
                        return false;

                    if (!CheckUntilAndRemove(remainder, "\"", out remainder))
                        return false;
                }

                if (i < cmdMeta.STDParams.ParamNames.Count - 1)
                {
                    // expect delimiter
                    if (!CheckUntilAndRemove(remainder, ",", out remainder))
                        return false;
                }
                else
                {
                    // expect close brace
                    if (CheckUntilAndRemove(remainder, ")", out remainder))
                    {
                        remaining = remainder;
                        return true; // found end of params
                    }
                }
            }


            return false;
        }

        private static bool CheckUntilAndRemove(string input, string match, out string remainder)
        {
            var index = input.IndexOf(match);
            if (index != -1)
            {
                remainder = input.Remove(0, index + match.Length);
                return true;
            }
            remainder = input;
            return false;
        }

    }
}
