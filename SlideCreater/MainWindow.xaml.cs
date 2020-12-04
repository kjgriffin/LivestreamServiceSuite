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

using Xenon.Compiler;
using Xenon.Renderer;
using Xenon.Helpers;
using Xenon.SlideAssembly;
using Xenon.AssetManagment;

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


        public CreaterEditorWindow()
        {
            InitializeComponent();
            dirty = false;
            ProjectState = ProjectState.NewProject;
            ActionState = ActionState.Ready;
        }


        private async void RenderSlides(object sender, RoutedEventArgs e)
        {

            string text = TbInput.Text;
            _proj.SourceCode = text;

            tbConsole.Text = string.Empty;


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
                tbConsole.Dispatcher.Invoke(() =>
                {
                    foreach (var msg in builder.Messages)
                    {
                        tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Render Failed]: {msg}";
                    }
                });
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
                    tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Render Failed]: Unknown Reason? {ex}";
                    foreach (var err in builder.Messages)
                    {
                        tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Renderer Error]: {err}";
                    }
                });
                return;
            }

            sbStatus.Dispatcher.Invoke(() =>
            {
                ActionState = ActionState.SuccessBuilding;
                foreach (var msg in builder.Messages)
                {
                    tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Renderer Message]: {msg}";
                }
                tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Render Succeded]: Slides built!";
            });

            slidelist.Items.Clear();
            slidepreviews.Clear();
            // add all slides to list
            foreach (var slide in slides)
            {
                SlideContentPresenter slideContentPresenter = new SlideContentPresenter();
                slideContentPresenter.Width = slidelist.Width;
                slideContentPresenter.Slide = slide;
                slideContentPresenter.Height = 200;
                slideContentPresenter.ShowSlide();
                slidelist.Items.Add(slideContentPresenter);
                slidepreviews.Add(slideContentPresenter);
            }

            // update focusslide
            if (slides?.Count > 0)
            {
                FocusSlide.Slide = slides.First();
                FocusSlide.ShowSlide();
                slidelist.SelectedIndex = 0;
            }


        }

        Project _proj = new Project(true);
        List<SlideContentPresenter> slidepreviews = new List<SlideContentPresenter>();

        private void SelectSlideToPreview(object sender, RenderedSlide slide)
        {
            FocusSlide.Slide = slide;
            FocusSlide.ShowSlide();
            FocusSlide.PlaySlide();
            this.slide = slides.FindIndex(s => s == slide) + 1;
            if (this.slide >= slides.Count)
            {
                this.slide = 0;
            }
        }

        List<RenderedSlide> slides = new List<RenderedSlide>();

        int slide = 0;

        private void Show(object sender, RoutedEventArgs e)
        {
            FocusSlide.Slide = slides[slide];
            FocusSlide.ShowSlide();
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
            ofd.Filter = "Images and Video (*.png;*.jpg;*.bmp;*.mp4)|*.png;*.jpg;*.bmp;*.mp4";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                AssetsChanged();
                // add assets
                foreach (var file in ofd.FileNames)
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
                    if (Regex.IsMatch(System.IO.Path.GetExtension(file), "\\.mp4", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file), @"(\.png)|(\.jpg)|(\.bmp)", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Image };
                    }


                    Assets.Add(asset);
                    _proj.Assets = Assets;
                    ShowProjectAssets();
                }
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
                assetItemCtrl.OnLiturgyInsertRequest += AssetItemCtrl_OnLiturgyInsertRequest;

                AssetList.Children.Add(assetItemCtrl);
            }
        }

        private void AssetItemCtrl_OnLiturgyInsertRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = $"\r\n#litimage({asset.Name})\r\n";
            int newindex = TbInput.CaretIndex + InsertCommand.Length;
            TbInput.Text = TbInput.Text.Insert(TbInput.CaretIndex, InsertCommand);
            TbInput.CaretIndex = newindex;
        }

        private void AssetItemCtrl_OnAutoFitInsertRequest(object sender, ProjectAsset asset)
        {
            string InsertCommand = $"\r\n#autofitimage({asset.Name})\r\n";
            int newindex = TbInput.CaretIndex + InsertCommand.Length;
            TbInput.Text = TbInput.Text.Insert(TbInput.CaretIndex, InsertCommand);
            TbInput.CaretIndex = newindex;
        }

        private void AssetItemCtrl_OnDeleteAssetRequest(object sender, ProjectAsset asset)
        {
            AssetsChanged();
            Assets.Remove(asset);
            _proj.Assets = Assets;
            ShowProjectAssets();
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
            int newindex = TbInput.CaretIndex + InsertCommand.Length;
            TbInput.Text = TbInput.Text.Insert(TbInput.CaretIndex, InsertCommand);
            TbInput.CaretIndex = newindex;
        }

        private async void ExportSlides(object sender, RoutedEventArgs e)
        {
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
            TbInput.Text = string.Empty;
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
        private void SourceTextChanged(object sender, TextChangedEventArgs e)
        {
            dirty = true;
            ProjectState = ProjectState.Dirty;
            ActionState = ActionState.Ready;
        }

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

    }
}
