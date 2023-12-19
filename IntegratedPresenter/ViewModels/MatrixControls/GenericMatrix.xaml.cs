using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace Integrated_Presenter.ViewModels.MatrixControls
{
    /// <summary>
    /// Interaction logic for _4x4Matrix.xaml
    /// </summary>
    public partial class GenericMatrix : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<(int, int)> OnButtonClick;
        public event PropertyChangedEventHandler PropertyChanged;

        uint matrix_width = 4;
        uint matrix_height = 3;
        Control[,] controls;
        List<(EventHandler h, int x, int y)> activeHandlers = new List<(EventHandler, int, int)>();


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public uint MatrixWidth
        {
            get => (uint)matrix_width;
            set
            {
                DestroyMatrix();
                matrix_width = value;
                GenerateMatrix();
                OnPropertyChanged();
            }
        }
        public uint MatrixHeight
        {
            get => (uint)matrix_height;
            set
            {
                DestroyMatrix();
                matrix_height = value;
                GenerateMatrix();
                OnPropertyChanged();
            }
        }


        public GenericMatrix()
        {
            controls = new Control[matrix_width, matrix_height];
            InitializeComponent();
            ClearMatrix();
        }


        private void DestroyMatrix()
        {
            ClearMatrix();
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }

        private void GenerateMatrix()
        {
            controls = new Control[matrix_width, matrix_height];

            // build row/column defs
            for (int i = 0; i < matrix_height; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < matrix_width; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            ClearMatrix();
        }

        public void InstallButton(int x, int y, string top, string bottom, string bcolor, string hcolor, bool enabled)
        {
            if (ValidMatrixIndex(x, y))
            {
                // remove active button
                RemoveButton(x, y);

                // remove active patch
                if (controls[x, y] != null)
                {
                    grid.Children.Remove(controls[x, y]);
                }

                // create new control
                MatrixTextButton btn = new MatrixTextButton();

                controls[x, y] = btn;

                // add to grid
                grid.Children.Add(btn);
                Grid.SetColumn(btn, x);
                Grid.SetRow(btn, y);
                btn.Enabled = enabled;
                btn.TopText = top;
                btn.BottomText = bottom;
                btn.HoverColor = ToColor(hcolor);
                btn.BackColor = ToColor(bcolor);
                EventHandler handler = (s, e) =>
                {
                    OnButtonClick?.Invoke(this, (x, y));
                };
                activeHandlers.Add((handler, x, y));
                btn.OnClick += handler;
            }
        }

        public void RemoveButton(int x, int y)
        {
            if (ValidMatrixIndex(x, y))
            {
                if (controls[x, y] != null)
                {
                    var c = controls[x, y];
                    if ((c as MatrixTextButton) != null)
                    {
                        ((MatrixTextButton)c).OnClick -= activeHandlers.FirstOrDefault(h => h.x == x && h.y == y).h;
                        activeHandlers.RemoveAll(h => h.x == x && h.y == y);
                        controls[x, y] = null;
                    }
                }
                if ((controls[x, y] as MatrixEmptyPatch) == null)
                {
                    // install the pannel
                    MatrixEmptyPatch patch = new MatrixEmptyPatch();
                    grid.Children.Add(patch);
                    Grid.SetColumn(patch, x);
                    Grid.SetRow(patch, y);
                    controls[x, y] = patch;
                }
            }
        }

        public void UpdateButton(int x, int y, string property, string value)
        {
            if (ValidMatrixIndex(x, y))
            {
                var btn = controls[x, y] as MatrixTextButton;
                if (btn != null)
                {
                    switch (property)
                    {
                        case nameof(MatrixTextButton.TopText):
                            btn.TopText = value;
                            break;
                        case nameof(MatrixTextButton.BottomText):
                            btn.BottomText = value;
                            break;
                        case nameof(MatrixTextButton.BackColor):
                            btn.BackColor = ToColor(value);
                            break;
                        case nameof(MatrixTextButton.HoverColor):
                            btn.HoverColor = ToColor(value);
                            break;
                        case nameof(MatrixTextButton.TextColor):
                            btn.TextColor = ToColor(value);
                            break;
                        case nameof(MatrixTextButton.Enabled):
                            bool.TryParse(value, out bool vb);
                            btn.Enabled = vb;
                            break;
                    }

                }
            }
        }

        private bool ValidMatrixIndex(int x, int y)
        {
            return (x >= 0 && x < matrix_width && y >= 0 && y < matrix_height);
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

        internal void ClearMatrix()
        {
            for (int i = 0; i < matrix_width; i++)
            {
                for (int j = 0; j < matrix_height; j++)
                {
                    RemoveButton(i, j);
                }
            }

        }
    }
}
