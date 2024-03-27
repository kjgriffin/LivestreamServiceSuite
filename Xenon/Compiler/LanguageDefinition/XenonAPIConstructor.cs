using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Xenon.Compiler.LanguageDefinition
{

    internal enum DefinitionRequirement
    {
        NONE,
        REQUIRED,
        OPTIONAL,
    }

    internal class XenonCommandAPIMetadata
    {
        public LanguageKeywordCommand Keyword { get; internal set; }
        public Type XType { get; internal set; }
        public XenonSTDCmdMetadataAttribute STDMetadata { get; internal set; }
        public DefinitionRequirement HasParams { get; internal set; } = DefinitionRequirement.NONE;
        public XenonSTDCmdParamsAttribute STDParams { get; internal set; }
        public DefinitionRequirement HasBody { get; internal set; } = DefinitionRequirement.NONE;
    }

    static class XenonAPIConstructor
    {
        public static Dictionary<LanguageKeywordCommand, XenonCommandAPIMetadata> APIMetadata { get; private set; }

        static XenonAPIConstructor()
        {
            // find everything
            var xenonAssembly = Assembly.GetAssembly(typeof(LanguageKeywords));

            var xenonCommands = xenonAssembly.GetTypes().Select(x => (type: x, stdcmd: x.GetCustomAttribute<XenonSTDCmdMetadataAttribute>())).Where(x => x.stdcmd != null).ToList();

            APIMetadata = new Dictionary<LanguageKeywordCommand, XenonCommandAPIMetadata>();
            foreach (var xcmd in xenonCommands)
            {
                var paramAttr = xcmd.type.GetCustomAttribute<XenonSTDCmdParamsAttribute>();

                APIMetadata[xcmd.stdcmd.Command] = new XenonCommandAPIMetadata()
                {
                    XType = xcmd.type,
                    Keyword = xcmd.stdcmd.Command,
                    STDMetadata = xcmd.stdcmd,
                    STDParams = paramAttr,
                    HasParams = paramAttr?.Requirement ?? DefinitionRequirement.NONE,
                };
            } 
        }


    }
}
