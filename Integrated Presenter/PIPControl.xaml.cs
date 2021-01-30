using Integrated_Presenter.BMDSwitcher.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter
{
    /// <summary>
    /// Interaction logic for PIPControl.xaml
    /// </summary>
    public partial class PIPControl : Window
    {

        Action<BMDUSKDVESettings> SetPIPOnSwitcher;
        Action<BMDUSKDVESettings> SetPIPKeyframeAOnSwitcher;
        Action<BMDUSKDVESettings> SetPIPKeyframeBOnSwitcher;
        MainWindow _parent;
        public PIPControl(MainWindow parent, Action<BMDUSKDVESettings> setpiponswitcher, Action<BMDUSKDVESettings> setpipkeyaonswitcher, Action<BMDUSKDVESettings> setpipkeybonswitcher, BMDUSKDVESettings current)
        {
            InitializeComponent();
            _parent = parent;
            SetPIPOnSwitcher = setpiponswitcher;
            SetPIPKeyframeAOnSwitcher = setpipkeyaonswitcher;
            SetPIPKeyframeBOnSwitcher = setpipkeybonswitcher;
            PIPSettingsUpdated(current);
            CheckIfAtTarget();
        }

        public bool HasClosed { get; set; } = false;


        public void PIPSettingsUpdated(BMDUSKDVESettings state)
        {
            truepipxoff = state.Current.PositionX;
            truepipyoff = state.Current.PositionY;
            truepipscale = Math.Max(state.Current.SizeX, state.Current.SizeY);
            truepipmlr = state.IsMasked == 1 ? Math.Max(state.MaskLeft, state.MaskRight) : 0;
            truepipmtb = state.IsMasked == 1 ? Math.Max(state.MaskTop, state.MaskBottom) : 0;

            keyapipscale = Math.Max(state.KeyFrameA.SizeX, state.KeyFrameA.SizeY);
            keyapipx = state.KeyFrameA.PositionX;
            keyapipy = state.KeyFrameA.PositionY;

            keybpipscale = Math.Max(state.KeyFrameB.SizeX, state.KeyFrameB.SizeY);
            keybpipx = state.KeyFrameB.PositionX;
            keybpipy = state.KeyFrameB.PositionY;

            CheckIfAtTarget();

            Dispatcher.Invoke(() =>
            {
                UpdateUI();
            });
        }

        private void CheckIfAtTarget()
        {
            if (PIPIsAtTarget())
            {
                // no need to send command to update pip position
                SendCmd = false;
            }
        }


        private void UpdateUI()
        {
            CheckIfAtTarget();


            // compute location of pip

            // set displaysize
            displaygrid.Width = truedisplaywidth * displayscale;
            displaygrid.Height = truedisplayheight * displayscale;

            // set scale size of pip
            maskedbox.Width = displaygrid.Width * pipscale;
            maskedbox.Height = displaygrid.Height * pipscale;

            truemaskedbox.Width = displaygrid.Width * truepipscale;
            truemaskedbox.Height = displaygrid.Height * truepipscale;


            keyamaskbox.Width = displaygrid.Width * keyapipscale;
            keyamaskbox.Height = displaygrid.Height * keyapipscale;

            keybmaskbox.Width = displaygrid.Width * keybpipscale;
            keybmaskbox.Height = displaygrid.Height * keybpipscale;

            // TODO: adjust width/height to account for masking
            viewbox.Width = Math.Max((displaygrid.Width * pipscale) - ((displaygrid.Width * pipscale) * (pipmlr * 2 / xrange)), 0);
            viewbox.Height = Math.Max((displaygrid.Height * pipscale) - ((displaygrid.Height * pipscale) * (pipmtb * 2 / yrange)), 0);

            trueviewbox.Width = Math.Max((displaygrid.Width * truepipscale) - ((displaygrid.Width * truepipscale) * (truepipmlr * 2 / xrange)), 0);
            trueviewbox.Height = Math.Max((displaygrid.Height * truepipscale) - ((displaygrid.Height * truepipscale) * (truepipmtb * 2 / yrange)), 0);


            // position the rectangles
            double x = pipxoff * (displaygrid.Width / xrange);
            double y = -pipyoff * (displaygrid.Height / yrange);

            double tx = truepipxoff * (displaygrid.Width / xrange);
            double ty = -truepipyoff * (displaygrid.Height / yrange);


            double kax = keyapipx * (displaygrid.Width / xrange);
            double kay = -keyapipy * (displaygrid.Height / yrange);

            double kbx = keybpipx * (displaygrid.Width / xrange);
            double kby = -keybpipy * (displaygrid.Height / yrange);

            maskedbox.RenderTransform = new TranslateTransform(x, y);
            viewbox.RenderTransform = new TranslateTransform(x, y);

            truemaskedbox.RenderTransform = new TranslateTransform(tx, ty);
            trueviewbox.RenderTransform = new TranslateTransform(tx, ty);

            keyamaskbox.RenderTransform = new TranslateTransform(kax, kay);
            keybmaskbox.RenderTransform = new TranslateTransform(kbx, kby);

            tbScale.Text = $"{pipscale:0.##} :: {pipscale * 100:0.##}%";
            tbX.Text = $"{pipxoff:0.##} :: {pipxoff / (xrange / 2) * 100:0.##}%";
            tbY.Text = $"{pipyoff:0.##} :: {pipyoff / (yrange / 2) * 100:0.##}%";
            tbMaskLR.Text = $"{pipmlr:0.##} :: {pipmlr / maxmlr * 100:0.##}%";
            tbMaskTB.Text = $"{pipmtb:0.##} :: {pipmtb / maxmtb * 100:0.##}%";

        }

        const double truedisplaywidth = 1920;
        const double truedisplayheight = 1080;

        double displayscale = 0.25;


        const double minscale = 0;
        const double maxscale = 1;
        const double xrange = 16 * 2;
        const double yrange = 9 * 2;
        const double minx = -32;
        const double maxx = 32;
        const double miny = -18;
        const double maxy = 18;
        const double maxmlr = 16;
        const double minmlr = 0;
        const double maxmtb = 9;
        const double minmtb = 0;


        double pipscale = 0.4;
        double pipxoff = 0;
        double pipyoff = 0;

        double pipmlr = 0;
        double pipmtb = 0;

        double truepipscale = 0.4;
        double truepipxoff = 9.6;
        double truepipyoff = 5.4;
        double truepipmlr = 0;
        double truepipmtb = 0;

        double keyapipscale = 0;
        double keyapipx = 0;
        double keyapipy = 0;
        double keybpipscale = 0;
        double keybpipx = 0;
        double keybpipy = 0;



        double scalespeed = 0.1;
        double translationspeed = 0.1;
        double maskspeed = 0.1;


        bool sendcmd = false;
        bool SendCmd
        {
            get => sendcmd; set
            {
                sendcmd = value;
                if (sendcmd)
                {
                    cmdlight.Fill = Brushes.Orange;
                }
                else
                {
                    cmdlight.Fill = Brushes.Gray;
                }
            }
        }
        bool cmdmode = false;
        bool CmdMode
        {
            get => cmdmode;
            set
            {
                cmdmode = value;
                if (cmdmode)
                {
                    ctrllight.Fill = Brushes.LawnGreen;
                }
                else
                {
                    ctrllight.Fill = Brushes.WhiteSmoke;
                }
            }
        }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _parent?.Focus();
            }


            if (Keyboard.IsKeyDown(Key.Space))
            {
                CmdMode = true;
                SwitcherPIPUpdate();
            }
            else
            {
                CmdMode = false;
            }


            if (e.Key == Key.Q)
            {
                mask_lr_decrease();
            }
            if (e.Key == Key.E)
            {
                mask_lr_increase();
            }

            if (e.Key == Key.R)
            {
                mask_tb_decrease();
            }
            if (e.Key == Key.F)
            {
                mask_tb_increase();
            }


            if (e.Key == Key.Z)
            {
                scale_down();
            }
            if (e.Key == Key.X)
            {
                scale_up();
            }

            if (e.Key == Key.W)
            {
                translate_up();
            }
            if (e.Key == Key.S)
            {
                translate_down();
            }
            if (e.Key == Key.A)
            {
                translate_left();
            }
            if (e.Key == Key.D)
            {
                translate_right();
            }

            if (e.Key == Key.NumPad7)
            {
                to_top_left();
            }
            if (e.Key == Key.NumPad9)
            {
                to_top_right();
            }
            if (e.Key == Key.NumPad1)
            {
                to_bottom_left();
            }
            if (e.Key == Key.NumPad3)
            {
                to_bottom_right();
            }

            if (e.Key == Key.NumPad8)
            {
                to_top_mid();
            }
            if (e.Key == Key.NumPad2)
            {
                to_bottom_mid();
            }
            if (e.Key == Key.NumPad4)
            {
                to_left_mid();
            }
            if (e.Key == Key.NumPad6)
            {
                to_right_mid();
            }

            if (e.Key == Key.NumPad5)
            {
                to_center();
            }

            if (e.Key == Key.Divide)
            {
                set_as_key_a();
            }
            if (e.Key == Key.Multiply)
            {
                set_as_key_b();
            }

        }

        private bool PIPIsAtTarget()
        {
            return pipscale == truepipscale && pipxoff == truepipxoff && pipyoff == truepipyoff && pipmlr == truepipmlr && pipmtb == truepipmlr;
        }

        private void SwitcherPIPUpdate()
        {
            // only update if in update mode
            if (CmdMode && SendCmd)
            {
                SendCmd = false;

                BMDUSKDVESettings config = new BMDUSKDVESettings();
                config.Current = new KeyFrameSettings();
                config.Current.PositionX = pipxoff;
                config.Current.PositionY = pipyoff;
                config.Current.SizeX = pipscale;
                config.Current.SizeY = pipscale;
                config.IsMasked = pipmtb != 0 || pipmlr != 0 ? 1 : 0;
                config.IsBordered = 0; // no border
                config.MaskTop = (float)pipmtb;
                config.MaskBottom = (float)pipmtb;
                config.MaskLeft = (float)pipmlr;
                config.MaskRight = (float)pipmlr;

                SetPIPOnSwitcher?.Invoke(config);
            }

        }

        private void set_as_key_a()
        {
            BMDUSKDVESettings config = new BMDUSKDVESettings();
            config.IsMasked = pipmtb != 0 || pipmlr != 0 ? 1 : 0;
            config.MaskTop = (float)pipmtb;
            config.MaskBottom = (float)pipmtb;
            config.MaskLeft = (float)pipmlr;
            config.MaskRight = (float)pipmlr;
            config.IsBordered = 0; // no border
            config.KeyFrameA = new KeyFrameSettings()
            {
                PositionX = pipxoff,
                PositionY = pipyoff,
                SizeX = pipscale,
                SizeY = pipscale,
            };
            SetPIPKeyframeAOnSwitcher?.Invoke(config);
        }

        private void set_as_key_b()
        {
            BMDUSKDVESettings config = new BMDUSKDVESettings();
            config.IsMasked = pipmtb != 0 || pipmlr != 0 ? 1 : 0;
            config.MaskTop = (float)pipmtb;
            config.MaskBottom = (float)pipmtb;
            config.MaskLeft = (float)pipmlr;
            config.MaskRight = (float)pipmlr;
            config.IsBordered = 0; // no border
            config.KeyFrameB = new KeyFrameSettings()
            {
                PositionX = pipxoff,
                PositionY = pipyoff,
                SizeX = pipscale,
                SizeY = pipscale,
            };
            SetPIPKeyframeBOnSwitcher?.Invoke(config);
        }

        private void scale_up()
        {
            pipscale += scalespeed;
            if (pipscale > maxscale)
            {
                pipscale = maxscale;
            }

            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void scale_down()
        {
            pipscale -= scalespeed;
            if (pipscale < minscale)
            {
                pipscale = minscale;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }


        private void mask_lr_increase()
        {
            pipmlr += maskspeed;
            if (pipmlr > maxmlr)
            {
                pipmlr = maxmlr;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_lr_decrease()
        {
            pipmlr -= maskspeed;
            if (pipmlr < minmlr)
            {
                pipmlr = minmlr;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_tb_increase()
        {
            pipmtb += maskspeed;
            if (pipmtb > maxmtb)
            {
                pipmtb = maxmtb;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_tb_decrease()
        {
            pipmtb -= maskspeed;
            if (pipmtb < minmtb)
            {
                pipmtb = minmtb;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }


        private void translate_left()
        {
            pipxoff -= translationspeed;
            if (pipxoff < minx)
            {
                pipxoff = minx;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void translate_right()
        {
            pipxoff += translationspeed;
            if (pipxoff > maxx)
            {
                pipxoff = maxx;
            }


            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void translate_down()
        {
            pipyoff -= translationspeed;
            if (pipyoff < miny)
            {
                pipyoff = miny;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void translate_up()
        {
            pipyoff += translationspeed;
            if (pipyoff > maxy)
            {
                pipyoff = maxy;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            HasClosed = true;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                CmdMode = false;
            }
        }


        private (double x, double y) calc_centers()
        {
            double fullhalfwidth = xrange / 2 * pipscale;
            double halfwidth = fullhalfwidth - (pipmlr * pipscale);

            double fullhalfheight = yrange / 2 * pipscale;
            double halfheight = fullhalfheight - (pipmtb * pipscale);

            return (halfwidth, halfheight);
        }


        private void to_top_left()
        {
            var h = calc_centers();

            pipxoff = -xrange / 2 + h.x;
            pipyoff = yrange / 2 - h.y;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_top_right()
        {
            var h = calc_centers();

            pipxoff = xrange / 2 - h.x;
            pipyoff = yrange / 2 - h.y;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_bottom_left()
        {
            var h = calc_centers();

            pipxoff = -xrange / 2 + h.x;
            pipyoff = -yrange / 2 + h.y;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_bottom_right()
        {
            var h = calc_centers();

            pipxoff = xrange / 2 - h.x;
            pipyoff = -yrange / 2 + h.y;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_top_mid()
        {
            var h = calc_centers();

            pipxoff = 0;
            pipyoff = yrange / 2 - h.y;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_bottom_mid()
        {
            var h = calc_centers();

            pipxoff = 0;
            pipyoff = -yrange / 2 + h.y;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_left_mid()
        {
            var h = calc_centers();

            pipxoff = -xrange / 2 + h.x;
            pipyoff = 0;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_right_mid()
        {
            var h = calc_centers();

            pipxoff = xrange / 2 - h.x;
            pipyoff = 0;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_center()
        {
            pipxoff = 0;
            pipyoff = 0;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }


    }
}
