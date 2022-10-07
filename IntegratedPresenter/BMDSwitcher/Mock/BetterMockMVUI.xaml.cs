using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.BMDSwitcher.Mock;

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

        public void RefreshUI(ICameraSourceProvider provider)
        {
            RunOnUI(() => Internal_RefreshUI(provider));
        }

        private void Internal_RefreshUI(ICameraSourceProvider provider)
        {
            // for now we'll just work on the simple pips
            for (int i = 0; i < 8; i++)
            {
                if (provider.TryGetSourceImage(m_pipWindowSourceRouting[i + 1], out BitmapImage img))
                {
                    m_simplePIPS[i].UpdateWithImage(img);
                }
                else
                {
                    // load black
                    m_simplePIPS[i].UpdateSource("");
                }
            }

            // TODO: update me/pv

        }







    }
}
