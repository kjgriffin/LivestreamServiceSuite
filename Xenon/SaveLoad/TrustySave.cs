using CommonVersionInfo;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Xenon.SlideAssembly;

namespace Xenon.SaveLoad
{
    public static class TrustySave
    {


        public static async Task<Project> OpenTrustily(BuildVersion VersionInfo, string filename, Action<string> preloadcode)
        {
            var proj = new Project(true);

            string sourcecode = "";
            Dictionary<string, string> assetfilemap = new Dictionary<string, string>();
            Dictionary<string, string> assetdisplaynames = new Dictionary<string, string>();
            Dictionary<string, string> assetextensions = new Dictionary<string, string>();

            using (ZipArchive archive = ZipFile.OpenRead(filename))
            {
                // dump source code
                var sourcecodefile = archive.GetEntry("sourcecode.txt");
                if (sourcecodefile != null)
                {
                    using (StreamReader sr = new StreamReader(sourcecodefile.Open()))
                    {
                        sourcecode = await sr.ReadToEndAsync();
                    }
                }

                // in case a UI want's to be responsive and show text before we finish extracting/copying all assets out to the project directory
                preloadcode?.Invoke(sourcecode);

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

                // Handle old projects without layout info. The default project constructor will supply enough to make it work.
                if (VersionInfo.ExceedsMinimumVersion(1, 7, 1, 21))
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
                            var lib = JsonSerializer.Deserialize<LayoutLibEntry>(json, new JsonSerializerOptions() { IncludeFields = true });
                            proj.ProjectLayouts.LoadLibrary(lib);

                        }
                    }
                }



                archive.ExtractToDirectory(proj.LoadTmpPath);
            }

            return proj;

        }


        public static Task SaveTrustily(Project proj, string path, IProgress<int> progress, string versioninfo)
        {
            return Task.Run(async () =>
            {
                progress?.Report(0);

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
                foreach (var asset in proj.Assets)
                {
                    string name = $"asset_{Interlocked.Increment(ref nameid)}{asset.Extension}";
                    if (File.Exists(asset.CurrentPath))
                    {
                        assetfilemap.TryAdd(asset.Name, Path.Combine(assetsfolderpath, name));
                        assetdisplaynamemap.TryAdd(asset.Name, asset.DisplayName);
                        assetextensions.TryAdd(asset.Name, asset.Extension);
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



                progress?.Report(95);


                // handle source code
                ZipArchiveEntry sourcecode = archive.CreateEntry("sourcecode.txt");
                using (StreamWriter writer = new StreamWriter(sourcecode.Open()))
                {
                    await writer.WriteAsync(proj.SourceCode);
                }

                // handle layouts

                progress?.Report(97);

                string layoutsfolderpath = $"layouts{Path.DirectorySeparatorChar}";
                var layoutsfolder = archive.CreateEntry(layoutsfolderpath);
                var libraries = proj.ProjectLayouts.GetAllLibraryLayoutsByGroup();
                Dictionary<string, string> layoutsmapdict = new Dictionary<string, string>();
                foreach (var lib in libraries)
                {
                    ZipArchiveEntry layoutlib_entry = archive.CreateEntry(Path.Combine(layoutsfolderpath, lib.LibraryName + ".json"));
                    var metadata = new { XenonVersion = versioninfo, Date = DateTime.Now.ToString("dd/MM/yyyy") };
                    using (StreamWriter writer = new StreamWriter(layoutlib_entry.Open()))
                    {
                        var obj = new
                        {
                            Lib = lib,
                            Metadata = metadata,
                        };
                        await writer.WriteAsync(JsonSerializer.Serialize(obj, new JsonSerializerOptions() { IncludeFields = true }));
                    }
                    layoutsmapdict[lib.LibraryName] = Path.Combine(layoutsfolderpath, lib.LibraryName + ".json");
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
