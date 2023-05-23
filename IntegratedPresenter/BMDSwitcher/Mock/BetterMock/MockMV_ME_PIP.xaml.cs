using IntegratedPresenter.BMDSwitcher.Config;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Integrated_Presenter.BMDSwitcher.Mock
{
    /// <summary>
    /// Interaction logic for MockMV_ME_PIP.xaml
    /// </summary>
    public partial class MockMV_ME_PIP : UserControl
    {

        public MockMV_ME_PIP()
        {
            InitializeComponent();
        }

        private double ATEM_TO_WPF_X { get => (gdisplay.Width / 2) / 16; }
        private double ATEM_TO_WPF_Y { get => (gdisplay.Height / 2) / 9; }

        public void SetPIPPosition(BMDUSKDVESettings state)
        {
            lUSK1_A_scale.ScaleX = state.Current.SizeX;
            lUSK1_A_scale.ScaleY = state.Current.SizeY;

            lUSK1_A_pos.X = state.Current.PositionX * ATEM_TO_WPF_X;
            lUSK1_A_pos.Y = state.Current.PositionY * -ATEM_TO_WPF_Y;

            lUSK1_A_clip.Rect = new Rect(
                ATEM_TO_WPF_X * state.MaskLeft,
                ATEM_TO_WPF_Y * state.MaskTop,
                gdisplay.Width - ((state.MaskRight + state.MaskLeft) * ATEM_TO_WPF_X),
                gdisplay.Height - ((state.MaskBottom + state.MaskTop) * ATEM_TO_WPF_Y));

        }

        public Brush GetOutput()
        {
            return new VisualBrush(gdisplay);
        }

    }
}
