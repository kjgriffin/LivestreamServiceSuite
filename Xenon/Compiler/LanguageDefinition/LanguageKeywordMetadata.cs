using Xenon.Compiler.AST;

namespace Xenon.Compiler.LanguageDefinition
{
    internal class LanguageKeywordMetadata
    {
        internal bool toplevel;
        internal LanguageKeywordCommand parent;
        internal bool hasLayoutInfo;
        internal bool acceptsOverrideExportMode;
        internal IXenonASTElement implementation;

        public LanguageKeywordMetadata((bool istoplevel, LanguageKeywordCommand parentcmd, bool hasLayout, bool acceptOverrideExportMode, IXenonASTElement impl) stuff)
        {
            toplevel = stuff.istoplevel;
            parent = stuff.parentcmd;
            hasLayoutInfo = stuff.hasLayout;
            implementation = stuff.impl;
            acceptsOverrideExportMode = stuff.acceptOverrideExportMode;
        }

        public static implicit operator LanguageKeywordMetadata((bool, LanguageKeywordCommand INVALIDUNKNOWN, bool layoutinfo, bool overrideexport, IXenonASTElement impl) v)
        {
            return new LanguageKeywordMetadata(v);
        }
    }

}
