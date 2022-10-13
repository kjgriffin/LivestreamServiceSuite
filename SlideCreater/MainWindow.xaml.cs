using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using Xenon.Compiler;
using Xenon.Renderer;
using Xenon.Helpers;
using Xenon.SlideAssembly;
using Xenon.AssetManagment;
using System.Diagnostics;
using SlideCreater.ViewControls;
using Xenon.SaveLoad;
using System.IO.Compression;
using System.Net;
using UIControls;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Xenon.Compiler.Suggestions;
using CommonVersionInfo;
using System.Net.Http;
using LutheRun;
using Xenon.Compiler.Formatter;
using System.IO.MemoryMappedFiles;
using IntegratedPresenterAPIInterop;
using CCU.Config;

namespace SlideCreater
{


    public enum ProjectState
    {
        NewProject,
        Saving,
        Saved,
        Dirty,
        LoadError,
    }

    public enum ActionState
    {
        Ready,
        Building,
        SuccessBuilding,
        ErrorBuilding,
        Exporting,
        SuccessExporting,
        ErrorExporting,
        Saving,
        Downloading,
        SuccessBuilding_HotReloading,
    }




    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CreaterEditorWindow : Window
    {

        ProjectState _mProjectState;
        public ProjectState ProjectState
        {
            get => _mProjectState;
            set
            {
                _mProjectState = value;
                UpdateProjectStatusLabel();
            }
        }

        ActionState _mActionState;
        public ActionState ActionState
        {
            get => _mActionState;
            set
            {
                _mActionState = value;
                UpdateActionStatusLabel();
                UpdateActionProgressBarVisibile();
            }
        }


        private void UpdateProjectStatusLabel()
        {
            switch (ProjectState)
            {
                case ProjectState.NewProject:
                    tbProjectStatus.Text = "New Project";
                    break;
                case ProjectState.Saved:
                    tbProjectStatus.Text = "Project Saved";
                    break;
                case ProjectState.Saving:
                    tbProjectStatus.Text = "Saving Project...";
                    break;
                case ProjectState.Dirty:
                    tbProjectStatus.Text = "*Unsaved Changes";
                    break;
                default:
                    break;
            }
        }

        private void UpdateActionStatusLabel()
        {
            switch (ActionState)
            {
                case ActionState.Ready:
                    tbActionStatus.Text = "";
                    tbSubActionStatus.Text = "";
                    sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
                    break;
                case ActionState.Building:
                    tbActionStatus.Text = "Buildling...";
                    sbStatus.Background = System.Windows.Media.Brushes.Orange;
                    break;
                case ActionState.SuccessBuilding:
                    tbActionStatus.Text = "Project Rendered";
                    tbSubActionStatus.Text = "";
                    sbStatus.Background = System.Windows.Media.Brushes.Green;
                    break;
                case ActionState.SuccessBuilding_HotReloading:
                    tbActionStatus.Text = "Project Rendered";
                    tbSubActionStatus.Text = "Hot Reload Made Available";
                    sbStatus.Background = System.Windows.Media.Brushes.Teal;
                    break;
                case ActionState.ErrorBuilding:
                    tbActionStatus.Text = "Render Failed";
                    sbStatus.Background = System.Windows.Media.Brushes.Red;
                    break;
                case ActionState.Exporting:
                    tbActionStatus.Text = "Exporting...";
                    sbStatus.Background = System.Windows.Media.Brushes.Orange;
                    break;
                case ActionState.SuccessExporting:
                    tbActionStatus.Text = "Slides Exported";
                    tbSubActionStatus.Text = "";
                    sbStatus.Background = System.Windows.Media.Brushes.Green;
                    break;
                case ActionState.ErrorExporting:
                    tbActionStatus.Text = "Export Failed";
                    tbSubActionStatus.Text = "";
                    sbStatus.Background = System.Windows.Media.Brushes.IndianRed;
                    break;
                case ActionState.Saving:
                    tbActionStatus.Text = "Saving...";
                    sbStatus.Background = System.Windows.Media.Brushes.Purple;
                    break;
                case ActionState.Downloading:
                    tbActionStatus.Text = "Downloading resources...";
                    sbStatus.Background = System.Windows.Media.Brushes.Orange;
                    break;
                default:
                    break;
            }
        }

        private void UpdateActionProgressBarVisibile()
        {
            switch (ActionState)
            {
                case ActionState.Ready:
                    pbActionStatus.Visibility = Visibility.Hidden;
                    break;
                case ActionState.Building:
                    pbActionStatus.Visibility = Visibility.Visible;
                    break;
                case ActionState.SuccessBuilding:
                    pbActionStatus.Visibility = Visibility.Hidden;
                    break;
                case ActionState.ErrorBuilding:
                    pbActionStatus.Visibility = Visibility.Hidden;
                    break;
                case ActionState.Exporting:
                    pbActionStatus.Visibility = Visibility.Visible;
                    break;
                case ActionState.SuccessExporting:
                    pbActionStatus.Visibility = Visibility.Hidden;
                    break;
                case ActionState.ErrorExporting:
                    pbActionStatus.Visibility = Visibility.Hidden;
                    break;
                case ActionState.Saving:
                    pbActionStatus.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }


        private BuildVersion VersionInfo;

        public CreaterEditorWindow()
        {
            InitializeComponent();

#if ! DEBUG
            miEdit.Visibility = Visibility.Collapsed;        
            miFormatSource.Visibility = Visibility.Collapsed;
#endif


            TbInput.LoadLanguage_XENON();
            TbInput.TextArea.TextEntered += TextArea_TextEntered;
            TbInput.TextArea.TextEntering += TextArea_TextEntering;
            TbInput.TextArea.PreviewTextInput += TextArea_PreviewTextInput;

            TbInput.TextArea.TextView.LinkTextForegroundBrush = System.Windows.Media.Brushes.LawnGreen;

            TbConfig.LoadLanguage_JSON();
            TbConfigCCU.LoadLanguage_JSON();
            // load default config file

            // setup indentation
            TbInput.Options.IndentationSize = 4;
            TbInput.Options.ConvertTabsToSpaces = true;
            //TbInput.TextArea.IndentationStrategy = new XenonIndentationStrategy();
            //TbInput.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
            TbConfig.Options.IndentationSize = 4;
            TbConfig.Options.ConvertTabsToSpaces = true;

            TbConfigCCU.Options.IndentationSize = 4;
            TbConfigCCU.Options.ConvertTabsToSpaces = true;

            // Load Version Number
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SlideCreater.version.json");
            using (StreamReader sr = new StreamReader(stream))
            {
                string json = sr.ReadToEnd();
                VersionInfo = JsonSerializer.Deserialize<BuildVersion>(json);
            }
            // Set title
            Title = $"Slide Creater - {VersionInfo.MajorVersion}.{VersionInfo.MinorVersion}.{VersionInfo.Revision}.{VersionInfo.Build}-{VersionInfo.Mode}";

            // Initialize XenonLibrary to use version
            Xenon.Versioning.Versioning.SetVersion(VersionInfo);




            NewProject().Wait();
            dirty = false;
            ProjectState = ProjectState.NewProject;
            ActionState = ActionState.Ready;

            // enable optional features
            EnableDisableOptionalFeatures();

            SetupLayoutsTreeVew();

            ShowProjectAssets();
        }


        private void EnableDisableOptionalFeatures()
        {
            if (VersionInfo.Mode != "Debug")
            {
                miimport_adv.Visibility = Visibility.Collapsed;
                miimport_adv.IsEnabled = false;
            }
        }


        XenonBuildService builder = new XenonBuildService();
        private async void RenderSlides(object sender, RoutedEventArgs e)
        {
            _proj.SourceCode = TbInput.Text;
            _proj.SourceConfig = TbConfig.Text;

            TryAutoSave();

            if (!m_agressive_autosave_enabled)
            {
                // always do it pre-render just to be safe
                await TryFullAutoSave();
            }

            //tbConsole.Text = string.Empty;

            var compileprogress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    tbSubActionStatus.Text = $"Compiling Project: {percent}%";
                    pbActionStatus.Value = percent;
                });
            });

            var rendererprogress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    tbSubActionStatus.Text = $"Rendering Project: {percent}%";
                    pbActionStatus.Value = percent;
                });
            });

            alllogs.Clear();

            // compile text
            //bool success = await builder.BuildProject(_proj, _proj.SourceCode, Assets, compileprogress);
            builder.Messages.Clear();

            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Compilation Started", ErrorName = "Compilation Started", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Debug, Token = "" });
            sbStatus.Dispatcher.Invoke(() =>
            {
                ActionState = ActionState.Building;
            });

            Stopwatch compileTimer = new Stopwatch();

            compileTimer.Start();
            var build = await builder.CompileProjectAsync(_proj, compileprogress);
            compileTimer.Stop();


            if (!build.success)
            {
                sbStatus.Dispatcher.Invoke(() =>
                {
                    ActionState = ActionState.ErrorBuilding;
                });
                alllogs.AddRange(builder.Messages);
                UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Compilation Failed!", ErrorName = "Compilation Failure", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Error, Token = "" });
                return;
            }


            alllogs.AddRange(builder.Messages);
            builder.Messages.Clear();
            var timestr = compileTimer.Elapsed.ToString(@"%s\.%fff");
            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = $"Compiled Successfully!", ErrorName = $"Project Compiled ({timestr})s", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Message, Token = "" });


            //try
            //{
            //slides = await builder.RenderProject(_proj, rendererprogress);
            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Render Starting", ErrorName = "Render Starting", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Debug, Token = "" });

            Stopwatch buildtimer = new Stopwatch();
            buildtimer.Start();
            slides = (await builder.RenderProjectAsync(build.project, rendererprogress, renderinparallel, renderclean)).OrderBy(s => s.Number).ToList();
            buildtimer.Stop();
            timestr = buildtimer.Elapsed.ToString(@"%s\.%fff");

            // ok- here we'll need to hack into the slides a bit to get previews to work correctly (for action slides that have alt display sources)
            ApplyDisplayOverridesOnSlides(slides);



            alllogs.AddRange(builder.Messages);
            builder.Messages.Clear();
            UpdateErrorReport(alllogs);

            //}
            //catch (Exception ex)
            //{
            //    sbStatus.Dispatcher.Invoke(() =>
            //    {
            //        ActionState = ActionState.ErrorBuilding;
            //        UpdateErrorReport(builder.Messages, new XenonCompilerMessage() { ErrorMessage = $"Render failed for reason: {ex}", ErrorName = "Render Failure", Generator = "Main Rendering", Inner = "", Level = XenonCompilerMessageType.Error, Token = "" });
            //    });
            //    return;
            //}


            Dispatcher.Invoke(() =>
                            {
                                tbSubActionStatus.Text = $"Generating Slide Previews...";
                                pbActionStatus.Value = 99;
                            });

            UpdatePreviews();

            sbStatus.Dispatcher.Invoke(() =>
                       {
                           ActionState = ActionState.SuccessBuilding;
                           UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = $"Slides built!", ErrorName = $"Render Success ({timestr})s", Generator = "Main Rendering", Inner = "", Level = XenonCompilerMessageType.Message, Token = "" });
                       });

            if (hotReloadEnabled)
            {
                PublishHotReload();
            }
        }

        private void ApplyDisplayOverridesOnSlides(List<RenderedSlide> slides)
        {
            // go through the slides and if they've got scripts referencing other slides graphics we'll grab them and modifiy the rendered slide
            // this is not really correct... but hey. Its fine??

            foreach (RenderedSlide slide in slides)
            {
                if (slide.RenderedAs == "Action")
                {
                    // TODO: now this assumes they'd be taken from a slide, and not a resource... may need to change that at some point
                    // though we currently can't really dump a slide into a resource yet
                    var srcoverride = Regex.Match(slide.Text, "!displaysrc='(?<src>\\d+)_.*\\.png';");
                    if (srcoverride.Success)
                    {
                        var sindex = Convert.ToInt32(srcoverride.Groups["src"].Value);
                        slide.Bitmap = slides.FirstOrDefault(s => s.Number == sindex)?.Bitmap;
                        slide.BitmapPNGMS = slides.FirstOrDefault(s => s.Number == sindex)?.BitmapPNGMS;
                    }
                    else
                    {
                        srcoverride = Regex.Match(slide.Text, "!displaysrc='(?<src>Resource_.*)\\.png';");
                        if (srcoverride.Success)
                        {
                            var sname = srcoverride.Groups["src"].Value;
                            slide.Bitmap = slides.FirstOrDefault(s => s?.OverridingBehaviour?.OverrideExportName == sname)?.Bitmap;
                            slide.BitmapPNGMS = slides.FirstOrDefault(s => s?.OverridingBehaviour?.OverrideExportName == sname)?.BitmapPNGMS;
                        }
                    }



                    var keyoverride = Regex.Match(slide.Text, "!keysrc='Key_(?<src>\\d+)\\.png';");
                    if (keyoverride.Success)
                    {
                        var sindex = Convert.ToInt32(keyoverride.Groups["src"].Value);
                        slide.KeyBitmap = slides.FirstOrDefault(s => s.Number == sindex)?.KeyBitmap;
                        slide.KeyPNGMS = slides.FirstOrDefault(s => s.Number == sindex)?.KeyPNGMS;
                    }
                    else
                    {
                        keyoverride = Regex.Match(slide.Text, "!keysrc='(?<src>Resource_.*)\\.png';");
                        if (keyoverride.Success)
                        {
                            var sname = srcoverride.Groups["src"].Value;
                            slide.KeyBitmap = slides.FirstOrDefault(s => s?.OverridingBehaviour?.OverrideExportKeyName == sname)?.KeyBitmap;
                            slide.KeyPNGMS = slides.FirstOrDefault(s => s?.OverridingBehaviour?.OverrideExportKeyName == sname)?.KeyPNGMS;
                        }
                    }
                }
            }

        }


        private Dictionary<XenonCompilerMessageType, bool> MessageVisiblity = new Dictionary<XenonCompilerMessageType, bool>()
        {

            [XenonCompilerMessageType.Debug] = false,
            [XenonCompilerMessageType.Message] = true,
            [XenonCompilerMessageType.Info] = false,
            [XenonCompilerMessageType.Warning] = true,
            [XenonCompilerMessageType.ManualIntervention] = true,
            [XenonCompilerMessageType.Error] = true,
        };


        List<XenonCompilerMessage> logs = new List<XenonCompilerMessage>();
        List<XenonCompilerMessage> alllogs = new List<XenonCompilerMessage>();
        private void UpdateErrorReport(List<XenonCompilerMessage> messages, XenonCompilerMessage other = null)
        {
            error_report_view.Dispatcher.Invoke(() =>
            {
                if (other != null)
                {
                    messages.Insert(0, other);
                }
                alllogs = messages;
                logs = messages.Where(m => MessageVisiblity[m.Level] == true).ToList();

                error_report_view.ItemsSource = logs;
            });
        }

        private void UpdatePreviews()
        {
            int oldSIndex = slidelist.SelectedIndex;

            slidelist.Items.Clear();
            //slidepreviews.Clear();
            previews.Clear();
            // add all slides to list
            foreach (var slide in slides.OrderBy(s => s.Number != -1 ? s.Number : int.MaxValue)) // insert slides by order. kick resources to bottom
            {
                MegaSlidePreviewer previewer = new MegaSlidePreviewer();
                previewer.Width = slidelist.Width;
                previewer.Slide = slide;
                slidelist.Items.Add(previewer);
                previews.Add(previewer);
            }

            // update focusslide
            if (slides?.Count > 0)
            {
                if (slides.Count > oldSIndex && oldSIndex >= 0)
                {
                    // try and refocus on last inspected slide
                    FocusSlide.Slide = slides[oldSIndex];
                    slidelist.SelectedIndex = oldSIndex;
                    slidelist.ScrollIntoView(slidelist.Items[oldSIndex]);
                }
                else
                {
                    FocusSlide.Slide = slides.First();
                    slidelist.SelectedIndex = 0;
                }
                FocusSlide.ShowSlide(previewkeys);
                tbSlideCount.Text = $"Slides: {slides.First().Number + 1}/{slides.Count()}";
            }
            else
            {
                tbSlideCount.Text = $"Slides: {slides.Count()}";
            }

        }

        Project _proj;
        //List<SlideContentPresenter> slidepreviews = new List<SlideContentPresenter>();
        List<MegaSlidePreviewer> previews = new List<MegaSlidePreviewer>();

        private void SelectSlideToPreview(object sender, RenderedSlide slide)
        {
            FocusSlide.Slide = slide;
            FocusSlide.ShowSlide(previewkeys);
            FocusSlide.PlaySlide();
            this.slide = slides.FindIndex(s => s == slide) + 1;
            if (this.slide >= slides.Count)
            {
                this.slide = 0;
            }
            tbSlideCount.Text = $"Slides: {slide.Number + 1}/{slides.Count()}";
        }

        List<RenderedSlide> slides = new List<RenderedSlide>();

        int slide = 0;

        private void Show(object sender, RoutedEventArgs e)
        {
            FocusSlide.Slide = slides[slide];
            FocusSlide.ShowSlide(previewkeys);
            if (slide + 1 < slides.Count)
            {
                slide += 1;
            }
            else
            {
                slide = 0;
            }
        }

        private void ClickAddAssets(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Add Assets";
            ofd.Filter = "Images, Video and Audio (*.png;*.jpg;*.bmp;*.mp4;*.mp3;*.wav)|*.png;*.jpg;*.bmp;*.mp4;*.mp3;*.wav";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                AddAssetsFromPaths(ofd.FileNames, "User");
            }
            TryAutoSave();
        }

        private void AddAssetsFromPaths(IEnumerable<string> filenames, string group)
        {
            MarkDirty();
            // add assets
            foreach (var file in filenames)
            {
                // copy to tmp folder
                string tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, "assets", System.IO.Path.GetFileName(file));
                if (File.Exists(tmpassetpath))
                {
                    tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, "assets", $"{System.IO.Path.GetFileNameWithoutExtension(file)}_{_proj.Assets.Count + 1}_{System.IO.Path.GetExtension(file)}");
                }
                try
                {
                    File.Copy(file, tmpassetpath, true);
                }
                catch (Exception)
                {
                    MessageBox.Show($"Unable to load {file}", "Load Asset Failed");
                    continue;
                }

                ProjectAsset asset = new ProjectAsset();
                if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.mp3)|(\.wav)", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Audio, Extension = System.IO.Path.GetExtension(file) };
                }
                else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Image, Extension = System.IO.Path.GetExtension(file) };
                }
                else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"\.mp4", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video, Extension = System.IO.Path.GetExtension(file) };
                }

                asset.Group = group;

                _proj.Assets.Add(asset);
                ShowProjectAssets();
            }
        }

        private void AddNamedAssetsFromPaths(IEnumerable<(string path, string name)> assets)
        {
            MarkDirty();
            // add assets
            foreach (var (path, name) in assets)
            {
                // copy to tmp folder
                string tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, "assets", System.IO.Path.GetFileName(path));
                try
                {
                    File.Copy(path, tmpassetpath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to load {name}, '{path}'", $"Load Asset Failed with exception {ex}");
                }

                ProjectAsset asset = new ProjectAsset();
                if (Regex.IsMatch(System.IO.Path.GetExtension(path).ToLower(), @"(\.mp3)|(\.wav)", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = name, OriginalPath = path, LoadedTempPath = tmpassetpath, Type = AssetType.Audio, Extension = System.IO.Path.GetExtension(path) };
                }
                else if (Regex.IsMatch(System.IO.Path.GetExtension(path).ToLower(), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = name, OriginalPath = path, LoadedTempPath = tmpassetpath, Type = AssetType.Image, Extension = System.IO.Path.GetExtension(path) };
                }
                else if (Regex.IsMatch(System.IO.Path.GetExtension(path).ToLower(), @"\.mp4", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = name, OriginalPath = path, LoadedTempPath = tmpassetpath, Type = AssetType.Video, Extension = System.IO.Path.GetExtension(path) };
                }

                _proj.Assets.Add(asset);
                ShowProjectAssets();
            }

        }

        private void ShowProjectAssets()
        {
            //AssetList.Children.Clear();
            ClearProjectAssetsView();

            bool loaderrors = false;
            List<string> missingassets = new List<string>();

            var assetcontrols = new List<AssetItemControl>();
            foreach (var asset in _proj.Assets)
            {
                try
                {
                    AssetItemControl assetItemCtrl = new AssetItemControl(asset);
                    assetItemCtrl.OnFitInsertRequest += AssetItemCtrl_OnFitInsertRequest;
                    assetItemCtrl.OnDeleteAssetRequest += AssetItemCtrl_OnDeleteAssetRequest;
                    assetItemCtrl.OnAutoFitInsertRequest += AssetItemCtrl_OnAutoFitInsertRequest;
                    assetItemCtrl.OnInsertResourceRequest += AssetItemCtrl_OnInsertResourceRequest;
                    assetItemCtrl.OnLiturgyInsertRequest += AssetItemCtrl_OnLiturgyInsertRequest;
                    assetItemCtrl.OnRenameAssetRequest += AssetItemCtrl_OnRenameAssetRequest;
                    //AssetList.Children.Add(assetItemCtrl);
                    assetcontrols.Add(assetItemCtrl);
                }
                catch (FileNotFoundException)
                {
                    loaderrors = true;
                    missingassets.Add(asset.Name);
                }
            }
            AddProjectAssetToTreeVeiw(assetcontrols);

            if (loaderrors)
            {
                throw new Exception($"Missing ({missingassets.Count}) asset(s): {missingassets.Aggregate((agg, item) => agg + ", '" + item + "'")}");
            }
        }

        Dictionary<string, bool> expansionStateAssetsList = new Dictionary<string, bool>();
        private void ClearProjectAssetsView()
        {
            expansionStateAssetsList.Clear();
            foreach (TreeViewItem item in AssetTree.Items)
            {
                var gname = item.Header.ToString();
                gname = gname.Split(' ').FirstOrDefault("default");
                expansionStateAssetsList[gname] = item.IsExpanded;
            }
            AssetTree.Items.Clear();
        }

        private void AddProjectAssetToTreeVeiw(List<AssetItemControl> ctrls)
        {
            HashSet<string> allassetgroups = new HashSet<string>();
            foreach (var asset in _proj.Assets)
            {
                allassetgroups.Add(asset.Group);
            }

            List<(TreeViewItem tvi, string group)> tvgroups = new List<(TreeViewItem, string)>();
            foreach (var group in allassetgroups)
            {
                TreeViewItem tvgroup = new TreeViewItem();
                var ctrlsingroup = ctrls.Where(ctrl => ctrl.AssetGroup == group).ToList();
                tvgroup.Header = $"{group} ({ctrlsingroup.Count})";

                tvgroup.IsExpanded = false;
                if (expansionStateAssetsList.TryGetValue(group, out bool state))
                {
                    tvgroup.IsExpanded = state;
                }
                else
                {
                    // by default only expand 'User' group
                    if (group == "User")
                    {
                        tvgroup.IsExpanded = true;
                    }
                }

                foreach (var assetctrl in ctrlsingroup)
                {
                    tvgroup.Items.Add(assetctrl);
                }
                tvgroups.Add((tvgroup, group));
            }

            // order groups

            foreach (var gitem in tvgroups.OrderBy(g =>
            {
                if (g.group == "User") return 1; // always first
                if (g.group == "Xenon.Core") return 3; // always last
                return 2; // otherwise we don't really care...
            }))
            {
                AssetTree.Items.Add(gitem.tvi);
            }

        }

        private void AssetItemCtrl_OnRenameAssetRequest(object sender, ProjectAsset asset, string newname)
        {
            MarkDirty();
            asset.Name = newname;

            ShowProjectAssets();
            TryAutoSave();
        }

        private void AssetItemCtrl_OnInsertResourceRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = $"\r\n#resource(\"{asset.Name}\", \"audio\")\r\n\r\n#script {{\r\n#Worship Bells;\r\n@arg0:DSK1FadeOff[Kill Liturgy];\r\n@arg0:OpenAudioPlayer;\r\n@arg1:LoadAudioFile(Resource_{asset.OriginalFilename})[Load Bells];\r\n@arg1:PresetSelect(7)[Preset Center];\r\n@arg1:DelayMs(100);\r\n@arg0:AutoTrans[Take Center];\r\n@arg1:DelayMs(2000);\r\n@arg0:PlayAuxAudio[Play Bells];\r\narg1:PresetSelect(8)[Preset Pulpit];\r\n}}\r\n";

            string InsertResourceCommand = $"#resource(\"{asset.Name}\",\"{asset.KindString}\")";

            InsertTextCommand(InsertResourceCommand);
        }

        private void AssetItemCtrl_OnLiturgyInsertRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = $"\r\n#litimage({asset.Name})\r\n";
            InsertTextCommand(InsertCommand);
        }

        private void AssetItemCtrl_OnAutoFitInsertRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = $"\r\n#autofitimage({asset.Name})\r\n";
            InsertTextCommand(InsertCommand);
        }

        private void InsertTextCommand(string InsertCommand)
        {
            TbInput.Document.Insert(TbInput.CaretOffset, System.Environment.NewLine);
            foreach (var line in InsertCommand.Split(Environment.NewLine))
            {
                TbInput.Document.Insert(TbInput.CaretOffset, line);
                TbInput.Document.Insert(TbInput.CaretOffset, System.Environment.NewLine);
            }
            //TbInput.InsertLinesAfterCursor(InsertCommand.Split(Environment.NewLine));
            //TbInput.InsertTextAtCursor(InsertCommand);
            //int newindex = TbInput.CaretIndex + InsertCommand.Length;
            //TbInput.Text = TbInput.Text.Insert(TbInput.CaretIndex, InsertCommand);
            //TbInput.CaretIndex = newindex;
        }

        private void AssetItemCtrl_OnDeleteAssetRequest(object sender, ProjectAsset asset)
        {
            MarkDirty();
            //Assets.Remove(asset);
            _proj.Assets.Remove(asset);
            ShowProjectAssets();
            TryAutoSave();
        }

        private void AssetItemCtrl_OnFitInsertRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = "";
            if (asset.Type == AssetType.Video)
            {
                InsertCommand = $"\r\n#video({asset.Name})\r\n";
            }
            if (asset.Type == AssetType.Image)
            {
                InsertCommand = $"\r\n#fitimage({asset.Name})\r\n";
            }
            InsertTextCommand(InsertCommand);
            //TbInput.InsertLinesAfterCursor(InsertCommand.Split(Environment.NewLine));
            //int newindex = TbInput.CaretIndex + InsertCommand.Length;
            //TbInput.Text = TbInput.Text.Insert(TbInput.CaretIndex, InsertCommand);
            //TbInput.CaretIndex = newindex;
        }

        private async void ExportSlides(object sender, RoutedEventArgs e)
        {
            TryAutoSave();
            ActionState = ActionState.Exporting;
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Title = "Select Output Folder";
            ofd.FileName = "Slide";

            Progress<int> exportProgress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    ActionState = ActionState.Saving;
                    tbSubActionStatus.Text = $"Exporting Slides: {percent}%";
                    pbActionStatus.Value = percent;
                });
            });

            if (ofd.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    SlideExporter.ExportSlides(System.IO.Path.GetDirectoryName(ofd.FileName), _proj, new List<XenonCompilerMessage>(), exportProgress); // for now ignore messages
                });
                ActionState = ActionState.SuccessExporting;
            }
            else
            {
                ActionState = ActionState.ErrorExporting;
            }
        }

        private async Task NewProject()
        {
            if (dirty)
            {
                bool saved = await CheckSaveChanges();
                if (!saved)
                {
                    return;
                }
            }
            dirty = false;
            slidelist.Items.Clear();
            //slidepreviews.Clear();
            previews.Clear();
            //AssetList.Children.Clear();
            ClearProjectAssetsView();
            FocusSlide.Clear();
            TbInput.Text = string.Empty;
            _proj?.CleanupResources();
            _proj = new Project(true);

            TbConfig.Text = _proj.SourceConfig;

            ShowProjectAssets();
            SetupLayoutsTreeVew();

            ProjectState = ProjectState.NewProject;
            ActionState = ActionState.Ready;

            UpdateErrorReport(new List<XenonCompilerMessage>());

        }

        /// <summary>
        /// Return true if should proceed, false to abort
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckSaveChanges()
        {
            var res = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                //await SaveProject();
                await TrustySave();
                return true;
            }
            else if (res == MessageBoxResult.No)
            {
                return true;
            }
            return false;
        }

        private async void ClickNew(object sender, RoutedEventArgs e)
        {
            await NewProject();
        }

        private bool dirty = false;

        private void MarkDirty()
        {
            dirty = true;
            ProjectState = ProjectState.Dirty;
            ActionState = ActionState.Ready;
        }

        private async void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dirty)
            {
                bool saved = await CheckSaveChanges();
                if (!saved)
                {
                    e.Cancel = true;
                    return;
                }
            }
            hotReloadServer?.StopListening();
            hotReloadServer?.Release();
            // close everything else too
            Application.Current.Shutdown();
        }

        private void ClickClearAssets(object sender, RoutedEventArgs e)
        {
            _proj.Assets.Clear();
            ShowProjectAssets();
            MarkDirty();
            FocusSlide.Clear();
            //slidepreviews.Clear();
            previews.Clear();
            TryAutoSave();
        }

        private void slidelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SlideContentPresenter scp = (SlideContentPresenter)slidelist.SelectedItem;
            MegaSlidePreviewer msp = slidelist.SelectedItem as MegaSlidePreviewer;
            if (msp != null)
            {
                SelectSlideToPreview(sender, msp.main.Slide);
            }
        }

        private void ClickHelpCommands(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/kjgriffin/LivestreamServiceSuite/wiki/Slide-Creator-Commands");
        }

        private bool previewkeys = false;

        private void ClickTogglePreviewSlide(object sender, RoutedEventArgs e)
        {
            previewkeys = false;
            mipreviewkey.IsChecked = false;
            mipreviewslide.IsChecked = true;
            UpdatePreviews();
        }

        private void ClickTogglePreviewKey(object sender, RoutedEventArgs e)
        {
            previewkeys = true;
            mipreviewkey.IsChecked = true;
            mipreviewslide.IsChecked = false;
            UpdatePreviews();
        }

        private void TryAutoSave()
        {
            // might even trust agressive auto saves more
            if (m_agressive_autosave_enabled)
            {
                Task.Run(async () =>
                {
                    await TryFullAutoSave();
                });
                return;
            }


            // create a simple json and store it in temp files
            string tmppath = System.IO.Path.Join(System.IO.Path.GetTempPath(), "slidecreaterautosaves");
            string filename = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.autosave.json";
            string fullpath = System.IO.Path.Join(tmppath, filename);
            AutoSave save = new AutoSave();
            foreach (var asset in _proj.Assets)
            {
                save.SourceAssets.Add(asset.OriginalPath);
                save.AssetMap.TryAdd(asset.Name, asset.OriginalPath);
                save.AssetRenames.TryAdd(asset.Name, asset.DisplayName);
                save.AssetExtensions.TryAdd(asset.Name, asset.Extension);
            }
            save.SourceCode = TbInput.Text;
            try
            {
                string json = JsonSerializer.Serialize(save);
                if (!Directory.Exists(tmppath))
                {
                    Directory.CreateDirectory(tmppath);
                }
                using (TextWriter writer = new StreamWriter(fullpath))
                {
                    writer.Write(json);
                }
            }
            catch (Exception ex)
            {
                //tbConsole.Text += $"Autosave Failed {ex}";
                Debug.WriteLine($"Autosave Failed {ex}");
            }
        }

        private async Task TryFullAutoSave()
        {
            await Xenon.SaveLoad.TrustySave.SaveTrustily(_proj, System.IO.Path.Combine(System.IO.Path.Join(System.IO.Path.GetTempPath(), "slidecreaterautosaves", $"{DateTime.Now:yyy-MM-dd_HH-mm-ss}.autosave.trusty.zip")), null, VersionInfo.ToString());
        }

        private async void RecoverAutoSave(object sender, RoutedEventArgs e)
        {
            // open tmp files folder and let 'em select an autosave
            string tmppath = System.IO.Path.GetTempPath();
            // check if we have a slidecreaterautosaves folder
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Recover Autosave";
            ofd.Filter = "Autosaves (*.json;*.trusty.zip)|*.json;*.trusty.zip";
            ofd.Multiselect = false;
            if (Directory.Exists(System.IO.Path.Join(tmppath, "slidecreaterautosaves")))
            {
                ofd.InitialDirectory = System.IO.Path.Join(tmppath, "slidecreaterautosaves");
            }
            if (ofd.ShowDialog() == true)
            {
                if (System.IO.Path.GetExtension(ofd.FileName) == ".json")
                {
                    // then load it in
                    try
                    {
                        using (TextReader reader = new StreamReader(ofd.FileName))
                        {
                            string json = reader.ReadToEnd();
                            AutoSave save = JsonSerializer.Deserialize<AutoSave>(json);
                            await OverwriteWithAutoSave(save);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to recover autosave: {ex}", "Recovery Failed");
                    }
                }
                else if (Regex.Match(ofd.FileName, @".*?\.trusty\.zip$").Success)
                {
                    await TrustyOpen(ofd.FileName);
                }
            }
        }

        private async Task OverwriteWithAutoSave(AutoSave save)
        {
            await NewProject();
            // load assets
            if (save.AssetMap == null)
            {
                AddAssetsFromPaths(save.SourceAssets, "recovered");
            }
            else
            {

                ActionState = ActionState.Downloading;
                double percent = 0;
                int total = save.AssetMap.Count;
                int progress = 1;
                foreach (var assetkey in save.AssetMap.Keys)
                {
                    percent = ((double)progress++ / (double)total) * 100.0d;
                    tbSubActionStatus.Text = $"Recovering resources [{assetkey}]: {percent:00.0}%";
                    pbActionStatus.Visibility = Visibility.Visible;
                    pbActionStatus.Value = (int)percent;
                    var file = save.AssetMap[assetkey];
                    var tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, assetkey);
                    ProjectAsset asset = new ProjectAsset();

                    if (!File.Exists(file))
                    {
                        UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorName = "Asset Recovery: File not found", ErrorMessage = $"Tried to recover asset with name '{assetkey}' from original source '{file}'. File does not exist on disk. Checking if it can be recovered from an original source online.", Generator = "Recover Auto Save: Overwrite Project()", Inner = "", Level = XenonCompilerMessageType.Info, Token = "" });
                        Uri uriResult;
                        bool result = Uri.TryCreate(file, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (result)
                        {
                            try
                            {
                                // try downloading it
                                using (Stream rstream = await Xenon.Helpers.WebHelpers.httpClient.GetStreamAsync(uriResult))
                                {
                                    using (Stream fstream = File.Open(System.IO.Path.Combine(_proj.LoadTmpPath, assetkey), FileMode.OpenOrCreate, FileAccess.Write))
                                    {
                                        rstream.Seek(0, SeekOrigin.Begin);
                                        rstream.CopyTo(fstream);
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorName = "Asset Recovery: File not found", ErrorMessage = $"Tried to recover asset with name '{assetkey}' from online source '{uriResult}'.", Generator = "Recover Auto Save: Overwrite Project()", Inner = $"{ex}", Level = XenonCompilerMessageType.Error, Token = "" });
                            }
                        }
                    }
                    else
                    {
                        // copy file
                        System.IO.File.Copy(file, System.IO.Path.Combine(_proj.LoadTmpPath, assetkey));
                    }

                    if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.mp3)|(\.wav)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Audio, InternalDisplayName = save.AssetRenames.GetOrDefault(assetkey, assetkey), Extension = save.AssetExtensions[assetkey] };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Image, InternalDisplayName = save.AssetRenames.GetOrDefault(assetkey, assetkey), Extension = save.AssetExtensions[assetkey] };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"\.mp4", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video, InternalDisplayName = save.AssetRenames.GetOrDefault(assetkey, assetkey), Extension = save.AssetExtensions[assetkey] };
                    }
                    asset.Group = "recovered";
                    _proj.Assets.Add(asset);
                    ShowProjectAssets();
                }
                pbActionStatus.Value = 0;
                pbActionStatus.Visibility = Visibility.Hidden;
            }
            TbInput.Text = save.SourceCode;
        }

        LutheRun.LSBImportOptions options = new LutheRun.LSBImportOptions();

        private async void ClickImportService(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import from save of Lutheran Service Bulletin";
            ofd.Filter = "LSB Service (*.html)|*.html";
            if (ofd.ShowDialog() == true)
            {
                ActionState = ActionState.Building;
                LutheRun.LSBParser parser = new LutheRun.LSBParser();
                parser.LSBImportOptions = options;
                if (options.UseThemedCreeds || options.UseThemedHymns)
                {
                    // make user select the theme
                    options.ServiceThemeLib = GetUserSelectedThemeForImport();
                    // lets ask Xenon for the macros for the libray
                    var tmp = IProjectLayoutLibraryManager.GetDefaultBundledLibraries();
                    options.Macros = IProjectLayoutLibraryManager.GetDefaultBundledLibraries()
                                                                 .FirstOrDefault(x => x.LibName == options.ServiceThemeLib)?.Macros ?? new Dictionary<string, string>();

                }
                await parser.ParseHTML(ofd.FileName);
                parser.Serviceify(options);

                if (options.FlightPlanning)
                {
                    parser.FlightPlan(options);
                }

                parser.CompileToXenon();
                ActionState = ActionState.Downloading;
                await parser.LoadWebAssets(_proj.CreateImageAsset);
                MarkDirty();
                ShowProjectAssets();
                ActionState = ActionState.Ready;
                TbInput.Text = parser.XenonText;
            }
        }

        private string GetUserSelectedThemeForImport()
        {
            TbPromptDialog dialog = new TbPromptDialog("Select Theme To Import With", "Layout Library Name", "Xenon.CommonColored");
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.ResultValue))
            {
                return dialog.ResultValue;
            }
            return "Xenon.Core";
        }

        private void cb_message_view_debug_Click(object sender, RoutedEventArgs e)
        {
            MessageVisiblity[XenonCompilerMessageType.Debug] = cb_message_view_debug.IsChecked ?? false;
            UpdateErrorReport(alllogs);
        }

        private void cb_message_view_info_Click(object sender, RoutedEventArgs e)
        {
            MessageVisiblity[XenonCompilerMessageType.Info] = cb_message_view_debug.IsChecked ?? false;
            UpdateErrorReport(alllogs);
        }

        private void ClickImportServiceAdv(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Title = "Import from save of Lutheran Service Bulletin";
            //ofd.Filter = "LSB Service (*.html)|*.html";
            //if (ofd.ShowDialog() == true)
            //{
            //    //WebViewUI webViewUI = new WebViewUI(ofd.FileName, CreateProjectFromImportWizard);
            //    //webViewUI.Show();
            //}
        }

        private async void CreateProjectFromImportWizard(List<(string, string)> assets, string text)
        {
            await NewProject();
            _proj.SourceCode = text;
            TbInput.Text = text;

            // add all assets
            AddNamedAssetsFromPaths(assets);

        }

        private async void ClickTrustySave(object sender, RoutedEventArgs e)
        {
            await TrustySave();
        }

        private async Task TrustySave()
        {
            var saveprogress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    ActionState = ActionState.Saving;

                    switch (percent)
                    {
                        case var expression when percent < 1:
                            tbSubActionStatus.Text = $"Saving Project [create readme]: {percent}%";
                            break;
                        case var expression when percent >= 1 && percent < 5:
                            tbSubActionStatus.Text = $"Saving Project [create assets folder]: {percent}%";
                            break;
                        case var expression when percent >= 5 && percent < 90:
                            tbSubActionStatus.Text = $"Saving Project [copying assets]: {percent}%";
                            break;
                        case var expression when percent >= 90 && percent < 95:
                            tbSubActionStatus.Text = $"Saving Project [creating assets map]: {percent}%";
                            break;
                        case var expression when percent >= 95 && percent < 97:
                            tbSubActionStatus.Text = $"Saving Project [copying source code]: {percent}%";
                            break;
                        case var expression when percent >= 97:
                            tbSubActionStatus.Text = $"Saving Project [copying layout libraries]: {percent}%";
                            break;
                        default:
                            tbSubActionStatus.Text = $"Saving Project: {percent}%";
                            break;
                    }
                    pbActionStatus.Value = percent;
                });
            });

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Project";
            sfd.DefaultExt = "trusty.zip";
            sfd.AddExtension = false;
            sfd.FileName = $"Service_{DateTime.Now:yyyyMMdd}.trusty";
            if (sfd.ShowDialog() == true)
            {
                // perhaps we should forcibly take the latest changes to the source
                _proj.SourceCode = TbInput.Text;
                _proj.SourceConfig = TbConfig.Text;
                await Xenon.SaveLoad.TrustySave.SaveTrustily(_proj, sfd.FileName, saveprogress, VersionInfo.ToString());
                Dispatcher.Invoke(() =>
                {
                    dirty = false;
                    ProjectState = ProjectState.Saved;
                    ActionState = ActionState.Ready;
                });
            }
        }

        private async void ClickTrustyOpen(object sender, RoutedEventArgs e)
        {
            await NewProject();

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Trusty Project";
            ofd.DefaultExt = "zip";
            ofd.Filter = "Trusty Projects (*.trusty.zip)|*.trusty.zip";
            if (ofd.ShowDialog() == true)
            {
                await TrustyOpen(ofd.FileName);
            }
        }

        private async Task TrustyOpen(string filename)
        {
            CodePreviewUpdateFunc updatecode = (string val) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TbInput.Text = val;
                });
            };

            Action<string, CCPUConfig_Extended> updateccu = (string raw, CCPUConfig_Extended cfg) =>
            {
                var fakeJson = SanatizePNGsFromCfg(cfg);
                Dispatcher.Invoke(() =>
                {
                    TbConfigCCU.Text = fakeJson;
                });
            };

            var opened = await Xenon.SaveLoad.TrustySave.OpenTrustily(VersionInfo, filename, updatecode, updateccu);
            _proj = opened.project;



            // add assets
            Dispatcher.Invoke(() =>
            {
                MarkDirty();
                ActionState = ActionState.Downloading;
                TbConfig.Text = _proj.SourceConfig;
            });
            double percent = 0;
            int total = opened.assetfilemap.Count;
            int progress = 1;

            foreach (var assetkey in opened.assetfilemap.Keys)
            {
                percent = ((double)progress++ / (double)total) * 100.0d;
                Dispatcher.Invoke(() =>
                {
                    tbSubActionStatus.Text = $"Importing resources [{assetkey}]: {percent:00.0}%";
                    pbActionStatus.Visibility = Visibility.Visible;
                    pbActionStatus.Value = (int)percent;
                });


                await Task.Run(() =>
                {
                    var file = opened.assetfilemap[assetkey];
                    var tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, file);
                    var assetgroup = opened.assetgroups.GetValueOrDefault(assetkey, "default");
                    ProjectAsset asset = new ProjectAsset();
                    if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.mp3)|(\.wav)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Audio, InternalDisplayName = opened.assetdisplaynames.GetOrDefault(assetkey, assetkey), Extension = opened.assetextensions[assetkey] };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Image, InternalDisplayName = opened.assetdisplaynames.GetOrDefault(assetkey, assetkey), Extension = opened.assetextensions[assetkey] };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"\.mp4", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video, InternalDisplayName = opened.assetdisplaynames.GetOrDefault(assetkey, assetkey), Extension = opened.assetextensions[assetkey] };
                    }
                    asset.Group = assetgroup;
                    _proj.Assets.Add(asset);
                });

            }
            Dispatcher.Invoke(() =>
            {
                pbActionStatus.Value = 100;
                ActionState = ActionState.Ready;
                ShowProjectAssets();
                pbActionStatus.Visibility = Visibility.Hidden;
                pbActionStatus.Value = 0;
                SetupLayoutsTreeVew();
            });
        }

        private void OpenTempFolder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Project Temp Folder";
            ofd.InitialDirectory = _proj.LoadTmpPath;
            ofd.ShowDialog();
        }


        private bool m_agressive_autosave_enabled = true;

        private void SourceTextChanged(object sender, EventArgs e)
        {
            dirty = true;
            ProjectState = ProjectState.Dirty;
            ActionState = ActionState.Ready;
        }


        bool always_show_suggestions = false;
        bool entersuggestionwithenter = false;

        CompletionWindow completionWindow;
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if ((e.Text[0]) == 9 || e.Text.StartsWith("\t") || (entersuggestionwithenter && e.Text.StartsWith(System.Environment.NewLine)))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                    if (always_show_suggestions)
                    {
                        ShowSuggestions();
                    }
                }
            }
        }

        private void TextArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //const string LITURGY = "liturgy";
            if (e.Text == " " && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ShowSuggestions();

                e.Handled = true;
            }
        }

        private void ShowSuggestions()
        {
            var suggestions = _proj.XenonSuggestionService.GetSuggestions(TbInput.Text, TbInput.CaretOffset);
            if (suggestions.Any())
            {
                completionWindow = new CompletionWindow(TbInput.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var suggestion in suggestions)
                {
                    data.Add(new CommonCompletion(suggestion.item, suggestion.description));
                }
                completionWindow.Show();
                completionWindow.CompletionList.SelectedItem = data.First(d => d.Text == suggestions.First().item);
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            else
            {
                completionWindow?.Close();
            }
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (always_show_suggestions)
            {
                ShowSuggestions();
            }
            else if (!string.IsNullOrWhiteSpace(e.Text))
            {
                //ShowSuggestions();
            }
        }

        private void error_report_view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollToErrorMessage();
        }

        private void ScrollToErrorMessage()
        {
            // try and scroll textbox to line from error if possible
            var i = error_report_view?.SelectedIndex ?? 0;
            if (i >= 0 && i < logs.Count)
            {
                var selection = logs[i];
                if (selection.Token.linenum <= TbInput.LineCount)
                {
                    TbInput.ScrollToLine(selection.Token.linenum);
                    TbInput.TextArea.Caret.Line = selection.Token.linenum + 1;
                    TbInput.TextArea.Caret.Column = 0;
                    TbInput.TextArea.Focus();
                    TbInput.Select(TbInput.CaretOffset, TbInput.Document.Lines[selection.Token.linenum].Length);
                }
                else
                {
                    TbInput.Focus();
                    TbInput.TextArea.ClearSelection();
                }
            }
        }

        private void error_report_view_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ScrollToErrorMessage();
        }


        bool renderinparallel = true;
        bool renderclean = false;

        private void CreateNewLayoutOnLibrary(string libname, string group)
        {
            if (!string.IsNullOrWhiteSpace(libname))
            {
                TbPromptDialog namedialg = new TbPromptDialog("Create New Layout", "Layout Name", $"{group}.NewLayout");
                if (namedialg.ShowDialog() == true)
                {
                    _proj.LayoutManager.CreateNewLayoutFromDefaults(libname, group, namedialg.ResultValue);
                    SetupLayoutsTreeVew();
                }
            }
        }


        private void SetupLayoutsTreeVew()
        {
            Dictionary<string, bool> oldstate = RememberTreeExpansion(LayoutsTreeView);

            foreach (var item in LayoutsTreeView.Items)
            {
                var library = item as TreeViewItem;
                if (library != null)
                {
                    library.Selected -= Treelibrary_Selected;
                    foreach (var citem in library.Items)
                    {
                        var group = citem as LayoutGroupTreeItem;
                        if (group != null)
                        {
                            group.Selected -= Treelibrary_Selected;
                        }
                    }
                }
            }

            LayoutsTreeView.Items.Clear();

            foreach (var library in _proj.LayoutManager.AllLibraries())
            {
                TreeViewItem treelibrary = new TreeViewItem();
                treelibrary.Header = $"{library.LibName}";
                treelibrary.Selected += Treelibrary_Selected;

                bool editable = library.LibName != ProjectLayoutLibraryManager.DEFAULTLIBNAME;

                List<string> foundGroups = new List<string>();

                var groupsToAdd = new List<LayoutGroupTreeItem>();

                foreach (var group in library.AsGroupedLayouts())
                {
                    LayoutGroupTreeItem treegroup = new LayoutGroupTreeItem(group.Group, group.Group, editable, (g) => CreateNewLayoutOnLibrary(library.LibName, g));
                    treegroup.Selected += Treegroup_Selected;
                    foreach (var layout in group.Layouts.OrderBy(x => x.Value.Name)) // order these consistently
                    {
                        LayoutTreeItem treelayoutleaf = new LayoutTreeItem(library.LibName,
                                                                           _proj.LayoutManager.AllLibraries()
                                                                                              .Select(x => x.LibName)
                                                                                              .ToList(),
                                                                           layout.Value.Name,
                                                                           _proj.LayoutManager.GetLayoutSource,
                                                                           group.Group,
                                                                           editable,
                                                                           _proj.LayoutManager.SaveLayoutToLibrary,
                                                                           () => Dispatcher.Invoke(SetupLayoutsTreeVew),
                                                                           (string lib, string group, string layout) =>
                                                                           {
                                                                               _proj.LayoutManager.DeleteLayout(lib, group, layout); Dispatcher.Invoke(SetupLayoutsTreeVew);
                                                                           },
                                                                           _proj.LayoutManager.GetLayoutPreview
                                                                           );
                        treegroup.Items.Add(treelayoutleaf);
                    }

                    treegroup.IsExpanded = oldstate.GetOrDefault($"{library.LibName}.{group.Group}", false);

                    //treelibrary.Items.Add(treegroup);
                    groupsToAdd.Add(treegroup);
                    foundGroups.Add(group.Group);
                }
                // add missing groups
                foreach (var missingGroup in _proj.LayoutManager.FindTypesSupportingLayouts().Where(x => !foundGroups.Contains(x)))
                {
                    LayoutGroupTreeItem treegroup = new LayoutGroupTreeItem(missingGroup, missingGroup, editable, (g) => CreateNewLayoutOnLibrary(library.LibName, g));
                    treegroup.Selected += Treegroup_Selected;

                    treegroup.IsExpanded = oldstate.GetOrDefault($"{library.LibName}.{missingGroup}", false);

                    //treelibrary.Items.Add(treegroup);
                    groupsToAdd.Add(treegroup);
                }

                // add macros editor
                treelibrary.Items.Add(new LibraryMacrosTreeItem("Macros...",
                                                                      library.LibName,
                                                                      true,
                                                                      _proj.LayoutManager.GetLibraryMacros,
                                                                      _proj.LayoutManager.EditLibraryMacros,
                                                                      _proj.LayoutManager.FindAllMacroRefs,
                                                                      _proj.LayoutManager.RenameAllMacroRefs));
                // add groups- but in order!
                foreach (var groupItem in groupsToAdd.OrderBy(x => x.HName))
                {
                    treelibrary.Items.Add(groupItem);
                }


                treelibrary.IsExpanded = oldstate.GetOrDefault(library.LibName, false);

                LayoutsTreeView.Items.Add(treelibrary);
            }

        }

        private Dictionary<string, bool> RememberTreeExpansion(TreeView tv)
        {
            Dictionary<string, bool> NodeState = new Dictionary<string, bool>();

            foreach (TreeViewItem tbLibNode in tv.Items)
            {
                string library = tbLibNode.Header.ToString();
                bool expanded = tbLibNode.IsExpanded;

                NodeState[library] = expanded;

                foreach (TreeViewItem layoutgroupnode in tbLibNode.Items)
                {
                    LayoutGroupTreeItem tvi = layoutgroupnode as LayoutGroupTreeItem;
                    if (tvi != null)
                    {
                        NodeState[$"{library}.{tvi.GroupName}"] = tvi.IsExpanded;
                    }
                }
            }

            return NodeState;
        }

        private string selectedLib = "";
        private string selectedGroup = "";
        private void Treegroup_Selected(object sender, RoutedEventArgs e)
        {
            var item = sender as TreeViewItem;
            selectedGroup = item.Header.ToString();
        }

        private void Treelibrary_Selected(object sender, RoutedEventArgs e)
        {
            var item = sender as TreeViewItem;
            selectedLib = item.Header.ToString();
        }

        private async void Click_ExporLayoutLibrary(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(selectedLib))
            {
                var lib = _proj.LayoutManager.GetLibraryByName(selectedLib);
                if (lib != null)
                {
                    // export 'er real good
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Title = "Export Layout Library";
                    dialog.Filter = "Xenon Layout Library (*.lib.json)|*.lib.json";
                    dialog.AddExtension = true;
                    dialog.DefaultExt = ".lib.json";
                    dialog.FileName = selectedLib;
                    if (dialog.ShowDialog() == true)
                    {
                        await Xenon.SaveLoad.TrustySave.ExportLibrary(VersionInfo.ToString(), lib, new StreamWriter(File.Open(dialog.FileName, FileMode.OpenOrCreate)));
                    }
                }
            }
        }

        private async void Click_ImportLayoutLibrary(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Import Layout Library";
            dialog.Filter = "Xenon Layout Library (*.lib.json)|*.lib.json";
            if (dialog.ShowDialog() == true)
            {
                await Xenon.SaveLoad.TrustySave.ImportLibrary(_proj, dialog.FileName);
                dirty = true;
                ProjectState = ProjectState.Dirty;
                SetupLayoutsTreeVew();
            }
        }

        private void Click_RemoveLayoutLibrary(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(selectedLib))
            {
                _proj.LayoutManager.RemoveLib(selectedLib);
                SetupLayoutsTreeVew();
            }
        }
        private void Click_NewLayoutLibrary(object sender, RoutedEventArgs e)
        {
            TbPromptDialog prompt = new TbPromptDialog("New Library", "Library Name", "User.Library");
            if (prompt.ShowDialog() == true)
            {
                _proj.LayoutManager.InitializeNewLibrary(prompt.ResultValue);
                SetupLayoutsTreeVew();
            }
        }

        private void mi_importoptions_chooseelements_Click(object sender, RoutedEventArgs e)
        {
            // get list of options to filter
            var props = options.Filter.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(BoolSettingAttribute)));

            List<(string name, bool value)> fields = new List<(string name, bool value)>();
            foreach (var prop in props)
            {
                fields.Add((prop.Name, (bool)prop.GetValue(options.Filter)));
            }

            // get user to select them
            CheckboxSelection dialog = new CheckboxSelection("Filter Import Elements", fields.ToArray());
            dialog.ShowDialog();

            int i = 0;
            foreach (var prop in props)
            {
                prop.SetValue(options.Filter, dialog.Fields[i++].value);
            }

        }

        private void mi_importoptions_choosebehaviour_Click(object sender, RoutedEventArgs e)
        {
            // get list of options
            var props = options.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(BoolSettingAttribute)));

            List<(string name, bool value)> fields = new List<(string name, bool value)>();
            foreach (var prop in props)
            {
                fields.Add((prop.Name, (bool)prop.GetValue(options)));
            }

            // get user to select them
            CheckboxSelection dialog = new CheckboxSelection("Import Behaviour", fields.ToArray());
            dialog.ShowDialog();

            int i = 0;
            foreach (var prop in props)
            {
                prop.SetValue(options, dialog.Fields[i++].value);
            }
        }

        private void ClickRenderModeToggle(object sender, RoutedEventArgs e)
        {
            renderinparallel = miRenderMode.IsChecked;
        }

        private void ClickRenderFromClean(object sender, RoutedEventArgs e)
        {
            renderclean = miRenderClean.IsChecked;
        }

        private void FormatCode(object sender, RoutedEventArgs e)
        {
#if DEBUG
            // not quite production ready :(
            //TbInput.Text = XenonFastFormatter.Reformat(TbInput.Text, 4);
            try
            {
                //string formatted = await XenonFastFormatter.CompileReformat(TbInput.Text, 4);
                //string formatted = XenonSimpleFormatter.Format(TbInput.Text);
                Task.Run(() =>
                {
                    string formatted = XenonTopLevelFormatter.FormatTopLevel(TbInput.Text);
                    Dispatcher.Invoke(() => TbInput.Text = formatted);
                });
            }
            catch (Exception)
            {
            }
#endif
        }

        private void ClickAnalyzePilotPresetUse(object sender, RoutedEventArgs e)
        {
            string report = Xenon.Analyzers.PilotReportGenerator.GeneratePilotPresetReport(_proj);
            MessageBox.Show(report, "Presets Used");
        }



        private void HotReload(object sender, RoutedEventArgs e)
        {
            //PublishHotReload();
            hotReloadEnabled = !hotReloadEnabled;

            if (hotReloadEnabled)
            {
                btnImgHotReload.Source = new BitmapImage(new Uri("pack://application:,,,/ViewControls/Images/OrangeFlame.png"));

                hotReloadServer = new HotReloadMonitor();
                hotReloadServer.StartListening();
                hotReloadServer.OnHotReloadConsumed += HotReloadServer_OnHotReloadConsumed;

                PublishHotReload();
            }
            else
            {
                btnImgHotReload.Source = new BitmapImage(new Uri("pack://application:,,,/ViewControls/Images/GreyFlame.png"));

                hotReloadServer?.StopListening();
                hotReloadServer?.Release();
                if (hotReloadServer != null)
                {
                    hotReloadServer.OnHotReloadConsumed -= HotReloadServer_OnHotReloadConsumed;
                    hotReloadServer = null;
                }
            }
        }

        private void HotReloadServer_OnHotReloadConsumed(object sender, EventArgs e)
        {
            sbStatus.Dispatcher.Invoke(() =>
                                               {
                                                   ActionState = ActionState.SuccessBuilding;
                                               });

        }

        bool hotReloadEnabled = false;

        HotReloadMonitor hotReloadServer;

        private void PublishHotReload()
        {
            if (hotReloadServer?.PublishReload(_proj, slides) == true)
            {
                sbStatus.Dispatcher.Invoke(() =>
                                       {
                                           UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = $"Presentation Pushed to Hot Reload", ErrorName = $"Hot Reload Requested", Generator = "Hot Reloader", Inner = "", Level = XenonCompilerMessageType.Message, Token = "" });
                                           ActionState = ActionState.SuccessBuilding_HotReloading;
                                       });
            }
        }

        private void CleanSlides(object sender, RoutedEventArgs e)
        {
            builder.CleanSlides();
        }

        CCUConfigEditor m_ccueditor;
        private void ClickOpenCCUConfigEditor(object sender, RoutedEventArgs e)
        {
            CCPUConfig_Extended cfg = null;
            if (string.IsNullOrEmpty(_proj.SourceCCPUConfigFull))
            {
                cfg = new CCPUConfig_Extended();
            }
            else
            {
                cfg = JsonSerializer.Deserialize<CCPUConfig_Extended>(_proj.SourceCCPUConfigFull);
            }

            if (m_ccueditor == null || m_ccueditor?.WasClosed == true)
            {
                m_ccueditor = new CCUConfigEditor(cfg, SaveCCUConfigChanges);
            }
            m_ccueditor.Show();
        }

        private void ClickLoadCCUFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open CCU Config File";
            ofd.Filter = "json (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(ofd.FileName))
                {
                    string jsonTxt = sr.ReadToEnd();

                    _proj.SourceCCPUConfigFull = jsonTxt;
                }

                var fullCFG = JsonSerializer.Deserialize<CCPUConfig_Extended>(_proj.SourceCCPUConfigFull);
                _proj.CCPUConfig = fullCFG;

                var fakeCFG = SanatizePNGsFromCfg(fullCFG);
                TbConfigCCU.Text = fakeCFG;
            }
        }

        private void SaveCCUConfigChanges(CCPUConfig_Extended cfg)
        {
            // build 2 copies of the JSON file...
            // 1 to display that will ignore the images to improve text loading...
            // 1 true copy to stuff into the project for rendering

            MarkDirty();

            var fakeJsonString = SanatizePNGsFromCfg(cfg);

            Dispatcher.Invoke(() =>
            {
                TbConfigCCU.Text = fakeJsonString;
            });
        }

        private string SanatizePNGsFromCfg(CCPUConfig_Extended cfg)
        {
            var trueCFG = JsonSerializer.Serialize(cfg);
            _proj.SourceCCPUConfigFull = trueCFG;

            var fakeCFG = JsonSerializer.Deserialize<CCPUConfig_Extended>(trueCFG);

            foreach (var mock in fakeCFG.MockPresetInfo)
            {
                foreach (var mockpst in mock.Value)
                {
                    mockpst.Value.Thumbnail = "(base64encoded png)";
                }
            }

            return JsonSerializer.Serialize(fakeCFG, new JsonSerializerOptions { WriteIndented = true });
        }

    }
}
