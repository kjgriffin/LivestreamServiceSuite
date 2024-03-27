using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xenon.Compiler.LanguageDefinition;
using Xenon.LayoutInfo;
using Xenon.SaveLoad;
using Xenon.SlideAssembly.LayoutManagement;

namespace Xenon.SlideAssembly
{
    public interface IProjectLayoutLibraryManager
    {
        SaveLayoutToLibrary SaveLayoutToLibrary { get; }
        ResolveLayoutMacros ResolveLayoutMacros { get; }
        GetLayoutPreview GetLayoutPreview { get; }

        GetLibraryMacros GetLibraryMacros { get; }
        EditLibraryMacros EditLibraryMacros { get; }
        FindAllMacroReferences FindAllMacroRefs { get; }
        RenameMacroReferences RenameAllMacroRefs { get; }
        GetLayoutSource GetLayoutSource { get; }

        void CreateNewLayoutFromDefaults(string libname, string group, string layoutname);
        void DeleteLayout(string libname, string group, string layout);
        List<XenonLayoutLibrary> AllLibraries();
        List<XenonLayout> AllLayouts();
        XenonLayoutLibrary GetLibraryByName(string libname);
        bool InitializeNewLibrary(string libname);
        void LoadDefaults();
        void LoadLibrary(LayoutLibEntry lib);
        void LoadLibrary(XenonLayoutLibrary lib);
        void RemoveLib(string libname);
        (bool found, LayoutSourceInfo info) FindLayoutByFullyQualifiedName(LanguageKeywordCommand type, string fullname, string defaultLibrary = "");
        List<string> FindTypesSupportingLayouts();

        void OverrideMacroOnScope(string libname, string macroname, string value, string scope);
        void ReleaseMacrosOnScope(string scope);


        public static List<XenonLayoutLibrary> GetDefaultBundledLibraries()
        {
            List<XenonLayoutLibrary> res = new List<XenonLayoutLibrary>();
            var names = System.Reflection.Assembly.GetAssembly(typeof(ASlideLayoutInfo))
                .GetManifestResourceNames()
                .Where(n => n.StartsWith("Xenon.LayoutInfo.Defaults.NewLibBundles"));

            foreach (var name in names)
            {
                var stream = System.Reflection.Assembly.GetAssembly(typeof(ASlideLayoutInfo)).GetManifestResourceStream(name);
                using (StreamReader sr = new StreamReader(stream))
                {
                    string json = sr.ReadToEnd();
                    var lib = JsonSerializer.Deserialize<XenonLayoutLibrary>(json, new JsonSerializerOptions() { IncludeFields = true });
                    res.Add(lib);
                }
            }
            return res;
        }


    }
}