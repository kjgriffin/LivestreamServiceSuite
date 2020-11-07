using HyperdeckControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDHyperdeck
{
    public class BMDHyperdeckManager
    {
        HyperdeckController m_hyperdeckController;

        public event StringMessageArgs OnMessageFromHyperDeck;

        public BMDHyperdeckManager()
        {
            m_hyperdeckController = new HyperdeckController();
        }

        public void Connect()
        {
            Connection connectionwindow = new Connection("Connect To Hyperdeck", "Hyperdeck Hostname:", "192.168.2.121");
            connectionwindow.ShowDialog();
            string hostname = connectionwindow.IP;

            m_hyperdeckController.OnMessageFromHyperDeck += M_hyperdeckController_OnMessageFromHyperDeck;

            m_hyperdeckController?.Connect(hostname);

        }

        private void M_hyperdeckController_OnMessageFromHyperDeck(object sender, string message)
        {
            OnMessageFromHyperDeck?.Invoke(this, message);
        }

        public void StartRecording()
        {
            m_hyperdeckController?.RecordStart();
        }

        public void StopRecording()
        {
            m_hyperdeckController?.HyperdeckStop();
        }




    }
}
