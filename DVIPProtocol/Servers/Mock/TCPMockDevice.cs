using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DVIPProtocol.Servers.Mock
{


    public abstract class DVIPEventArgs : EventArgs
    {

    }

    public class DVIPPanTiltDriveEventArgs : DVIPEventArgs
    {
        public int PanSpeed { get; set; }
        public int TiltSpeed { get; set; }
        public bool Up { get; set; }
        public bool Down { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
    }


    enum ZoomState
    {
        STOP,
        TELE,
        WIDE
    }


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
        int zoomSpeed = 0; // 1-8
        ZoomState zoomState = ZoomState.STOP;
        bool _zoomUpdate = false;

        public event EventHandler<DVIPEventArgs> OnStateChange;


        Stopwatch cmdTimer = new Stopwatch();

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
                var previousState = zoomState;
                if (msg[0] == 0x81 && msg[1] == 0x01 && msg[2] == 0x04 && msg[3] == 0x07 && msg[5] == 0xFF)
                {
                    if (msg[4] == 0x00)
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom STOP command. Responding with OK.");
                        good = true;
                        zoomState = ZoomState.STOP;
                    }
                    else if (msg[4] == 0x02)
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom STD TELE command. Responding with OK.");
                        good = true;
                        zoomState = ZoomState.TELE;
                    }
                    else if (msg[4] == 0x03)
                    {
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom STD WIDE command. Responding with OK.");
                        good = true;
                        zoomState = ZoomState.WIDE;
                    }
                    else if ((msg[4] & 0xF0) == 0x20)
                    {
                        var speed = msg[4] & 0x0F;
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom VAR {speed} TELE command. Responding with OK.");
                        good = true;
                        zoomState = ZoomState.TELE;
                    }
                    else if ((msg[4] & 0xF0) == 0x30)
                    {
                        var speed = msg[4] & 0x0F;
                        Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Zoom VAR {speed} WIDE command. Responding with OK.");
                        good = true;
                        zoomState = ZoomState.WIDE;
                    }
                }
                if (good)
                {
                    if (zoomState != ZoomState.STOP)
                    {
                        _zoomUpdate = true;
                    }
                    PerformZoom(previousState);
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
            if (msg.Length == 9)
            {
                if (msg[0] == 0x81 && msg[1] == 0x01 && msg[2] == 0x06 && msg[3] == 0x01 && msg[8] == 0xFF)
                {
                    Console.WriteLine($"[{Thread.CurrentThread.Name}] recieved Drive command. Responding with OK.");
                }

                // parse it
                byte panSpeed = msg[4];
                byte tiltSpeed = msg[5];

                byte panDir = msg[6];
                byte tiltDir = msg[7];

                bool left = panDir == 0x01;
                bool right = panDir == 0x02;
                bool up = tiltDir == 0x01;
                bool down = tiltDir == 0x02;


                // fire event
                Task.Run(() =>
                {
                    OnStateChange?.Invoke(this, new DVIPPanTiltDriveEventArgs
                    {
                        Down = down,
                        Left = left,
                        Right = right,
                        Up = up,
                        PanSpeed = panSpeed,
                        TiltSpeed = tiltSpeed,
                    });
                });

                return new byte[]
                {
                        0x00,
                        0x05,
                        0x90,
                        0x50,
                        0xFF,
                };

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

        void PerformZoom(ZoomState previous)
        {
            var interval = cmdTimer.ElapsedMilliseconds;
            // use elapsed time to calculate curent state
            CalculateZoom(previous, interval);
            if (zoomState == ZoomState.STOP)
            {
                cmdTimer.Stop();
            }
            else
            {
                cmdTimer.Restart();
                // let it finish eventually....
                Task.Run(async () =>
                {
                    await Task.Delay(4000);
                    if (_zoomUpdate)
                    {
                        var previous = zoomState;
                        if (cmdTimer.IsRunning)
                        {
                            var interval = cmdTimer.ElapsedMilliseconds;
                            cmdTimer.Stop();
                            zoomState = ZoomState.STOP;
                        }
                        CalculateZoom(previous, interval);
                    }
                });
            }
        }

        void CalculateZoom(ZoomState previous, long ms)
        {
            _zoomUpdate = false;
            // based on curent zoom speed- we'll figure out how far it has zoomed
            // 0.03 zoom stops /ms ?? at speed = 7
            // 0.014 zoom /ms at speed = 0 ??
            // assume linear
            const double slowPerMS = 0.03;
            const double stepPerSpeed = 0.00196;

            double speedPerMs = slowPerMS + zoomSpeed * stepPerSpeed;
            double zoomDiff = speedPerMs * ms;
            int dir = previous == ZoomState.WIDE ? -1 : previous == ZoomState.TELE ? 1 : 0;
            int zoom = (int)(dir * zoomDiff) + this.zoom;

            this.zoom = Math.Clamp(zoom, 0, 99);

            Console.WriteLine($"--INTERNAL-- <zoom> now at {this.zoom}");
        }


    }
}
