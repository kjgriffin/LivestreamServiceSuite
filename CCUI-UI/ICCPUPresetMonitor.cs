using CCU.Config;

using System.Net;

namespace CCUI_UI
{
    public interface ICCPUPresetMonitor : ICCPUPresetMonitor_Executor
    {
        event CCUEvent OnCommandUpdate;

        void ChirpZoom(string camname, int direction, int duration);
        void ChirpZoom_RELATIVE(string camname, int direction, int duration);
        CCPUConfig ExportStateToConfig();
        void FireZoomLevel(string camname, string presetname);
        void LoadConfig(CCPUConfig? cfg);
        void RemovePreset(string camname, string presetname);
        void RunZoom(string camname, int direction);
        void SavePreset(string camname, string presetname);
        void SaveZoomLevel(string camname, string presetname, int zlevel, string mode);
        void ShowUI();
        void Shutdown();
        void StartCamera(string name, IPEndPoint endpoint);
        void StopCamera(string name);
    }
}