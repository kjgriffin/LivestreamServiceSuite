using DVIPProtocol.Protocol.Lib.Command.PTDrive;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using log4net;

using System.Net;

namespace CameraDriver
{
    public delegate void CameraPresetSaved(string cname, string pname);
    public delegate void CameraZoomSaved(string cname, string pname);
    public interface ISimpleCamServer
    {
        event CameraPresetSaved OnPresetSavedSuccess;

        void Start();
        void Shutdown();

        void StartCamClient(string cnameID, IPEndPoint endpoint);
        void StopCamClient(string cnameID);
        void StopAllCamClients();

        void Cam_SaveCurentPosition(string cnameID, string presetName);
        void Cam_SaveRawPosition(string cnameID, string presetName, RESP_PanTilt_Position value);
        void Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10);

        void RemovePreset(string cnameID, string presetName);

        void ClearAllPresets();

        List<(string camName, IPEndPoint endpoint)> GetClientConfig();
        List<string> GetClientNamesWithPresets();
        Dictionary<string, RESP_PanTilt_Position> GetKnownPresetsForClient(string cnameID);


        public static ISimpleCamServer Instantiate(ILog log)
        {
            return new SimpleCamServer(log: log);
        }

        public static ISimpleCamServer Instantiate_Mock(ILog log)
        {
            return new SimpleCamServer(log, true);
        }

        public static IRobustCamServer InstantiateRobust(ILog log)
        {
            return new RobustCamServer(log);
        }

    }

    public delegate void RobustReport(string cnameID, params string[] args);

    public interface IRobustCamServer : ISimpleCamServer, IMotionCameraDriver
    {
        event RobustReport OnWorkCompleted;
        event RobustReport OnWorkFailed;
        event RobustReport OnWorkStarted;

        public event CameraZoomSaved OnZoomSavedSuccess;

        Guid Cam_RunZoomChrip(string cnameID, int direction, int duration, Guid _rid = default(Guid));
        Guid Cam_RunZoomChrip_RELATIVE(string cnameID, int direction, int duration, Guid _rid = default(Guid));
        void Cam_RunZoomProgram(string cnameID, int direction);
        new Guid Cam_RecallPresetPosition(string cnameID, string presetName, byte speed = 0x10);
        void Cam_SaveZoomPresetProgram(string cnameID, string presetName, int zlevel, string mode);
        Guid Cam_RecallZoomPresetPosition(string cnameID, string presetName);
        Dictionary<string, ZoomProgram> GetKnownZoomPresetsForClient(string cnameID);
    }

    public interface IMotionCameraDriver
    {
        Guid Cam_RunDriveProgram(string cnameID, PanTiltDirection dir, byte speed, uint msDriveTime);
    }


}
