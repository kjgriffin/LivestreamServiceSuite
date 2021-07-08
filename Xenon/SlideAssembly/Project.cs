using Xenon.AssetManagment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Linq;
using System.Diagnostics;

namespace Xenon.SlideAssembly
{
    public class Project
    {
        public SlideLayout Layouts { get; set; } = new SlideLayout();
        public List<Slide> Slides { get; set; } = new List<Slide>();

        public Dictionary<string, List<string>> ProjectVariables = new Dictionary<string, List<string>>();

        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();
        public string SourceCode { get; set; } = string.Empty;


        private int slidenum = 0;
        public int NewSlideNumber => slidenum++;


        public void AddAttribute(string key, string value)
        {
            if (!ProjectVariables.ContainsKey(key))
            {
                ProjectVariables[key] = new List<string>();
            }
            ProjectVariables[key].Add(value);
        }

        public List<string> GetAttribute(string key)
        {
            if (ProjectVariables.ContainsKey(key))
            {
                return ProjectVariables[key];
            }
            return new List<string>();
        }


        public void Save(string filename)
        {
            try
            {
                var sobj = JsonSerializer.Serialize<Project>(this);
                using var sw = new StreamWriter(filename);
                sw.Write(sobj);
            }
            catch (Exception)
            {
                //MessageBox.Show("Failed to save project");
                throw new Exception("Failed to save project");
            }
        }

        public Task SaveProject(string filename, IProgress<int> progress)
        {
            return Task.Run(async () =>
            {
                progress.Report(0);

                using FileStream ziptoopen = new FileStream($"{filename}", FileMode.Create);
                using ZipArchive archive = new ZipArchive(ziptoopen, ZipArchiveMode.Update);
                // create readme
                ZipArchiveEntry readmeEntry = archive.CreateEntry("Readme.txt");
                using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                {
                    await writer.WriteLineAsync("Information about this package.");
                    await writer.WriteLineAsync("===============================");
                }

                progress.Report(1);



                // create assets folder
                string assetsfolderpath = $"assets{Path.DirectorySeparatorChar}";
                var assetsfolder = archive.CreateEntry(assetsfolderpath);

                progress.Report(5);

                int count = Assets.Count;

                int completed = 0;
                // copy assets
                //foreach (var asset in Assets)
                //{
                //    ZipArchiveEntry zippedasset = archive.CreateEntryFromFile(asset.CurrentPath, Path.Combine(assetsfolderpath, asset.OriginalFilename));
                //    completed++;
                //    progress.Report(5 + (int)((double)completed / (double)count * 100 * 0.9));
                //}




                Parallel.ForEach(Assets, (asset) =>
                 {
                     ZipArchiveEntry zippedasset = archive.CreateEntryFromFile(asset.CurrentPath, Path.Combine(assetsfolderpath, asset.OriginalFilename));
                     Interlocked.Increment(ref completed);
                     double assetssaved = (completed / (double)count) * 100;
                     progress.Report(5 + (int)(assetssaved * 0.85));
                 });

                // update project assets to point to zip


                progress.Report(90);

                // save json format
                ZipArchiveEntry jsonfile = archive.CreateEntry("Project.json");
                var sobj = JsonSerializer.Serialize(this);
                using (StreamWriter writer = new StreamWriter(jsonfile.Open()))
                {
                    await writer.WriteLineAsync(sobj);
                }
            });
        }

        internal void Clear()
        {
            Slides.Clear();
            ProjectVariables.Clear();
            SourceCode = "";
            slidenum = 0;
        }

        private string _loadTmpPath = "";
        private bool disposedValue;

        public string LoadTmpPath { get => _loadTmpPath; }

        public Project(bool withtemp = false)
        {
            if (withtemp)
            {
                // create temp folder for unpacking zipped assets into
                _loadTmpPath = Path.Combine(Path.GetTempPath(), $"tmpprojassets_newproj_{DateTime.Now:yyyyMMddhhmmss}");
                Directory.CreateDirectory(_loadTmpPath);
                Directory.CreateDirectory(Path.Combine(_loadTmpPath, "assets"));
            }
        }

        public Project()
        {

        }

        public static async Task<Project> LoadProject(string filename)
        {

            Project p = null;

            using (ZipArchive archive = ZipFile.OpenRead(filename))
            {
                var projectjson = archive.GetEntry("Project.json");
                string contents = "";
                using (StreamReader sr = new StreamReader(projectjson.Open()))
                {
                    contents = sr.ReadToEnd();
                }
                p = JsonSerializer.Deserialize<Project>(contents);

                // create temp folder for unpacking zipped assets into
                p._loadTmpPath = Path.Combine(Path.GetTempPath(), $"tmpprojassets_{Path.GetFileNameWithoutExtension(filename)}_{DateTime.Now:yyyyMMddhhmmss}");
                Directory.CreateDirectory(p._loadTmpPath);

                // unzip all assets
                archive.ExtractToDirectory(p._loadTmpPath, true);

                // update all asset temp locations
                foreach (var asset in p.Assets)
                {
                    asset.UpdateTmpLocation(Path.Combine(p._loadTmpPath, "assets"));
                }

                // try checking that extraction worked...
                foreach (var assetfile in p.Assets)
                {
                    if (!System.IO.File.Exists(assetfile.CurrentPath))
                    {
                        Debug.WriteLine($"Failed to extract file {assetfile.CurrentPath} for {assetfile.Name}");
                    }
                }

            }

            return p;
        }

        public static Project Load(string filename)
        {
            try
            {
                string contents;
                using (var sr = new StreamReader(filename))
                {
                    contents = sr.ReadToEnd();
                }
                return JsonSerializer.Deserialize<Project>(contents);
            }
            catch (Exception)
            {
                //MessageBox.Show("Failed to load project");
                throw new Exception("Failed to load project");
            }
        }


        public void CleanupResources()
        {
            slidenum = 0;
            // delete temp directory if needed
            if (_loadTmpPath != null && _loadTmpPath != string.Empty)
            {
                try
                {
                    Directory.Delete(_loadTmpPath, true);
                }
                catch (Exception)
                {
                }
            }
        }

        public void CreateImageAsset(Bitmap b, string sourceurl, string name)
        {
            string tmpassetpath = System.IO.Path.Combine(LoadTmpPath, "assets", sourceurl.Split("/").Last());
            try
            {
                b.Save(tmpassetpath);
                ProjectAsset asset = new ProjectAsset() { Id = Guid.NewGuid(), LoadedTempPath = tmpassetpath, Name = name, OriginalPath = sourceurl, Type = AssetType.Image };
                Assets.Add(asset);
            }
            catch (Exception)
            {
            }

        }

    }
}
