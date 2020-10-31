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

namespace Xenon.SlideAssembly
{
    public class Project
    {
        public SlideLayout Layouts { get; set; } = new SlideLayout();
        public List<Slide> Slides { get; set; } = new List<Slide>();

        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();
        public string SourceCode { get; set; } = string.Empty;


        private int slidenum = 0;
        public int NewSlideNumber => slidenum++;


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

        public async Task SaveProject(string filename, IProgress<int> progress)
        {
            await Task.Run(async () =>
            {
                progress.Report(0);

                using FileStream ziptoopen = new FileStream($"{filename}", FileMode.Create);
                using ZipArchive archive = new ZipArchive(ziptoopen, ZipArchiveMode.Update);
                // create readme
                ZipArchiveEntry readmeEntry = archive.CreateEntry("Readme.txt");
                using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                {
                    writer.WriteLine("Information about this package.");
                    writer.WriteLine("===============================");
                }

                progress.Report(1);



                // create assets folder
                string assetsfolderpath = $"assets{Path.DirectorySeparatorChar}";
                var assetsfolder = archive.CreateEntry(assetsfolderpath);

                progress.Report(5);

                int count = Assets.Count;

                int completed = 0;
                // copy assets
                foreach (var asset in Assets)
                {
                    ZipArchiveEntry zippedasset = archive.CreateEntryFromFile(asset.CurrentPath, Path.Combine(assetsfolderpath, asset.OriginalFilename));
                    completed++;
                    progress.Report(5 + (int)((double)completed / (double)count * 100 * 0.9));
                }


                //Parallel.ForEach(Assets, (asset) =>
                //{
                //    ZipArchiveEntry zippedasset = archive.CreateEntryFromFile(asset.CurrentPath, Path.Combine(assetsfolderpath, asset.OriginalFilename));
                //    completed++;
                //    progress.Report(5 + (int)((double)0 / (double)count * 100 * 0.9));
                //});

                // update project assets to point to zip


                progress.Report(90);

                // save json format
                ZipArchiveEntry jsonfile = archive.CreateEntry("Project.json");
                var sobj = JsonSerializer.Serialize(this);
                using (StreamWriter writer = new StreamWriter(jsonfile.Open()))
                {
                    await writer.WriteAsync(sobj);
                }
                progress.Report(100);
            });
        }

        private string _loadTmpPath = "";

        public string LoadTmpPath { get => _loadTmpPath; }

        ~Project()
        {
            // delete temp directory if needed
            if (_loadTmpPath != null && _loadTmpPath != string.Empty)
            {
                try
                {
                    //Directory.Delete(_loadTmpPath, true);
                }
                catch (Exception)
                {
                }
            }
        }

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

                foreach (var a in p.Assets)
                {
                    // extract asset to temp folder
                    a.LoadedTempPath = Path.Combine(p._loadTmpPath, "assets", a.OriginalFilename);
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
    }
}
