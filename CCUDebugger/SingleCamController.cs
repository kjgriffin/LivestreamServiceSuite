using DVIPProtocol.Clients;
using DVIPProtocol.Protocol.ControlCommand;
using DVIPProtocol.Protocol.ControlCommand.Cmd.Other;
using DVIPProtocol.Protocol.ControlCommand.Cmd.PanTiltDrive;
using DVIPProtocol.Protocol.ControlCommand.Cmd.Memory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DVIPProtocol.Protocol.ControlCommand.Cmd.Inq;
using DVIPProtocol.Protocol.ControlCommand.Cmd.Zoom;

namespace CCUDebugger
{

    delegate ControlCommand CamCommand(int value);

    internal class SingleCamController
    {

        IPAddress ip;
        int port;

        static Dictionary<string, CamCommand> CommandDispatcher = new Dictionary<string, CamCommand>()
        {
            ["left"] = CMDPanTiltLeft,
            ["right"] = CMDPanTiltRight,
            ["up"] = CMDPanTiltUp,
            ["down"] = CMDPanTiltDown,
            ["zoom"] = CMDZoom,
            ["stop"] = CMDPanTiltStop,
            ["vf1080p60"] = CMDVideoFormat,
            ["set"] = CMDSetPreset,
            ["recall"] = CMDRecallPreset,
            ["raw"] = CMDCommandRAW,
        };

        private static void ListCommands()
        {
            Console.WriteLine($"Awaiting commands ({string.Join(',', CommandDispatcher.Keys)}, sublive, exit) ...");
        }

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
                ListCommands();

                var cmdstr = Console.ReadLine();
                var cmd = CreateCommand(cmdstr, _client);
                if (cmd != null)
                {
                    Console.WriteLine($"Sending {cmdstr} cmd");
                    FireCommand(_client, cmd);
                }
            }

        }

        private void FireCommand(SimpleTCPClient _client, ControlCommand? cmd)
        {
            cmd.OnCommandReturnPacketRecieved += Cmd_OnCommandReturnPacketRecieved;
            cmd.OnCommandRejected += Cmd_OnCommandRejected;
            _client.SendCommand(cmd);
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
                ListCommands();
                Cmd_Release(cmd);
            }
        }

        private void Cmd_Release(ControlCommand cmd)
        {
            cmd.OnCommandRejected -= Cmd_OnCommandRejected;
            cmd.OnCommandReturnPacketRecieved -= Cmd_OnCommandReturnPacketRecieved;
        }

        private ControlCommand? CreateCommand(string cmd, SimpleTCPClient client)
        {

            if (CommandDispatcher.ContainsKey(cmd))
            {
                return CommandDispatcher[cmd](-1);
            }

            switch (cmd)
            {
                case "sublive":
                    SubLiveMode(client);
                    return null;
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

        private static int GetSpeed()
        {
            Console.WriteLine("Speed: ");
            return int.TryParse(Console.ReadLine(), out int speed) ? speed : -1;
        }
        private static ZoomDir_STD GetZoomDir()
        {
            Console.WriteLine("Zoom Direction (tele/wide): ");
            var x = Console.ReadLine();
            if (x == "tele")
            {
                return ZoomDir_STD.TELE;
            }
            else if (x == "wide")
            {
                return ZoomDir_STD.WIDE;
            }
            return ZoomDir_STD.STOP;
        }


        private static ControlCommand CMDPanTiltLeft(int speed = -1)
        {
            if (speed == -1)
            {
                speed = GetSpeed();
            }
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Left, (byte)speed, 0x00);
        }

        private static ControlCommand CMDPanTiltRight(int speed = -1)
        {
            if (speed == -1)
            {
                speed = GetSpeed();
            }
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Right, (byte)speed, 0x00);
        }

        private static ControlCommand CMDPanTiltUp(int speed = -1)
        {
            if (speed == -1)
            {
                speed = GetSpeed();
            }
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Up, 0x00, (byte)speed);
        }

        private static ControlCommand CMDPanTiltDown(int speed = -1)
        {
            if (speed == -1)
            {
                speed = GetSpeed();
            }
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Down, 0x00, (byte)speed);
        }

        private static ControlCommand CMDZoom(int dir = -1)
        {
            var z = GetZoomDir();
            return ZoomCommand_STD.Create(z);
        }


        private static ControlCommand CMDCommandRAW(int value)
        {
            Console.WriteLine("RAW Command Bytes:");
            var hex = Console.ReadLine();

            try
            {
                byte[] data = Enumerable.Range(0, hex.Length)
                                        .Where(x => x % 2 == 0)
                                        .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                        .ToArray();

                return new ControlCommand_Raw(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }

        }


        private static ControlCommand CMDPanTiltStop(int value = 0)
        {
            return PanTiltDrive_Direction_Command.Create(PanTiltDriveDirection.Stop, 0x00, 0x00);
        }

        private static ControlCommand CMDVideoFormat(int value = 0)
        {
            return VideoFormatCommand.Create(VideoFormatBytes.VF_1080_60p_NTSC);
        }

        private static ControlCommand CMDSetPreset(int value = 0)
        {
            Console.WriteLine("Preset Num: ");
            int.TryParse(Console.ReadLine(), out int num);
            return SetMemory_Command.Create(num);
        }

        private static ControlCommand CMDRecallPreset(int value = 0)
        {
            Console.WriteLine("Preset Num: ");
            int.TryParse(Console.ReadLine(), out int num);
            return RecallMemory_Command.Create(num);
        }

        private void SubLiveMode(SimpleTCPClient client)
        {
            // poll for status update every 100ms

            // await keyboard input to move

            // wasd to move camera
            // z/x for speed
            // q to exit mode

            Console.Clear();
            Console.WriteLine("Live Control Mode");
            LiveLoop(client);
        }

        private void LiveLoop(SimpleTCPClient client)
        {
            bool run = true;


            long lastInputTicks = 0;
            long lastPollTicks = 0;

            byte speed = 0x05;

            while (run)
            {
                long nowTicks = DateTime.Now.Ticks;

                if (Console.KeyAvailable)
                {
                    // next command
                    char cmd = (char)Console.Read();

                    switch (cmd)
                    {
                        case 'w':
                            FireCommand(client, CMDPanTiltUp(speed));
                            break;
                        case 'a':
                            FireCommand(client, CMDPanTiltLeft(speed));
                            break;
                        case 's':
                            FireCommand(client, CMDPanTiltDown(speed));
                            break;
                        case 'd':
                            FireCommand(client, CMDPanTiltRight(speed));
                            break;
                        case 'x':
                            speed = (byte)Math.Min(speed + 1, 0x18);
                            break;
                        case 'z':
                            speed = (byte)Math.Max(speed - 1, 0x00);
                            break;
                        case 'e':
                            FireCommand(client, CMDPanTiltStop());
                            break;
                        case 'q':
                            // stop and quit
                            FireCommand(client, CMDPanTiltStop());
                            run = false;
                            break;
                        default:
                            break;
                    }
                    lastInputTicks = nowTicks;
                }
                else if (nowTicks - lastInputTicks > TimeSpan.FromMilliseconds(100).Ticks)
                {
                    // if we exceed 100ms timeout issue stop command
                    FireCommand(client, CMDPanTiltStop());
                }

                // if we need to poll for new data, do so now
                if (nowTicks - lastPollTicks > TimeSpan.FromMilliseconds(100).Ticks)
                {
                    FireCommand(client, PanTiltPos_Inq.Create());
                    lastPollTicks = nowTicks;
                }

            }

        }



    }
}
