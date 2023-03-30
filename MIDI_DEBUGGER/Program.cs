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
                Console.WriteLine(MidiIn.DeviceInfo(inputid));
            }
            Console.WriteLine("Looking for MIDI OUTPUT devices");
            for (int inputid = 0; inputid < MidiIn.NumberOfDevices; inputid++)
            {
                Console.WriteLine(MidiIn.DeviceInfo(inputid));
            }
        }
    }
}