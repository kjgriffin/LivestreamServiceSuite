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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CreaterEditorWindow : Window
    {
        public CreaterEditorWindow()
        {
            InitializeComponent();
            sbStatus.Background = System.Windows.Media.Brushes.LightGray;
            tbStatusText.Text = "Open Project";
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
                    sbStatus.Background = System.Windows.Media.Brushes.Orange;
                    tbStatusText.Text = $"Compiling Project: {percent}%";
                });
            });

            var rendererprogress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    sbStatus.Background = System.Windows.Media.Brushes.Orange;
                    tbStatusText.Text = $"Rendering Project: {percent}%";
                });
            });


            // compile text
            XenonBuildService builder = new XenonBuildService();
            bool success = await builder.BuildProject(_proj.SourceCode, Assets, compileprogress);

            if (!success)
            {
                sbStatus.Dispatcher.Invoke(() =>
                {
                    sbStatus.Background = System.Windows.Media.Brushes.Crimson;
                    tbStatusText.Text = "Build Failed";
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
            else
            {
                _proj = builder.Project;
            }




            try
            {
                slides = await builder.RenderProject(_proj, rendererprogress);
            }
            catch (Exception ex)
            {
                sbStatus.Dispatcher.Invoke(() =>
                {
                    sbStatus.Background = System.Windows.Media.Brushes.Crimson;
                    tbStatusText.Text = "Render Failed";
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
                sbStatus.Background = System.Windows.Media.Brushes.Green;
                tbStatusText.Text = "Project Rendered";
                foreach (var msg in builder.Messages)
                {
                    tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Renderer Message]: {msg}";
                }
                tbConsole.Text = tbConsole.Text + $"{Environment.NewLine}[Render Succeded]: Slides built!";
            });

            slidelist.Children.Clear();
            slidepreviews.Clear();
            // add all slides to list
            foreach (var slide in slides)
            {
                SlideContentPresenter slideContentPresenter = new SlideContentPresenter();
                slideContentPresenter.Width = slidelist.Width;
                slideContentPresenter.Slide = slide;
                slideContentPresenter.ShowSlide();
                slideContentPresenter.OnSlideClicked += SlideContentPresenter_OnSlideClicked;
                slidelist.Children.Add(slideContentPresenter);
                slidepreviews.Add(slideContentPresenter);
            }

            // update focusslide
            if (slides?.Count > 0)
            {
                FocusSlide.Slide = slides.First();
                FocusSlide.ShowSlide();
                slidepreviews.First().ShowSelected(true);
            }
        }

        Project _proj = new Project(true);
        List<SlideContentPresenter> slidepreviews = new List<SlideContentPresenter>();

        private void SlideContentPresenter_OnSlideClicked(object sender, RenderedSlide slide)
        {
            foreach (var spv in slidepreviews)
            {
                spv.ShowSelected(false);
            }
            FocusSlide.Slide = slide;
            FocusSlide.ShowSlide();
            FocusSlide.PlaySlide();
            this.slide = slides.FindIndex(s => s == slide) + 1;
            if (this.slide >= slides.Count)
            {
                this.slide = 0;
            }
            (sender as SlideContentPresenter).ShowSelected(true);
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
            ofd.Filter = "Images and Video (*.png;*.mp4)|*.png;*.mp4";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                AssetsChanged();
                // add assets
                foreach (var file in ofd.FileNames)
                {
                    // copy to tmp folder
                    string tmpassetpath = System.IO.Path.Combine(_proj.LoadTmpPath, "assets", System.IO.Path.GetFileName(file));
                    File.Copy(file, tmpassetpath);

                    ProjectAsset asset = new ProjectAsset();
                    if (Regex.IsMatch(System.IO.Path.GetExtension(file), "\\.mp4", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), OriginalPath = file, LoadedTempPath = tmpassetpath, Type = AssetType.Video };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file), "\\.png", RegexOptions.IgnoreCase))
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
            sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
            tbStatusText.Text = "Exporting Slides";
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Title = "Select Output Folder";
            ofd.FileName = "Slide";
            if (ofd.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    SlideExporter.ExportSlides(System.IO.Path.GetDirectoryName(ofd.FileName), _proj, new List<XenonCompilerMessage>()); // for now ignore messages
                });
                sbStatus.Background = System.Windows.Media.Brushes.GreenYellow;
                tbStatusText.Text = "Slides Exported";
            }
            else
            {
                sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
                tbStatusText.Text = "Abort Export";
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
                    sbStatus.Background = System.Windows.Media.Brushes.Orange;
                    tbStatusText.Text = $"Saving Project: {percent}%";
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
                    sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
                    tbStatusText.Text = "Project Saved";
                    dirty = false;
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
                sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
                tbStatusText.Text = "Project Saved";
                dirty = false;
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
                slidelist.Children.Clear();
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
                    sbStatus.Background = System.Windows.Media.Brushes.Crimson;
                    tbStatusText.Text = "Failed to Load Project";
                    return;
                }
                sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
                tbStatusText.Text = "Project Saved";
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
                slidelist.Children.Clear();
                slidepreviews.Clear();
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
                    sbStatus.Background = System.Windows.Media.Brushes.Crimson;
                    tbStatusText.Text = "Failed to Load Project";
                    return;
                }
                sbStatus.Background = System.Windows.Media.Brushes.CornflowerBlue;
                tbStatusText.Text = "Project Saved";
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
            slidelist.Children.Clear();
            slidepreviews.Clear();
            Assets.Clear();
            AssetList.Children.Clear();
            FocusSlide.Clear();
            TbInput.Text = string.Empty;
            _proj = new Project();

            sbStatus.Background = System.Windows.Media.Brushes.LightGray;
            tbStatusText.Text = "Empty Project";

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
            sbStatus.Background = System.Windows.Media.Brushes.Brown;
            tbStatusText.Text = "Project Unsaved";
        }

        private void AssetsChanged()
        {
            dirty = true;
            sbStatus.Background = System.Windows.Media.Brushes.Brown;
            tbStatusText.Text = "Project Unsaved";
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
        }

        private void ClickSaveJSON(object sender, RoutedEventArgs e)
        {
            SaveAsJSON();
        }

        private void ClickOpenJSON(object sender, RoutedEventArgs e)
        {
            OpenProjectJSON();
        }
    }
}
