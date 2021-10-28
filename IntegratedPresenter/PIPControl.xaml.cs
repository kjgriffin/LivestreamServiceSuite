using IntegratedPresenter.BMDSwitcher.Config;
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

namespace IntegratedPresenter.Main
{
    /// <summary>
    /// Interaction logic for PIPControl.xaml
    /// </summary>
    public partial class PIPControl : Window
    {

        Action<BMDUSKDVESettings> SetPIPOnSwitcher;
        Action<BMDUSKDVESettings> SetPIPKeyframeAOnSwitcher;
        Action<BMDUSKDVESettings> SetPIPKeyframeBOnSwitcher;
        Func<int, int> ConvertButtonToSourceID;

        MainWindow _parent;
        public PIPControl(MainWindow parent, Action<BMDUSKDVESettings> setpiponswitcher, Action<BMDUSKDVESettings> setpipkeyaonswitcher, Action<BMDUSKDVESettings> setpipkeybonswitcher, BMDUSKDVESettings current, long? USK1FillSource, Func<int, int> convertbuttontosourceid)
        {
            InitializeComponent();

            pipfills[0] = preset_PIP_1;
            pipfills[1] = preset_PIP_2;
            pipfills[2] = preset_PIP_3;
            pipfills[3] = preset_PIP_4;
            pipfills[4] = preset_PIP_5;
            pipfills[5] = preset_PIP_6;
            pipfills[6] = preset_PIP_7;
            pipfills[7] = preset_PIP_8;

            _parent = parent;
            SetPIPOnSwitcher = setpiponswitcher;
            SetPIPKeyframeAOnSwitcher = setpipkeyaonswitcher;
            SetPIPKeyframeBOnSwitcher = setpipkeybonswitcher;
            ConvertButtonToSourceID = convertbuttontosourceid;
            PIPSettingsUpdated(current, USK1FillSource ?? 0);

            presetfillsource = (int)USK1FillSource;

            CheckIfAtTarget();
            _parent.ForceStateUpdateOnSwitcher();
        }

        public bool HasClosed { get; set; } = false;


        public void PIPSettingsUpdated(BMDUSKDVESettings state, long USK1FillSource)
        {
            truepipxoff = state.Current.PositionX;
            truepipyoff = state.Current.PositionY;
            truepipscale = Math.Max(state.Current.SizeX, state.Current.SizeY);
            truepipml = state.IsMasked == 1 ? state.MaskLeft : 0;
            truepipmr = state.IsMasked == 1 ? state.MaskRight : 0;
            truepipmt = state.IsMasked == 1 ? state.MaskTop : 0;
            truepipmb = state.IsMasked == 1 ? state.MaskBottom : 0;

            /*
            keyapipscale = Math.Max(state.KeyFrameA.SizeX, state.KeyFrameA.SizeY);
            keyapipx = state.KeyFrameA.PositionX;
            keyapipy = state.KeyFrameA.PositionY;

            keybpipscale = Math.Max(state.KeyFrameB.SizeX, state.KeyFrameB.SizeY);
            keybpipx = state.KeyFrameB.PositionX;
            keybpipy = state.KeyFrameB.PositionY;
            */

            currentfillsource = (int)USK1FillSource;

            CheckIfAtTarget();

            Dispatcher.Invoke(() =>
            {
                UpdateUI();
            });
        }

        private void CheckIfAtTarget()
        {
            SendCmd = true;
            if (PIPIsAtTarget())
            {
                // no need to send command to update pip position
                SendCmd = false;
            }
        }

        bool enableKeyFrameFeatures = false;


        Border[] pipfills = new Border[8];

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

            if (enableKeyFrameFeatures)
            {
                keyamaskbox.Visibility = Visibility.Visible;
                keybmaskbox.Visibility = Visibility.Visible;
                keyamaskbox.Width = displaygrid.Width * keyapipscale;
                keyamaskbox.Height = displaygrid.Height * keyapipscale;

                keybmaskbox.Width = displaygrid.Width * keybpipscale;
                keybmaskbox.Height = displaygrid.Height * keybpipscale;
            }
            else
            {
                keyamaskbox.Visibility = Visibility.Collapsed;
                keybmaskbox.Visibility = Visibility.Collapsed;
            }

            // TODO: adjust width/height to account for masking
            viewbox.Width = Math.Max((displaygrid.Width * pipscale) - ((displaygrid.Width * pipscale) * ((pipml + pipmr) / xrange)), 0);
            viewbox.Height = Math.Max((displaygrid.Height * pipscale) - ((displaygrid.Height * pipscale) * ((pipmt + pipmb) / yrange)), 0);

            trueviewbox.Width = Math.Max((displaygrid.Width * truepipscale) - ((displaygrid.Width * truepipscale) * ((truepipml + truepipmr) / xrange)), 0);
            trueviewbox.Height = Math.Max((displaygrid.Height * truepipscale) - ((displaygrid.Height * truepipscale) * ((truepipmt + truepipmb) / yrange)), 0);


            // position the rectangles
            double x = pipxoff * (displaygrid.Width / xrange);
            double y = -pipyoff * (displaygrid.Height / yrange);

            double slewx = x + (((pipml - pipmr) / 2) * pipscale * (displaygrid.Width / xrange));
            double slewy = y + (((pipmt - pipmb) / 2) * pipscale * (displaygrid.Height / yrange));

            double tx = truepipxoff * (displaygrid.Width / xrange);
            double ty = -truepipyoff * (displaygrid.Height / yrange);

            double tslewx = tx + (((truepipml - truepipmr) / 2) * pipscale * (displaygrid.Width / xrange));
            double tslewy = ty + (((truepipmt - truepipmb) / 2) * truepipscale * (displaygrid.Height / yrange));

            maskedbox.RenderTransform = new TranslateTransform(x, y);
            viewbox.RenderTransform = new TranslateTransform(slewx, slewy);

            truemaskedbox.RenderTransform = new TranslateTransform(tx, ty);
            trueviewbox.RenderTransform = new TranslateTransform(tslewx, tslewy);

            if (enableKeyFrameFeatures)
            {
                double kax = keyapipx * (displaygrid.Width / xrange);
                double kay = -keyapipy * (displaygrid.Height / yrange);

                double kbx = keybpipx * (displaygrid.Width / xrange);
                double kby = -keybpipy * (displaygrid.Height / yrange);

                keyamaskbox.RenderTransform = new TranslateTransform(kax, kay);
                keybmaskbox.RenderTransform = new TranslateTransform(kbx, kby);
            }

            tbScale.Text = $"{pipscale:0.##} :: {pipscale * 100:0.##}%";
            tbX.Text = $"{pipxoff:0.##} :: {pipxoff / (xrange / 2) * 100:0.##}%";
            tbY.Text = $"{pipyoff:0.##} :: {pipyoff / (yrange / 2) * 100:0.##}%";
            tbMaskLR.Text = $"{pipml:0.##}/{pipmr:0.##} :: {(pipml + pipmr) / 2 / maxmlr * 100:0.##}%";
            tbMaskTB.Text = $"{pipmt:0.##}/{pipmb:0.##} :: {(pipmt + pipmb) / 2 / maxmtb * 100:0.##}%";

            tbFineMode.Text = mode_FineControl ? "FINE" : "OFF";
            tbFineMode.Foreground = mode_FineControl ? Brushes.Yellow : Brushes.LightSlateGray;

            tbSlewDrive.Text = mode_SlewDrive ? "ON" : "OFF";
            tbSlewDrive.Foreground = mode_SlewDrive ? Brushes.Yellow : Brushes.LightSlateGray;

            tbPresetFillEnabled.Text = featurePresetEnabled ? "ACTIVE" : "DISABLED";
            tbPresetFillEnabled.Foreground = featurePresetEnabled ? Brushes.Yellow : Brushes.LightSlateGray;


            for (int i = 0; i < 8; i++)
            {
                pipfills[i].Background = Brushes.Gray;
                pipfills[i].BorderBrush = Brushes.Transparent;
                if (currentfillsource == ConvertButtonToSourceID(i + 1))
                {
                    pipfills[i].Background = Brushes.Red;
                    if (presetfillsource == i + 1)
                    {
                        pipfills[i].BorderBrush = Brushes.LimeGreen;
                    }
                }
                else if (presetfillsource == i + 1)
                {
                    pipfills[i].Background = Brushes.LimeGreen;
                }
            }

        }

        const double EPSILON = 0.01;

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

        //double pipmlr = 0;
        //double pipmtb = 0;

        double pipml = 0;
        double pipmr = 0;
        double pipmt = 0;
        double pipmb = 0;


        double truepipscale = 0.4;
        double truepipxoff = 9.6;
        double truepipyoff = 5.4;
        double truepipml = 0;
        double truepipmr = 0;
        double truepipmt = 0;
        double truepipmb = 0;

        double keyapipscale = 0;
        double keyapipx = 0;
        double keyapipy = 0;
        double keybpipscale = 0;
        double keybpipx = 0;
        double keybpipy = 0;

        int currentfillsource = 0;
        int presetfillsource = 0;



        double scalespeed = 0.1;
        double translationspeed = 0.1;
        double maskspeed = 0.1;


        bool mode_SlewDrive = true;
        bool featurePresetEnabled = false;

        bool mode_FineControl
        {
            get
            {
                return scalespeed < 0.1 || translationspeed < 0.1 || maskspeed < 0.1;
            }
            set
            {
                if (value)
                {
                    scalespeed = 0.01;
                    translationspeed = 0.01;
                    maskspeed = 0.01;
                }
                else
                {
                    scalespeed = 0.1;
                    translationspeed = 0.1;
                    maskspeed = 0.1;
                }
                UpdateUI();
            }
        }


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

            if (e.Key == Key.G)
            {
                mask_lr_tb_reset();
            }


            if (e.Key == Key.J)
            {
                mask_lr_slew_left();
            }
            if (e.Key == Key.L)
            {
                mask_lr_slew_right();
            }
            if (e.Key == Key.I)
            {
                mask_tb_slew_up();
            }
            if (e.Key == Key.K)
            {
                mask_tb_slew_down();
            }

            if (e.Key == Key.H)
            {
                mask_slew_center();
            }

            if (e.Key == Key.U)
            {
                toggle_slewdrive();
            }

            if (e.Key == Key.P)
            {
                toggle_presetFeature();
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

            /* ignore keyframes for now, replace with maskoffsets
            if (e.Key == Key.K)
            {
                enableKeyFrameFeatures = !enableKeyFrameFeatures;
                UpdateUI();
            }
            */


            // handle number keys for source switching
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Key == Key.D1)
                {
                    presetfillsource = 1;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D2)
                {
                    presetfillsource = 2;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D3)
                {
                    presetfillsource = 3;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D4)
                {
                    presetfillsource = 4;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D5)
                {
                    presetfillsource = 5;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D6)
                {
                    presetfillsource = 6;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D7)
                {
                    presetfillsource = 7;
                    sendcmd = true;
                    UpdateUI();
                }
                if (e.Key == Key.D8)
                {
                    presetfillsource = 8;
                    sendcmd = true;
                    UpdateUI();
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Key == Key.D1)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(1);
                    });
                }
                if (e.Key == Key.D2)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(2);
                    });
                }
                if (e.Key == Key.D3)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(3);
                    });
                }
                if (e.Key == Key.D4)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(4);
                    });
                }
                if (e.Key == Key.D5)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(5);
                    });
                }
                if (e.Key == Key.D6)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(6);
                    });
                }
                if (e.Key == Key.D7)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(7);
                    });
                }
                if (e.Key == Key.D8)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(8);
                    });
                }
            }
            else
            {
                if (e.Key == Key.D1)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(1);
                    });
                }
                if (e.Key == Key.D2)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(2);
                    });
                }
                if (e.Key == Key.D3)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(3);
                    });
                }
                if (e.Key == Key.D4)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(4);
                    });
                }
                if (e.Key == Key.D5)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(5);
                    });
                }
                if (e.Key == Key.D6)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(6);
                    });
                }
                if (e.Key == Key.D7)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(7);
                    });
                }
                if (e.Key == Key.D8)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ClickPreset(8);
                    });
                }
            }

            if (e.Key == Key.Enter)
            {
                // auto transition

                _parent.Dispatcher.Invoke(() =>
                {
                    _parent.TakeAutoTransition();
                });
            }

            if (e.Key == Key.Decimal)
            {
                // usk1 tie
                _parent.Dispatcher.Invoke(() =>
                {
                    _parent.ToggleTransKey1();
                });
            }

            if (e.Key == Key.RightShift)
            {
                mode_FineControl = true;
            }
        }




        private bool PIPIsAtTarget()
        {
            return pipscale == truepipscale && pipxoff == truepipxoff && pipyoff == truepipyoff && pipml == truepipml && pipmr == truepipmr && pipmt == truepipmt && pipmb == truepipmb && ConvertButtonToSourceID(presetfillsource) == currentfillsource;
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
                config.IsMasked = (pipml != 0 || pipmr != 0 || pipmt != 0 | pipmb != 0) ? 1 : 0;
                config.IsBordered = 0; // no border
                config.MaskTop = (float)pipmt;
                config.MaskBottom = (float)pipmb;
                config.MaskLeft = (float)pipml;
                config.MaskRight = (float)pipmr;

                SetPIPOnSwitcher?.Invoke(config);

                if (featurePresetEnabled && currentfillsource != presetfillsource)
                {
                    _parent.Dispatcher.Invoke(() =>
                    {
                        _parent.ChangeUSK1FillSource(presetfillsource);
                    });
                }
            }

        }

        private void set_as_key_a()
        {
            if (!enableKeyFrameFeatures)
            {
                return;
            }
            BMDUSKDVESettings config = new BMDUSKDVESettings();
            config.IsMasked = (pipmt != 0 || pipmb != 0 || pipml != 0 || pipmr != 0) ? 1 : 0;
            config.MaskTop = (float)pipmt;
            config.MaskBottom = (float)pipmb;
            config.MaskLeft = (float)pipml;
            config.MaskRight = (float)pipmr;
            config.IsBordered = 0; // no border
            config.KeyFrameA = new KeyFrameSettings()
            {
                PositionX = pipxoff,
                PositionY = pipyoff,
                SizeX = pipscale,
                SizeY = pipscale,
            };
            SetPIPKeyframeAOnSwitcher?.Invoke(config);
            _parent.ForceStateUpdateOnSwitcher();
        }

        private void set_as_key_b()
        {
            if (!enableKeyFrameFeatures)
            {
                return;
            }
            BMDUSKDVESettings config = new BMDUSKDVESettings();
            config.IsMasked = (pipmt != 0 || pipmb != 0 || pipml != 0 || pipmr != 0) ? 1 : 0;
            config.MaskTop = (float)pipmt;
            config.MaskBottom = (float)pipmb;
            config.MaskLeft = (float)pipml;
            config.MaskRight = (float)pipmr;
            config.IsBordered = 0; // no border
            config.KeyFrameB = new KeyFrameSettings()
            {
                PositionX = pipxoff,
                PositionY = pipyoff,
                SizeX = pipscale,
                SizeY = pipscale,
            };
            SetPIPKeyframeBOnSwitcher?.Invoke(config);
            _parent.ForceStateUpdateOnSwitcher();
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

        private void mask_lr_tb_reset()
        {
            pipml = minmlr;
            pipmr = minmlr;
            pipmt = minmtb;
            pipmb = minmtb;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }


        private void mask_lr_increase()
        {
            pipml += maskspeed;
            pipmr += maskspeed;
            if (pipml > maxmlr)
            {
                pipml = maxmlr;
            }
            if (pipmr > maxmlr)
            {
                pipmr = maxmlr;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_lr_decrease()
        {
            pipml -= maskspeed;
            pipmr -= maskspeed;
            if (pipml < minmlr)
            {
                pipml = minmlr;
            }
            if (pipmr < minmlr)
            {
                pipmr = minmlr;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_slew_center()
        {
            double pipmlravg = (pipml + pipmr) / 2;
            double pipmtbavg = (pipmt + pipmb) / 2;

            pipml = pipmlravg;
            pipmr = pipmlravg;
            pipmt = pipmtbavg;
            pipmb = pipmtbavg;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_lr_slew_left()
        {
            if (pipml == minmlr)
            {
                return;
            }
            pipml -= maskspeed;
            pipmr += maskspeed;
            if (pipml < minmlr + EPSILON)
            {
                pipml = minmlr;
            }
            if (pipmr + EPSILON > maxmlr)
            {
                pipmr = maxmlr;
            }

            if (mode_SlewDrive)
            {
                translate_right(maskspeed * pipscale);
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_lr_slew_right()
        {
            if (pipmr == minmlr)
            {
                return;
            }
            pipml += maskspeed;
            pipmr -= maskspeed;
            if (pipmr < minmlr + EPSILON)
            {
                pipmr = minmlr;
            }
            if (pipml + EPSILON > maxmlr)
            {
                pipml = maxmlr;
            }

            if (mode_SlewDrive)
            {
                translate_left(maskspeed * pipscale);
            }


            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_tb_slew_up()
        {
            if (pipmt == minmtb)
            {
                return;
            }
            pipmt -= maskspeed;
            pipmb += maskspeed;
            if (pipmt < minmtb + EPSILON)
            {
                pipmt = minmtb;
            }
            if (pipmb + EPSILON > maxmtb)
            {
                pipmb = maxmtb;
            }

            if (mode_SlewDrive)
            {
                translate_down(maskspeed * pipscale);
            }


            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_tb_slew_down()
        {
            if (pipmb == minmtb)
            {
                return;
            }
            pipmb -= maskspeed;
            pipmt += maskspeed;
            if (pipmb < minmtb + EPSILON)
            {
                pipmb = minmtb;
            }
            if (pipmt + EPSILON > maxmtb)
            {
                pipmt = maxmtb;
            }

            if (mode_SlewDrive)
            {
                translate_up(maskspeed * pipscale);
            }


            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }



        private void mask_tb_increase()
        {
            pipmt += maskspeed;
            pipmb += maskspeed;
            if (pipmt > maxmtb)
            {
                pipmt = maxmtb;
            }
            if (pipmb > maxmtb)
            {
                pipmb = maxmtb;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void mask_tb_decrease()
        {
            pipmt -= maskspeed;
            pipmb -= maskspeed;
            if (pipmt < minmtb)
            {
                pipmt = minmtb;
            }
            if (pipmb < minmtb)
            {
                pipmb = minmtb;
            }

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }


        private void translate_left()
        {
            translate_left(translationspeed);
        }
        private void translate_left(double speed)
        {
            pipxoff -= speed;
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
            translate_right(translationspeed);
        }
        private void translate_right(double speed)
        {
            pipxoff += speed;
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
            translate_down(translationspeed);
        }
        private void translate_down(double speed)
        {
            pipyoff -= speed;
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
            translate_up(translationspeed);
        }
        private void translate_up(double speed)
        {
            pipyoff += speed;
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
            if (e.Key == Key.RightShift)
            {
                mode_FineControl = false;
            }
        }


        private (double x, double y) calc_centers()
        {
            double fullhalfwidth = xrange / 2 * pipscale;
            double halfwidth = fullhalfwidth - ((pipml + pipmr) / 2 * pipscale);

            double fullhalfheight = yrange / 2 * pipscale;
            double halfheight = fullhalfheight - ((pipmt + pipmb) / 2 * pipscale);

            return (halfwidth, halfheight);
        }

        private (double cx, double cy) compensate_slew_centers()
        {
            double xslew = -((pipml - pipmr) / 2 * pipscale);
            double yslew = ((pipmt - pipmb) / 2 * pipscale);

            return (xslew, yslew);
        }


        private void to_top_left()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = -xrange / 2 + h.x + s.cx;
            pipyoff = yrange / 2 - h.y + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_top_right()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = xrange / 2 - h.x + s.cx;
            pipyoff = yrange / 2 - h.y + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_bottom_left()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = -xrange / 2 + h.x + s.cx;
            pipyoff = -yrange / 2 + h.y + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_bottom_right()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = xrange / 2 - h.x + s.cx;
            pipyoff = -yrange / 2 + h.y + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }

        private void to_top_mid()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = 0 + s.cx;
            pipyoff = yrange / 2 - h.y + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_bottom_mid()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = 0 + s.cx;
            pipyoff = -yrange / 2 + h.y + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_left_mid()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = -xrange / 2 + h.x + s.cx;
            pipyoff = 0 + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_right_mid()
        {
            var h = calc_centers();
            var s = compensate_slew_centers();

            pipxoff = xrange / 2 - h.x + s.cx;
            pipyoff = 0 + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();

        }

        private void to_center()
        {
            var s = compensate_slew_centers();
            pipxoff = 0 + s.cx;
            pipyoff = 0 + s.cy;

            SendCmd = true;
            SwitcherPIPUpdate();
            UpdateUI();
        }


        private void toggle_slewdrive()
        {
            mode_SlewDrive = !mode_SlewDrive;
            UpdateUI();
        }

        private void toggle_presetFeature()
        {
            featurePresetEnabled = !featurePresetEnabled;
            UpdateUI();
        }


    }
}
