using System;

namespace LSBgenerator
{
    [Serializable]
    public class LiturgyLineState
    {
        public bool SpeakerWrap { get; set; } = false;
        public Speaker LastSpeaker { get; set; } = Speaker.None;
    }
}
