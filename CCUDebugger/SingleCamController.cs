using DVIPProtocol.Clients;
using DVIPProtocol.Protocol.ControlCommand;
using DVIPProtocol.Protocol.ControlCommand.Cmd.PanTiltDrive;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CCUDebugger
{
    internal class SingleCamController
    {

        IPAddress ip;
        int port;
        internal void Start()
        {
            // create a new client
            Console.WriteLine("Initializing Client!");
            Console.WriteLine("Enter Client IP Addr:");
            var ipstr = Console.ReadLine();
            Console.WriteLine("Enter Client Port:");
            var portstr = Console.ReadLine();

            if (!IPAddress.TryParse(ipstr, out ip))
            {
                Console.WriteLine("Invalid IP Address!");
                return;
            }

            if (!int.TryParse(portstr, out port))
            {
                Console.WriteLine("Invalid Port!");
                return;
            }

            SimpleTCPClient _client = new SimpleTCPClient();
            _client.Create(ip, port);

            Console.WriteLine("Started client!");

            while (true)
            {
                Console.WriteLine("Awaiting commmands (left, right, stop, test1, exit) ...");

                var cmdstr = Console.ReadLine();
                var cmd = CreateCommand(cmdstr);
                if (cmd != null)
                {
                    Console.WriteLine($"Sending {cmdstr} cmd");
                    cmd.OnCommandReturnPacketRecieved += Cmd_OnCommandReturnPacketRecieved;
                    cmd.OnCommandRejected += Cmd_OnCommandRejected;
                    _client.SendCommand(cmd);
                }
            }

        }

        private void Cmd_OnCommandRejected(object? sender, DVIPProtocol.Protocol.RejectionReason e)
        {
            ControlCommand cmd = sender as ControlCommand;
            if (cmd != null)
            {
                Console.WriteLine($"Command rejected! {e}");
                Cmd_Release(cmd);
            }
        }

        private void Cmd_OnCommandReturnPacketRecieved(object? sender, int e)
        {
            ControlCommand cmd = sender as ControlCommand;
            if (cmd != null)
            {
                Console.WriteLine($">>>>>>> Command completed with response: {BitConverter.ToString(cmd.CompletedData)}");
                Console.WriteLine("Awaiting commmands (left, right, stop, test1, exit) ...");
                Cmd_Release(cmd);
            }
        }

        private void Cmd_Release(ControlCommand cmd)
        {
            cmd.OnCommandRejected -= Cmd_OnCommandRejected;
            cmd.OnCommandReturnPacketRecieved -= Cmd_OnCommandReturnPacketRecieved;
        }

        private ControlCommand? CreateCommand(string cmd)
        {

            switch (cmd)
            {
                case "left":
                    return CMDPanTiltLeft();
                case "right":
                    return CMDPanTiltRight();
                case "stop":
                    return CMDPanTiltStop();
                case "test1":
                    UDP_PRESET();
                    return null;
                case "exit":
                    Environment.Exit(0);
                    return null;
                default:
                    Console.WriteLine($"Unknown command {{{cmd}}}!");
                    return null;
            }

        }

        private void UDP_PRESET()
        {
            UDP_Debug_Test.RecallPreset(ip, port);
        }

        private ControlCommand CMDPanTiltLeft()
        {
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Left, 0x01, 0x00);
        }

        private ControlCommand CMDPanTiltRight()
        {
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Right, 0x00, 0x01);
        }

        private ControlCommand CMDPanTiltStop()
        {
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Stop, 0x00, 0x00);
        }


    }
}
