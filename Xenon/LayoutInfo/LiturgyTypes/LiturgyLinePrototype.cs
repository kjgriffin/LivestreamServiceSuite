namespace Xenon.LayoutInfo.LiturgyTypes
{
    /// <summary>
    /// Used to define how (inside a <see cref="Xenon.LayoutInfo.BaseTypes.TextboxLayout"/>)
    /// a line of liturgy will appear. eg. speaker text etc.
    /// </summary>
    class LiturgyLinePrototype
    {
        public LWJFont FontOverride { get; set; }
        public LWJColor ColorOverride { get; set; }
        public bool ShowSpeaker { get; set; }
        public LWJFont SpeakerFont { get; set; }
        public LWJColor SpeakerColor { get; set; }
        public int SpeakerMargin { get; set; }
    }
}
