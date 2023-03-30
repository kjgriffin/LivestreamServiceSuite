using MIDI_DEBUGGER.MidiDriver;
using NAudio.Midi;

namespace MIDI_DEBUGGER
{
    internal class Program
    {
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
            driver.SetMute(-1, true);

            bool run = true;
            while (run)
            {
                var input = Console.ReadKey();

                if (input.Key == ConsoleKey.Escape)
                {
                    run = false;
                }
                driver.SetMute(13 - 1, input.Key == ConsoleKey.M);
            }
        }
    }
}