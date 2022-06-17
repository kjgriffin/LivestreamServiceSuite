using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Servers.Mock
{
    public class TCPMockDevice
    {

        IPEndPoint endpoint;
        Thread workerThread;

        // for now only implement handling 1 active control connection

        public void Start(IPEndPoint endpoint, bool keepalive = false)
        {
            this.endpoint = endpoint;
            workerThread = new Thread(Internal_Loop);
            workerThread.IsBackground = !keepalive;
            workerThread.Name = "TCPMockDevice worker thread";

            workerThread.Start();
        }

        private void Internal_Loop()
        {
            Console.WriteLine($"[{Thread.CurrentThread.Name}] started!");
            TcpListener listener = new TcpListener(endpoint);
            Console.WriteLine($"[{Thread.CurrentThread.Name}] started server");
            listener.Start();
            Console.WriteLine($"[{Thread.CurrentThread.Name}] waiting for connection...");
            // block for first client, and only allow 1 active connection
            TcpClient client = listener.AcceptTcpClient();

            Console.WriteLine($"[{Thread.CurrentThread.Name}] connection accepted!");


            client.NoDelay = true;

            NetworkStream stream = client.GetStream();

            while (true)
            {
                try
                {
                    // wait for data
                    int sizeHigh = stream.ReadByte();
                    int sizeLow = stream.ReadByte();
                    int expectedSize = (sizeHigh << 8 | sizeLow) - 2;
                    byte[] data = new byte[expectedSize];
                    for (int i = 0; i < expectedSize; i++)
                    {
                        int d = stream.ReadByte();
                        data[i] = (byte)d;
                    }
                    var resp = ParseAndRespond(data);
                    stream.Write(resp, 0, resp.Length);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private byte[] ParseAndRespond(byte[] msg)
        {
            // for now only implement abs pos and pos inq
            // 81 09 06 12 FF -> pos inq
            // 81 01 06 02 s 00 0p 0q 0r 0s 0t 0a 0b 0c 0d FF // abs position
            // reject all else
            if (msg.Length == 5)
            {
                if (msg[0] == 0x81 && msg[1] == 0x09 && msg[2] == 0x06 && msg[3] == 0x12 && msg[4] == 0xFF)
                {
                    Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Pan/Tilt Position Inquiry. Responding with test position.");

                    int size = RESP_PanTilt_Position.ExpectedResponseLength + 2;
                    var data = RESP_PanTilt_Position.Create(1000, 8000, true).Data;
                    byte[] resp = new byte[size];
                    resp[0] = (byte)(size >> 8 & 0xFF);
                    resp[1] = (byte)(size & 0xFF);
                    for (int i = 2; i < size; i++)
                    {
                        resp[i] = data[i - 2];
                    }
                    return resp;
                }
            }
            if (msg.Length == 16)
            {
                if (msg[0] == 0x81 && msg[1] == 0x01 && msg[2] == 0x06 && msg[3] == 0x02 && msg[5] == 0x00 && msg[15] == 0xFF)
                {
                    byte speed = msg[4];
                    long pan = 0;
                    pan |= (byte)(msg[6] >> 16 & 0xFF);
                    pan |= (byte)(msg[7] >> 12 & 0xFF);
                    pan |= (byte)(msg[8] >> 8 & 0xFF);
                    pan |= (byte)(msg[9] >> 4 & 0xFF);
                    pan |= (byte)(msg[10] >> 0 & 0xFF);

                    int tilt = 0;
                    tilt |= (byte)(msg[11] >> 12 & 0xFF);
                    tilt |= (byte)(msg[12] >> 8 & 0xFF);
                    tilt |= (byte)(msg[13] >> 4 & 0xFF);
                    tilt |= (byte)(msg[14] >> 0 & 0xFF);

                    Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Pan/Tilt absolute posittion command. Responding with OK.");

                    return new byte[]
                    {
                        0x00,
                        0x05,
                        0x90,
                        0x50,
                        0xFF,
                    };
                }
            }
            Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved invalid command: {BitConverter.ToString(msg)}. Responding with Syntax Error");
            return new byte[6]
            {
                    0x00,
                    0x06,
                    0x90,
                    0x60,
                    0x02,
                    0xFF,
            };
        }


    }
}
