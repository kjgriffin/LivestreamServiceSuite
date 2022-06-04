﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Advanced
{

    public class TCPFullClient : IFullClient
    {

        IPEndPoint m_endpoint;
        Thread? m_thread;
        TcpClient? m_client;
        CancellationTokenSource m_cancel;
        bool m_init = false;
        ConcurrentQueue<DVIP_Inq> m_commands;
        ManualResetEvent m_cmdAvail;

        public TCPFullClient(IPEndPoint endpoint)
        {
            m_endpoint = endpoint;
            m_commands = new ConcurrentQueue<DVIP_Inq>();
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
            m_thread.Name = "TCPFullClientThread";
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
                while (m_commands.TryDequeue(out var inq))
                {
                    // send commands
                    SendAndWaitTCPCommand(inq);
                }

                m_cmdAvail.Reset();
            }
        }

        private void SendAndWaitTCPCommand(DVIP_Inq inq)
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
            stream.Write(inq.Data);

            // wait for read...
            byte[] respBuffer = new byte[512];

            int read = 0;

            // ok- so here we're going to play fast and loose...
            // assumption is that the network:
            // 1. works (if not, I'll claim there's bigger problems I can't solve)
            // 2. is fast (relatively)
            // 3. all communications are rather small
            if (stream.DataAvailable)
            {
                stream.Read(respBuffer, 0, 512);
            }

            // send out response
            inq.ReplyDelegate(respBuffer);
        }

        public void SendRequest(byte[] data, int expectedResponseLength, OnRequestReply reply)
        {
            m_commands.Enqueue(new DVIP_Inq
            {
                Data = data,
                ExpectedResponseSize = expectedResponseLength,
                ReplyDelegate = reply
            });
            m_cmdAvail.Set();
        }

        public void SendCommand(byte[] data)
        {
            // turn cmd into inq and just ignore result
            // ... though techincally there should be an ACK response, but who cares?
            m_commands.Enqueue(new DVIP_Inq
            {
                Data = data,
                ExpectedResponseSize = 0,
                ReplyDelegate = IgnoreCommand,
            });
            m_cmdAvail.Set();
        }

        private void IgnoreCommand(byte[] resp)
        {
            // .... ?
            // log it perhaps, or just toss it?
            // may be a use for /devnull as a service
            // look into a subscription https://devnull-as-a-service.com/
        }
    }
}