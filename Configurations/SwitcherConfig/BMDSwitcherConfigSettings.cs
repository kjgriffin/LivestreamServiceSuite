using Configurations.SwitcherConfig;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IntegratedPresenter.BMDSwitcher.Config
{
    public class BMDSwitcherConfigSettings
    {

        public List<ButtonSourceMapping> Routing { get; set; } = new List<ButtonSourceMapping>();

        public int DefaultAuxSource { get; set; }

        public BMDDSKSettings DownstreamKey1Config { get; set; } = new BMDDSKSettings();

        public BMDDSKSettings DownstreamKey2Config { get; set; } = new BMDDSKSettings();

        public BMDMultiviewerSettings MultiviewerConfig { get; set; } = new BMDMultiviewerSettings();

        public BMDMixEffectSettings MixEffectSettings { get; set; } = new BMDMixEffectSettings();

        public BMDUSKSettings USKSettings { get; set; } = new BMDUSKSettings();

        public BMDSwitcherVideoSettings VideoSettings { get; set; } = new BMDSwitcherVideoSettings();

        public BMDSwitcherAudioSettings AudioSettings { get; set; } = new BMDSwitcherAudioSettings();

        public PrerollSettings PrerollSettings { get; set; } = new PrerollSettings();

        public PIPPresetSettings PIPPresets { get; set; } = new PIPPresetSettings();

        public static BMDSwitcherConfigSettings Load(string filename)
        {
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    return LoadJson(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return Configurations.SwitcherConfig.DefaultConfig.GetDefaultConfig();
            }
        }

        public static BMDSwitcherConfigSettings LoadJson(string json)
        {
            var cfg = JsonSerializer.Deserialize<BMDSwitcherConfigSettings>(json);

            if (cfg.PIPPresets == null || !cfg.PIPPresets.IsValid())
            {
                cfg.PIPPresets = PIPPresetSettings.Default();
            }
            return cfg;
        }

        public void Save(string filename)
        {
            try
            {
                var obj = JsonSerializer.Serialize(this);
                using (var sw = new StreamWriter(filename))
                {
                    sw.Write(obj);
                }
            }
            catch (Exception)
            {
            }
        }


    }
}
