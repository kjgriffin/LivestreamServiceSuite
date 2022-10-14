using CommonGraphics;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Integrated_Presenter.BMDSwitcher.Mock.BetterMock.Dialogs
{
    /// <summary>
    /// Interaction logic for MoveCameraUI.xaml
    /// </summary>
    public partial class MoveCameraUI : Window
    {

        List<(string, int)> _cams = new List<(string, int)>();

        int selectedChoice = 0;

        public (int cid, string img) Result = (-1, "");
        public bool Valid = false;

        int cid = 0;

        public MoveCameraUI(List<(string, int)> cams, List<BitmapImage> options)
        {
            InitializeComponent();
            _cams = cams;
            foreach (var cam in _cams)
            {
                var ctrl = new TextBlock(new Run(cam.Item1));
                ctrl.FontSize = 30;
                cbCams.Items.Add(ctrl);
            }
            cbCams.SelectedIndex = 0;


            foreach (var choice in options)
            {
                var ctrl = new CameraThumbnailOptionItem(choice, cid);
                ctrl.OnRequestSelection += Ctrl_OnRequestSelection;

                wpOptions.Children.Add(ctrl);
                cid++;
            }

            SelectedChoiceChanged(selectedChoice);
        }

        private void Ctrl_OnRequestSelection(object sender, int e)
        {
            SelectedChoiceChanged(e);
        }

        private void SelectedChoiceChanged(int id)
        {
            selectedChoice = id;
            int i = 0;
            foreach (var x in wpOptions.Children)
            {
                var ctrl = x as CameraThumbnailOptionItem;
                if (ctrl != null)
                {
                    ctrl.UpdateSelected(i == selectedChoice);
                }
                i++;
            }
        }


        private void MoveCamera(object sender, RoutedEventArgs e)
        {
            int scam = -1;

            if (cbCams.SelectedIndex != -1)
            {
                scam = _cams[cbCams.SelectedIndex].Item2;
            }

            if (scam != -1)
            {
                Valid = true;
                Result = (scam, (wpOptions.Children[selectedChoice] as CameraThumbnailOptionItem)?.GetImageAsBase64String() ?? "");
            }

            Close();
        }

        private void LoadCustom(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Custom Image";
            ofd.Filter = "Images (*.png)|*.png";
            if (ofd.ShowDialog() == true)
            {

                BitmapImage img = new BitmapImage(new Uri(ofd.FileName));

                var ctrl = new CameraThumbnailOptionItem(img, cid);
                ctrl.OnRequestSelection += Ctrl_OnRequestSelection;

                wpOptions.Children.Add(ctrl);

                SelectedChoiceChanged(cid);
                cid++;
            }
        }
    }
}
