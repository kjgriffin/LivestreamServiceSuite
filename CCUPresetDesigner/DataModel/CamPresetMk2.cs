using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CCUPresetDesigner.DataModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ZoomDir : ushort
    {
        STOP = 00,
        TELE = 02,
        WIDE = 03,
    }


    public class CamPresetMk2
    {
        public string Id { get; set; }
        public string DefaultName { get; set; }
        public string Description { get; set; }
        public string Camera { get; set; }
        public int Pan { get; set; }
        public int Tilt { get; set; }
        public ZoomDir ZoomDir { get; set; }
        public int ZoomMs { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string ImgUrl { get; set; }
    }
}
