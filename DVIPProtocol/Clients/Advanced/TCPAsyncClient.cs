using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Advanced
{
    internal class TCPAsyncClient
    {

        IPEndPoint m_endpoint;
        Thread m_thread;
        TcpClient? m_client;

        public TCPAsyncClient(IPEndPoint endpoint)
        {
            m_endpoint = endpoint;
            m_thread = new Thread(OnStart_Internal);
            m_thread.Name = "TCPClientThread";
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        private void OnStart_Internal()
        {
            // main thread...




        }






    }
}
