using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        bool injectRandomErrors = false;
        Random rnd = new Random(DateTime.Now.Millisecond);

        long pan = 0;
        long tilt = 0;
        int zoom = 0; // 0 - 99

        public void Start(IPEndPoint endpoint, bool keepalive = false, bool randomErrors = false)
        {
            this.injectRandomErrors = randomErrors;
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
                    Console.WriteLine($"Finished recieveing data at: {DateTime.Now:hh:mm:ss:ffff}");
                    if (injectRandomErrors)
                    {
                        Stopwatch timer = Stopwatch.StartNew();
                        int delay = rnd.Next(10, 250);
                        Console.WriteLine($"Randomly delaying {delay} ms");
                        while (timer.ElapsedMilliseconds < delay) ;
                    }
                    Console.WriteLine($"Sending response at: {DateTime.Now:hh:mm:ss:ffff}");
                    if (injectRandomErrors && rnd.NextDouble() < 0.1)
                    {
                        Console.WriteLine($"Oops! You've been hit by a smooth data corruption");
                        stream.Write(new byte[] { 0x00, 0x01, 0x03 }, 0, 3); // random junk here
                    }
                    else if (injectRandomErrors && rnd.NextDouble() < 0.1)
                    {
                        Console.WriteLine($"Maybe later... Command dropped silently");
                    }
                    else
                    {
                        stream.Write(resp, 0, resp.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong? {ex.Message}");
                    Console.WriteLine($"----------RESTARTING--------------");
                    client?.Close();
                    listener?.Stop();
                    Internal_Loop();
                }
            }
        }

        private byte[] ParseAndRespond(byte[] msg)
        {
            // for now only implement abs pos and pos inq
            // 81 09 06 12 FF -> pos inq
            // 81 01 06 02 s 00 0p 0q 0r 0s 0t 0a 0b 0c 0d FF -> abs position
            // 81 01 04 07 XX FF -> zoom
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

                    this.pan = pan;
                    this.tilt = tilt;

                    Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Pan/Tilt absolute posittion command. Responding with OK.");
                    Console.WriteLine($"--INTERNAL-- <pan> now at {this.pan}");
                    Console.WriteLine($"--INTERNAL-- <tilt> now at {this.tilt}");

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
            if (msg.Length == 6)
            {
                bool good = false;
                // its a zoom command
                if (msg[0] == 0x81 && msg[1] == 0x01 && msg[2] == 0x04 && msg[3] == 0x07 && msg[5] == 0xFF)
                {
                    if (msg[4] == 0x00)
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom STOP command. Responding with OK.");
                        good = true;
                    }
                    else if (msg[4] == 0x02)
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom STD TELE command. Responding with OK.");
                        good = true;
                        this.zoom = 99;
                    }
                    else if (msg[4] == 0x03)
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom STD WIDE command. Responding with OK.");
                        good = true;
                        this.zoom = 0;
                    }
                    else if ((msg[4] & 0x20) == 0x20)
                    {
                        var speed = msg[4] & 0x0F;
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom VAR {speed} TELE command. Responding with OK.");
                        good = true;
                        this.zoom--;
                        Math.Clamp(this.zoom, 0, 99);
                    }
                    else if ((msg[4] & 0x30) == 0x30)
                    {
                        var speed = msg[4] & 0x0F;
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom VAR {speed} WIDE command. Responding with OK.");
                        good = true;
                        this.zoom++;
                        Math.Clamp(this.zoom, 0, 99);
                    }
                }
                if (good)
                {
                    Console.WriteLine($"--INTERNAL-- <zoom> now at {this.zoom}");
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
