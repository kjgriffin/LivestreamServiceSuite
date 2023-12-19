using System;

namespace CCUI_UI
{
    public interface ICCPUPresetMonitor_Executor
    {
        void FirePreset(string camname, string presetname, int speed);

        Guid FirePreset_Tracked(string camname, string presetname, int speed);
        Guid FireZoom_Tracked(string camname, int zoomdir, int msdur);
        (Guid move, Guid zoom) FirePresetWithZoom_Tracked(string camname, string presetname, int speed, int zoomdir, int msdur);
        Guid FireZoomLevel_Tracked(string camname, string presetname);
        void PanTiltDrive(string camname, int Xdir, int Ydir, int Xspeed, int Yspeed);
    }
}