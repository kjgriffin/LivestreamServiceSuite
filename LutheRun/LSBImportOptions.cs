using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    public class LSBImportOptions
    {
        public bool InferPostset { get; set; } = true;
        public bool UseUpNextForHymns { get; set; } = true;
        public bool OnlyKnownCaptions { get; set; } = true;
    }
}
