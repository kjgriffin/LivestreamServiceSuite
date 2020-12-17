using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Integrated_Presenter.BMDSwitcher.Config
{
    public class BMDSwitcherConfigSettings
    {

        public List<ButtonSourceMapping> Routing { get; set; } = new List<ButtonSourceMapping>();

        public BMDDSKSettings DownstreamKey1Config { get; set; } = new BMDDSKSettings();

        public BMDDSKSettings DownstreamKey2Config { get; set; } = new BMDDSKSettings();

        public BMDMultiviewerSettings MultiviewerConfig { get; set; } = new BMDMultiviewerSettings();

        public BMDMixEffectSettings MixEffectSettings { get; set; } = new BMDMixEffectSettings();

        public BMDUSKSettings PIPSettings { get; set; } = new BMDUSKSettings();

        public BMDSwitcherVideoSettings VideoSettings { get; set; } = new BMDSwitcherVideoSettings();

        public BMDSwitcherAudioSettings AudioSettings { get; set; } = new BMDSwitcherAudioSettings();



        public static BMDSwitcherConfigSettings Load(string filename)
        {
            var src = "";
            using (var sr = new StreamReader(filename))
            {
                src = sr.ReadToEnd();
            }
            return JsonSerializer.Deserialize<BMDSwitcherConfigSettings>(src);
        }

        public void Save(string filename)
        {
            var obj = JsonSerializer.Serialize(this);
            using (var sw = new StreamWriter(filename))
            {
                sw.Write(obj);
            }
        }


    }
}
