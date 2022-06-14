using DVIPProtocol.Protocol.Lib.Command;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Advanced
{
    public class TCPCmdClient : ICmdClient
    {

        IPEndPoint m_endpoint;
        Thread? m_thread;
        TcpClient? m_client;
        CancellationTokenSource m_cancel;
        bool m_init = false;
        ConcurrentQueue<byte[]> m_commands;
        ManualResetEvent m_cmdAvail;

        public TCPCmdClient(IPEndPoint endpoint)
        {
            m_endpoint = endpoint;
            m_commands = new ConcurrentQueue<byte[]>();
            m_cmdAvail = new ManualResetEvent(false);
        }

        IPEndPoint IClient.Endpoint { get; set; }

        void IClient.Stop()
        {
            m_cancel?.Cancel();
            if (!m_init)
            {
                Dispose_Internal();
            }
        }

        private void Dispose_Internal()
        {
            m_cancel?.Dispose();
            m_client?.Close();
            m_client?.Dispose();
        }

        void IClient.Init()
        {
            m_init = true;
            m_cancel = new CancellationTokenSource();
            m_thread = new Thread(OnStart_Internal);
            m_thread.Name = "TCPCmdClientThread";
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        private void OnStart_Internal()
        {
            Run();
        }

        private void Run()
        {
            while (true)
            {
                if (m_cancel.IsCancellationRequested == true)
                {
                    //... need to stop
                    Dispose_Internal();
                }

                // to prevent blocking forever (and missing the cancel)
                // we will wait on a mre, not the collection
                // once we're signaled that the collectio has data
                // consume as much as possible
                WaitHandle.WaitAny(new[] { m_cancel.Token.WaitHandle, m_cmdAvail });

                // wait for command to send
                while (m_commands.TryDequeue(out var cmd))
                {
                    // send commands
                    SendTCPCommand(cmd);
                }

                m_cmdAvail.Reset();
            }
        }

        void ICmdClient.SendCommand(ICommand cmd)
        {
            m_commands?.Enqueue(cmd.PackagePayload());
            m_cmdAvail.Set();
        }

        private void SendTCPCommand(byte[] data)
        {
            if (m_client == null)
            {
                m_client = new TcpClient();
            }
            if (!m_client.Connected)
            {
                try
                {
                    m_client.Connect(m_endpoint);
                }
                catch (Exception ex)
                {
                    // ... reject for now
                    return;
                }
            }
            var stream = m_client.GetStream();
            stream.Write(data);
        }

    }
}
