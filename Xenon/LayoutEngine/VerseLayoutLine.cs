﻿using System.Collections.Generic;

namespace Xenon.LayoutEngine
{
    class VerseLayoutLine
    {
        public List<string> Words { get; set; } = new List<string>();
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
