using NAudio.Midi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_DEBUGGER.MidiDriver
{

    public interface ISQDriver
    {
        bool GetMute(int srcID);
        bool GetLevel(int srcID, int destID);

        void SetMute(int srcID, bool muted);
        void SetInputLevelOnMix(int srcID, int MixID, int level);
        void SetMixLevelOutput(int mixID, int level);

        void ChangeScene(int sceneNum);
    }

    public class SQDriver : ISQDriver
    {
        MidiIn _midiInput;
        MidiOut _midiOutput;
        public SQDriver(int MIDIdeviceInID, int MIDIdeviceOutID, int MIDIcontrolChannel)
        {
            // setup midi in/out channels
            try
            {
                _midiInput = new MidiIn(MIDIdeviceInID);
                _midiOutput = new MidiOut(MIDIdeviceOutID);

                Console.WriteLine($"SQDriver MIDI driver set to use input {MidiIn.DeviceInfo(MIDIdeviceInID).ProductName}");
                Console.WriteLine($"SQDriver MIDI driver set to use input {MidiIn.DeviceInfo(MIDIdeviceInID).ProductId}");
                Console.WriteLine($"SQDriver MIDI driver set to use output {MidiOut.DeviceInfo(MIDIdeviceOutID).ProductName}");
                Console.WriteLine($"SQDriver MIDI driver set to use output {MidiOut.DeviceInfo(MIDIdeviceOutID).ProductId}");

                _midiInput.MessageReceived += _midiInput_MessageReceived;
                _midiInput.ErrorReceived += _midiInput_ErrorReceived;

                _midiInput.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION {ex}");
            }
        }

        private void _midiInput_MessageReceived(object? sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine($"MIDI::RX {e.MidiEvent}");
        }

        private void _midiInput_ErrorReceived(object? sender, MidiInMessageEventArgs e)
        {
            Console.WriteLine($"MIDI::RX {e}");
        }

        public void ChangeScene(int sceneNum)
        {
            throw new NotImplementedException();
        }

        public bool GetLevel(int srcID, int destID)
        {
            throw new NotImplementedException();
        }

        public bool GetMute(int srcID)
        {
            throw new NotImplementedException();
        }

        public void SetMute(int srcID, bool muted)
        {
            var cmd = SQProtocol.GenerateCommand_MuteSrc(0, (byte)srcID, muted);
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
            _midiOutput?.SendBuffer(cmd);
        }

        public void SetInputLevelOnMix(int srcID, int MixID, int level)
        {
            throw new NotImplementedException();
        }

        public void SetMixLevelOutput(int mixID, int level)
        {
            throw new NotImplementedException();
        }

    }

    static class SQProtocol
    {

        static short[] MuteRouting;
        static short[] MasterLevelRouting;
        static short[,] InputLevelRouting;



        private static short[] LoadSingleArrayAs3ColumnHEXLSBMSB(string input)
        {
            List<short> result = new List<short>();
            // remove header
            foreach (var line in input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                // split on ,
                var parts = line.Split(',');

                var msb = int.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);
                var lsb = int.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);

                short val = (short)(msb << 8 | lsb);

                result.Add(val);
            }
            return result.ToArray();
        }

        private static short[,] LoadMultiArrayAsNColumnHEXLSBMSB(string input, int columns)
        {
            var lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
            short[,] result = new short[lines.Count, columns];

            // remove header
            int lnum = 0;
            foreach (var line in lines)
            {
                // split on ,
                var parts = line.Split(',');

                for (int i = 1; i <= columns; i += 2)
                {
                    var msb = int.Parse(parts[i], System.Globalization.NumberStyles.HexNumber);
                    var lsb = int.Parse(parts[i + 1], System.Globalization.NumberStyles.HexNumber);
                    short val = (short)(msb << 8 | lsb);
                    result[lnum, i - 1] = val;
                }

                lnum++;
            }

            return result;
        }


        private static string LoadStream(string sname)
        {
            var name = System.Reflection.Assembly.GetAssembly(typeof(SQProtocol))
                                                         .GetManifestResourceNames()
                                                         .FirstOrDefault(x => x.Contains(sname));
            var stream = System.Reflection.Assembly.GetAssembly(typeof(SQProtocol))
                .GetManifestResourceStream(name);

            var stext = "";
            using (StreamReader sr = new StreamReader(stream))
            {
                stext = sr.ReadToEnd();
            }
            return stext;
        }

        static SQProtocol()
        {
            // load config

            string mutestr = LoadStream("SQ6Protocol_MuteRouting.csv");
            MuteRouting = LoadSingleArrayAs3ColumnHEXLSBMSB(mutestr);

            string masterlevel = LoadStream("SQ6Protocol_MasterLevelRouting.csv");
            MasterLevelRouting = LoadSingleArrayAs3ColumnHEXLSBMSB(masterlevel);

            string inputlevel = LoadStream("SQ6Protocol_InputLevelRouting.csv");
            InputLevelRouting = LoadMultiArrayAsNColumnHEXLSBMSB(inputlevel, 13);
        }

        internal static short Lookup_InputLevelRouting(int srcID, int mixID)
        {
            return InputLevelRouting[srcID, mixID];
        }

        internal static short Lookup_MasterLevelRouting(int mixID)
        {
            return MasterLevelRouting[mixID];
        }

        internal static short Lookup_MuteRouting(int srcID)
        {
            return MuteRouting[srcID];
        }







        private static byte BuildByte(byte MSB, byte LSB)
        {
            byte v1 = (byte)(MSB << 4);
            byte v2 = (byte)(LSB & 0b00001111);
            return (byte)(v1 | v2);
        }

        internal static byte[] GenerateCommand_MuteSrc(byte midiChannel, ushort srcID, bool mute)
        {
            short rid = Lookup_MuteRouting(srcID);
            byte pmsb = (byte)((rid & 0xFF00) >> 8);
            byte plsb = (byte)(rid & 0x00FF);
            return new byte[]
            {
                BuildByte(0xB, (byte)midiChannel),
                0x63,
                pmsb,

                BuildByte(0xB, (byte)midiChannel),
                0x62,
                plsb,

                BuildByte(0xB, (byte)midiChannel),
                0x06,
                0x00,

                BuildByte(0xB, (byte)midiChannel),
                0x26,
                BuildByte(0x00, (byte)(mute ? 0x01 : 0x00)),
            };
        }



    }


}
