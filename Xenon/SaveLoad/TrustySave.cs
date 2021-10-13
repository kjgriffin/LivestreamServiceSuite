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

                /*
                // create a bunch of tasks
                var AssetSaveTasks = new List<Task>();

                ConcurrentDictionary<string, string> assetfilemap = new ConcurrentDictionary<string, string>();
                int nameid = 0;
                foreach (var asset in proj.Assets)
                {
                    AssetSaveTasks.Add(Task.Run(() =>
                    {
                        string name = $"asset_{Interlocked.Increment(ref nameid)}{Path.GetExtension(asset.OriginalFilename)}";
                        if (File.Exists(asset.CurrentPath))
                        {
                            assetfilemap.TryAdd(asset.Name, Path.Combine(assetsfolderpath, name));
                            ZipArchiveEntry zippedasset = archive.CreateEntryFromFile(asset.CurrentPath, Path.Combine(assetsfolderpath, name));
                            double savepercent = (completed / (double)assetcount) * 100;
                            progress.Report(5 + 1 + (int)(savepercent) * (100 - 5 - 1 - 10));
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }));
                }

                await Task.WhenAll(AssetSaveTasks);
                */

                // Perhaps we'll just have to sacrifice performance on save to make sure that all assets do get written...

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

                progress?.Report(100);

            });
        }





    }
}
