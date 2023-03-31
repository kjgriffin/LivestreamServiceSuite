using NAudio.Midi;
using NAudio.Wave.SampleProviders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MIDI_DEBUGGER.MidiDriver
{

    public interface ISQDriver
    {
        bool GetMute(int srcID);
        bool GetLevel(int srcID, int destID);

        void SetMute(string srcID, bool muted);
        void SetLevelABS(string srcID, string destID, int level);

        void ChangeScene(int sceneNum);
    }

    public class SQDriver : ISQDriver
    {
        const byte _midiCTRLChannel = 0;

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

        /// <summary>
        /// Send ChangeScene command.
        /// </summary>
        /// <param name="sceneNum">Scene Number [1, 300]</param>
        public void ChangeScene(int sceneNum)
        {
            var cmd = SQProtocol.GenerateCommand_RecallScene(_midiCTRLChannel, (ushort)sceneNum);
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
            _midiOutput?.SendBuffer(cmd);
        }

        public bool GetLevel(int srcID, int destID)
        {
            throw new NotImplementedException();
        }

        public bool GetMute(int srcID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send Mute Command.
        /// </summary>
        /// <param name="srcID">Source to Mute.</param>
        /// <param name="muted"><see langword="true"/> to mute channel.</param>
        public void SetMute(string srcID, bool muted)
        {
            var cmd = SQProtocol.GenerateCommand_MuteSrc(_midiCTRLChannel, srcID, muted);
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
            _midiOutput?.SendBuffer(cmd);
        }

        /// <summary>
        /// Send Absolute Level Command.
        /// </summary>
        /// <param name="srcID">Source to set.</param>
        /// <param name="destID">Destination to set source level on.</param>
        /// <param name="level">Level to set 00=-inf;7F=+10db</param>
        public void SetLevelABS(string srcID, string destID, int level)
        {
            string route = $"{srcID}:{destID}";
            var cmd = SQProtocol.GenerateCommand_SetLevelABS(_midiCTRLChannel, route, (ushort)level);
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
            _midiOutput?.SendBuffer(cmd);
        }

    }

    static class SQProtocol
    {

        class NRPNData
        {
            public string ID { get; set; }
            public short dVal { get; set; }
            public byte MSB { get; set; }
            public byte LSB { get; set; }
        }

        static Dictionary<string, NRPNData> MuteRouting;
        static Dictionary<string, NRPNData> LevelRouting;

        private static IEnumerable<NRPNData> LoadStringAsLinesOf_ID_DVAL_MSB_LSB(string input)
        {
            // remove header
            foreach (var line in input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                // split on ,
                var parts = line.Split(',');

                short val = short.Parse(parts[1]);
                byte msb = byte.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);
                byte lsb = byte.Parse(parts[3], System.Globalization.NumberStyles.HexNumber);

                yield return new NRPNData
                {
                    ID = parts[0],
                    dVal = val,
                    LSB = lsb,
                    MSB = msb,
                };
            }
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
            string mutestr = LoadStream("Mutes.csv");
            MuteRouting = new Dictionary<string, NRPNData>(LoadStringAsLinesOf_ID_DVAL_MSB_LSB(mutestr).Select(x => new KeyValuePair<string, NRPNData>(x.ID, x)));
            string levelsstr = LoadStream("Levels.csv");
            LevelRouting = new Dictionary<string, NRPNData>(LoadStringAsLinesOf_ID_DVAL_MSB_LSB(levelsstr).Select(x => new KeyValuePair<string, NRPNData>(x.ID, x)));
        }



        private static byte BuildByte(byte MSB, byte LSB)
        {
            byte v1 = (byte)(MSB << 4);
            byte v2 = (byte)(LSB & 0b00001111);
            return (byte)(v1 | v2);
        }

        internal static byte[] GenerateCommand_MuteSrc(byte midiChannel, string srcID, bool mute)
        {
            var routing = MuteRouting[srcID];
            return new byte[]
            {
                BuildByte(0xB, (byte)midiChannel),
                0x63,
                routing.MSB,

                BuildByte(0xB, (byte)midiChannel),
                0x62,
                routing.LSB,

                BuildByte(0xB, (byte)midiChannel),
                0x06,
                0x00,

                BuildByte(0xB, (byte)midiChannel),
                0x26,
                BuildByte(0x00, (byte)(mute ? 0x01 : 0x00)),
            };
        }

        internal static byte[] GenerateCommand_SetLevelABS(byte midiChannel, string lrouteID, ushort value)
        {
            var routing = LevelRouting[lrouteID];
            byte vc = (byte)((value & 0xFF00) >> 8);
            byte vf = (byte)(value & 0x00FF);
            return new byte[]
            {
                BuildByte(0xB, (byte)midiChannel),
                0x63,
                routing.MSB,

                BuildByte(0xB, (byte)midiChannel),
                0x62,
                routing.LSB,

                BuildByte(0xB, (byte)midiChannel),
                0x06,
                vc,

                BuildByte(0xB, (byte)midiChannel),
                0x26,
                vf
            };
        }

        /// <summary>
        /// Generates MIDI byte stream for command.
        /// </summary>
        /// <param name="midiChannel"></param>
        /// <param name="sceneNum">1 - 300</param>
        /// <returns></returns>
        internal static byte[] GenerateCommand_RecallScene(byte midiChannel, ushort sceneNum)
        {
            byte bank = 0;
            byte program = 0;

            if (sceneNum > 0 && sceneNum <= 128)
            {
                bank = 0;
                program = (byte)(sceneNum - 1);
            }
            else if (sceneNum > 128 && sceneNum <= 256)
            {
                bank = 1;
                program = (byte)(sceneNum - 1 - 128);
            }
            else if (sceneNum > 256 && sceneNum <= 300)
            {
                bank = 2;
                program = (byte)(sceneNum - 1 - 256);
            }
            else
            {
                // default into scene 0 for now
            }

            return new byte[]
            {
                BuildByte(0xB, (byte)midiChannel),
                0x00,
                bank,

                0xC0,
                program
            };
        }



    }


}
