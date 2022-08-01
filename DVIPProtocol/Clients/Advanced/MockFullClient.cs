using DVIPProtocol.Protocol.Lib.Command;
using DVIPProtocol.Protocol.Lib.Inquiry;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Advanced
{

    public class MockFullClient : IFullClient
    {

        IPEndPoint m_endpoint;
        Thread? m_thread;
        //TcpClient? m_client;
        CancellationTokenSource m_cancel;
        bool m_init = false;
        ConcurrentQueue<DVIP_Inq> m_commands;
        ManualResetEvent m_cmdAvail;

        public MockFullClient(IPEndPoint endpoint)
        {
            m_endpoint = endpoint;
            m_commands = new ConcurrentQueue<DVIP_Inq>();
            m_cmdAvail = new ManualResetEvent(false);
        }

        IPEndPoint IClient.Endpoint { get => m_endpoint; }

        void IClient.Stop()
        {
            m_cancel?.Cancel();
        }

        private void Dispose_Internal()
        {
            m_cancel?.Dispose();
        }

        void IClient.Init()
        {
            m_init = true;
            m_cancel = new CancellationTokenSource();
            m_thread = new Thread(OnStart_Internal);
            m_thread.Name = "MockFullClientThread";
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        private void OnStart_Internal()
        {
            Run();
        }

        private void Run()
        {
#if DEBUG
            Debug.WriteLine($"[{Thread.CurrentThread.Name}] Started.");
#endif

            while (!m_cancel.IsCancellationRequested)
            {
                // to prevent blocking forever (and missing the cancel)
                // we will wait on a mre, not the collection
                // once we're signaled that the collectio has data
                // consume as much as possible
                WaitHandle.WaitAny(new[] { m_cancel.Token.WaitHandle, m_cmdAvail });

                if (!m_cancel.IsCancellationRequested)
                {
                    // wait for command to send
                    while (m_commands.TryDequeue(out var inq))
                    {
                        // send commands
                        SendAndWaitTCPCommand(inq);
                    }
                    m_cmdAvail.Reset();
                }
            }
            Dispose_Internal();
        }

        private void SendAndWaitTCPCommand(DVIP_Inq inq)
        {
            // fake sending the data, and just respond immediately
#if DEBUG
            Debug.WriteLine($"[{Thread.CurrentThread.Name}] Sending data: {BitConverter.ToString(inq.Data)}");
#endif

            byte[] respBuffer = new byte[512];
            inq.ReplyDelegate(inq?.GoodFakeResp ?? respBuffer);
        }

        public void SendRequest<TResp>(IInquiry<IResponse> inq, int expectedResponseLength, OnRequestReply reply)
        {
            m_commands.Enqueue(new DVIP_Inq
            {
                GoodFakeResp = inq.Parse_FakeGood().Data,
                BadFakeResp = inq.Parse_FakeBad().Data,
                Data = inq.PackagePayload(),
                ExpectedResponseSize = expectedResponseLength,
                ReplyDelegate = reply
            });
            m_cmdAvail.Set();
        }

        public void SendCommand(ICommand cmd)
        {
            // turn cmd into inq and just ignore result
            // ... though techincally there should be an ACK response, but who cares?
            m_commands.Enqueue(new DVIP_Inq
            {
                Data = cmd.PackagePayload(),
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
