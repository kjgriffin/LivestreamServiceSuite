using System;
using VonEX;

namespace HyperdeckControl
{

    public delegate void StringMessageArgs(object sender, string message);
    public class HyperdeckController
    {

        public event StringMessageArgs OnMessageFromHyperDeck;

        SlaveConnection connection;

        private bool m_isConnected = false;
        public bool IsConnected { get => m_isConnected; }

        public HyperdeckController()
        {

        }

        public void Connect(string hostname)
        {
            connection = new SlaveConnection();
            connection.OnConnectionFromMaster += Connection_OnConnectionFromMaster;
            connection.OnDataRecieved += Connection_OnDataRecieved;
            /* https://documents.blackmagicdesign.com/UserManuals/HyperDeckManual.pdf
               Hyperdeck server listens for tcp connection on port 9993 
            */
            connection.Connect(hostname, 9993);

        }

        private void Connection_OnDataRecieved(string data)
        {
            // for now just assume commands work?
            OnMessageFromHyperDeck?.Invoke(this, data);
        }

        private void Connection_OnConnectionFromMaster(string senderip, bool connected)
        {
            m_isConnected = connected;
        }

        
        public void RecordStart()
        {
            if (IsConnected)
            {
                connection.SendStringASCII("record\r\n");
            }
        }

        public void HyperdeckStop()
        {
            if (IsConnected)
            {
                connection.SendStringASCII("stop\r\n");
            }
        }




    }
}
