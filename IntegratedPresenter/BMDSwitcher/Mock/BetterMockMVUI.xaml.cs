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
            pip_ME_program.lBKGD_A.Fill = GetGeneratedSourceById((int)state.ProgramID, cameras);

            // handle usk1 layer

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

    }
}
