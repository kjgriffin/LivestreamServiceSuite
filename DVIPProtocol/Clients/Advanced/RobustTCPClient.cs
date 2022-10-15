using DVIPProtocol.Clients.Execution;
using DVIPProtocol.Protocol.Lib.Command;
using DVIPProtocol.Protocol.Lib.Inquiry;

using log4net;

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

    public class RobustTCPClient : IRobustClient
    {

        IPEndPoint m_endpoint;
        Thread? m_thread;
        TcpClient? m_client;
        CancellationTokenSource m_cancel;
        bool m_init = false;
        ConcurrentQueue<IRobustWork> m_commands;
        ManualResetEvent m_cmdAvail;
        ILog? m_log;

        public RobustTCPClient(IPEndPoint endpoint, ILog log)
        {
            m_endpoint = endpoint;
            m_commands = new ConcurrentQueue<IRobustWork>();
            m_cmdAvail = new ManualResetEvent(false);
            m_log = log;
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
            m_log?.Info($"RobustTCPClient for {m_endpoint.ToString()} starting.");
            m_init = true;
            m_cancel = new CancellationTokenSource();
            m_thread = new Thread(OnStart_Internal);
            m_thread.Name = "RobustTCPClientThread";
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        private void OnStart_Internal()
        {
            Run();
        }

        private void Run()
        {
            m_log?.Info($"[{m_endpoint.ToString()}] Started worker loop");
            while (!m_cancel.IsCancellationRequested)
            {

                // to prevent blocking forever (and missing the cancel)
                // we will wait on a mre, not the collection
                // once we're signaled that the collection has data
                // consume as much as possible
                m_log?.Info($"[{m_endpoint.ToString()}] waiting for work...");
                WaitHandle.WaitAny(new[] { m_cancel.Token.WaitHandle, m_cmdAvail });


                if (!m_cancel.IsCancellationRequested)
                {
                    // wait for command to send
                    m_log?.Info($"[{m_endpoint.ToString()}] executing work.");
                    while (m_commands.TryDequeue(out var workItem))
                    {
                        ForceConection();
                        // send commands
                        m_log?.Info($"[{m_endpoint.ToString()}] send tcp cmd.");
                        DoWork(workItem);
                    }

                    m_cmdAvail.Reset();
                }
            }
            Dispose_Internal();
        }


        private void ForceConection()
        {
            if (m_client == null)
            {
                m_log?.Info($"[{m_endpoint.ToString()}] creating new TCP client.");
                m_client = new TcpClient();
            }
            if (!m_client.Connected)
            {
                try
                {
                    m_log?.Info($"[{m_endpoint.ToString()}] re-connect client.");
                    m_client.Connect(m_endpoint);
                    m_log?.Info($"[{m_endpoint.ToString()}] client connected.");
                }
                catch (Exception ex)
                {
                    // ... reject for now
                    return;
                }
            }
            m_log?.Info($"[{m_endpoint.ToString()}] client connected-> proceeding.");
            // send it immediately
            m_client.NoDelay = true;

        }



        private void DoWork(IRobustWork work)
        {
            // loop through all the work items
            // execute them
            var sequence = work as RobustSequence;
            if (sequence != null)
            {
                PerformSequence(sequence);
            }
            var cmd = work as RobustCommand;
            if (cmd != null)
            {
                PerformCommand(cmd);
            }
        }

        private void PerformSequence(RobustSequence seq)
        {
            for (int i = 0; i < seq.RetryAttempts; i++)
            {
                bool proceed = true;

                byte[] resp = new byte[0];

                if (seq.Setup != null && seq.Setup.Length > 0)
                {
                    var s = DoTCPTimed(seq.Setup, seq.SetupDelayMS);
                    proceed = s.success;
                    resp = s.resp;
                }

                if (proceed)
                {
                    var c1 = DoTCPTimed(seq.First, seq.DelayMS);
                    proceed = c1.success;
                    resp = resp.Concat(c1.resp).ToArray();
                }

                if (proceed)
                {
                    var c2 = DoTCPTimed(seq.Second, 0);
                    proceed = c2.success;
                    resp = resp.Concat(c2.resp).ToArray();
                }

                if (proceed)
                {
                    seq.OnCompleted(i + 1, resp);
                    return;
                }

                if (seq.Reset != null && seq.Reset.Length > 0)
                {
                    // forcibly run reset until it works
                    // max out at retry attempts and fail command at that point
                    bool reset = false;
                    for (int j = 0; j < seq.RetryAttempts; j++)
                    {
                        if (DoTCPTimed(seq.Reset, seq.DelayMS).success)
                        {
                            reset = true;
                            j = seq.RetryAttempts;
                        }
                    }
                    if (!reset)
                    {
                        seq.OnFail(seq.RetryAttempts);
                    }
                }
            }
            seq.OnFail(seq.RetryAttempts);
        }

        private void PerformCommand(RobustCommand cmd)
        {
            for (int i = 0; i < cmd.RetryAttempts; i++)
            {
                var attempt = DoTCPTimed(cmd.Data, 0, cmd.IgnoreResponse);
                if (attempt.success)
                {
                    cmd.OnCompleted(i + 1, attempt.resp);
                    return;
                }
            }
            cmd.OnFail(cmd.RetryAttempts);
        }

        private (bool success, byte[] resp) DoTCPTimed(byte[] msg, int msTime, bool ignoreRESP = false)
        {
            NetworkStream stream = null;
            try
            {
                stream = m_client.GetStream();
                stream.ReadTimeout = 5000; // TODO: figure out how long this really should be...
            }
            catch (Exception ex)
            {
                m_log?.Info($"[{m_endpoint.ToString()}] threw exception while getting the underlying network stream {ex}");
                return (false, new byte[0]);
            }

            if (stream == null)
            {
                m_log?.Info($"[{m_endpoint.ToString()}] got a null stream.");
                return (false, new byte[0]);
            }
            // idealy we'd never give-up, but abort after some time
            // probably can get away with an order of seconds here in practice

            // effectively flush the read buffer
            // in the event that we'd timed out previously but it did come eventually
            // we'll just ignore it because it's stale and we're firing again
            try
            {
                while (stream.DataAvailable)
                {
                    stream.ReadByte();
                }
            }
            catch (Exception ex)
            {
                m_log?.Info($"[{m_endpoint.ToString()}] threw exception while flushing the read buffer {ex}");
            }

            Stopwatch timer = new Stopwatch();

            // send data
            m_log?.Info($"[{m_endpoint.ToString()}] sending data: {BitConverter.ToString(msg)}");
            timer.Start();
            stream.Write(msg);

            m_log?.Info($"[{m_endpoint.ToString()}] expecting response.");
            try
            {

                // DVIP protocol specifies 2 bytes returned with response lenght
                int sizehigh = 0;
                int sizelow = 0;
                uint dataSize = 0;

                if (ignoreRESP)
                {
                    return (false, new byte[0]);
                }

                m_log?.Info($"[{m_endpoint.ToString()}] reading size highbyte.");
                sizehigh = stream.ReadByte();
                m_log?.Info($"[{m_endpoint.ToString()}] reading size lowbyte.");
                sizelow = stream.ReadByte();
                if (sizehigh == -1 || sizelow == -1)
                {
                    // unexpected end of stream too soon
                    // abort
                    m_log?.Info($"[{m_endpoint.ToString()}] invalid size. Abort");
                    return (false, new byte[0]);
                }
                // get data here
                dataSize = (uint)(sizehigh << 8 | sizelow) - 2;
                // validate dataSize
                if (dataSize < 0 || dataSize > 512)
                {
                    // reject
                    m_log?.Info($"[{m_endpoint.ToString()}] expecting invalid datasize of {dataSize}. REJECTED!");
                    return (false, new byte[0]);
                }
                byte[] data = new byte[dataSize];
                m_log?.Info($"[{m_endpoint.ToString()}] expecting size {dataSize}");
                for (int x = 0; x < dataSize; x++)
                {
                    m_log?.Info($"[{m_endpoint.ToString()}] reading byte {x} of {dataSize}");
                    int b = stream.ReadByte();
                    if (b != -1)
                    {
                        data[x] = (byte)b;
                    }
                    else
                    {
                        // unexpected end of stream too soon
                        // abort
                        m_log?.Info($"[{m_endpoint.ToString()}] invalid data. Abort");
                        return (false, new byte[0]);
                    }
                }
                m_log?.Info($"[{m_endpoint.ToString()}] finished reading response: {BitConverter.ToString(data)}");

                // wait for spin time
                if (timer.ElapsedMilliseconds < msTime)
                {
                    m_log?.Info($"[{m_endpoint.ToString()}] spin waiting for timeout.");
                }
                while (timer.ElapsedMilliseconds < msTime) ;
                timer.Stop();
                return (true, data);
            }
            catch (System.IO.IOException ex)
            {
                // send invalid response
                m_log?.Info($"[{m_endpoint.ToString()}] exception {ex}. Abort");
                return (false, new byte[0]);
            }

        }

        private void SubmitWork(IRobustWork work)
        {
            m_log?.Info($"[{m_endpoint.ToString()}] enqueue work item.");
            m_commands.Enqueue(work);
            m_cmdAvail.Set();
        }


        private void IgnoreCommand(byte[] resp)
        {
            // .... ?
            // log it perhaps, or just toss it?
            // may be a use for /devnull as a service
            // look into a subscription https://devnull-as-a-service.com/
        }

        void IRobustClient.DoRobustWork(IRobustWork work)
        {
            SubmitWork(work);
        }
    }
}
