using CameraDriver;

using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System.Collections.Generic;

namespace CCUI_UI
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


}
