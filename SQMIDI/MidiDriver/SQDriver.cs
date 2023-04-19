using NAudio.Midi;
using NAudio.Wave.SampleProviders;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SQMIDI.MidiDriver
{

    public interface ISQDriver
    {
        void GetMute(string srcID, Action<int, int> OnCompleteCallback);
        void GetLevel(string srcID, string destID, Action<int, int> OnCompleteCallback);
        void GetParam(byte msb, byte lsb);

        void SetMute(string srcID, bool muted);
        void SetLevelABS(string srcID, string destID, int level);

        void ChangeScene(int sceneNum);
    }

    public class SQDriver : ISQDriver
    {
        const byte _midiCTRLChannel = 0;

        MidiIn _midiInput;
        MidiOut _midiOutput;

        Thread _listenThread;
        ConcurrentQueue<MidiInMessageEventArgs> messageQueue;
        ManualResetEvent messageReady;

        object valueLock;
        List<NRPNGetWorkItem> nrpnGetReqs;

        class NRPNGetWorkItem
        {
            public int ParamID { get; set; }
            public Action<int, int> OnSuccessWorkItem { get; set; }
        }

        public SQDriver(int MIDIdeviceInID, int MIDIdeviceOutID, int MIDIcontrolChannel)
        {
            // setup midi in/out channels
            try
            {
                _midiInput = new MidiIn(MIDIdeviceInID);
                _midiOutput = new MidiOut(MIDIdeviceOutID);

#if DEBUG
                Console.WriteLine($"SQDriver MIDI driver set to use input {MidiIn.DeviceInfo(MIDIdeviceInID).ProductName}");
                Console.WriteLine($"SQDriver MIDI driver set to use input {MidiIn.DeviceInfo(MIDIdeviceInID).ProductId}");
                Console.WriteLine($"SQDriver MIDI driver set to use output {MidiOut.DeviceInfo(MIDIdeviceOutID).ProductName}");
                Console.WriteLine($"SQDriver MIDI driver set to use output {MidiOut.DeviceInfo(MIDIdeviceOutID).ProductId}");
#endif

                nrpnGetReqs = new List<NRPNGetWorkItem>();
                valueLock = new object();

                messageReady = new ManualResetEvent(false);
                messageQueue = new ConcurrentQueue<MidiInMessageEventArgs>();
                _listenThread = new Thread(ListenLoop);
                _listenThread.Name = "SQDriver MIDI Listener Thread";
                _listenThread.IsBackground = true;
                _listenThread.Start();

                _midiInput.MessageReceived += _midiInput_MessageReceived;
                _midiInput.ErrorReceived += _midiInput_ErrorReceived;

                _midiInput.Start();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"EXCEPTION {ex}");
#endif
            }
        }

        private void _midiInput_MessageReceived(object? sender, MidiInMessageEventArgs e)
        {
            //Console.WriteLine($"MIDI::RX {e.MidiEvent}");
            messageQueue.Enqueue(e);
            messageReady.Set();
        }

        private void _midiInput_ErrorReceived(object? sender, MidiInMessageEventArgs e)
        {
            //Console.WriteLine($"MIDI::RX {e}");
        }

        private void ListenLoop()
        {
            // simple state machine here to listen on all messages and pull out only NRPN sequences

            int lastCCController = -1;
            int step = -1;
            int pmsb = 0;
            int plsb = 0;
            int vc = 0;
            int vf = 0;

            // run forever
            while (true)
            {
                // wait for work
                messageReady.WaitOne();

                while (messageQueue.TryDequeue(out var msg))
                {
                    var cc = msg.MidiEvent as ControlChangeEvent;
                    if (cc != null)
                    {
                        if (cc.Channel == 1)
                        {
                            if (TryParseCCToNRPN(ref lastCCController, ref step, cc, ref pmsb, ref plsb, ref vc, ref vf))
                            {
                                // completed
                                // build an NRPN event from values, clear values and start again
                                int param = (pmsb << 8) + plsb;
                                int value = (vc << 8) + vf;

                                // run whatever task we want
                                lock (valueLock)
                                {
                                    var item = nrpnGetReqs.FirstOrDefault(x => x.ParamID == param);
                                    if (item != null)
                                    {
                                        nrpnGetReqs.Remove(item);
                                        Task.Run(() => item.OnSuccessWorkItem(param, value));
                                    }
                                }
                            }
                        }
                    }
                }


            }
        }

        private bool TryParseCCToNRPN(ref int lastController, ref int step, ControlChangeEvent e, ref int msb, ref int lsb, ref int vc, ref int vf)
        {
            if (lastController == 99 && step == 1)
            {
                if ((int)e.Controller == 98)
                {
                    lsb = e.ControllerValue;
                    step = 2;
                }
                else
                {
                    // reject!
                    msb = -1;
                    lsb = -1;
                    vf = -1;
                    vc = -1;
                }
            }
            else if (lastController == 98 && step == 2)
            {
                if ((int)e.Controller == 6)
                {
                    vc = e.ControllerValue;
                    step = 3;
                }
                else
                {
                    // reject!
                    msb = -1;
                    lsb = -1;
                    vf = -1;
                    vc = -1;
                }
            }
            else if (lastController == 6 && step == 3)
            {
                if ((int)e.Controller == 38)
                {
                    vf = e.ControllerValue;
                    lastController = (int)e.Controller;
                    step = -1;

                    return true;
                }
                else
                {
                    // reject!
                    msb = -1;
                    lsb = -1;
                    vf = -1;
                    vc = -1;
                }
            }
            // allow any prev message to enter fsm for parsing
            else
            {
                if ((int)e.Controller == 99)
                {
                    step = 1;
                    msb = e.ControllerValue;
                    lsb = -1;
                    vf = -1;
                    vc = -1;
                }
                else
                {
                    // reject!
                    msb = -1;
                    lsb = -1;
                    vf = -1;
                    vc = -1;
                }
            }

            lastController = (int)e.Controller;

            return false;
        }

        /// <summary>
        /// Send ChangeScene command.
        /// </summary>
        /// <param name="sceneNum">Scene Number [1, 300]</param>
        public void ChangeScene(int sceneNum)
        {
            var cmd = SQProtocol.GenerateCommand_RecallScene(_midiCTRLChannel, (ushort)sceneNum);
#if DEBUG
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
#endif
            _midiOutput?.SendBuffer(cmd);
        }

        public void GetLevel(string srcID, string destID, Action<int, int> OnCompletedCallback)
        {
            // calcluate param number
            var param = SQProtocol.LevelRouting[$"{srcID}:{destID}"];
            lock (valueLock)
            {
                nrpnGetReqs.Add(new NRPNGetWorkItem
                {
                    ParamID = param.dVal,
                    OnSuccessWorkItem = OnCompletedCallback
                });
            }
            GetParam(param.MSB, param.LSB);
        }

        public void GetMute(string srcID, Action<int, int> OnCompletedCallback)
        {
            // calcluate param number
            var param = SQProtocol.MuteRouting[srcID];
            lock (valueLock)
            {
                nrpnGetReqs.Add(new NRPNGetWorkItem
                {
                    ParamID = param.dVal,
                    OnSuccessWorkItem = OnCompletedCallback
                });
            }
            GetParam(param.MSB, param.LSB);
        }

        /// <summary>
        /// Send Mute Command.
        /// </summary>
        /// <param name="srcID">Source to Mute.</param>
        /// <param name="muted"><see langword="true"/> to mute channel.</param>
        public void SetMute(string srcID, bool muted)
        {
            var cmd = SQProtocol.GenerateCommand_MuteSrc(_midiCTRLChannel, srcID, muted);
#if DEBUG
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
#endif
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
#if DEBUG
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
#endif
            _midiOutput?.SendBuffer(cmd);
        }

        public void GetParam(byte paramMSB, byte paramLSB)
        {
            var cmd = SQProtocol.GenerateCommand_GetParam(_midiCTRLChannel, paramMSB, paramLSB);
#if DEBUG
            Console.WriteLine($"Sending Raw MIDI: {BitConverter.ToString(cmd)}");
#endif
            _midiOutput?.SendBuffer(cmd);
        }

    }

    static class SQProtocol
    {

        internal class NRPNData
        {
            public string ID { get; set; }
            public short dVal { get; set; }
            public byte MSB { get; set; }
            public byte LSB { get; set; }
        }

        internal static Dictionary<string, NRPNData> MuteRouting { private set; get; }
        internal static Dictionary<string, NRPNData> LevelRouting { private set; get; }

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
                BuildByte(0xB, midiChannel),
                0x63,
                routing.MSB,

                BuildByte(0xB, midiChannel),
                0x62,
                routing.LSB,

                BuildByte(0xB, midiChannel),
                0x06,
                0x00,

                BuildByte(0xB, midiChannel),
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
                BuildByte(0xB, midiChannel),
                0x63,
                routing.MSB,

                BuildByte(0xB, midiChannel),
                0x62,
                routing.LSB,

                BuildByte(0xB, midiChannel),
                0x06,
                vc,

                BuildByte(0xB, midiChannel),
                0x26,
                vf
            };
        }

        internal static byte[] GenerateCommand_GetParam(byte midiChannel, byte msb, byte lsb)
        {
            return new byte[]
            {
                BuildByte(0xB, midiChannel),
                0x63,
                msb,

                BuildByte(0xB, midiChannel),
                0x62,
                lsb,

                BuildByte(0xB, midiChannel),
                0x60,
                0x7f,
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
                BuildByte(0xB, midiChannel),
                0x00,
                bank,

                0xC0,
                program
            };
        }



    }


}
