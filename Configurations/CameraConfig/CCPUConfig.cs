using CameraDriver;

using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CCU.Config
{
    public class CCPUConfig
    {

        public class ClientConfig
        {
            public string IPAddress { get; set; } = "";
            public int Port { get; set; } = 0;
            public string Name { get; set; } = "";
        }


        public Dictionary<string, Dictionary<string, RESP_PanTilt_Position>> KeyedPresets { get; set; } = new Dictionary<string, Dictionary<string, RESP_PanTilt_Position>>();
        public Dictionary<string, Dictionary<string, ZoomProgram>> KeyedZooms { get; set; } = new Dictionary<string, Dictionary<string, ZoomProgram>>();
        public List<ClientConfig> Clients { get; set; } = new List<ClientConfig>();




    }

    public class CCPUConfig_Extended : CCPUConfig
    {

        public class PresetMockInfo
        {
            public string Thumbnail { get; set; }
            public int RuntimeMS { get; set; }
        }



        public Dictionary<string, Dictionary<string, PresetMockInfo>> MockPresetInfo { get; set; } = new Dictionary<string, Dictionary<string, PresetMockInfo>>();
        public Dictionary<string, string> CameraAssociations { get; set; } = new Dictionary<string, string>();


        public List<CombinedPresetInfo> CompileIntoPresetInfo()
        {
            List<CombinedPresetInfo> result = new List<CombinedPresetInfo>();


            foreach (var camgroup in KeyedPresets)
            {
                foreach (var preset in camgroup.Value)
                {
                    CombinedPresetInfo info = new CombinedPresetInfo
                    {
                        CamName = camgroup.Key,
                        PresetPosName = preset.Key,
                        Pan = preset.Value.Pan,
                        Tilt = preset.Value.Tilt,
                        Valid = preset.Value.Valid,
                    };
                    if (KeyedZooms.TryGetValue(camgroup.Key, out var zpsts))
                    {
                        if (zpsts.TryGetValue(preset.Key, out var zprog))
                        {
                            info.ZoomPresetName = preset.Key;
                            info.ZoomMode = zprog.Mode;
                            info.ZoomMS = zprog.ZoomMS;
                        }
                        else
                        {
                            var ematch = zpsts.FirstOrDefault(x => x.Key.EndsWith(preset.Key, false, CultureInfo.InvariantCulture)).Value;
                            if (ematch != null)
                            {
                                info.ZoomPresetName = preset.Key;
                                info.ZoomMode = ematch.Mode;
                                info.ZoomMS = ematch.ZoomMS;
                            }
                        }
                    }
                    if (MockPresetInfo.TryGetValue(camgroup.Key, out var mocks))
                    {
                        if (mocks.TryGetValue(preset.Key, out var minfo))
                        {
                            info.MoveMS = minfo.RuntimeMS;
                            info.Thumbnail = minfo.Thumbnail;
                        }
                    }
                    result.Add(info);
                }
            }

            return result;
        }
    }

    public class CombinedPresetInfo
    {
        public string CamName { get; set; }
        public string PresetPosName { get; set; }
        public long Pan { get; set; }
        public int Tilt { get; set; }
        public bool Valid { get; set; }
        public int MoveMS { get; set; }
        public string ZoomPresetName { get; set; }
        public string ZoomMode { get; set; }
        public int ZoomMS { get; set; }

        public string Thumbnail { get; set; }

        public static CombinedPresetInfo DefaultTemplate()
        {
            return new CombinedPresetInfo
            {
                CamName = "UNNAMED CAMERA",
                PresetPosName = "NEW PRESET",
                Pan = 1040000, // maybe centered? for some??
                Tilt = 64500, // this seems to be about level
                Valid = true, // or-else a server won't load the preset
                MoveMS = 10000,
                ZoomPresetName = "NEW PRESET",
                ZoomMode = "WIDE",
                ZoomMS = 1000,
                Thumbnail = "",
            };
        }
    }


}
