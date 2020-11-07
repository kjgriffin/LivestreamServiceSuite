using HyperdeckControl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrated_Presenter.BMDHyperdeck
{
    public class BMDHyperdeckManager
    {
        HyperdeckController m_hyperdeckController;     

        public BMDHyperdeckManager()
        {
            m_hyperdeckController = new HyperdeckController();
        }

        public void Connect()
        {
            Connection connectionwindow = new Connection("Connect To Hyperdeck", "Hyperdeck Hostname:", "");
            connectionwindow.ShowDialog();
            string hostname = connectionwindow.IP;

            m_hyperdeckController?.Connect(hostname);

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
