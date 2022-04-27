using System.Collections.Generic;

using Xenon.Compiler;
using Xenon.SaveLoad;
using Xenon.SlideAssembly.LayoutManagement;

namespace Xenon.SlideAssembly
{
    public interface IProjectLayoutLibraryManager
    {
        SaveLayoutToLibrary SaveLayoutToLibrary { get; }

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
        (bool found, string json) FindLayoutByFullyQualifiedName(LanguageKeywordCommand type, string fullname, string defaultLibrary = "");
        List<string> FindTypesSupportingLayouts();
    }
}