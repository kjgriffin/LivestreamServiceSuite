using SlideCreater.AssetManagment;
using System;
using System.Collections.Generic;
using System.Text;
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
    public delegate void InsertAssetEvent(object sender, ProjectAsset asset);
    /// <summary>
    /// Interaction logic for AssetItemControl.xaml
    /// </summary>
    public partial class AssetItemControl : UserControl
    {


        public event InsertAssetEvent OnFitInsertRequest;

        ProjectAsset Asset;

        public AssetItemControl(ProjectAsset asset)
        {
            InitializeComponent();
            Asset = asset;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            // display name
            AssetName.Text = Asset.Name;
            // load image/video
            ImgAsset.Source = null;
            VideoAsset.Source = null;
            if (Asset.Type == AssetType.Image)
            {
                ImgAsset.Source = new BitmapImage(new Uri(Asset.RelativePath));
            }
            if (Asset.Type == AssetType.Video)
            {
                VideoAsset.Source = new Uri(Asset.RelativePath);
            }

        }

        private void ClickFitInsert(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => OnFitInsertRequest?.Invoke(this, Asset)); 
        }
    }
}
