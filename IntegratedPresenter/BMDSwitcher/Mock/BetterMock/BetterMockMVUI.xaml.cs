using BMDSwitcherAPI;

using IntegratedPresenter.BMDSwitcher;
using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.BMDSwitcher.Mock;
using IntegratedPresenter.Main;

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter.BMDSwitcher.Mock
{
    /// <summary>
    /// Interaction logic for BetterMockMVUI.xaml
    /// </summary>
    public partial class BetterMockMVUI : Window
    {

        #region Helpers

        private void RunOnUI(Action work)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(work);
                return;
            }
            work.Invoke();
        }

        #endregion


        BMDSwitcherConfigSettings _cfg;

        string MEpvBKGDBus = "A";

        MockMV_Simple_PIP[] m_simplePIPS;
        Dictionary<int, int> m_pipWindowSourceRouting = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2,
            [3] = 3,
            [4] = 4,
            [5] = 5,
            [6] = 6,
            [7] = 7,
            [8] = 8,
        };


        public BetterMockMVUI()
        {
            InitializeComponent();

            m_simplePIPS = new MockMV_Simple_PIP[8]
            {
                pip_1,
                pip_2,
                pip_3,
                pip_4,
                pip_5,
                pip_6,
                pip_7,
                pip_8,
            };

            // turn off audio
            for (int i = 0; i < 8; i++)
            {
                m_simplePIPS[i].videoPlayerA.Volume = 0;
            }

        }


        public void ReConfigure(BMDSwitcherConfigSettings _cfg)
        {
            RunOnUI(() => Internal_ReConfigure(_cfg));
        }

        private void Internal_ReConfigure(BMDSwitcherConfigSettings _cfg)
        {
            this._cfg = _cfg;

            m_pipWindowSourceRouting[1] = _cfg.MultiviewerConfig.Window2;
            m_pipWindowSourceRouting[2] = _cfg.MultiviewerConfig.Window3;
            m_pipWindowSourceRouting[3] = _cfg.MultiviewerConfig.Window4;
            m_pipWindowSourceRouting[4] = _cfg.MultiviewerConfig.Window5;
            m_pipWindowSourceRouting[5] = _cfg.MultiviewerConfig.Window6;
            m_pipWindowSourceRouting[6] = _cfg.MultiviewerConfig.Window7;
            m_pipWindowSourceRouting[7] = _cfg.MultiviewerConfig.Window8;
            m_pipWindowSourceRouting[8] = _cfg.MultiviewerConfig.Window9;

            foreach (var map in m_pipWindowSourceRouting)
            {
                m_simplePIPS[map.Key - 1].tbPIPName.Text = _cfg.Routing.FirstOrDefault(x => x.PhysicalInputId == map.Value).LongName;
            }
        }

        public void RefreshUI(ICameraSourceProvider cameras, ISwitcherStateProvider switcher)
        {
            RunOnUI(() => Internal_RefreshUI(cameras, switcher));
        }

        private void Internal_RefreshUI(ICameraSourceProvider cameras, ISwitcherStateProvider switcher)
        {
            var state = switcher.GetState();

            // for now we'll just work on the simple pips
            for (int i = 0; i < 8; i++)
            {
                if (cameras.TryGetSourceImage(m_pipWindowSourceRouting[i + 1], out BitmapImage img))
                {
                    m_simplePIPS[i].UpdateWithImage(img);
                }
                else if (cameras.TryGetSourceVideo(m_pipWindowSourceRouting[i + 1], out string path))
                {
                    m_simplePIPS[i].UpdateWithVideo(path);
                }
                else
                {
                    // load black
                    m_simplePIPS[i].UpdateSource("");
                }

                // update on-air state
                int pipSID = m_pipWindowSourceRouting[i + 1];
                // for now assumes hard-coded DSK1 fill source
                // ignore DSK2 for now
                if (state.ProgramID == pipSID
                    || (state.DSK1OnAir && pipSID == _cfg.DownstreamKey1Config.InputFill)
                    || (state.USK1OnAir && state.USK1FillSource == pipSID))
                {
                    m_simplePIPS[i].bdr.BorderBrush = Brushes.Red;
                }
                else if (state.PresetID == pipSID
                         || (state.DSK1Tie && pipSID == _cfg.DownstreamKey1Config.InputFill)
                         || (state.TransNextKey1 && state.USK1FillSource == pipSID))
                {
                    m_simplePIPS[i].bdr.BorderBrush = Brushes.LimeGreen;
                }
                else
                {
                    m_simplePIPS[i].bdr.BorderBrush = Brushes.LightGray;
                }
            }

            // TODO: update me/pv

            // build preview/me bkgd layers

            pip_PV_preset.pvBKDG.Fill = GetGeneratedSourceById((int)state.PresetID, cameras);

            if (!state.InTransition) // else?? probably doing stuff somewhere else
            {
                if (MEpvBKGDBus == "A")
                {
                    pip_ME_program.lBKGD_A.Fill = GetGeneratedSourceById((int)state.ProgramID, cameras);
                    pip_ME_program.lBKGD_B.Fill = Brushes.Transparent;
                }
                else if (MEpvBKGDBus == "B")
                {
                    pip_ME_program.lBKGD_B.Fill = GetGeneratedSourceById((int)state.ProgramID, cameras);
                    pip_ME_program.lBKGD_A.Fill = Brushes.Transparent;
                }
            }


            // handle usk1 layer
            // here we can handle non-fade's (fades are handled by ME animation Fade request)

            // it can be the fill source changes even while fading
            if (state.USK1OnAir) // goes on-air immediately upon animation
            {
                pip_ME_program.lUSK1_A.Fill = GetGeneratedSourceById((int)state.USK1FillSource, cameras);
                pip_PV_preset.pvUSK1.Fill = Brushes.Transparent;
            }
            else
            {
                pip_ME_program.lUSK1_A.Fill = Brushes.Transparent;
                if (state.TransNextKey1)
                {
                    pip_PV_preset.pvUSK1.Fill = GetGeneratedSourceById((int)state.USK1FillSource, cameras);
                }
                else
                {
                    pip_PV_preset.pvUSK1.Fill = Brushes.Transparent;
                }
            }
            // always update pip position
            pip_ME_program.SetPIPPosition(state.DVESettings);
            pip_PV_preset.SetPIPPosition(state.DVESettings);

            // handle dsk1 layer
            if (state.DSK1OnAir)
            {
                // TODO: figure out if in transition...

                // show it
                pip_ME_program.lDSK1_A.Fill = GetGeneratedSourceById(GetSlideSourceID(), cameras);

                // hide it for preview
                pip_PV_preset.pvDSK1.Fill = Brushes.Transparent;
            }
            else
            {
                // TODO: handle in transition
                pip_ME_program.lDSK1_A.Fill = Brushes.Transparent;

                // try tie?
                if (state.DSK1Tie)
                {
                    // show it
                    pip_PV_preset.pvDSK1.Fill = GetGeneratedSourceById(GetSlideSourceID(), cameras);
                }
                else
                {
                    pip_PV_preset.pvDSK1.Fill = Brushes.Transparent;
                }
            }

            // handle dsk2 layer

        }

        private Brush GetGeneratedSourceById(int id, ICameraSourceProvider cams)
        {
            // check if we're generating it directly with a PIP,
            // otherwise it's a switcher internal source (technically media players (we'll ignore those for now), color bars, color gernerators or black)

            if (id > 0 && id < 9)
            {
                // translate id's
                var tid = m_pipWindowSourceRouting.First(x => x.Value == id).Key;

                // generated by a pip
                return m_simplePIPS[tid - 1].GetOutput();
            }
            else
            {
                cams.TryGetSourceImage(id, out var img);
                return new ImageBrush(img);
            }
        }

        private int GetSlideSourceID()
        {
            return _cfg.Routing.FirstOrDefault(x => x.KeyName == "slide").PhysicalInputId;
        }

        internal void SlideVideoPlaybackUpdate(MainWindow.MediaPlaybackEventArgs e)
        {
            RunOnUI(() => Internal_SlideVideoPlaybackUpdate(e));
        }

        internal void Internal_SlideVideoPlaybackUpdate(MainWindow.MediaPlaybackEventArgs e)
        {
            // for now only handle 'slide' videos not 'akey' videos
            // find slide pip
            int pid = _cfg.Routing.FirstOrDefault(x => x.KeyName == "slide").PhysicalInputId;
            int sid = m_pipWindowSourceRouting.FirstOrDefault(x => x.Value == pid).Key;

            m_simplePIPS[sid - 1].UpdatePlaybackState(e);
        }

        /// <summary>
        /// Start a fade in/out
        /// </summary>
        /// <param name="dir">1 = in , -1 = out</param>
        internal void StartDSK1Fade(int dir, ISwitcherStateProvider switcher)
        {
            RunOnUI(() => Internal_StartDSK1Fade(dir, switcher));
        }

        private void Internal_StartDSK1Fade(int dir, ISwitcherStateProvider switcher)
        {
            var dur = TimeSpan.FromSeconds(_cfg.DownstreamKey1Config.Rate / _cfg.VideoSettings.VideoFPS);
            var fade = new DoubleAnimation()
            {
                From = dir == 1 ? 0 : 1.0,
                To = dir == 1 ? 1.0 : 0,
                Duration = dur,
            };
            Storyboard.SetTarget(fade, pip_ME_program.lDSK1_A);
            Storyboard.SetTargetProperty(fade, new PropertyPath(Rectangle.OpacityProperty));
            var sb = new Storyboard();
            sb.Children.Add(fade);
            sb.Begin();
            Task.Run(async () =>
            {
                await Task.Delay((int)dur.TotalMilliseconds);
                RunOnUI(() =>
                {
                    pip_ME_program.lUSK1_A.Opacity = dir == 1 ? 1 : 0;
                    sb.Stop();
                    switcher.ReportDSKFadeComplete(1, dir == 1 ? 1 : 0);
                });
            });

        }
        internal void StartMEFade(ISwitcherStateProvider switcher, ICameraSourceProvider cameras, int key1Dir)
        {
            RunOnUI(() => Internal_StartMEFade(switcher, cameras, key1Dir));
        }

        private void Internal_StartMEFade(ISwitcherStateProvider switcher, ICameraSourceProvider cameras, int key1Dir)
        {

            var origState = switcher.GetState();

            int FromSID = (int)origState.ProgramID;
            int ToSID = (int)origState.PresetID;


            var dur = TimeSpan.FromSeconds(_cfg.MixEffectSettings.Rate / _cfg.VideoSettings.VideoFPS);
            var sb = new Storyboard();


            if (origState.TransNextBackground && FromSID != ToSID)
            {
                // find the inactive bus and set it to the origPresetSource
                if (MEpvBKGDBus == "A")
                {
                    pip_ME_program.lBKGD_B.Opacity = 0;
                    pip_ME_program.lBKGD_B.Fill = GetGeneratedSourceById(ToSID, cameras);
                }
                else if (MEpvBKGDBus == "B")
                {
                    pip_ME_program.lBKGD_A.Opacity = 0;
                    pip_ME_program.lBKGD_A.Fill = GetGeneratedSourceById(ToSID, cameras);
                }


                var fadein = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = dur,
                };
                var fadeout = new DoubleAnimation()
                {
                    From = 1,
                    To = 0,
                    Duration = dur,
                };

                // build a fade animation for them to swap between
                if (MEpvBKGDBus == "A")
                {
                    Storyboard.SetTarget(fadeout, pip_ME_program.lBKGD_A);
                    Storyboard.SetTargetProperty(fadeout, new PropertyPath(Rectangle.OpacityProperty));

                    Storyboard.SetTarget(fadein, pip_ME_program.lBKGD_B);
                    Storyboard.SetTargetProperty(fadein, new PropertyPath(Rectangle.OpacityProperty));
                }
                else if (MEpvBKGDBus == "B")
                {
                    Storyboard.SetTarget(fadeout, pip_ME_program.lBKGD_B);
                    Storyboard.SetTargetProperty(fadeout, new PropertyPath(Rectangle.OpacityProperty));

                    Storyboard.SetTarget(fadein, pip_ME_program.lBKGD_A);
                    Storyboard.SetTargetProperty(fadein, new PropertyPath(Rectangle.OpacityProperty));
                }

                sb.Children.Add(fadein);
                sb.Children.Add(fadeout);
            }



            // usk1 goes through the ME engine, so any animations should be done here
            if (origState.TransNextKey1 && key1Dir != 0)
            {
                var usk1Animation = new DoubleAnimation()
                {
                    From = key1Dir == 1 ? 0 : 1,
                    To = key1Dir == 1 ? 1 : 0,
                    Duration = dur,
                };

                Storyboard.SetTarget(usk1Animation, pip_ME_program.lUSK1_A);
                Storyboard.SetTargetProperty(usk1Animation, new PropertyPath(Rectangle.OpacityProperty));
                sb.Children.Add(usk1Animation);
            }


            sb.Begin();

            Task.Run(async () =>
            {
                await Task.Delay((int)dur.TotalMilliseconds);
                RunOnUI(() =>
                {
                    // cleanup and report state of everything done
                    sb.Stop();

                    if (origState.TransNextBackground && FromSID != ToSID)
                    {
                        if (MEpvBKGDBus == "A")
                        {
                            pip_ME_program.lBKGD_B.Opacity = 1;
                            pip_ME_program.lBKGD_A.Opacity = 0;
                            pip_ME_program.lBKGD_A.Fill = Brushes.Transparent;
                            MEpvBKGDBus = "B";

                            // setup z-ordering so we always do transitions with the right source on top
                            Panel.SetZIndex(pip_ME_program.lBKGD_A, 1);
                            Panel.SetZIndex(pip_ME_program.lBKGD_B, 2);
                        }
                        else if (MEpvBKGDBus == "B")
                        {
                            pip_ME_program.lBKGD_A.Opacity = 1;
                            pip_ME_program.lBKGD_B.Opacity = 0;
                            pip_ME_program.lBKGD_B.Fill = Brushes.Transparent;
                            MEpvBKGDBus = "A";

                            // setup z-ordering so we always do transitions with the right source on top
                            Panel.SetZIndex(pip_ME_program.lBKGD_A, 2);
                            Panel.SetZIndex(pip_ME_program.lBKGD_B, 1);
                        }
                    }

                    bool newUSKstate = origState.USK1OnAir;
                    var newPGM = (int)origState.ProgramID;
                    var newPST = (int)origState.PresetID;
                    if (origState.TransNextKey1)
                    {
                        // report usk1 state
                        if (key1Dir == 1)
                        {
                            newUSKstate = true;
                            pip_ME_program.lUSK1_A.Opacity = 1;
                        }
                        else if (key1Dir == -1)
                        {
                            newUSKstate = false;
                            pip_ME_program.lUSK1_A.Opacity = 0;
                        }
                    }
                    if (origState.TransNextBackground)
                    {
                        newPGM = ToSID;
                        newPST = FromSID;
                    }
                    switcher.ReportMETransitionComplete(newPGM, newPST, newUSKstate);
                });
            });

        }

        internal void StartFTB(ISwitcherStateProvider switcher, bool onair)
        {
            RunOnUI(() => Internal_StartFTB(switcher, onair));
        }

        private void Internal_StartFTB(ISwitcherStateProvider switcher, bool onair)
        {
            var origState = switcher.GetState();

            var dur = TimeSpan.FromSeconds(_cfg.MixEffectSettings.Rate / _cfg.VideoSettings.VideoFPS);
            var fade = new DoubleAnimation()
            {
                From = origState.FTB ? 1.0 : 0,
                To = onair ? 1.0 : 0,
                Duration = dur,
            };
            Storyboard.SetTarget(fade, pip_ME_program.lFTB);
            Storyboard.SetTargetProperty(fade, new PropertyPath(Rectangle.OpacityProperty));
            var sb = new Storyboard();
            sb.Children.Add(fade);
            sb.Begin();
            Task.Run(async () =>
            {
                await Task.Delay((int)dur.TotalMilliseconds);
                RunOnUI(() =>
                {
                    pip_ME_program.lFTB.Opacity = onair ? 1 : 0;
                    sb.Stop();
                    switcher.ReportFTBComplete(onair);
                });
            });

        }

        internal void UpdatePIPAnimations(ICameraSourceProvider camDriver)
        {
            RunOnUI(() => Internal_UpdatePIPAnimations(camDriver));
        }
        private void Internal_UpdatePIPAnimations(ICameraSourceProvider camDriver)
        {
            // find all the live cameras
            // update the pip's as requested by the state for animation

            for (int i = 0; i < 8; i++)
            {
                if (camDriver.TryGetLiveCamState(m_pipWindowSourceRouting[i + 1], out var live))
                {

                    if (live.ReqZoom)
                    {
                        live.ReqZoom = false;

                        // setup and begin the zoom animation

                        double setupStart = 1;
                        double setupEnd = 1;

                        if (live.ZDir == -1)
                        {
                            setupStart = 1;
                            setupEnd = 4;
                        }
                        else if (live.ZDir == 1)
                        {
                            setupStart = 1;
                            setupEnd = 0;
                        }

                        var zXAnim_setup = new DoubleAnimation()
                        {
                            From = setupStart,
                            To = setupEnd,
                            Duration = TimeSpan.FromMilliseconds(live.ZSetupMS),
                        };
                        var zYAnim_setup = new DoubleAnimation()
                        {
                            From = setupStart,
                            To = setupEnd,
                            Duration = TimeSpan.FromMilliseconds(live.ZSetupMS),
                        };

                        Storyboard.SetTarget(zXAnim_setup, m_simplePIPS[i].bkgdSrc);
                        Storyboard.SetTargetProperty(zXAnim_setup, new PropertyPath("RenderTransform.ScaleX"));
                        Storyboard.SetTarget(zYAnim_setup, m_simplePIPS[i].bkgdSrc);
                        Storyboard.SetTargetProperty(zYAnim_setup, new PropertyPath("RenderTransform.ScaleY"));
                        var sb = new Storyboard();
                        sb.Children.Add(zXAnim_setup);
                        sb.Children.Add(zYAnim_setup);
                        m_simplePIPS[i].bkgdSrcZoomScale.ScaleX = 0;
                        m_simplePIPS[i].bkgdSrcZoomScale.ScaleY = 0;
                        sb.Begin();

                        // keep going
                        Internal_FinishZoomTask(i, live.ZDir, live.ZRunMS);

                    }
                    else if (!live.Zooming)
                    {
                        m_simplePIPS[i].bkgdSrcZoomScale.ScaleX = 1;
                        m_simplePIPS[i].bkgdSrcZoomScale.ScaleY = 1;
                    }

                    if (live.ReqMove)
                    {
                        live.ReqMove = false;

                        // setup and begin the move animation

                        // this is simple- just animate the opacity of the fill from 0 to 1 over the course of the run 
                        /*
                        var fade = new DoubleAnimation()
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(live.MRunMS),
                        };
                        Storyboard.SetTarget(fade, m_simplePIPS[i].bkgdSrc);
                        Storyboard.SetTargetProperty(fade, new PropertyPath(Grid.OpacityProperty));
                        var sb = new Storyboard();
                        sb.Children.Add(fade);
                        m_simplePIPS[i].bkgdSrc.Opacity = 0;
                        m_simplePIPS[i].bkgdNoise.Opacity = 1;
                        sb.Begin();
                        */

                        // since we're using 'run' actions that have a zoom program with the preset move
                        // I think we can show 'movement' by a slight overlay of stuff
                        // TODO::: just got idea: add a 'camera shake!!!!!' (probably use some sort of repeating animation for duration that does a translate transform x)
                        m_simplePIPS[i].bkgdSrc.Opacity = 1;
                        m_simplePIPS[i].bkgdNoise.Opacity = 0.4;
                    }
                    else if (!live.Moving)
                    {
                        // park the shot with full opacity
                        m_simplePIPS[i].bkgdNoise.Opacity = 0;
                        m_simplePIPS[i].bkgdSrc.Opacity = 1;
                    }

                }
            }
        }

        private void Internal_FinishZoomTask(int iPIP, int zDir, int zMS)
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(zMS));
                RunOnUI(() => Internal_FinishZoomAnimation(zDir, zMS, iPIP));
            });
        }

        private void Internal_FinishZoomAnimation(int zDir, int zMS, int iPIP)
        {
            double zoomStart = 1;
            double zoomEnd = 1;

            if (zDir == -1)
            {
                zoomStart = 4;
                zoomEnd = 1;
            }
            else if (zDir == 1)
            {
                zoomStart = 0;
                zoomEnd = 1;
            }

            var zXAnim_run = new DoubleAnimation()
            {
                From = zoomStart,
                To = zoomEnd,
                Duration = TimeSpan.FromMilliseconds(zMS),
            };
            var zYAnim_run = new DoubleAnimation()
            {
                From = zoomStart,
                To = zoomEnd,
                Duration = TimeSpan.FromMilliseconds(zMS),
            };

            var sb = new Storyboard();
            Storyboard.SetTarget(zXAnim_run, m_simplePIPS[iPIP].bkgdSrc);
            Storyboard.SetTargetProperty(zXAnim_run, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(zYAnim_run, m_simplePIPS[iPIP].bkgdSrc);
            Storyboard.SetTargetProperty(zYAnim_run, new PropertyPath("RenderTransform.ScaleY"));

            sb.Children.Add(zXAnim_run);
            sb.Children.Add(zYAnim_run);
            m_simplePIPS[iPIP].bkgdSrcZoomScale.ScaleX = zoomStart;
            m_simplePIPS[iPIP].bkgdSrcZoomScale.ScaleY = zoomStart;

            sb.Begin();
        }
    }
}
