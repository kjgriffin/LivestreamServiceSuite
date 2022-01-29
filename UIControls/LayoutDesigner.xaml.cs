using System;
using System.Collections.Generic;
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

        private string SaveToLayoutName;
        private string SaveToLayoutLibrary;
        private string LayoutJson { get; set; }

        readonly string Group;
        private string LibName { get; set; }

        private bool Editable { get; set; }

        SaveLayoutToLibrary Save;

        DispatcherTimer textChangeTimeoutTimer = new DispatcherTimer();
        bool stillChanging = false;

        Action UpdateCallback;

        public LayoutDesigner(string libname, List<string> alllibs, string layoutname, string layoutjson, string group, bool editable, SaveLayoutToLibrary save, Action updateCallback)
        {
            InitializeComponent();
            TbJson.LoadLanguage_JSON();

            textChangeTimeoutTimer.Interval = TimeSpan.FromSeconds(1);
            LayoutName = $"{layoutname}";
            LayoutJson = layoutjson;
            LibName = libname;
            Group = group;
            TbJson.Text = LayoutJson;
            tbnameorig.Text = LayoutName;
            //tbnameorig1.Text = LayoutName;
            tbName.Text = $"{LayoutName}";
            Editable = editable;

            cbLibs.Items.Clear();
            alllibs.ForEach((x) => cbLibs.Items.Add(x));
            cbLibs.SelectedItem = libname;



            if (!editable)
            {
                tbName.IsEnabled = false;
                cbLibs.IsEnabled = false;
                btn_save.IsEnabled = false;
                Title = Title + " [Read Only]";
                TbJson.IsReadOnly = true;
            }

            Save = save;
            UpdateCallback = updateCallback;

            ShowPreviews(layoutjson);
        }

        private void ShowPreviews(string layoutjson)
        {
            var r = ProjectLayoutLibraryManager.GetLayoutPreview(Group, layoutjson);
            if (r.isvalid)
            {
                srcinvalid.Visibility = Visibility.Hidden;
                keyinvalid.Visibility = Visibility.Hidden;
                ImgMain.Source = r.main.ConvertToBitmapImage();
                ImgKey.Source = r.key.ConvertToBitmapImage();
            }
            else
            {
                ImgMain.Source = null;
                ImgKey.Source = null;
                srcinvalid.Visibility = Visibility.Visible;
                keyinvalid.Visibility = Visibility.Visible;
            }
        }

        private async void SourceTextChanged(object sender, EventArgs e)
        {
            if (!Editable)
            {
                return;
            }
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
            if (GetNames())
            {
                Save?.Invoke(SaveToLayoutLibrary, SaveToLayoutName, Group, TbJson.Text);
                UpdateCallback?.Invoke();
            }
        }

        private bool GetNames()
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                return false;
            }
            if ((string)cbLibs.SelectedItem == ProjectLayoutLibraryManager.DEFAULTLIBNAME)
            {
                return false;
            }
            SaveToLayoutName = tbName.Text;
            SaveToLayoutLibrary = (string)cbLibs.SelectedItem;
            return true;
        }

        private void tbNameChanged(object sender, TextChangedEventArgs e)
        {
            if (tbName.Text != LayoutName)
            {
                btn_save.Content = "Save As";
                btn_save.Background = Brushes.LimeGreen;
            }
            else
            {
                btn_save.Content = "Overwrite";
                btn_save.Background = Brushes.Orange;
            }
        }
    }
}
