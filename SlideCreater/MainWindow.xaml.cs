using Microsoft.Win32;
using SlideCreater.AssetManagment;
using SlideCreater.Compiler;
using SlideCreater.Renderer;
using SlideCreater.SlideAssembly;
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
        }

        private async void RenderSlides(object sender, RoutedEventArgs e)
        {

            string text = TbInput.Text;

            await Task.Run(() =>
            {

                // compile text
                XenonCompiler compiler = new XenonCompiler();
                _proj = compiler.Compile(text, Assets);

                SlideRenderer sr = new SlideRenderer(_proj);


                slides.Clear();
                for (int i = 0; i < _proj.Slides.Count; i++)
                {
                    slides.Add(sr.RenderSlide(i));
                }
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
        }

        Project _proj = new Project();
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
                // add assets
                foreach (var file in ofd.FileNames)
                {
                    ProjectAsset asset = new ProjectAsset();
                    if (Regex.IsMatch(System.IO.Path.GetExtension(file), "\\.mp4", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), RelativePath = file, Type = AssetType.Video };
                    }
                    else if (Regex.IsMatch(System.IO.Path.GetExtension(file), "\\.png", RegexOptions.IgnoreCase))
                    {
                        asset = new ProjectAsset() { Id = Guid.NewGuid(), Name = System.IO.Path.GetFileNameWithoutExtension(file), RelativePath = file, Type = AssetType.Image };
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

                AssetList.Children.Add(assetItemCtrl);
            }
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
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Title = "Select Output Folder";
            ofd.FileName = "Slide";
            if (ofd.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    SlideExporter.ExportSlides(System.IO.Path.GetDirectoryName(ofd.FileName), _proj);
                });
            }
        }
    }
}
