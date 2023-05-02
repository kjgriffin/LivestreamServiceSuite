using System.Collections.Generic;

namespace Xenon.SlideAssembly
{
    public class SlideLineContent
    {
        public string Data { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        internal int Hash()
        {
            unchecked
            {
                return Data.GetHashCode() ^ Attributes.GetHashCode();
            }
        }
    }
}
