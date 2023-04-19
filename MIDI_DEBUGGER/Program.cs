using SQMIDI.MidiDriver;
using NAudio.Midi;

namespace MIDI_DEBUGGER
{
    internal class Program
    {

        static Dictionary<string, string> ChannelAssigments = new Dictionary<string, string>
        {
            ["Astley"] = "Ip13",
            ["Roggow"] = "Ip21",
            ["Choul"] = "Ip15",
            ["Zabel"] = "Ip14",

            ["Pulpit"] = "Ip1",
            ["Lectern"] = "Ip2",
            ["Alter"] = "Ip3",
            ["Ambient"] = "Ip4",

            ["Handmic"] = "Ip16",

            ["GrandP"] = "Grp4",

            ["Computer"] = "Ip23" // and Ip24
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Looking for MIDI INPUT devices");
            for (int inputid = 0; inputid < MidiIn.NumberOfDevices; inputid++)
            {
                Console.WriteLine(MidiIn.DeviceInfo(inputid).ProductName);
            }
            Console.WriteLine("Looking for MIDI OUTPUT devices");
            for (int inputid = 0; inputid < MidiOut.NumberOfDevices; inputid++)
            {
                try
                {
                    Console.WriteLine(MidiIn.DeviceInfo(inputid).ProductName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            ISQDriver driver = new SQDriver(0, 1, 1);

            Console.WriteLine("m <src> (mute|unmute)");
            Console.WriteLine("l <src> <dest> <value>");
            Console.WriteLine("s <scene>");

            bool run = true;
            while (run)
            {
                var input = Console.ReadLine();

                if (input.StartsWith("m"))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        string ch = parts[1];
                        bool mute = parts[2] != "unmute";
                        if (ChannelAssigments.TryGetValue(parts[1], out string val))
                        {
                            ch = val;
                        }
                        driver.SetMute(ch, mute);
                    }
                }
                else if (input.StartsWith("l"))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        string ch = parts[1];
                        if (ChannelAssigments.TryGetValue(parts[1], out string val))
                        {
                            ch = val;
                        }
                        int level = int.Parse(parts[3]);
                        driver.SetLevelABS(ch, parts[2], level);
                    }
                }
                else if (input.StartsWith("s"))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        int scene = int.Parse(parts[1]);
                        driver.ChangeScene(scene);
                    }
                }
                else if (input.StartsWith("g"))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        byte msb = byte.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                        byte lsb = byte.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);
                        driver.GetParam(msb, lsb);
                    }
                }
            }
        }
    }
}