using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer;
using Xenon.SaveLoad;

namespace Xenon.SlideAssembly
{

    public delegate bool SaveLayoutToLibrary(string libname, string layoutname, string group, string json);

    public class ProjectLayoutLibraryManager
    {
        public const string DEFAULTLIBNAME = "Xenon.Core";

        internal Dictionary<string, Dictionary<LanguageKeywordCommand, Dictionary<string, string>>> AllLibraries { get; set; } = new Dictionary<string, Dictionary<LanguageKeywordCommand, Dictionary<string, string>>>();

        public List<LayoutLibrary> GetAllLibraryLayoutsByGroup()
        {
            List<LayoutLibrary> res = new List<LayoutLibrary>();
            foreach (var library in AllLibraries)
            {

                List<LayoutGroup> layouts = new List<LayoutGroup>();

                foreach (var group in library.Value)
                {
                    layouts.Add((LanguageKeywords.Commands[group.Key], group.Value));
                }
                res.Add((library.Key, layouts));
            }
            return res;
        }

        public void DeleteLayout(string libname, string group, string layout)
        {
            if (libname == DEFAULTLIBNAME)
            {
                return;
            }

            if (AllLibraries.TryGetValue(libname, out var lib))
            {
                var cmd = LanguageKeywords.Commands.First(x => x.Value == group).Key;

                lib[cmd].Remove(layout);
            }
        }

        public LayoutLibrary? GetLibraryByName(string libname)
        {
            if (AllLibraries.ContainsKey(libname))
            {
                return new LayoutLibrary(libname, AllLibraries[libname].Select(x => new LayoutGroup(LanguageKeywords.Commands[x.Key], x.Value)).ToList());
            }
            return null;
        }

        internal (bool found, string json) GetLayoutByFullyQualifiedName(LanguageKeywordCommand type, string fullname, string defaultLibrary = DEFAULTLIBNAME)
        {
            // parse the library, or fallback to default it none specified
            var match = Regex.Match(fullname, @"((?<libname>.*)::)?(?<layoutname>.*)");
            if (match.Success)
            {
                string libname = match.Groups["libname"].Value ?? defaultLibrary;
                string layoutname = match.Groups["layoutname"].Value;

                if (AllLibraries.TryGetValue(libname, out var lib))
                {
                    if (lib.TryGetValue(type, out var layouts))
                    {
                        if (layouts.TryGetValue(layoutname, out var value))
                        {
                            return (true, value);
                        }
                    }
                }
            }
            return (false, "");
        }

        public bool InitializeNewLibrary(string libname)
        {
            if (AllLibraries.ContainsKey(libname))
            {
                return false;
            }
            AllLibraries[libname] = new Dictionary<LanguageKeywordCommand, Dictionary<string, string>>();
            // Add known groups to library
            foreach (var cmd in LanguageKeywords.Commands.Keys)
            {
                var cmdmeta = LanguageKeywords.LanguageKeywordMetadata[cmd];
                if (cmdmeta.hasLayoutInfo)
                {
                    var layouts = AllLibraries[libname];
                    layouts[cmd] = new Dictionary<string, string>();
                }
            }
            return true;
        }

        public void CreateNewLayoutFromDefaults(string libname, string group, string layoutname)
        {
            InitializeNewLibrary(libname);
            var cmd = LanguageKeywords.Commands.First(x => x.Value == group).Key;
            var libgroup = AllLibraries[libname][cmd];

            var d = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultInfo(LanguageKeywords.LayoutForType[cmd].defaultJsonFile);
            //string json = d.GetDefaultJson();
            string json = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultJson(LanguageKeywords.LayoutForType[cmd].defaultJsonFile);
            libgroup[layoutname] = json;
        }

        internal bool _Internal_SaveLayoutToLibrary(string libname, string layoutname, string group, string json)
        {
            InitializeNewLibrary(libname); // we're ok if it already exists

            var lib = AllLibraries[libname];

            var cmd = LanguageKeywords.Commands.First(x => x.Value == group).Key;
            var layouts = lib[cmd];

            layouts[layoutname] = json;
            // TODO: do object ref's have it set on the project by now?

            return true;
        }

        [JsonIgnore]
        public SaveLayoutToLibrary SaveLayoutToLibrary
        {
            get => _Internal_SaveLayoutToLibrary;
        }

        public static (bool isvalid, Bitmap main, Bitmap key) GetLayoutPreview(string layoutname, string layoutjson)
        {

            if (LanguageKeywords.Commands.ContainsValue(layoutname))
            {
                LanguageKeywordCommand cmd = LanguageKeywords.Commands.FirstOrDefault(x => x.Value == layoutname).Key;
                if (LanguageKeywords.LayoutForType.TryGetValue(cmd, out var proto))
                {
                    if (proto.prototypicalLayoutPreviewer.IsValidLayoutJson(layoutjson))
                    {
                        var r = proto.prototypicalLayoutPreviewer.GetPreviewForLayout(layoutjson);
                        return (true, r.main, r.key);
                    }
                }
            }

            return (false, new Bitmap(1920, 1080), new Bitmap(1920, 1080));
        }

        public void LoadDefaults()
        {
            AllLibraries.TryGetValue(DEFAULTLIBNAME, out var defaultlib);
            if (defaultlib == null)
            {
                defaultlib = new Dictionary<LanguageKeywordCommand, Dictionary<string, string>>();
            }
            foreach (var cmd in LanguageKeywords.Commands.Keys)
            {
                var cmdmeta = LanguageKeywords.LanguageKeywordMetadata[cmd];
                if (cmdmeta.hasLayoutInfo)
                {
                    //var name = cmdmeta.implementation.GetType().Name;
                    var name = "Default";

                    //var d = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultInfo();
                    //string json = d.GetDefaultJson();
                    var d = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultInfo(LanguageKeywords.LayoutForType[cmd].defaultJsonFile);
                    string json = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultJson(LanguageKeywords.LayoutForType[cmd].defaultJsonFile);

                    defaultlib.TryGetValue(cmd, out var dict);

                    if (dict == null)
                    {
                        dict = new Dictionary<string, string>();
                    }
                    dict[name] = json;
                    defaultlib[cmd] = dict;
                }
            }
            AllLibraries[DEFAULTLIBNAME] = defaultlib;
            LoadBundledLibraries();
        }

        private void LoadBundledLibraries()
        {
            // not ready to release bundles yet
            // only for debug builds
#if DEBUG

            // find all libaries
            var names = System.Reflection.Assembly.GetAssembly(typeof(ASlideLayoutInfo))
                .GetManifestResourceNames()
                .Where(n => n.StartsWith("Xenon.LayoutInfo.Defaults.LibBundles"));

            foreach (var name in names)
            {
                var stream = System.Reflection.Assembly.GetAssembly(typeof(ASlideLayoutInfo)).GetManifestResourceStream(name);
                using (StreamReader sr = new StreamReader(stream))
                {
                    string json = sr.ReadToEnd();
                    var lib = JsonSerializer.Deserialize<LayoutLibEntry>(json, new JsonSerializerOptions() { IncludeFields = true });
                    LoadLibrary(lib);
                }
            }
#endif
        }

        public void LoadLibrary(LayoutLibEntry lib)
        {
            AllLibraries[lib.Lib.LibraryName] = lib.Lib.ToTypedLayouts();
        }

        public void RemoveLib(string libname)
        {
            if (libname != DEFAULTLIBNAME)
            {
                AllLibraries.Remove(libname);
            }
        }
    }

    public struct LayoutLibrary
    {
        public string LibraryName;
        public List<LayoutGroup> Library;

        public LayoutLibrary(string libname, List<LayoutGroup> lib)
        {
            this.LibraryName = libname;
            this.Library = lib;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutLibrary other &&
                   LibraryName == other.LibraryName &&
                   EqualityComparer<List<LayoutGroup>>.Default.Equals(Library, other.Library);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibraryName, Library);
        }

        public void Deconstruct(out string libname, out List<LayoutGroup> lib)
        {
            libname = this.LibraryName;
            lib = this.Library;
        }

        public static implicit operator (string libname, List<LayoutGroup> lib)(LayoutLibrary value)
        {
            return (value.LibraryName, value.Library);
        }

        public static implicit operator LayoutLibrary((string libname, List<LayoutGroup> lib) value)
        {
            return new LayoutLibrary(value.libname, value.lib);
        }

        internal Dictionary<LanguageKeywordCommand, Dictionary<string, string>> ToTypedLayouts()
        {
            Dictionary<LanguageKeywordCommand, Dictionary<string, string>> res = new Dictionary<LanguageKeywordCommand, Dictionary<string, string>>();
            foreach (var group in Library)
            {
                var typedgroup = group.ToTypedLayout();
                res[typedgroup.cmd] = typedgroup.layouts;
            }
            return res;
        }
    }

    public struct LayoutGroup
    {
        public string group;
        public Dictionary<string, string> layouts;

        public LayoutGroup(string group, Dictionary<string, string> layouts)
        {
            this.group = group;
            this.layouts = layouts;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutGroup other &&
                   group == other.group &&
                   EqualityComparer<Dictionary<string, string>>.Default.Equals(layouts, other.layouts);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(group, layouts);
        }

        public void Deconstruct(out string group, out Dictionary<string, string> layouts)
        {
            group = this.group;
            layouts = this.layouts;
        }

        public static implicit operator (string group, Dictionary<string, string> layouts)(LayoutGroup value)
        {
            return (value.group, value.layouts);
        }

        public static implicit operator LayoutGroup((string group, Dictionary<string, string> layouts) value)
        {
            return new LayoutGroup(value.group, value.layouts);
        }

        internal (LanguageKeywordCommand cmd, Dictionary<string, string> layouts) ToTypedLayout()
        {
            string group = this.group;
            LanguageKeywordCommand cmd = LanguageKeywords.Commands.First(x => x.Value == group).Key;
            return (cmd, this.layouts);
        }

    }
}
