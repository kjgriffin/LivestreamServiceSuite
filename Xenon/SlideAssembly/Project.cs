using CCU.Config;

using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;

using Xenon.AssetManagment;
using Xenon.Compiler.Suggestions;
using Xenon.SlideAssembly.LayoutManagement;

namespace Xenon.SlideAssembly
{
    public class Project
    {
        public SlideLayout Layouts { get; set; } = new SlideLayout();
        //public ProjectLayoutLibraryManager ProjectLayouts { get; set; } = new ProjectLayoutLibraryManager();

        public IProjectLayoutLibraryManager LayoutManager { get; private set; } = new XenonLayoutManager();

        public List<Slide> Slides { get; set; } = new List<Slide>();

        public Dictionary<string, List<string>> ProjectVariables = new Dictionary<string, List<string>>();

        public List<ProjectAsset> Assets { get; set; } = new List<ProjectAsset>();
        public string SourceCode { get; set; } = string.Empty;
        public string SourceConfig { get; set; } = string.Empty;

        public Dictionary<string, string> ExtraSourceFiles { get; set; } = new Dictionary<string, string>();

        public string SourceCCPUConfigFull { get; set; } = string.Empty;
        public CCPUConfig_Extended CCPUConfig { get; set; } = new CCPUConfig_Extended();


        private int slidenum = 0;
        public int NewSlideNumber => slidenum++;

        private int resourceslide = 0;
        public int NewResourceSlideNumber => resourceslide++;

        public XenonSuggestionService XenonSuggestionService { get; set; }

        public BMDSwitcherConfigSettings BMDSwitcherConfig { get; set; } = DefaultConfig.GetDefaultConfig();

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



        internal void Clear()
        {
            Slides.Clear();
            ProjectVariables.Clear();
            SourceCode = "";
            slidenum = 0;
            resourceslide = 0;
        }

        internal void ClearSlidesAndVariables()
        {
            Slides.Clear();
            ProjectVariables.Clear();
            slidenum = 0;
            resourceslide = 0;
        }

        private string _loadTmpPath = "";

        public string LoadTmpPath { get => _loadTmpPath; }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="withtemp">Initializez a new temp folder to store assets</param>
        /// <param name="adddefaultassets">Adds default assets to project. Set to <see langword="false"/> if loading a project from a save</param>
        public Project(bool withtemp = false, bool adddefaultassets = true)
        {
            if (withtemp)
            {
                // create temp folder for unpacking zipped assets into
                _loadTmpPath = Path.Combine(Path.GetTempPath(), $"tmpprojassets_newproj_{DateTime.Now:yyyyMMddhhmmss}");
                Directory.CreateDirectory(_loadTmpPath);
                Directory.CreateDirectory(Path.Combine(_loadTmpPath, "assets"));
            }
            LayoutManager.LoadDefaults();
            if (adddefaultassets)
            {
                InitializeDefaultAssets();
            }
            else
            {
                Assets = new List<ProjectAsset>();
            }
            //ProjectLayouts.InitializeNewLibrary("User.Library");
            XenonSuggestionService = new XenonSuggestionService(this);
            SourceConfig = JsonSerializer.Serialize(BMDSwitcherConfig, new JsonSerializerOptions() { WriteIndented = true });
        }

        public Project()
        {
            LayoutManager.LoadDefaults();
            //ProjectLayouts.InitializeNewLibrary("User.Library");
            XenonSuggestionService = new XenonSuggestionService(this);
            InitializeDefaultAssets();
        }


        public void InitializeDefaultAssets()
        {

            try
            {
                var defaultimages = ProjectResources.ProjectDefaults.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true)
                                                                                    .Cast<DictionaryEntry>()
                                                                                    .Where(x => x.Value.GetType() == typeof(Bitmap))
                                                                                    .Select(x => new { Name = x.Key.ToString(), Image = x.Value as Bitmap })
                                                                                    .ToList();

                foreach (var image in defaultimages)
                {
                    try
                    {
                        CreateImageAsset(image.Image, $"xenondefault_{image.Name}.png", image.Name, "Xenon.Core");
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            catch
            {
                return;
            }

        }

        public void CleanupResources()
        {
            slidenum = 0;
            resourceslide = 0;
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

        public void CreateImageAsset(Bitmap b, string sourceurl, string name, string group = "default")
        {
            string tmpassetpath = System.IO.Path.Combine(LoadTmpPath, "assets", sourceurl.Split("/").Last());
            try
            {
                b.Save(tmpassetpath);
                ProjectAsset asset = new ProjectAsset()
                {
                    Id = Guid.NewGuid(),
                    LoadedTempPath = tmpassetpath,
                    Name = name,
                    OriginalPath = sourceurl,
                    Type = AssetType.Image,
                    Extension = System.IO.Path.GetExtension(sourceurl),
                    Group = group,
                };
                Assets.Add(asset);
            }
            catch (Exception)
            {
            }

        }

    }
}
