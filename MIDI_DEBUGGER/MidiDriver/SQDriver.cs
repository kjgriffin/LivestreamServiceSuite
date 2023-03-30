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
            _midiInput = new MidiIn(MIDIdeviceInID);
            _midiOutput = new MidiOut(MIDIdeviceOutID);

            Console.WriteLine($"SQDriver MIDI driver set to use input {MidiIn.DeviceInfo(MIDIdeviceInID).ProductName}");
            Console.WriteLine($"SQDriver MIDI driver set to use output {MidiOut.DeviceInfo(MIDIdeviceOutID).ProductName}");
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
            throw new NotImplementedException();
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

        private static byte BuildByte(byte MSB, byte LSB)
        {
            byte v1 = (byte)(MSB << 4);
            byte v2 = (byte)(MSB & 0b00001111);
            return (byte)(v1 | v2);
        }

        static byte[] GenerateCommand_MuteSrc(byte midiChannel, ushort srcID, bool mute)
        {
            byte pmsb;
            byte plsb;
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
                BuildByte(0xB, (byte)(mute ? 0x1 : 0x0)),
            };
        }



    }


}
