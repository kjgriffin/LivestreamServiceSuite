using DVIPProtocol.Clients.Advanced;
using DVIPProtocol.Protocol.Lib.Command.CamCTRL;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CCUDebugger
{
    internal class AdvCtrl
    {

        int zSpeed = 0;
        int ptSpeed = 0;

        bool run = true;


        ICmdClient client;

        internal void Start()
        {

            Console.WriteLine("Enter IP:");
            string addr = Console.ReadLine();
            Console.WriteLine("Enter Port (5002):");
            string port = Console.ReadLine();

            if (!(IPAddress.TryParse(addr, out var ip) && int.TryParse(port, out var portNum)))
            {
                Console.WriteLine($"Invalid Endpoint {addr}:{port}");
                return;
            }
            IPEndPoint endpoint = new IPEndPoint(ip, portNum);
            client = new TCPCmdClient(endpoint);
            client.Init();


            while (run)
            {
                ReadInput();
                Display();

            }

            client.Stop();
        }

        StringBuilder sb = new StringBuilder();
        private void Display()
        {
            sb.Clear();
            sb.AppendLine("[wasd]       - pan/tilt  ");
            sb.AppendLine("[rf]         - wide/tele ");
            sb.AppendLine("[pg up/down] - speed     ");
            sb.AppendLine("[space]      - stop      ");
            sb.AppendLine("[q]          - quit      ");
            sb.AppendLine("-------------------------");
            sb.AppendLine($"PTSpeed: {ptSpeed:0}     ");
            sb.AppendLine($"ZSpeed:  {zSpeed:0}     ");
            sb.AppendLine();
            sb.AppendLine();


            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());

            // hmmmmm.....mmmmm.....
            // client should be ok- since that's got its own background thread
            // this will simply impact how responsive/fast we are to user input...
            Thread.Sleep(10);
        }

        private void _cmd_STOP()
        {
            client.SendCommand(CMD_Zoom_Std.Create(ZoomDir.STOP).PackagePayload());
            client.SendCommand(CMD_PanTiltDrive.CMD_STOP_DRIVE().PackagePayload());
        }

        private void ReadInput()
        {
            if (!Console.KeyAvailable)
            {
                return;
            }
            var key = Console.ReadKey();

            if (key.KeyChar == 'q')
            {
                run = false;
                _cmd_STOP();
                return;
            }

            if (key.Key == ConsoleKey.Spacebar)
            {
                _cmd_STOP();
            }

            if (key.KeyChar == 'w')
            {
                client.SendCommand(CMD_PanTiltDrive.UpDownLeftRight(PanTiltDirection.UP, (byte)ptSpeed).PackagePayload());
            }
            if (key.KeyChar == 'a')
            {
                client.SendCommand(CMD_PanTiltDrive.UpDownLeftRight(PanTiltDirection.LEFT, (byte)ptSpeed).PackagePayload());
            }
            if (key.KeyChar == 's')
            {
                client.SendCommand(CMD_PanTiltDrive.UpDownLeftRight(PanTiltDirection.DOWN, (byte)ptSpeed).PackagePayload());
            }
            if (key.KeyChar == 'd')
            {
                client.SendCommand(CMD_PanTiltDrive.UpDownLeftRight(PanTiltDirection.RIGHT, (byte)ptSpeed).PackagePayload());
            }

            if (key.KeyChar == 'r')
            {
                client.SendCommand(CMD_Zoom_Std.Create(ZoomDir.WIDE).PackagePayload());
            }
            if (key.KeyChar == 'f')
            {
                client.SendCommand(CMD_Zoom_Std.Create(ZoomDir.TELE).PackagePayload());
            }

            if (key.KeyChar == 't')
            {
                client.SendCommand(CMD_Zoom_Variable.Create(ZoomDir.WIDE, (byte)zSpeed).PackagePayload());
            }
            if (key.KeyChar == 'g')
            {
                client.SendCommand(CMD_Zoom_Variable.Create(ZoomDir.TELE, (byte)zSpeed).PackagePayload());
            }



            if (key.KeyChar == '[')
            {
                if (zSpeed - 1 >= 0)
                {
                    zSpeed--;
                }
            }
            if (key.KeyChar == ']')
            {
                if (zSpeed + 1 <= 7)
                {
                    zSpeed++;
                }
            }

            if (key.Key == ConsoleKey.PageDown)
            {
                if (ptSpeed - 1 >= 0)
                {
                    ptSpeed--;
                }
            }
            if (key.Key == ConsoleKey.PageUp)
            {
                if (ptSpeed + 1 <= 0x14) // pan goes up to 18 not 14, but whateves
                {
                    ptSpeed++;
                }
            }



        }



    }
}
