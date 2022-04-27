using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.SaveLoad;

namespace Xenon.SlideAssembly.LayoutManagement
{

    public static class XenonLayoutLibraryExtensions
    {
        public static List<XenonLayout> ExtractLayouts(this LayoutLibrary lib, string version)
        {
            List<XenonLayout> res = new List<XenonLayout>();

            foreach (var group in lib.Library)
            {
                foreach (var layout in group.layouts)
                {
                    res.Add(new XenonLayout
                    {
                        Group = group.group,
                        Name = layout.Key,
                        LayoutVersion = version,
                        RawSource = layout.Value,
                    });
                }
            }

            return res;
        }
    }

    public class XenonLayoutLibrary
    {
        public string LibName { get; set; }
        public string LibVersion { get; set; }
        public string Date { get; set; }
        public List<XenonLayout> Layouts { get; set; }

        public List<(string Group, Dictionary<string, XenonLayout> Layouts)> AsGroupedLayouts()
        {
            List<(string group, Dictionary<string, XenonLayout> layouts)> res = new List<(string, Dictionary<string, XenonLayout>)>();

            var grouped = Layouts.GroupBy(x => x.Group);

            foreach (var group in grouped)
            {
                Dictionary<string, XenonLayout> lgroup = new Dictionary<string, XenonLayout>();
                foreach (var layout in group)
                {
                    lgroup.Add(layout.Name, layout);
                }
                res.Add((group.Key, lgroup));
            }

            return res;
        }
    }

    public class XenonLayout
    {
        public string Name { get; set; }
        public string LayoutVersion { get; set; }
        public string Group { get; set; }
        public string RawSource { get; set; }
    }

    internal class XenonLayoutManager : IProjectLayoutLibraryManager
    {

        private Dictionary<string, XenonLayoutLibrary> m_libraries = new Dictionary<string, XenonLayoutLibrary>();

        public const string DEFAULTLIBNAME = "Xenon.Core";

        [JsonIgnore]
        public SaveLayoutToLibrary SaveLayoutToLibrary { get => _Internal_SaveLayoutToLibrary; }

        private bool _Internal_SaveLayoutToLibrary(string libname, string layoutname, string group, string json)
        {
            InitializeNewLibrary(libname);
            var lib = m_libraries[libname];
            lib.Layouts.RemoveAll(lib => lib.Name == layoutname && lib.Group == group);
            lib.Layouts.Add(new XenonLayout
            {
                Group = group,
                Name = layoutname,
                LayoutVersion = Versioning.Versioning.Version.ToString(),
                RawSource = json
            });
            return true;
        }

        public void CreateNewLayoutFromDefaults(string libname, string group, string layoutname)
        {
            InitializeNewLibrary(libname);
            XenonLayoutLibrary lib = m_libraries[libname];

            var cmd = LanguageKeywords.Commands.First(x => x.Value == group).Key;
            string json = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultJson(LanguageKeywords.LayoutForType[cmd].defaultJsonFile);

            lib.Layouts.Add(new XenonLayout
            {
                Name = layoutname,
                Group = group,
                LayoutVersion = Versioning.Versioning.Version.ToString(),
                RawSource = json
            });
        }

        public void DeleteLayout(string libname, string group, string layout)
        {
            if (libname == DEFAULTLIBNAME)
            {
                return;
            }
            if (m_libraries.TryGetValue(libname, out var library))
            {
                library.Layouts.RemoveAll(x => x.Group == group && x.Name == layout);
            }
        }

        public List<XenonLayoutLibrary> AllLibraries()
        {
            return m_libraries.Values.ToList();
        }

        public List<XenonLayout> AllLayouts()
        {
            var res = new List<XenonLayout>();
            foreach (var library in AllLibraries())
            {
                res.AddRange(library.Layouts);
            }
            return res;
        }

        public XenonLayoutLibrary GetLibraryByName(string libname)
        {
            if (m_libraries.TryGetValue(libname, out var lib))
            {
                return lib;
            }
            return null;
        }

        public bool InitializeNewLibrary(string libname)
        {
            if (m_libraries.ContainsKey(libname))
            {
                return false;
            }
            m_libraries.Add(libname, new XenonLayoutLibrary
            {
                Date = DateTime.Now.ToString("dd/MM/yyyy"),
                Layouts = new List<XenonLayout>(),
                LibName = libname,
                LibVersion = Versioning.Versioning.Version.ToString()
            });
            return true;
        }

        public List<string> FindTypesSupportingLayouts()
        {
            List<string> res = new List<string>();
            foreach (var cmd in LanguageKeywords.Commands.Keys)
            {
                var cmdmeta = LanguageKeywords.LanguageKeywordMetadata[cmd];
                if (cmdmeta.hasLayoutInfo)
                {
                    res.Add(LanguageKeywords.Commands[cmd]);
                }
            }
            return res;
        }

        public void LoadDefaults()
        {
            m_libraries.Clear();

            var defaultlib = new XenonLayoutLibrary
            {
                Date = DateTime.Now.ToString("dd/MM/yyyy"),
                Layouts = new List<XenonLayout>(),
                LibName = DEFAULTLIBNAME,
                LibVersion = Versioning.Versioning.Version.ToString()
            };

            // dynamically build the core library to include a layout for each type
            foreach (var cmd in LanguageKeywords.Commands.Keys)
            {
                var cmdmeta = LanguageKeywords.LanguageKeywordMetadata[cmd];
                if (cmdmeta.hasLayoutInfo)
                {
                    string json = LanguageKeywords.LayoutForType[cmd].layoutResolver._Internal_GetDefaultJson(LanguageKeywords.LayoutForType[cmd].defaultJsonFile);
                    XenonLayout layout = new XenonLayout
                    {
                        Name = "Default",
                        Group = LanguageKeywords.Commands[cmd],
                        LayoutVersion = Versioning.Versioning.Version.ToString(),
                        RawSource = json,
                    };
                    defaultlib.Layouts.Add(layout);
                }
            }

            m_libraries[DEFAULTLIBNAME] = defaultlib;

            LoadBundledLibraries();
        }

        private void LoadBundledLibraries()
        {
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
        }


        public void LoadLibrary(LayoutLibEntry lib)
        {
            XenonLayoutLibrary library = new XenonLayoutLibrary
            {
                LibName = lib.Lib.LibraryName,
                Date = lib.Metadata.Date,
                LibVersion = lib.Metadata.XenonVersion,
                Layouts = lib.Lib.ExtractLayouts(lib.Metadata.XenonVersion),
            };
            m_libraries[library.LibName] = library;
        }

        public void RemoveLib(string libname)
        {
            if (libname != DEFAULTLIBNAME)
            {
                m_libraries.Remove(libname);
            }
        }

        public (bool found, string json) FindLayoutByFullyQualifiedName(LanguageKeywordCommand type, string fullname, string defaultLibrary = DEFAULTLIBNAME)
        {
            // parse the library, or fallback to default it none specified
            var match = Regex.Match(fullname, @"((?<libname>.*)::)?(?<layoutname>.*)");
            if (match.Success)
            {
                string libname = match.Groups["libname"].Value ?? defaultLibrary;
                string layoutname = match.Groups["layoutname"].Value;

                if (m_libraries.TryGetValue(libname, out var lib))
                {
                    var typeMatched = lib.Layouts.Where(x => x.Group == LanguageKeywords.Commands[type]).FirstOrDefault(x => x.Name == layoutname);

                    if (typeMatched != null)
                    {
                        return (true, typeMatched.RawSource);
                    }
                }
            }
            return (false, "");

        }
    }
}
