using CCU.Config;

using CommonVersionInfo;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer;
using Xenon.SlideAssembly;
using Xenon.SlideAssembly.LayoutManagement;

namespace Xenon.SaveLoad
{

    public delegate void CodePreviewUpdateFunc(string code);

    public static class TrustySave
    {


        public static async Task<(Project project, Dictionary<string, string> assetfilemap, Dictionary<string, string> assetdisplaynames, Dictionary<string, string> assetextensions, Dictionary<string, string> assetgroups)> OpenTrustily(BuildVersion currentVersion, string filename, CodePreviewUpdateFunc preloadcode, Action<string, CCPUConfig_Extended> preloadCfg)
        {
            var proj = new Project(true, false);

            string sourcecode = "";
            Dictionary<string, string> assetfilemap = new Dictionary<string, string>();
            Dictionary<string, string> assetdisplaynames = new Dictionary<string, string>();
            Dictionary<string, string> assetextensions = new Dictionary<string, string>();
            Dictionary<string, string> assetgroups = new Dictionary<string, string>();

            BuildVersion originalVersion = new BuildVersion();

            using (ZipArchive archive = ZipFile.OpenRead(filename))
            {
                // get version

                var readmetext = archive.GetEntry("Readme.txt");
                if (readmetext != null)
                {
                    using (StreamReader sr = new StreamReader(readmetext.Open()))
                    {
                        var readmetextcontent = await sr.ReadToEndAsync();
                        var buildstring = Regex.Match(readmetextcontent, @"Created with version (?<version>[^\s]+)");
                        originalVersion = BuildVersion.Parse(buildstring.Groups["version"].Value);
                    }
                }

                // dump source code
                var sourcecodefile = archive.GetEntry("sourcecode.txt");
                if (sourcecodefile != null)
                {
                    using (StreamReader sr = new StreamReader(sourcecodefile.Open()))
                    {
                        sourcecode = await sr.ReadToEndAsync();
                        proj.SourceCode = sourcecode;
                    }
                }

                // get configuration
                if (originalVersion.ExceedsMinimumVersion(1, 7, 2, 26))
                {
                    var configsourcefile = archive.GetEntry("bmdconfig.json");
                    if (configsourcefile != null)
                    {
                        using (StreamReader sr = new StreamReader(configsourcefile.Open()))
                        {
                            string cfg = sr.ReadToEnd();
                            // try and parse it
                            try
                            {
                                var config = JsonSerializer.Deserialize<IntegratedPresenter.BMDSwitcher.Config.BMDSwitcherConfigSettings>(cfg);
                                proj.BMDSwitcherConfig = config;
                            }
                            catch
                            {
                            }
                            // always recover the source though
                            proj.SourceConfig = cfg;
                        }
                    }
                }

                // get configuration
                string ccucfgstr = "";
                if (originalVersion.MeetsMinimumVersion(1, 9, 0, 0))
                {
                    var configsourcefile = archive.GetEntry("ccuconfig.json");
                    if (configsourcefile != null)
                    {
                        using (StreamReader sr = new StreamReader(configsourcefile.Open()))
                        {
                            ccucfgstr = sr.ReadToEnd();
                            // try and parse it
                            try
                            {
                                var config = JsonSerializer.Deserialize<CCPUConfig_Extended>(ccucfgstr);
                                proj.CCPUConfig = config;
                            }
                            catch
                            {
                            }
                            // always recover the source though
                            proj.SourceCCPUConfigFull = ccucfgstr;
                        }
                    }
                }


                // in case a UI want's to be responsive and show text before we finish extracting/copying all assets out to the project directory
                preloadcode?.Invoke(sourcecode);
                preloadCfg?.Invoke(ccucfgstr, proj.CCPUConfig);

                var assetfilemapfile = archive.GetEntry("assets.json");
                if (assetfilemapfile != null)
                {
                    using (StreamReader sr = new StreamReader(assetfilemapfile.Open()))
                    {
                        string json = await sr.ReadToEndAsync();
                        assetfilemap = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    }
                }

                var assetdisplaynamefile = archive.GetEntry("assets_displaynames.json");
                if (assetdisplaynamefile != null)
                {
                    using (StreamReader sr = new StreamReader(assetdisplaynamefile.Open()))
                    {
                        string json = await sr.ReadToEndAsync();
                        assetdisplaynames = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    }
                }

                var assetextensionsfile = archive.GetEntry("assets_extensions.json");
                if (assetextensionsfile != null)
                {
                    using (StreamReader sr = new StreamReader(assetextensionsfile.Open()))
                    {
                        string json = await sr.ReadToEndAsync();
                        assetextensions = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    }
                }

                var assetgroupsfile = archive.GetEntry("assets_groups.json");
                if (assetgroupsfile != null)
                {
                    using (StreamReader sr = new StreamReader(assetgroupsfile.Open()))
                    {
                        string json = await sr.ReadToEndAsync();
                        assetgroups = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    }
                }


                // Project's will auto upgrade old projects, since default project constructor will have initialized the default layout library
                if (originalVersion.ExceedsMinimumVersion(1, 7, 1, 21))
                {
                    var layoutsmapfile = archive.GetEntry("layouts.json");
                    Dictionary<string, string> layoutsmap = new Dictionary<string, string>();
                    using (StreamReader sr = new StreamReader(layoutsmapfile.Open()))
                    {
                        string json = await sr.ReadToEndAsync();
                        layoutsmap = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    }

                    foreach (var layoutlib in layoutsmap)
                    {
                        var entry = archive.GetEntry(layoutlib.Value);
                        using (StreamReader sr = new StreamReader(entry.Open()))
                        {
                            string json = await sr.ReadToEndAsync();
                            // Project has layouts defined
                            // we can replace the defaults created by new Project()
                            if (originalVersion.MeetsMinimumVersion(1, 7, 2, 31))
                            {
                                // new style of libraries
                                var lib = JsonSerializer.Deserialize<XenonLayoutLibrary>(json, new JsonSerializerOptions() { IncludeFields = true });
                                proj.LayoutManager.LoadLibrary(lib); // TODO: upgrade libs...
                            }
                            else
                            {

                                var lib = JsonSerializer.Deserialize<LayoutLibEntry>(json, new JsonSerializerOptions() { IncludeFields = true });
                                proj.LayoutManager.LoadLibrary(lib.TrustilyUpgradeOldLayoutLibrary(originalVersion, currentVersion));
                            }
                        }
                    }
                }

                await TrustilyUpgradeOldVersion(archive, proj, originalVersion, currentVersion);

                TrustilyUpgradeModernLayoutLibraries(proj, originalVersion, currentVersion);

                preloadcode?.Invoke(proj.SourceCode);

                archive.ExtractToDirectory(proj.LoadTmpPath);
            }

            return (proj, assetfilemap, assetdisplaynames, assetextensions, assetgroups);

        }

        private static void TrustilyUpgradeModernLayoutLibraries(Project proj, BuildVersion originalVersion, BuildVersion currentVersion)
        {
            // macros break this approach... not sure what to do instead
            return;

            // for now only breaking change is stitched image
            if (!originalVersion.MeetsMinimumVersion(1, 9, 0, 16))
            {
                foreach (var library in proj.LayoutManager.AllLibraries())
                {
                    List<(string, string, string, string)> newlayouts = new List<(string, string, string, string)>();
                    foreach (var candidatelayout in library.Layouts.Where(x => x.Group == LanguageKeywords.Commands[LanguageKeywordCommand.StitchedImage]))
                    {
                        // validate candidate layout has the new properties
                        if (ILayoutInfoResolver<StitchedImageSlideLayoutInfo>.TryParseJson(candidatelayout.RawSource, out var layout))
                        {
                            layout.AutoBoxSplitOnRefrain = false;
                            layout.MusicBoxes = new List<LayoutInfo.BaseTypes.DrawingBoxLayout>
                            {
#pragma warning disable CS0612 // Type or member is obsolete
                                layout.MusicBox
                            };
#pragma warning restore CS0612 // Type or member is obsolete


                            newlayouts.Add((library.LibName, candidatelayout.Name, candidatelayout.Group, JsonSerializer.Serialize(layout)));

                        }
                        else
                        {
                            // perhaps warn???
                        }
                    }
                    foreach (var upgradedlayout in newlayouts)
                    {
                        proj.LayoutManager.SaveLayoutToLibrary(upgradedlayout.Item1, upgradedlayout.Item2, upgradedlayout.Item3, upgradedlayout.Item4);
                    }
                }

            }
        }

        private static LayoutLibEntry TrustilyUpgradeOldLayoutLibrary(this LayoutLibEntry oldLib, BuildVersion originalVersionSC, BuildVersion targetVersionSC)
        {
            LayoutLibEntry upgradedLibrary = new LayoutLibEntry();
            upgradedLibrary.Metadata = new LibraryMetadata
            {
                Date = $"{DateTime.Now.Date}",
                XenonVersion = targetVersionSC.ToString()
            };

            // at this point we rely upon the library's internal version since it's possible it was build with a different version that the source project
            upgradedLibrary.Lib = oldLib.Lib.TrustilyUpgradeOldLayoutLibrary(BuildVersion.Parse(oldLib.Metadata.XenonVersion), targetVersionSC);

            return upgradedLibrary;
        }

        private static LayoutLibrary TrustilyUpgradeOldLayoutLibrary(this LayoutLibrary lib, BuildVersion originalLibVersion, BuildVersion targetVersionSC)
        {
            // 1. handle groups (for now grouping has remained the same)
            // note: if we ever move layouts from one group to another etc. we'd need to handle that case here
            var regroupedLib = lib.TrustilyUpgradeOldLayoutLibrary_Regroup(originalLibVersion, targetVersionSC);

            // 2. upgrade individual types based on group (i.e. if a group now has a different underlying layout type, we'd need to handle that here)
            var retypedGroups = new List<LayoutGroup>();
            foreach (var group in regroupedLib)
            {
                retypedGroups.Add(group.TrustilyUpgradeOldLayoutGroup_ToNewType(originalLibVersion, targetVersionSC));
            }

            // 3. uprade existing types, but to newer verion of same type
            // (it would be nice if this could be extracted out to the type itself...)
            // perhaps require it as an abstract on any <ALayoutInfo>
            var upgradedLayoutGroups = new List<LayoutGroup>();
            foreach (var group in retypedGroups)
            {
                LayoutGroup ugroup = new LayoutGroup();
                ugroup.group = group.group;
                ugroup.layouts = new Dictionary<string, string>();
                foreach (var layout in group.layouts)
                {
                    var resolver = LanguageKeywords.LayoutForType[LanguageKeywords.Commands.First(cmd => cmd.Value == group.group).Key].layoutResolver;
                    var typedLayout = resolver.Deserialize(layout.Value);
                    typedLayout.UpgradeLayoutFromPreviousVersion(originalLibVersion, targetVersionSC);
                    ugroup.layouts.Add(layout.Key, resolver.Serialize(typedLayout));
                }
                upgradedLayoutGroups.Add(ugroup);
            }
            return new LayoutLibrary
            {
                Library = upgradedLayoutGroups,
                LibraryName = lib.LibraryName
            };
        }


        private static LayoutGroup TrustilyUpgradeOldLayoutGroup_ToNewType(this LayoutGroup group, BuildVersion buildVersion, BuildVersion taretVersion)
        {
            LayoutGroup newGroup = new LayoutGroup();

            if (!buildVersion.ExceedsMinimumVersion(1, 7, 2, 11, matchMode: false, "Debug") && group.group == LanguageKeywords.Commands[LanguageKeywordCommand.TwoPartTitle])
            {
                // pre SC 1.7.2.11 2title was its own layout type
                // with SC 1.7.2.11+ 2title will use customtext
                newGroup.group = group.group;
                newGroup.layouts = new Dictionary<string, string>();
                foreach (var oldLayoutKVP in group.layouts)
                {
                    var oldLayout = JsonSerializer.Deserialize<_2TitleSlideLayoutInfo>(oldLayoutKVP.Value);
                    // attempt to create an equivalent customtext layout
                    ShapeAndTextLayoutInfo newlayout = new ShapeAndTextLayoutInfo()
                    {
                        SlideSize = oldLayout.SlideSize,
                        BackgroundColor = oldLayout.BackgroundColor,
                        KeyColor = oldLayout.KeyColor,
                        SlideType = oldLayout.SlideType,
                        // TODO: need to sort out what should happen for old 'horizontal'/'vertical' layouts
                        Textboxes = new List<LayoutInfo.BaseTypes.TextboxLayout> { oldLayout.MainText, oldLayout.SubText },
                        Shapes = new List<LWJPolygon>
                        {
                            new LWJPolygon
                            {
                                BorderColor = oldLayout.Banner.FillColor,
                                FillColor = oldLayout.Banner.FillColor,
                                KeyFillColor = oldLayout.Banner.KeyColor,
                                KeyBorderColor = oldLayout.Banner.KeyColor,
                                BorderWidth = 0,
                                Verticies = new List<LWJPoint>
                                {
                                    new LWJPoint(0, 0),
                                    new LWJPoint(oldLayout.Banner.Box.Size.Width, 0),
                                    new LWJPoint(oldLayout.Banner.Box.Size.Width, oldLayout.Banner.Box.Size.Height),
                                    new LWJPoint(0, oldLayout.Banner.Box.Size.Height),
                                },
                                Transforms = new LWJTransformSet
                                {
                                    Scale = new LWJScaleTransform
                                    {
                                        XScale = 1,
                                        YScale = 1
                                    },
                                    Translate = new LWJTranslateTransform
                                    {
                                        XShift = oldLayout.Banner.Box.Origin.X,
                                        YShift = oldLayout.Banner.Box.Origin.Y
                                    }
                                }
                            }
                        }
                    };
                    newGroup.layouts.Add(oldLayoutKVP.Key, JsonSerializer.Serialize(newlayout));
                }
            }
            else
            {
                newGroup = group;
            }
            return newGroup;
        }


        private static List<LayoutGroup> TrustilyUpgradeOldLayoutLibrary_Regroup(this LayoutLibrary lib, BuildVersion originalVerions, BuildVersion targetVersionSC)
        {
            List<LayoutGroup> newGroups = new List<LayoutGroup>();
            foreach (var oldgroup in lib.Library)
            {
                // here we could change/merge groups if required
                // currently no change required
                newGroups.Add(oldgroup);
            }
            // here we can add new groups if required
            return newGroups;
        }


        private static Task TrustilyUpgradeOldVersion(ZipArchive archive, Project proj, BuildVersion originalVersion, BuildVersion targetVersion)
        {
            // rendering behaviour post 1.7.1.21 will be to assume premultiply alpha instead of only using it it explicitly set
            // to retain legacy render behaviour, for an old project we will add a #set command to mark global.rendermode.alpha = legacy

            // this isn't truly perfect: but to do that we'd need to haul in the entire compiler and view the project's variables. So hopefully this is close enough
            // we'll add an xml comment too
            if (!originalVersion.MeetsMinimumVersion(1, 7, 1, 22))
            {
                const string xmlwarn = @"/// </MANUAL_UPDATE name='compatibility upgrade'>";
                string commentexplain = $"// Project was upgraded from version {originalVersion}{Environment.NewLine}// where the default render behaviour didn't premultiply slides with their key.{Environment.NewLine}// Xenon Compatibility layer has added the following command to match that behaviour.{Environment.NewLine}// Remove the following line if the old default behaviour is unwanted.";
                string setvar = @"#set(""global.rendermode.alpha"", ""legacy"")";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(xmlwarn);
                sb.AppendLine(commentexplain);
                sb.AppendLine(setvar);
                sb.AppendLine(proj.SourceCode);
                proj.SourceCode = sb.ToString();

                // default config file will need to be reset
                proj.BMDSwitcherConfig.DownstreamKey1Config.IsPremultipled = 0;
                proj.BMDSwitcherConfig.DownstreamKey1Config.IsMasked = 1;
                proj.BMDSwitcherConfig.DownstreamKey1Config.MaskTop = -5.5f;
                proj.BMDSwitcherConfig.DownstreamKey1Config.MaskBottom = -9f;
                proj.BMDSwitcherConfig.DownstreamKey1Config.MaskLeft = -16f;
                proj.BMDSwitcherConfig.DownstreamKey1Config.MaskRight = 16f;
            }

            return Task.CompletedTask;
        }


        public static Task SaveTrustily(Project proj, string path, IProgress<int> progress, string versioninfo)
        {
            return Task.Run(async () =>
            {
                progress?.Report(0);

                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                using FileStream ziptoopen = new FileStream(path, FileMode.Create);
                using ZipArchive archive = new ZipArchive(ziptoopen, ZipArchiveMode.Update);

                // setup readme with version
                ZipArchiveEntry readme = archive.CreateEntry("Readme.txt");
                using (StreamWriter writer = new StreamWriter(readme.Open()))
                {
                    await writer.WriteLineAsync("Trusty Save");
                    await writer.WriteLineAsync($"Created with version {versioninfo}");
                }

                progress?.Report(1);


                // handle assets
                string assetsfolderpath = $"assets{Path.DirectorySeparatorChar}";
                var assetsfolder = archive.CreateEntry(assetsfolderpath);

                progress?.Report(5);
                int assetcount = proj.Assets.Count;
                int completed = 0;

                int nameid = 0;
                Dictionary<string, string> assetfilemap = new Dictionary<string, string>();
                Dictionary<string, string> assetdisplaynamemap = new Dictionary<string, string>();
                Dictionary<string, string> assetextensions = new Dictionary<string, string>();
                Dictionary<string, string> assetgroups = new Dictionary<string, string>();
                foreach (var asset in proj.Assets)
                {
                    string name = $"asset_{Interlocked.Increment(ref nameid)}{asset.Extension}";
                    if (File.Exists(asset.CurrentPath))
                    {
                        assetfilemap.TryAdd(asset.Name, Path.Combine(assetsfolderpath, name));
                        assetdisplaynamemap.TryAdd(asset.Name, asset.DisplayName);
                        assetextensions.TryAdd(asset.Name, asset.Extension);
                        assetgroups.TryAdd(asset.Name, asset.Group);
                        ZipArchiveEntry zippedasset = archive.CreateEntryFromFile(asset.CurrentPath, Path.Combine(assetsfolderpath, name));
                        double savepercent = (completed / (double)assetcount) * 100;
                        progress?.Report(5 + 1 + (int)(savepercent) * (100 - 5 - 1 - 10));
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }

                progress?.Report(90);

                // save assets json

                string assetsjsonstr = JsonSerializer.Serialize(assetfilemap);
                ZipArchiveEntry assetsjson = archive.CreateEntry("assets.json");
                using (StreamWriter writer = new StreamWriter(assetsjson.Open()))
                {
                    await writer.WriteAsync(assetsjsonstr);
                }

                string assetsdisplaynamejsonstr = JsonSerializer.Serialize(assetdisplaynamemap);
                ZipArchiveEntry assetsdisplaynamejson = archive.CreateEntry("assets_displaynames.json");
                using (StreamWriter writer = new StreamWriter(assetsdisplaynamejson.Open()))
                {
                    await writer.WriteAsync(assetsdisplaynamejsonstr);
                }

                string assetsextensionsjsonstr = JsonSerializer.Serialize(assetextensions);
                ZipArchiveEntry assetsextensionsjson = archive.CreateEntry("assets_extensions.json");
                using (StreamWriter writer = new StreamWriter(assetsextensionsjson.Open()))
                {
                    await writer.WriteAsync(assetsextensionsjsonstr);
                }

                string assetsgroupsjsonstr = JsonSerializer.Serialize(assetgroups);
                ZipArchiveEntry assetsgroupsjson = archive.CreateEntry("assets_groups.json");
                using (StreamWriter writer = new StreamWriter(assetsgroupsjson.Open()))
                {
                    await writer.WriteAsync(assetsgroupsjsonstr);
                }


                progress?.Report(95);


                // handle source code
                ZipArchiveEntry sourcecode = archive.CreateEntry("sourcecode.txt");
                using (StreamWriter writer = new StreamWriter(sourcecode.Open()))
                {
                    await writer.WriteAsync(proj.SourceCode);
                }

                // handle config
                ZipArchiveEntry bmdconfigfile = archive.CreateEntry("bmdconfig.json");
                using (StreamWriter writer = new StreamWriter(bmdconfigfile.Open()))
                {
                    await writer.WriteAsync(proj.SourceConfig);
                }

                ZipArchiveEntry ccuconfig = archive.CreateEntry("ccuconfig.json");
                using (StreamWriter writer = new StreamWriter(ccuconfig.Open()))
                {
                    await writer.WriteAsync(proj.SourceCCPUConfigFull);
                }



                // handle layouts

                progress?.Report(97);

                string layoutsfolderpath = $"layouts{Path.DirectorySeparatorChar}";
                var layoutsfolder = archive.CreateEntry(layoutsfolderpath);
                var libraries = proj.LayoutManager.AllLibraries();
                Dictionary<string, string> layoutsmapdict = new Dictionary<string, string>();
                foreach (var lib in libraries)
                {
                    ZipArchiveEntry layoutlib_entry = archive.CreateEntry(Path.Combine(layoutsfolderpath, lib.LibName + ".json"));
                    //await ExportLibrary(versioninfo, lib, new StreamWriter(layoutlib_entry.Open()));
                    string json = JsonSerializer.Serialize(lib);
                    using (var writer = new StreamWriter(layoutlib_entry.Open()))
                    {
                        await writer.WriteAsync(json);
                    }
                    layoutsmapdict[lib.LibName] = Path.Combine(layoutsfolderpath, lib.LibName + ".json");
                }

                string layoutsmapjsonstr = JsonSerializer.Serialize(layoutsmapdict);
                ZipArchiveEntry layoutsjson = archive.CreateEntry("layouts.json");
                using (StreamWriter writer = new StreamWriter(layoutsjson.Open()))
                {
                    await writer.WriteAsync(layoutsmapjsonstr);
                }



                progress?.Report(100);

            });
        }

        public static async Task ExportLibrary(string versioninfo, XenonLayoutLibrary lib, StreamWriter writer)
        {
            //var metadata = new { XenonVersion = versioninfo, Date = DateTime.Now.ToString("dd/MM/yyyy") };
            using (writer)
            {
                //var obj = new
                //{
                //    Lib = lib,
                //    Metadata = metadata,
                //};
                await writer.WriteAsync(JsonSerializer.Serialize(lib, new JsonSerializerOptions() { IncludeFields = true }));
            }
        }

        public static async Task ImportLibrary(Project proj, string filename)
        {
            using (StreamReader sr = new StreamReader(File.Open(filename, FileMode.Open)))
            {
                string json = await sr.ReadToEndAsync();
                try
                {
                    var lib = JsonSerializer.Deserialize<LayoutLibEntry>(json, new JsonSerializerOptions() { IncludeFields = true });
                    if (lib.Lib.LibraryName != null && lib.Lib.Library != null && lib.Metadata.XenonVersion != null && lib.Metadata.Date != null)
                    {
                        proj?.LayoutManager?.LoadLibrary(lib);
                    }
                    else
                    {
                        var libnew = JsonSerializer.Deserialize<XenonLayoutLibrary>(json, new JsonSerializerOptions() { IncludeFields = true });
                        proj?.LayoutManager?.LoadLibrary(libnew);
                    }
                }
                catch (Exception)
                {
                }

            }
        }

    }

    public struct LayoutLibEntry
    {
        public LayoutLibrary Lib { get; set; }
        public LibraryMetadata Metadata { get; set; }
    }

    public struct LibraryMetadata
    {
        public string XenonVersion { get; set; }
        public string Date { get; set; }
    }


}
