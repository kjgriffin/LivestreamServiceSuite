using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Integrated_Presenter.ViewModels.MatrixControls
{
    /// <summary>
    /// Interaction logic for MatrixTextButton.xaml
    /// </summary>
    public partial class MatrixTextButton : UserControl
    {

        public event EventHandler OnClick;


        private bool m_overState = false;


        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                RefreshUI();
            }
        }

        private Color _hoverColor = Colors.Orange;
        public Color HoverColor
        {
            get => _hoverColor;
            set
            {
                _hoverColor = value;
                RefreshUI();
            }
        }

        private Color _backgroundColor = Color.FromRgb(0xea, 0xea, 0xea);
        public Color BackColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                RefreshUI();
            }
        }

        private Color _textColor = Colors.Black;
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                RefreshUI();
            }
        }


        private string _topText;
        public string TopText
        {
            get => _topText;
            set
            {
                _topText = value;
                RefreshUI();
            }
        }

        private string _bottomText;
        public string BottomText
        {
            get => _bottomText;
            set
            {
                _bottomText = value;
                RefreshUI();
            }
        }

        public MatrixTextButton()
        {
            InitializeComponent();
            border.IsMouseDirectlyOverChanged += Border_IsMouseDirectlyOverChanged;
            border.PreviewMouseDown += Border_PreviewMouseDown;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnClick?.Invoke(this, e);
        }

        private void Border_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            m_overState = (bool)e.NewValue;
            UpdateMouseOver();
        }

        private void UpdateMouseOver()
        {
            Dispatcher.Invoke(() =>
            {
                if (m_overState && Enabled)
                {
                    // mouse over
                    border.BorderBrush = new SolidColorBrush(HoverColor);
                    border.Cursor = Cursors.Hand;
                }
                else
                {
                    // mouse not over
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xa0, 0xa0, 0xa0));
                    border.Cursor = null;
                }
            });
        }

        private void RefreshUI()
        {
            Dispatcher.Invoke(() =>
            {
                tbTop.Foreground = new SolidColorBrush(TextColor);
                tbBottom.Foreground = new SolidColorBrush(TextColor);
                border.Background = new SolidColorBrush(BackColor);
                tbTop.Text = TopText;
                tbBottom.Text = BottomText;

                if (Enabled)
                {
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xa0, 0xa0, 0xa0));
                }
                else
                {
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x50));
                }
            });
            UpdateMouseOver();
        }
    }
}
