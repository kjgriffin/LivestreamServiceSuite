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

            TbInput.LoadLanguage_XENON();
            TbInput.TextArea.TextEntered += TextArea_TextEntered;
            TbInput.TextArea.TextEntering += TextArea_TextEntering;
            TbInput.TextArea.PreviewTextInput += TextArea_PreviewTextInput;

            dirty = false;
            ProjectState = ProjectState.NewProject;
            ActionState = ActionState.Ready;
            // Load Version Number
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SlideCreater.version.json");
            using (StreamReader sr = new StreamReader(stream))
            {
                string json = sr.ReadToEnd();
                VersionInfo = JsonSerializer.Deserialize<BuildVersion>(json);
            }
            // Set title
            Title = $"Slide Creater - {VersionInfo.MajorVersion}.{VersionInfo.MinorVersion}.{VersionInfo.Revision}.{VersionInfo.Build}-{VersionInfo.Mode}";

            // enable optional features
            EnableDisableOptionalFeatures();

            SetupLayoutsTreeVew();
        }


        private void EnableDisableOptionalFeatures()
        {
            if (VersionInfo.Mode != "Debug")
            {
                miimport_adv.Visibility = Visibility.Collapsed;
                miimport_adv.IsEnabled = false;
            }
        }


        private async void RenderSlides(object sender, RoutedEventArgs e)
        {
            TryAutoSave();
            if (!m_agressive_autosave_enabled)
            {
                // always do it pre-render just to be safe
                await TryFullAutoSave();
            }
            string text = TbInput.Text;
            _proj.SourceCode = text;

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
            XenonBuildService builder = new XenonBuildService();
            //bool success = await builder.BuildProject(_proj, _proj.SourceCode, Assets, compileprogress);
            builder.Messages.Clear();

            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Compilation Started", ErrorName = "Compilation Started", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Debug, Token = "" });
            sbStatus.Dispatcher.Invoke(() =>
            {
                ActionState = ActionState.Building;
            });


            var build = await builder.BuildProjectAsync(_proj, compileprogress);

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
            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Compiled Successfully!", ErrorName = "Project Compiled", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Message, Token = "" });


            //try
            //{
            //slides = await builder.RenderProject(_proj, rendererprogress);
            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Render Starting", ErrorName = "Render Starting", Generator = "Compile/Generate", Inner = "", Level = XenonCompilerMessageType.Debug, Token = "" });
            slides = (await builder.RenderProjectAsync(build.project, rendererprogress, renderinparallel)).OrderBy(s => s.Number).ToList();


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

            sbStatus.Dispatcher.Invoke(() =>
            {
                ActionState = ActionState.SuccessBuilding;
                UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorMessage = "Slides built!", ErrorName = "Render Success", Generator = "Main Rendering", Inner = "", Level = XenonCompilerMessageType.Message, Token = "" });
            });

            UpdatePreviews();

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
            slidelist.Items.Clear();
            //slidepreviews.Clear();
            previews.Clear();
            // add all slides to list
            foreach (var slide in slides)
            {
                /*
                SlideContentPresenter slideContentPresenter = new SlideContentPresenter();
                slideContentPresenter.Width = slidelist.Width;
                slideContentPresenter.Slide = slide;
                slideContentPresenter.Height = 200;
                slideContentPresenter.ShowSlide(previewkeys);
                slidelist.Items.Add(slideContentPresenter);
                slidepreviews.Add(slideContentPresenter);
                */
                MegaSlidePreviewer previewer = new MegaSlidePreviewer();
                previewer.Width = slidelist.Width;
                previewer.Slide = slide;
                slidelist.Items.Add(previewer);
                previews.Add(previewer);
            }

            // update focusslide
            if (slides?.Count > 0)
            {
                FocusSlide.Slide = slides.First();
                FocusSlide.ShowSlide(previewkeys);
                slidelist.SelectedIndex = 0;
                tbSlideCount.Text = $"Slides: {slides.First().Number + 1}/{slides.Count()}";
            }
            else
            {
                tbSlideCount.Text = $"Slides: {slides.Count()}";
            }

        }

        Project _proj = new Project(true);
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


        List<ProjectAsset> Assets = new List<ProjectAsset>();
        private void ClickAddAssets(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Add Assets";
            ofd.Filter = "Images, Video and Audio (*.png;*.jpg;*.bmp;*.mp4;*.mp3;*.wav)|*.png;*.jpg;*.bmp;*.mp4;*.mp3;*.wav";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                AddAssetsFromPaths(ofd.FileNames);
            }
            TryAutoSave();
        }

        private void AddAssetsFromPaths(IEnumerable<string> filenames)
        {
            AssetsChanged();
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



                Assets.Add(asset);
                _proj.Assets = Assets;
                ShowProjectAssets();
            }
        }

        private void AddNamedAssetsFromPaths(IEnumerable<(string path, string name)> assets)
        {
            AssetsChanged();
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



                Assets.Add(asset);
                _proj.Assets.Add(asset);
                ShowProjectAssets();
            }

        }

        private void ShowProjectAssets()
        {
            AssetList.Children.Clear();

            bool loaderrors = false;
            List<string> missingassets = new List<string>();

            foreach (var asset in _proj.Assets)
            {
                try
                {
                    AssetItemControl assetItemCtrl = new AssetItemControl(asset);
                    assetItemCtrl.OnFitInsertRequest += AssetItemCtrl_OnFitInsertRequest;
                    assetItemCtrl.OnDeleteAssetRequest += AssetItemCtrl_OnDeleteAssetRequest;
                    assetItemCtrl.OnAutoFitInsertRequest += AssetItemCtrl_OnAutoFitInsertRequest;
                    assetItemCtrl.OnInsertBellsRequest += AssetItemCtrl_OnInsertBellsRequest;
                    assetItemCtrl.OnLiturgyInsertRequest += AssetItemCtrl_OnLiturgyInsertRequest;
                    assetItemCtrl.OnRenameAssetRequest += AssetItemCtrl_OnRenameAssetRequest;
                    AssetList.Children.Add(assetItemCtrl);
                }
                catch (FileNotFoundException ex)
                {
                    loaderrors = true;
                    missingassets.Add(asset.Name);
                }
            }

            if (loaderrors)
            {
                throw new Exception($"Missing ({missingassets.Count}) asset(s): {missingassets.Aggregate((agg, item) => agg + ", '" + item + "'")}");
            }
        }

        private void AssetItemCtrl_OnRenameAssetRequest(object sender, ProjectAsset asset, string newname)
        {
            AssetsChanged();
            asset.Name = newname;

            ShowProjectAssets();
            TryAutoSave();
        }

        private void AssetItemCtrl_OnInsertBellsRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = $"\r\n#resource(\"{asset.Name}\", \"audio\")\r\n\r\n#script {{\r\n#Worship Bells;\r\n@arg0:OpenAudioPlayer;\r\n@arg1:LoadAudioFile(Resource_{asset.OriginalFilename})[Load Bells];\r\n@arg1:PresetSelect(7)[Preset Center];\r\n@arg1:DelayMs(100);\r\n@arg0:AutoTrans[Take Center];\r\n@arg1:DelayMs(2000);\r\n@arg0:PlayAuxAudio[Play Bells];\r\narg1:PresetSelect(8)[Preset Pulpit];\r\n}}\r\n";
            InsertTextCommand(InsertCommand);
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
            AssetsChanged();
            Assets.Remove(asset);
            _proj.Assets = Assets;
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
            if (ofd.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    SlideExporter.ExportSlides(System.IO.Path.GetDirectoryName(ofd.FileName), _proj, new List<XenonCompilerMessage>()); // for now ignore messages
                });
                ActionState = ActionState.SuccessExporting;
            }
            else
            {
                ActionState = ActionState.ErrorExporting;
            }
        }

        private async void ClickSave(object sender, RoutedEventArgs e)
        {
            await SaveProject();
        }

        private async Task SaveProject()
        {
            TryAutoSave();
            var saveprogress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    ActionState = ActionState.Saving;
                    tbSubActionStatus.Text = $"Saving Project: {percent}%";
                    pbActionStatus.Value = percent;
                });
            });

            _proj.SourceCode = TbInput.Text;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Project";
            //sfd.DefaultExt = "json";
            sfd.DefaultExt = "zip";
            sfd.AddExtension = false;
            sfd.FileName = $"Service_{DateTime.Now:yyyyMMdd}";
            if (sfd.ShowDialog() == true)
            {
                await _proj.SaveProject(sfd.FileName, saveprogress);
                Dispatcher.Invoke(() =>
                {
                    dirty = false;
                    ProjectState = ProjectState.Saved;
                    ActionState = ActionState.Ready;
                });
            }
        }

        private void SaveAsJSON()
        {
            TryAutoSave();
            _proj.SourceCode = TbInput.Text;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Project";
            sfd.DefaultExt = "json";
            sfd.AddExtension = true;
            sfd.FileName = $"Service_{DateTime.Now:yyyyMMdd}";
            if (sfd.ShowDialog() == true)
            {
                _proj.Save(sfd.FileName);
                dirty = false;
                ProjectState = ProjectState.Saved;
                ActionState = ActionState.Ready;
            }
        }


        private async Task OpenProjectJSON()
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Project";
            ofd.DefaultExt = "json";
            ofd.FileName = $"Service_{DateTime.Now:yyyyMMdd}";
            if (ofd.ShowDialog() == true)
            {
                Assets.Clear();
                slidelist.Items.Clear();
                //slidepreviews.Clear();
                previews.Clear();
                _proj = Project.Load(ofd.FileName);
                TbInput.Text = _proj.SourceCode;
                Assets = _proj.Assets;
                try
                {
                    ShowProjectAssets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load project assets");
                    Assets.Clear();
                    _proj.Assets.Clear();
                    ProjectState = ProjectState.LoadError;
                    return;
                }
                dirty = false;
                ProjectState = ProjectState.Saved;
                ActionState = ActionState.Ready;
            }

        }

        private async Task OpenProject()
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Project";
            ofd.DefaultExt = "zip";
            if (ofd.ShowDialog() == true)
            {
                Assets.Clear();
                slidelist.Items.Clear();
                //slidepreviews.Clear();
                previews.Clear();
                FocusSlide.Clear();
                _proj.CleanupResources();
                _proj = await Project.LoadProject(ofd.FileName);
                TbInput.Text = _proj.SourceCode;
                Assets = _proj.Assets;
                try
                {
                    ShowProjectAssets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Failed to load project assets");
                    Assets.Clear();
                    _proj.Assets.Clear();
                    ProjectState = ProjectState.LoadError;
                    return;
                }
                dirty = false;
                ProjectState = ProjectState.Saved;
                ActionState = ActionState.Ready;
            }
            UpdateErrorReport(new List<XenonCompilerMessage>());

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
            Assets.Clear();
            AssetList.Children.Clear();
            FocusSlide.Clear();
            TbInput.Text = string.Empty;
            _proj.CleanupResources();
            _proj = new Project(true);
            _proj.Assets = Assets;

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

        private async void ClickOpen(object sender, RoutedEventArgs e)
        {
            await OpenProject();
        }

        private async void ClickNew(object sender, RoutedEventArgs e)
        {
            await NewProject();
        }

        private bool dirty = false;

        private void AssetsChanged()
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
                }
            }
        }

        private void ClickClearAssets(object sender, RoutedEventArgs e)
        {
            Assets.Clear();
            _proj.Assets = Assets;
            ShowProjectAssets();
            AssetsChanged();
            FocusSlide.Clear();
            //slidepreviews.Clear();
            previews.Clear();
            TryAutoSave();
        }

        private void ClickSaveJSON(object sender, RoutedEventArgs e)
        {
            SaveAsJSON();
        }

        private async void ClickOpenJSON(object sender, RoutedEventArgs e)
        {
            await OpenProjectJSON();
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
            foreach (var asset in Assets)
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
                AddAssetsFromPaths(save.SourceAssets);
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
                        Uri uriResult;
                        bool result = Uri.TryCreate(file, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (result)
                        {
                            // try downloading it
                            WebClient client = new WebClient();
                            await client.DownloadFileTaskAsync(uriResult, System.IO.Path.Combine(_proj.LoadTmpPath, assetkey));
                        }
                        else
                        {
                            // hmmmm... file doesn't exist and we can't fall back to download
                            UpdateErrorReport(alllogs, new XenonCompilerMessage() { ErrorName = "Auto Recover Asset Failed", ErrorMessage = $"Tried to recover asset with name '{assetkey}' from original source '{file}'. File does not exist on disk, and could not be found online.", Generator = "Recover Auto Save: Overwrite Project()", Inner = "", Level = XenonCompilerMessageType.Error, Token = "" });
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
                    Assets.Add(asset);
                    _proj.Assets.Add(asset);
                    ShowProjectAssets();
                }
                pbActionStatus.Value = 0;
                pbActionStatus.Visibility = Visibility.Hidden;
            }
            TbInput.Text = save.SourceCode;
        }

        private async void ClickImportService(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import from save of Lutheran Service Bulletin";
            ofd.Filter = "LSB Service (*.html)|*.html";
            if (ofd.ShowDialog() == true)
            {
                ActionState = ActionState.Building;
                LutheRun.LSBParser parser = new LutheRun.LSBParser();
                await parser.ParseHTML(ofd.FileName);
                parser.Serviceify();
                parser.CompileToXenon();
                ActionState = ActionState.Downloading;
                await parser.LoadWebAssets(_proj.CreateImageAsset);
                Assets = _proj.Assets;
                AssetsChanged();
                ShowProjectAssets();
                ActionState = ActionState.Ready;
                //TbInput.SetText("/*\r\n" + parser.XenonDebug() + "*/\r\n" + parser.XenonText);
                TbInput.Text = parser.XenonText;
            }
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import from save of Lutheran Service Bulletin";
            ofd.Filter = "LSB Service (*.html)|*.html";
            if (ofd.ShowDialog() == true)
            {
                WebViewUI webViewUI = new WebViewUI(ofd.FileName, CreateProjectFromImportWizard);
                webViewUI.Show();
            }
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
            sfd.DefaultExt = "zip";
            sfd.AddExtension = false;
            sfd.FileName = $"Service_{DateTime.Now:yyyyMMdd}.trusty";
            if (sfd.ShowDialog() == true)
            {
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
            _proj = new Project(true);

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
                Dispatcher.Invoke(() =>
                {
                    TbInput.Text = sourcecode;
                });
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
                            _proj.ProjectLayouts.LoadLibrary(lib);

                        }
                    }
                }



                archive.ExtractToDirectory(_proj.LoadTmpPath);
            }

            // add assets
            Dispatcher.Invoke(() =>
            {
                AssetsChanged();
                ActionState = ActionState.Downloading;
            });
            double percent = 0;
            int total = assetfilemap.Count;
            int progress = 1;

            foreach (var assetkey in assetfilemap.Keys)
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
                    var file = assetfilemap[assetkey];
                    var tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, file);
                    ProjectAsset asset = new ProjectAsset();
                    if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.mp3)|(\.wav)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Audio, InternalDisplayName = assetdisplaynames.GetOrDefault(assetkey, assetkey), Extension = assetextensions[assetkey] };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Image, InternalDisplayName = assetdisplaynames.GetOrDefault(assetkey, assetkey), Extension = assetextensions[assetkey] };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"\.mp4", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = assetkey, OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video, InternalDisplayName = assetdisplaynames.GetOrDefault(assetkey, assetkey), Extension = assetextensions[assetkey] };
                    }
                    Assets.Add(asset);
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

        private void ClickCBAutoSave(object sender, RoutedEventArgs e)
        {
            m_agressive_autosave_enabled = !m_agressive_autosave_enabled;
            cbAgressiveSave.IsChecked = m_agressive_autosave_enabled;
        }

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
            var suggestions = XenonSuggestionService.GetSuggestions(TbInput.Text, TbInput.CaretOffset);
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
                ShowSuggestions();
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

        bool renderinparallel = false;
        private void ClickCBRenderParallel(object sender, RoutedEventArgs e)
        {
            renderinparallel = !renderinparallel;
            if (renderinparallel)
            {
                cbRenderParallel.IsChecked = renderinparallel;
            }
        }








        private void SetupLayoutsTreeVew()
        {
            LayoutsTreeView.Items.Clear();

            foreach (var library in _proj.ProjectLayouts.GetAllLibraryLayoutsByGroup())
            {
                TreeViewItem treelibrary = new TreeViewItem();
                treelibrary.Header = $"{library.LibraryName}";
                treelibrary.Selected += Treelibrary_Selected;

                foreach (var group in library.Library)
                {
                    TreeViewItem treegroup = new TreeViewItem();
                    treegroup.Header = $"{group.group}";
                    treegroup.Selected += Treegroup_Selected;
                    foreach (var layout in group.layouts)
                    {
                        LayoutTreeItem treelayoutleaf = new LayoutTreeItem(library.LibraryName, layout.Key, layout.Value, group.group, _proj.ProjectLayouts.SaveLayoutToLibrary, () => Dispatcher.Invoke(SetupLayoutsTreeVew));
                        treegroup.Items.Add(treelayoutleaf);
                    }
                    treelibrary.Items.Add(treegroup);
                }


                LayoutsTreeView.Items.Add(treelibrary);
            }

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

        private void Click_ExporLayoutLibrary(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(selectedLib))
            {
                // export 'er real good
            }
        }

        private void Click_ImportLayoutLibrary(object sender, RoutedEventArgs e)
        {

        }

        private void Click_RemoveLayoutLibrary(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(selectedLib))
            {
                _proj.ProjectLayouts.RemoveLib(selectedLib);
                SetupLayoutsTreeVew();
            }
        }
        private void Click_NewLayoutLibrary(object sender, RoutedEventArgs e)
        {
            TbPromptDialog prompt = new TbPromptDialog("New Library", "Library Name", "User.Library");
            if (prompt.ShowDialog() == true)
            {
                _proj.ProjectLayouts.InitializeNewLibrary(prompt.ResultValue);
                SetupLayoutsTreeVew();
            }
        }
    }
}
