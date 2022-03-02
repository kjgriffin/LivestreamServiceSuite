using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xenon.SlideAssembly
{
    public class SlideLine
    {
        public List<SlideLineContent> Content { get; set; } = new List<SlideLineContent>();

        internal int Hash()
        {
            // borrowing the implementation from: https://thomaslevesque.com/2020/05/15/things-every-csharp-developer-should-know-1-hash-codes/
            unchecked // Allow arithmetic overflow, numbers will just "wrap around"
            {
                int hashcode = 1430287;
                foreach (var slc in Content)
                {
                    hashcode = hashcode * 7302013 ^ slc.Hash();
                }

                return hashcode;
            }

        }
    }
}
