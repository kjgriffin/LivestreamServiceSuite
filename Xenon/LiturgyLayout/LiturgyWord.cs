using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xenon.LiturgyLayout
{
    class LiturgyWord
    {
        public string Value { get; set; }
        public bool IsBold { get; set; }
        public bool IsLSBSymbol { get; set; }
        public bool IsSized { get; set; }
        public SizeF Size { get; set; }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
