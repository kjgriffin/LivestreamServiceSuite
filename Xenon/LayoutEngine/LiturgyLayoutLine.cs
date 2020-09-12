using System;
using System.Collections.Generic;

namespace Xenon.LayoutEngine
{
    struct LiturgyLayoutLine
    {
        public string speaker;
        public List<string> words;
        public float width;
        public float height;
        public bool startofline;
        public bool fulltextonline;

        public LiturgyLayoutLine(string speaker, List<string> words, float width, float height, bool startofline, bool fulltextonline)
        {
            this.speaker = speaker;
            this.words = words;
            this.width = width;
            this.height = height;
            this.startofline = startofline;
            this.fulltextonline = fulltextonline;
        }

        public override bool Equals(object obj)
        {
            return obj is LiturgyLayoutLine other &&
                   speaker == other.speaker &&
                   EqualityComparer<List<string>>.Default.Equals(words, other.words) &&
                   width == other.width &&
                   height == other.height &&
                   startofline == other.startofline &&
                   fulltextonline == other.fulltextonline;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(speaker, words, width, height, startofline, fulltextonline);
        }

        public void Deconstruct(out string speaker, out List<string> words, out float width, out float height, out bool startofline, out bool fulltextonline)
        {
            speaker = this.speaker;
            words = this.words;
            width = this.width;
            height = this.height;
            startofline = this.startofline;
            fulltextonline = this.fulltextonline;
        }

        public static implicit operator (string speaker, List<string> words, float width, float height, bool startofline, bool fulltextonline)(LiturgyLayoutLine value)
        {
            return (value.speaker, value.words, value.width, value.height, value.startofline, value.fulltextonline);
        }

        public static implicit operator LiturgyLayoutLine((string speaker, List<string> words, float width, float height, bool startofline, bool fulltextonline) value)
        {
            return new LiturgyLayoutLine(value.speaker, value.words, value.width, value.height, value.startofline, value.fulltextonline);
        }
    }
}
