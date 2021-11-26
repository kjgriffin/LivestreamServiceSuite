using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using Xenon.Helpers;
using Xenon.SlideAssembly;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for LayoutDesigner.xaml
    /// </summary>
    public partial class LayoutDesigner : Window
    {

        private string LayoutName { get; set; }
        private string LayoutJson { get; set; }

        readonly string Group;

        SaveLayoutToLibrary Save;

        DispatcherTimer textChangeTimeoutTimer = new DispatcherTimer();
        bool stillChanging = false;

        Action UpdateCallback;

        public LayoutDesigner(string layoutname, string layoutjson, string group, SaveLayoutToLibrary save, Action updateCallback)
        {
            InitializeComponent();
            textChangeTimeoutTimer.Interval = TimeSpan.FromSeconds(1);
            LayoutName = layoutname;
            LayoutJson = layoutjson;
            Group = group;
            TbJson.Text = LayoutJson;
            tbnameorig.Text = LayoutName;
            tbnameorig1.Text = LayoutName;
            tbName.Text = $"{LayoutName}-Copy";

            Save = save;
            UpdateCallback = updateCallback;

            ShowPreviews(layoutjson);
        }

        private void ShowPreviews(string layoutjson)
        {
            var r = ProjectLayoutLibraryManager.GetLayoutPreview(Group, layoutjson);
            ImgMain.Source = r.main.ConvertToBitmapImage();
            ImgKey.Source = r.key.ConvertToBitmapImage();
        }

        private async void SourceTextChanged(object sender, EventArgs e)
        {
            stillChanging = true;
            if (!textChangeTimeoutTimer.IsEnabled)
            {
                textChangeTimeoutTimer.Start();
            }
            await ReRender();
        }

        private async Task ReRender()
        {
            while (stillChanging)
            {
                await Task.Delay(1000);
                stillChanging = false;
            }
            try
            {
                ShowPreviews(TbJson.Text);
            }
            catch (Exception)
            {
                // warn for invalid json
            }
        }

        private void Click_SaveAs(object sender, RoutedEventArgs e)
        {
            Save?.Invoke("User.Library", tbName.Text, Group, TbJson.Text);
            UpdateCallback?.Invoke();
        }
    }
}
