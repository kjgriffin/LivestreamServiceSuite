using IntegratedPresenter.BMDSwitcher.Config;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Integrated_Presenter.BMDSwitcher.Mock
{
    /// <summary>
    /// Interaction logic for MockMV_PV_PIP.xaml
    /// </summary>
    public partial class MockMV_PV_PIP : UserControl
    {
        public MockMV_PV_PIP()
        {
            InitializeComponent();
        }

        private double ATEM_TO_WPF_X { get => (gdisplay.Width / 2) / 16; }
        private double ATEM_TO_WPF_Y { get => (gdisplay.Height / 2) / 9; }

        public void SetPIPPosition(BMDUSKDVESettings state)
        {
            pvUSK1_scale.ScaleX = state.Current.SizeX;
            pvUSK1_scale.ScaleY = state.Current.SizeY;

            pvUSK1_pos.X = state.Current.PositionX * ATEM_TO_WPF_X;
            pvUSK1_pos.Y = state.Current.PositionY * -ATEM_TO_WPF_Y;

            pvUSK1_clip.Rect = new Rect(
                ATEM_TO_WPF_X * state.MaskLeft,
                ATEM_TO_WPF_Y * state.MaskTop,
                gdisplay.Width - ((state.MaskRight + state.MaskLeft) * ATEM_TO_WPF_X),
                gdisplay.Height - ((state.MaskBottom + state.MaskTop) * ATEM_TO_WPF_Y));

        }

    }
}
