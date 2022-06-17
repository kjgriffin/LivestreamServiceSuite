using DVIPProtocol.Protocol.Lib.Command;
using DVIPProtocol.Protocol.Lib.Inquiry;

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

        IPEndPoint IClient.Endpoint { get => m_endpoint; }

        void IClient.Stop()
        {
            m_cancel?.Cancel();
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
            // send it immediately
            m_client.NoDelay = true;

            /*
            Ok- so I've actually read the underlying VISCA protocol spec now.
            It's expected that you ONLY send one command at a time, and wait to recieve an ACK/Error
            or response (for inquiry)

            This means that using a blocking send/recieve approach here (rathern than async recieve)
            IS correct and exactly what should be done.

            The only issue here is we don't really handle network level disconnect/reconnect
            so its' entirely possible that we could block waiting for data after being disconnected

            The good thing here is that the workerThread is all that should be blocked...
            though I suppose any consuming code that blocks on the replyDelegate could block forever- so just don't do that

            Thus the only downfall is having a stale/dead/blocked client.
            The fix is probably to just kill it and start it again. Or just accept that no further communication/commands can ever
            be fired.
             */


            var stream = m_client.GetStream();

            // send data
            stream.Write(inq.Data);

            if (inq.ExpectedResponseSize > 0)
            {
                try
                {
                    // DVIP protocol specifies 2 bytes returned with response lenght
                    int sizehigh = 0;
                    int sizelow = 0;
                    uint dataSize = 0;
                    sizehigh = stream.ReadByte();
                    sizelow = stream.ReadByte();
                    if (sizehigh == -1 || sizelow == -1)
                    {
                        // unexpected end of stream too soon
                        // abort
                        inq.ReplyDelegate(new byte[0]);
                    }
                    // get data here
                    dataSize = (uint)(sizehigh << 8 | sizelow) - 2;
                    byte[] data = new byte[dataSize];
                    for (int x = 0; x < dataSize; x++)
                    {
                        int b = stream.ReadByte();
                        if (b != -1)
                        {
                            data[x] = (byte)b;
                        }
                        else
                        {
                            // unexpected end of stream too soon
                            // abort
                            inq.ReplyDelegate(new byte[0]);
                        }
                    }
                    inq.ReplyDelegate(data);
                }
                catch (System.IO.IOException ex)
                {
                    // send invalid response
                    inq.ReplyDelegate(new byte[0]);
                }
            }
            else
            {
                // send out response
                inq.ReplyDelegate(new byte[0]);
            }
        }

        public void SendRequest<TResp>(IInquiry<IResponse> inq, int expectedResponseLength, OnRequestReply reply)
        {
            m_commands.Enqueue(new DVIP_Inq
            {
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
                ExpectedResponseSize = 3, // eat the ACK
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
