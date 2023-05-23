using System;
using System.Collections.Generic;

namespace Xenon.LayoutEngine
{
    struct PlainTextLayoutLine
    {
        public List<string> words;
        public float width;
        public float height;
        public bool startofline;
        public bool fulltextonline;

        public PlainTextLayoutLine(List<string> words, float width, float height, bool startofline, bool fulltextonline)
        {
            this.words = words;
            this.width = width;
            this.height = height;
            this.startofline = startofline;
            this.fulltextonline = fulltextonline;
        }

        public override bool Equals(object obj)
        {
            return obj is LiturgyLayoutLine other &&
                   EqualityComparer<List<string>>.Default.Equals(words, other.words) &&
                   width == other.width &&
                   height == other.height &&
                   startofline == other.startofline &&
                   fulltextonline == other.fulltextonline;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(words, width, height, startofline, fulltextonline);
        }

        public void Deconstruct(out List<string> words, out float width, out float height, out bool startofline, out bool fulltextonline)
        {
            words = this.words;
            width = this.width;
            height = this.height;
            startofline = this.startofline;
            fulltextonline = this.fulltextonline;
        }

        public static implicit operator (List<string> words, float width, float height, bool startofline, bool fulltextonline)(PlainTextLayoutLine value)
        {
            return (value.words, value.width, value.height, value.startofline, value.fulltextonline);
        }

        public static implicit operator PlainTextLayoutLine((List<string> words, float width, float height, bool startofline, bool fulltextonline) value)
        {
            return new PlainTextLayoutLine(value.words, value.width, value.height, value.startofline, value.fulltextonline);
        }

    }
}
