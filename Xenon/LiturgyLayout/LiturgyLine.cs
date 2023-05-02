using System.Collections.Generic;
using System.Linq;

namespace Xenon.LiturgyLayout
{
    class LiturgyLine
    {
        public string SpeakerText { get; set; }
        public List<LiturgyWord> Words { get; set; } = new List<LiturgyWord>();

        public override string ToString()
        {
            return $"[{SpeakerText}] {string.Join("", Words.Select(s => s.Value))}";
        }
    }
}
