using System.Collections.Generic;

using Xenon.Compiler.Suggestions;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.AST
{
    internal interface IXenonASTCommandPostGenerateWithInfo
    {

    }

    internal interface IXenonASTCommand : IXenonASTElement
    {

        public int _SourceLine { get; set; }

        public TopLevelCommandContextualSuggestions GetContextualSuggestions(XenonSuggestionService service, string sourcecode)
        {
            return (false, new List<(string suggestion, string description)>());
        }

        public static XenonASTExpression GetParentExpression(IXenonASTCommand cmd)
        {
            IXenonASTElement parent = cmd.Parent;
            while (parent != null)
            {
                if (parent is XenonASTExpression)
                {
                    return parent as XenonASTExpression;
                }
                parent = parent.Parent;
            }
            return null;
        }

        public static T GetInstance<T>() where T : new()
        {
            return new T();
        }

        (bool found, string json) GetLayoutOverrideFromProj(Project project, XenonErrorLogger Logger, LanguageKeywordCommand layoutGroup)
        {
            var layoutfromproj = TryGetScopedVariable(LanguageKeywords.LayoutVarName(layoutGroup), out string layoutoverridefromproj);
            var layoutfromcode = TryGetScopedVariable(LanguageKeywords.LayoutJsonVarName(layoutGroup), out string layoutoverridefromcode);

            if (layoutfromproj.found && layoutfromcode.found)
            {
                Logger.Log(new XenonCompilerMessage { ErrorName = "Conflicting Layouts", ErrorMessage = $"A layout for {{#{LanguageKeywords.Commands[layoutGroup]}}} was defined on the scope:{{{layoutfromproj.scopename}}} as well as on the project with the name{{{layoutoverridefromproj}}}", Generator = "IXenonASTCommand::ApplyLayoutOverride()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", GetParentExpression(this)._SourceLine) });
            }

            // TODO: warn about overwrite from code
            if (layoutfromcode.found)
            {
                return (true, layoutoverridefromcode);
            }

            // TODO: warn if we don't actualy find it on the project
            if (layoutfromproj.found)
            {
                var l = project.LayoutManager.FindLayoutByFullyQualifiedName(layoutGroup, layoutoverridefromproj);
                if (l.found)
                {
                    return (true, l.info.RawSource);
                }
            }

            return (false, "");
        }

        void ApplyLayoutOverride(Project project, XenonErrorLogger Logger, Slide slide, LanguageKeywordCommand layoutGroup)
        {
            var layoutfromproj = TryGetScopedVariable(LanguageKeywords.LayoutVarName(layoutGroup), out string layoutoverridefromproj);
            var layoutfromcode = TryGetScopedVariable(LanguageKeywords.LayoutJsonVarName(layoutGroup), out string layoutoverridefromcode);

            if (layoutfromproj.found && layoutfromcode.found)
            {
                Logger.Log(new XenonCompilerMessage { ErrorName = "Conflicting Layouts", ErrorMessage = $"A layout for {{#{LanguageKeywords.Commands[layoutGroup]}}} was defined on the scope:{{{layoutfromproj.scopename}}} as well as on the project with the name{{{layoutoverridefromproj}}}", Generator = "IXenonASTCommand::ApplyLayoutOverride()", Inner = "", Level = XenonCompilerMessageType.Warning, Token = ("", GetParentExpression(this)._SourceLine) });
            }

            // TODO: warn if we don't actualy find it on the project
            if (layoutfromproj.found)
            {
                var l = project.LayoutManager.FindLayoutByFullyQualifiedName(layoutGroup, layoutoverridefromproj);
                if (l.found)
                {
                    slide.Data[Slide.LAYOUT_INFO_KEY] = l.info.RawSource;
                }
            }
            // TODO: warn about overwrite from code
            if (layoutfromcode.found)
            {
                slide.Data[Slide.LAYOUT_INFO_KEY] = layoutoverridefromcode;
            }

        }
    }
}