using IntegratedPresenterAPIInterop;
using IntegratedPresenterAPIInterop.DynamicDrivers;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
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

        public void UpdateButton(IntegratedPresenterAPIInterop.DynamicDrivers.DynamicDrawExpression drawExpr, IntegratedPresenterAPIInterop.ICalculatedVariableManager calculator)
        {
            switch (drawExpr.PKey)
            {
                case nameof(MatrixTextButton.TopText):
                    this.TopText = ComputeDrawValue(drawExpr, calculator);// drawExpr.Value;
                    break;
                case nameof(MatrixTextButton.BottomText):
                    this.BottomText = ComputeDrawValue(drawExpr, calculator);// drawExpr.Value;
                    break;
                case nameof(MatrixTextButton.BackColor):
                    this.BackColor = ToColor(drawExpr.Value);
                    break;
                case nameof(MatrixTextButton.HoverColor):
                    this.HoverColor = ToColor(drawExpr.Value);
                    break;
                case nameof(MatrixTextButton.TextColor):
                    this.TextColor = ToColor(drawExpr.Value);
                    break;
                case nameof(MatrixTextButton.Enabled):
                    bool.TryParse(drawExpr.Value, out bool vb);
                    this.Enabled = vb;
                    break;
            }
        }

        private string ComputeDrawValue(DynamicDrawExpression expr, ICalculatedVariableManager calculatedVariableManager)
        {
            string displayVal = expr.Value;

            if (expr.IsDynamicValue)
            {
                if (calculatedVariableManager.TryEvaluateVariableValue<int>(expr.VExpr, out int ival))
                {
                    displayVal = ival.ToString();
                }
                else if (calculatedVariableManager.TryEvaluateVariableValue<bool>(expr.VExpr, out bool bval))
                {
                    displayVal = bval.ToString();
                }
                else if (calculatedVariableManager.TryEvaluateVariableValue<string>(expr.VExpr, out string sval))
                {
                    displayVal = sval;
                }
                else if (calculatedVariableManager.TryEvaluateVariableValue<double>(expr.VExpr, out double dval))
                {
                    displayVal = Math.Round(dval, 2).ToString("0.00");
                }
            }

            return displayVal;
        }

        private Color ToColor(string col)
        {
            if (col.Length == 7 && Regex.Match(col, "#[0-9,a-f,A-F]{6}").Success)
            {
                byte r = Byte.Parse(col.Substring(1, 2), NumberStyles.HexNumber);
                byte g = Byte.Parse(col.Substring(3, 2), NumberStyles.HexNumber);
                byte b = Byte.Parse(col.Substring(5, 2), NumberStyles.HexNumber);
                return Color.FromRgb(r, g, b);
            }
            return Colors.Black;
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
