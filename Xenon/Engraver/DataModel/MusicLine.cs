using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Engraver.DataModel
{

    internal class MusicLine
    {
        public MusicLineMetadata Metadata { get; set; } = new MusicLineMetadata();
        public string SBegin { get; set; } = "";
        public string SEnd { get; set; } = "";
        public MusicSequence Notes { get; set; } = new MusicSequence();

    }

}
