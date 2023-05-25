using Microsoft.VisualBasic.Devices;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Xenon.Helpers;
using Xenon.SlideAssembly;
using Xenon.SlideAssembly.LayoutManagement;

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
        private LayoutSourceInfo SourceInfo { get; set; }

        readonly string Group;
        private string LibName { get; set; }

        private bool Editable { get; set; }

        SaveLayoutToLibrary Save;

        Action UpdateCallback;
        GetLayoutPreview getLayoutPreview;

        bool _dirty = false;
        bool DirtyChanges
        {
            get => _dirty;
            set
            {
                Dispatcher.Invoke(() =>
                {

                    btn_update.Background = value ? new SolidColorBrush(Color.FromRgb(0xb2, 0x42, 0xdb)) : new SolidColorBrush(Colors.Gray);
                });
                _dirty = value;
            }
        }

        public LayoutDesigner(string libname, List<string> alllibs, string layoutname, LayoutSourceInfo rawlayoutsource, string group, bool editable, SaveLayoutToLibrary save, Action updateCallback, GetLayoutPreview getLayoutPreview)
        {
            InitializeComponent();
            this.PreviewKeyDown += LayoutDesigner_PreviewKeyDown;
            SourceInfo = rawlayoutsource;

            if (rawlayoutsource.LangType == "json")
            {
                TbJson.LoadLanguage_JSON();
                editor_JSON.Visibility = Visibility.Visible;
                editor_HTML.Visibility = Visibility.Hidden;
                TbJson.Text = SourceInfo.RawSource;
            }
            else if (rawlayoutsource.LangType == "html")
            {
                TbHtml.LoadLanguage_HTML();
                TbKey.LoadLanguage_HTML();
                TbCSS.LoadLanguage_CSS();
                editor_JSON.Visibility = Visibility.Hidden;
                editor_HTML.Visibility = Visibility.Visible;
                TbHtml.Text = SourceInfo.RawSource;
                TbKey.Text = SourceInfo.RawSource_Key;
                if (SourceInfo.OtherData?.TryGetValue("css", out var css) == true)
                {
                    TbCSS.Text = css;
                }
                else
                {
                    TbCSS.Text = "";
                }
            }

            LayoutName = $"{layoutname}";
            LibName = libname;
            Group = group;

            tbnameorig.Text = LayoutName;
            //tbnameorig1.Text = LayoutName;
            tbName.Text = $"{LayoutName}";
            Editable = editable;

            this.getLayoutPreview = getLayoutPreview;

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

            ShowPreviews(SourceInfo);

            DirtyChanges = false;
        }

        private void LayoutDesigner_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.R && System.Windows.Input.Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                e.Handled = true;
                ReRender();
            }
        }

        private void ShowPreviews(LayoutSourceInfo src)
        {
            //string resolvedJson = ResolveLayoutMacros?.Invoke(layoutjson, LayoutName, Group, LibName);

            //var r = ProjectLayoutLibraryManager.GetLayoutPreview(Group, layoutjson);
            var r = getLayoutPreview.Invoke(LayoutName, Group, LibName, src, SourceInfo.LangType);
            if (r.isvalid)
            {
                srcinvalid.Visibility = Visibility.Hidden;
                keyinvalid.Visibility = Visibility.Hidden;
                ImgMain.Source = r.main.ToBitmapImage();
                ImgKey.Source = r.key.ToBitmapImage();
            }
            else
            {
                ImgMain.Source = null;
                ImgKey.Source = null;
                srcinvalid.Visibility = Visibility.Visible;
                keyinvalid.Visibility = Visibility.Visible;
            }
        }

        private void SourceTextChanged(object sender, EventArgs e)
        {
            if (Editable)
            {
                DirtyChanges = true;
            }
            //await ReRender();
        }

        private void ReRender()
        {
            if (!DirtyChanges)
            {
                return;
            }
            try
            {
                if (SourceInfo.LangType == "json")
                {
                    SourceInfo.RawSource = TbJson.Text;
                }
                else if (SourceInfo.LangType == "html")
                {
                    SourceInfo.RawSource = TbHtml.Text;
                    SourceInfo.RawSource_Key = TbKey.Text;
                    if (SourceInfo.OtherData == null)
                    {
                        SourceInfo.OtherData = new Dictionary<string, string>();
                    }
                    SourceInfo.OtherData["css"] = TbCSS.Text;
                }
                ShowPreviews(SourceInfo);
                DirtyChanges = false;
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
                LayoutSourceInfo newinfo = new LayoutSourceInfo()
                {
                    LangType = SourceInfo.LangType,
                    RawSource = SourceInfo.LangType == "json" ? TbJson.Text : TbHtml.Text,
                    RawSource_Key = SourceInfo.LangType == "html" ? TbKey.Text : "",
                    OtherData = new Dictionary<string, string>
                    {
                        ["css"] = SourceInfo.LangType == "html" ? TbCSS.Text : "",
                    }
                };
                Save?.Invoke(SaveToLayoutLibrary, SaveToLayoutName, Group, newinfo);
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

        private void Click_Update(object sender, RoutedEventArgs e)
        {
            ReRender();
        }
    }
}
