using System;

namespace LSBgenerator
{
    [Serializable]
    public class TypesetCommand : ITypesettable
    {
        public Command Command { get; set; }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            if (Command == Command.NewSlide)
            {
                // force new slide
                r.Slides.Add(r.FinalizeSlide(slide));
                return new RenderSlide() { Order = slide.Order + 1 };
            }
            if (Command == Command.WrapSpeakerText)
            {
                r.LLState.SpeakerWrap = true;
            }
            return slide;
        }
    }
}
