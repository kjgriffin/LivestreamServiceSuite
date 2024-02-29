using IntegratedPresenterAPIInterop;
using IntegratedPresenterAPIInterop.DynamicDrivers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace Integrated_Presenter.ViewModels.MatrixControls
{
    /// <summary>
    /// Interaction logic for _4x4Matrix.xaml
    /// </summary>
    public partial class _4x3Matrix : UserControl
    {
        public event EventHandler<(int, int)> OnButtonClick;

        const int matrix_width = 4;
        const int matrix_height = 3;
        Control[,] controls = new Control[matrix_width, matrix_height];
        List<(EventHandler h, int x, int y)> activeHandlers = new List<(EventHandler, int, int)>();

        public _4x3Matrix()
        {
            InitializeComponent();
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

        public void UpdateButton(int x, int y, DynamicDrawExpression dExpr, ICalculatedVariableManager calc)
        {
            if (ValidMatrixIndex(x, y))
            {
                var btn = controls[x, y] as MatrixTextButton;
                btn?.UpdateButton(dExpr, calc);
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
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    RemoveButton(i, j);
                }
            }

        }
    }
}
