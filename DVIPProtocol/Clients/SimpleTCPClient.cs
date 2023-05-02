using DVIPProtocol.Protocol;
using DVIPProtocol.Protocol.ControlCommand;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DVIPProtocol.Clients
{
    public class SimpleTCPClient
    {

        Thread m_clientThread;
        BlockingCollection<ControlCommand> m_requests;
        CancellationTokenSource m_cts = new CancellationTokenSource();
        TcpClient? m_client;

        IPAddress m_address;
        int m_port;

        bool m_created = false;

        public void Create(IPAddress address, int port)
        {
            m_address = address;
            m_port = port;
            m_requests = new BlockingCollection<ControlCommand>();
            m_clientThread = new Thread(WorkerLoop);
            m_created = true;
            m_clientThread.Start();
        }

        public void SendCommand(ControlCommand cmd)
        {
            if (!m_created)
            {
                cmd.Reject(RejectionReason.ClientNotCreated);
                return;
            }
            m_requests.Add(cmd);
        }


        private void WorkerLoop()
        {
            while (true)
            {
                if (m_cts.IsCancellationRequested == true)
                {
                    // TODO: handle shutdown requests
                }


                var req = m_requests.Take(m_cts.Token);

                try
                {
                    // send request
                    Internal_MakeReq(req);
                }
                catch (Exception ex)
                {
                    req.Reject(RejectionReason.Failed);
                }
            }
        }

        private void Internal_MakeReq(ControlCommand req)
        {
            if (m_client == null)
            {
                m_client = new TcpClient();
            }

            try
            {
                if (m_client.Connected)
                {
#if DEBUG
                    Console.WriteLine($"    [TCP Client Already Connected]");
#endif
                }
                else
                {
#if DEBUG
                    Console.WriteLine($"    [TCP Client Begin Connection]");
#endif
                    m_client.Connect(m_address, m_port);
#if DEBUG
                    Console.WriteLine($"    [TCP Client Connection Established]");
#endif
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"    [TCP exception] {ex.ToString()}");
                Debugger.Break();
#endif
            }


            Stream stream = m_client.GetStream();

            // Send command
            byte[] data = req.PackagePayload();


#if DEBUG
            Console.WriteLine("    [Sending TCP request]");
#endif
            stream.Write(data, 0, data.Length);


#if DEBUG
            Console.WriteLine("    [Sent TCP request]");
#endif

            byte[] readbuffer = new byte[512 + 2]; // max length command has 512 parameters + 2 bytes for packet length high & low

            bool read = false;
            if (read)
            {

#if DEBUG
                Console.WriteLine("    [Reading TCP response size]");
#endif
                // since we're sending a controlcommand we know that we should get at least 2 bytes telling us how much more to expect
                stream.Read(readbuffer, 0, 1);
                stream.Read(readbuffer, 1, 1);
                // figure out how much more we expect
                int returnpacketlength = readbuffer[0] << 8 | readbuffer[1];
#if DEBUG
                Console.WriteLine($"    [Reading TCP response data (expected size: {returnpacketlength - 2})]");
#endif
                stream.Read(readbuffer, 2, returnpacketlength - 2);

#if DEBUG
                Console.WriteLine("    [Finished reading TCP response]");
#endif


                // let the command parse its return value


                // we're done firing this command
                // command can alert whoever created it that its done
                byte[] retdata = new byte[returnpacketlength - 2];
                Buffer.BlockCopy(readbuffer, 2, retdata, 0, returnpacketlength - 2);
                req.Complete(retdata);
            }
            else
            {
                req.Complete(new[] { (byte)0 });
            }
        }

        private void SetupClient()
        {
            if (m_client != null)
            {
                // safely shutdown and dispose
                m_client.Close();
                m_client.Dispose();
                m_client = null;
            }
            m_client = new TcpClient();
            try
            {
                m_client.Connect(m_address, m_port);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
        }




    }
}
