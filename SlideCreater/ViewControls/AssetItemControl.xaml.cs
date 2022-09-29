using SlideCreater.ViewControls;

using System;
using System.Collections.Generic;
using System.IO;
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

using Xenon.AssetManagment;

namespace SlideCreater
{
    public delegate void InsertAssetEvent(object sender, ProjectAsset asset);
    public delegate void DeleteAssetEvent(object sender, ProjectAsset asset);
    public delegate void RenameAssetEvent(object sender, ProjectAsset asset, string newname);
    /// <summary>
    /// Interaction logic for AssetItemControl.xaml
    /// </summary>
    public partial class AssetItemControl : UserControl
    {


        public event InsertAssetEvent OnFitInsertRequest;
        public event InsertAssetEvent OnAutoFitInsertRequest;
        public event InsertAssetEvent OnLiturgyInsertRequest;
        public event InsertAssetEvent OnInsertResourceRequest;
        public event DeleteAssetEvent OnDeleteAssetRequest;
        public event RenameAssetEvent OnRenameAssetRequest;

        ProjectAsset Asset;

        public string AssetGroup { get => Asset.Group; }

        public AssetItemControl(ProjectAsset asset)
        {
            InitializeComponent();
            Asset = asset;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            // display name
            AssetName.Text = Asset.DisplayName;
            ttName.Content = Asset.DisplayName;
            // load image/video
            ImgAsset.Source = null;
            VideoAsset.Source = null;
            lbresource.Visibility = Visibility.Visible;
            if (Asset.Type == AssetType.Image)
            {
                ImgAsset.Source = new BitmapImage(new Uri(Asset.CurrentPath));
                lbhymn.Visibility = Visibility.Visible;
                lbliturgy.Visibility = Visibility.Visible;
                tbunloaded.Visibility = Visibility.Hidden;
            }
            if (Asset.Type == AssetType.Video)
            {
                VideoAsset.Source = new Uri(Asset.CurrentPath);
                VideoAsset.MediaEnded += VideoAsset_MediaEnded;
                VideoAsset.Volume = 0;
                VideoAsset.Play();
                lbhymn.Visibility = Visibility.Hidden;
                lbliturgy.Visibility = Visibility.Hidden;
                tbunloaded.Visibility = Visibility.Hidden;
            }
            if (Asset.Type == AssetType.Audio)
            {
                ImgAsset.Source = new BitmapImage(new Uri("pack://application:,,,/ViewControls/Images/musicnote.png"));
                lbhymn.Visibility = Visibility.Hidden;
                lbliturgy.Visibility = Visibility.Hidden;
                lbinsert.Visibility = Visibility.Hidden;
                tbunloaded.Visibility = Visibility.Hidden;
            }

        }

        private void VideoAsset_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoAsset.Stop();
            VideoAsset.Play();
        }

        private void ClickFitInsert(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => OnFitInsertRequest?.Invoke(this, Asset));
        }

        private void ClickDeleteAsset(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => OnDeleteAssetRequest?.Invoke(this, Asset));
        }

        private void ClickAutoFitInsert(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => OnAutoFitInsertRequest?.Invoke(this, Asset));
        }

        private void ClickLiturgyInsert(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => OnLiturgyInsertRequest?.Invoke(this, Asset));
        }

        private void ClickAddAsResource(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => OnInsertResourceRequest?.Invoke(this, Asset));
        }

        private void ClickRenameAsset(object sender, RoutedEventArgs e)
        {
            RenameDialog renameDialog = new RenameDialog(Asset.Name);
            renameDialog.ShowDialog();
            if (renameDialog.Result)
            {
                Dispatcher.Invoke(() => OnRenameAssetRequest(this, Asset, renameDialog.NewName));
            }
        }
    }
}
