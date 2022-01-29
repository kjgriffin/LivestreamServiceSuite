﻿using Xenon.Layouts;

namespace Xenon.LayoutInfo.BaseTypes
{
    internal class LiturgyTextboxLayout : TextboxLayout
    {
        public int MaxSpeakers { get; set; }
        public bool EnforceCallResponse { get; set; }
        public bool ShowSpeaker { get; set; }
        public LWJFont SpeakerFont { get; set; }
        public LWJColor SpeakerColor { get; set; }
        public int SpeakerColumnWidth { get; set; }
        public bool VPaddingEnabled { get; set; }
        public float MinInterLineSpace { get; set; }
    }
}