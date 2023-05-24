using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        DispatcherTimer textChangeTimeoutTimer = new DispatcherTimer();
        bool stillChanging = false;

        Action UpdateCallback;

        GetLayoutPreview getLayoutPreview;

        public LayoutDesigner(string libname, List<string> alllibs, string layoutname, LayoutSourceInfo rawlayoutsource, string group, bool editable, SaveLayoutToLibrary save, Action updateCallback, GetLayoutPreview getLayoutPreview)
        {
            InitializeComponent();
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
                TbCSS.LoadLanguage_HTML();
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


            textChangeTimeoutTimer.Interval = TimeSpan.FromSeconds(1);
            textChangeTimeoutTimer.Tick += TextChangeTimeoutTimer_Tick;
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
        }

        private void TextChangeTimeoutTimer_Tick(object sender, EventArgs e)
        {
            ReRender();
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
            if (!Editable)
            {
                return;
            }
            //stillChanging = true;
            if (!textChangeTimeoutTimer.IsEnabled)
            {
                textChangeTimeoutTimer.Stop();
                textChangeTimeoutTimer.Start();
            }
            //await ReRender();
        }

        private void ReRender()
        {
            //while (stillChanging)
            //{
            //    await Task.Delay(1000);
            //    stillChanging = false;
            //}
            textChangeTimeoutTimer.Stop();
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
    }
}
