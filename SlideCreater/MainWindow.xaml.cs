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
        }


        private async void RenderSlides(object sender, RoutedEventArgs e)
        {
            TryAutoSave();
            string text = TbInput.GetAllText();
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

            ActionState = ActionState.Building;

            // compile text
            XenonBuildService builder = new XenonBuildService();
            bool success = await builder.BuildProject(_proj, _proj.SourceCode, Assets, compileprogress);

            if (!success)
            {
                sbStatus.Dispatcher.Invoke(() =>
                {
                    ActionState = ActionState.ErrorBuilding;
                });
                UpdateErrorReport(builder.Messages);
                return;
            }




            try
            {
                slides = await builder.RenderProject(_proj, rendererprogress);
            }
            catch (Exception ex)
            {
                sbStatus.Dispatcher.Invoke(() =>
                {
                    ActionState = ActionState.ErrorBuilding;
                    UpdateErrorReport(builder.Messages, new XenonCompilerMessage() { ErrorMessage = $"Render failed for reason: {ex}", ErrorName = "Render Failure", Generator = "Main Rendering", Inner = "", Level = XenonCompilerMessageType.Error, Token = "" });
                });
                return;
            }

            sbStatus.Dispatcher.Invoke(() =>
            {
                ActionState = ActionState.SuccessBuilding;
                UpdateErrorReport(builder.Messages, new XenonCompilerMessage() { ErrorMessage = "Slides build!", ErrorName = "Render Success", Generator = "Main Rendering", Inner = "", Level = XenonCompilerMessageType.Message, Token = "" });
            });

            UpdatePreviews();

        }

        private void UpdateErrorReport(List<XenonCompilerMessage> messages, XenonCompilerMessage other = null)
        {
            error_report.Dispatcher.Invoke(() =>
            {
                error_report.Children.Clear();
                if (other != null)
                {
                    messages.Insert(0, other);
                }
                foreach (var msg in messages)
                {
                    //Paragraph p = new Paragraph(new Run(msg.ToString()));
                    TextBlock p = new TextBlock();
                    p.Text = msg.ToString();
                    switch (msg.Level)
                    {
                        case XenonCompilerMessageType.Debug:
                            p.Background = System.Windows.Media.Brushes.LightGray;
                            p.Foreground = System.Windows.Media.Brushes.Black;
                            break;
                        case XenonCompilerMessageType.Message:
                            p.Background = System.Windows.Media.Brushes.DeepSkyBlue;
                            p.Foreground = System.Windows.Media.Brushes.Black;
                            break;
                        case XenonCompilerMessageType.Info:
                            p.Background = System.Windows.Media.Brushes.WhiteSmoke;
                            p.Foreground = System.Windows.Media.Brushes.Black;
                            break;
                        case XenonCompilerMessageType.Warning:
                            p.Background = System.Windows.Media.Brushes.Orange;
                            p.Foreground = System.Windows.Media.Brushes.Black;
                            break;
                        case XenonCompilerMessageType.Error:
                            p.Background = System.Windows.Media.Brushes.Red;
                            p.Foreground = System.Windows.Media.Brushes.Black;
                            break;
                        default:
                            break;
                    }
                    error_report.Children.Add(p);
                }
            });
        }

        private void UpdatePreviews()
        {
            slidelist.Items.Clear();
            slidepreviews.Clear();
            // add all slides to list
            foreach (var slide in slides)
            {
                SlideContentPresenter slideContentPresenter = new SlideContentPresenter();
                slideContentPresenter.Width = slidelist.Width;
                slideContentPresenter.Slide = slide;
                slideContentPresenter.Height = 200;
                slideContentPresenter.ShowSlide(previewkeys);
                slidelist.Items.Add(slideContentPresenter);
                slidepreviews.Add(slideContentPresenter);
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
        List<SlideContentPresenter> slidepreviews = new List<SlideContentPresenter>();

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
                try
                {
                    File.Copy(file, tmpassetpath, true);
                }
                catch (Exception)
                {
                    MessageBox.Show($"Unable to load {file}", "Load Asset Failed");
                }

                ProjectAsset asset = new ProjectAsset();
                if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.mp3)|(\.wav)", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Audio };
                }
                else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Image };
                }
                else if (Regex.IsMatch(System.IO.Path.GetExtension(file).ToLower(), @"\.mp4", RegexOptions.IgnoreCase))
                {
                    asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video };
                }



                Assets.Add(asset);
                _proj.Assets.Add(asset);
                ShowProjectAssets();
            }
        }

        private void ShowProjectAssets()
        {
            AssetList.Children.Clear();
            foreach (var asset in _proj.Assets)
            {
                AssetItemControl assetItemCtrl = new AssetItemControl(asset);
                assetItemCtrl.OnFitInsertRequest += AssetItemCtrl_OnFitInsertRequest;
                assetItemCtrl.OnDeleteAssetRequest += AssetItemCtrl_OnDeleteAssetRequest;
                assetItemCtrl.OnAutoFitInsertRequest += AssetItemCtrl_OnAutoFitInsertRequest;
                assetItemCtrl.OnInsertBellsRequest += AssetItemCtrl_OnInsertBellsRequest;
                assetItemCtrl.OnLiturgyInsertRequest += AssetItemCtrl_OnLiturgyInsertRequest;

                AssetList.Children.Add(assetItemCtrl);
            }
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
            TbInput.InsertLinesAfterCursor(InsertCommand.Split(Environment.NewLine));
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
            TbInput.InsertLinesAfterCursor(InsertCommand.Split(Environment.NewLine));
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

        private void ClickSave(object sender, RoutedEventArgs e)
        {
            SaveProject();
        }

        private async void SaveProject()
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

            _proj.SourceCode = TbInput.GetAllText();
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
            _proj.SourceCode = TbInput.GetAllText();
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


        private void OpenProjectJSON()
        {
            if (dirty)
            {
                if (!CheckSaveChanges())
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
                slidepreviews.Clear();
                _proj = Project.Load(ofd.FileName);
                TbInput.SetText(_proj.SourceCode);
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

        private async void OpenProject()
        {
            if (dirty)
            {
                if (!CheckSaveChanges())
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
                slidepreviews.Clear();
                FocusSlide.Clear();
                _proj.CleanupResources();
                _proj = await Project.LoadProject(ofd.FileName);
                TbInput.SetText(_proj.SourceCode);
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


        private void NewProject()
        {
            if (dirty)
            {
                if (!CheckSaveChanges())
                {
                    return;
                }
            }
            dirty = false;
            slidelist.Items.Clear();
            slidepreviews.Clear();
            Assets.Clear();
            AssetList.Children.Clear();
            FocusSlide.Clear();
            TbInput.SetText(string.Empty);
            _proj.CleanupResources();
            _proj = new Project(true);

            ProjectState = ProjectState.NewProject;
            ActionState = ActionState.Ready;

        }

        /// <summary>
        /// Return true if should proceed, false to abort
        /// </summary>
        /// <returns></returns>
        private bool CheckSaveChanges()
        {
            var res = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButton.YesNoCancel);
            if (res == MessageBoxResult.Yes)
            {
                SaveProject();
                return true;
            }
            else if (res == MessageBoxResult.No)
            {
                return true;
            }
            return false;
        }

        private void ClickOpen(object sender, RoutedEventArgs e)
        {
            OpenProject();
        }

        private void ClickNew(object sender, RoutedEventArgs e)
        {
            NewProject();
        }

        private bool dirty = false;
        //private void SourceTextChanged(object sender, TextChangedEventArgs e)
        //{
        //}

        private void AssetsChanged()
        {
            dirty = true;
            ProjectState = ProjectState.Dirty;
            ActionState = ActionState.Ready;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dirty)
            {
                if (!CheckSaveChanges())
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
            slidepreviews.Clear();
            TryAutoSave();
        }

        private void ClickSaveJSON(object sender, RoutedEventArgs e)
        {
            SaveAsJSON();
        }

        private void ClickOpenJSON(object sender, RoutedEventArgs e)
        {
            OpenProjectJSON();
        }

        private void slidelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SlideContentPresenter scp = (SlideContentPresenter)slidelist.SelectedItem;
            if (scp != null)
            {
                SelectSlideToPreview(sender, scp.Slide);
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
            // create a simple json and store it in temp files
            string tmppath = System.IO.Path.Join(System.IO.Path.GetTempPath(), "slidecreaterautosaves");
            string filename = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.autosave.json";
            string fullpath = System.IO.Path.Join(tmppath, filename);
            AutoSave save = new AutoSave();
            foreach (var asset in Assets)
            {
                save.SourceAssets.Add(asset.OriginalPath);
            }
            save.SourceCode = TbInput.GetAllText();
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

        private void RecoverAutoSave(object sender, RoutedEventArgs e)
        {
            // open tmp files folder and let 'em select an autosave
            string tmppath = System.IO.Path.GetTempPath();
            // check if we have a slidecreaterautosaves folder
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Recover Autosave";
            ofd.Filter = "Autosaves (*.json)|*.json";
            ofd.Multiselect = false;
            if (Directory.Exists(System.IO.Path.Join(tmppath, "slidecreaterautosaves")))
            {
                ofd.InitialDirectory = System.IO.Path.Join(tmppath, "slidecreaterautosaves");
            }
            if (ofd.ShowDialog() == true)
            {
                // then load it in
                try
                {
                    using (TextReader reader = new StreamReader(ofd.FileName))
                    {
                        string json = reader.ReadToEnd();
                        AutoSave save = JsonSerializer.Deserialize<AutoSave>(json);
                        OverwriteWithAutoSave(save);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to recover autosave: {ex}", "Recovery Failed");
                }
            }
        }

        private void OverwriteWithAutoSave(AutoSave save)
        {
            NewProject();
            // load assets
            AddAssetsFromPaths(save.SourceAssets);
            TbInput.SetText(save.SourceCode);
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
                parser.CompileToXenon();
                ActionState = ActionState.Downloading;
                await parser.LoadWebAssets(_proj.CreateImageAsset);
                AssetsChanged();
                ShowProjectAssets();
                ActionState = ActionState.Ready;
                TbInput.SetText("/*\r\n" + parser.XenonDebug() + "*/\r\n" + parser.XenonText);
            }
        }

        private void SourceTextChanged(object sender, TextChangedEventArgs e)
        {
            dirty = true;
            ProjectState = ProjectState.Dirty;
            ActionState = ActionState.Ready;
        }
    }
}
