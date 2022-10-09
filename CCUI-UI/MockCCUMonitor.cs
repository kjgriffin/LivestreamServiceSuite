using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using static CCUI_UI.CCPUConfig_Extended;

namespace CCUI_UI
{

    public class MockPreset
    {
        public string AssociatedCamera { get; set; }
        public string CameraName { get; set; }
        public string PresetName { get; set; }
        public string ZoomName { get; set; }

        public int RunTimeMS { get; set; }
        public string ThumbnailPath { get; set; }
        public string ThumbnailFile { get; set; }

    }

    [Flags]
    public enum CameraMoveState
    {
        Arrived = 0,
        Moving = 1,
        Zomming = 2,
    }

    public abstract class CameraUpdateEventArgs : EventArgs
    {
        protected CameraUpdateEventArgs(string cameraName, CameraMoveState camState)
        {
            CameraName = cameraName;
            CamState = camState;
        }

        public string CameraName { get; private set; }
        public CameraMoveState CamState { get; private set; }
    }

    public class CameraDriveEventArgs : CameraUpdateEventArgs
    {
        public CameraDriveEventArgs(string cameraName, CameraMoveState camState, string thumbnail, int runTimeMs) : base(cameraName, camState)
        {
            Thumbnail = thumbnail;
            RunTimeMS = runTimeMs;
        }

        public string Thumbnail { get; private set; }
        public int RunTimeMS { get; private set; }
    }

    public class CameraZoomEventArgs : CameraUpdateEventArgs
    {
        public CameraZoomEventArgs(string cameraName, CameraMoveState camState, ZDir zoomDirection, int zSetupDur, int zDriveDur) : base(cameraName, camState)
        {
            ZoomDirection = zoomDirection;
            ZSetupDur = zSetupDur;
            ZDriveDur = zDriveDur;
        }

        public enum ZDir
        {
            WIDE,
            TELE
        }
        public ZDir ZoomDirection { get; private set; }

        public int ZSetupDur { get; private set; }
        public int ZDriveDur { get; private set; }
    }




    public class MockCCUMonitor : ICCPUPresetMonitor
    {
        public event CCUEvent OnCommandUpdate;
        public event EventHandler<CameraUpdateEventArgs> OnCameraMoved;


        MockCameraDriver UiWindow;


        CCPUConfig_Extended m_presetConfig;


        public void ChirpZoom(string camname, int direction, int duration)
        {
            throw new NotImplementedException();
        }

        public CCPUConfig ExportStateToConfig()
        {
            return m_presetConfig;
        }

        public void FirePreset(string camname, string presetname, int speed)
        {
            throw new NotImplementedException();
        }

        public (Guid move, Guid zoom) FirePresetWithZoom_Tracked(string camname, string presetname, int speed, int zoomdir, int msdur)
        {
            throw new NotImplementedException();
        }

        public Guid FirePreset_Tracked(string camname, string presetname, int speed)
        {
            Guid reqid = Guid.NewGuid();
            //var pst = _knownPresets.FirstOrDefault(x => x.AssociatedCamera == camname && x.PresetName == presetname);

            PresetMockInfo pst = null;
            m_presetConfig.CameraAssociations.TryGetValue(camname, out var associatedCam);

            if (!(m_presetConfig.KeyedPresets.TryGetValue(camname, out var camPresets) && camPresets.TryGetValue(presetname, out _)))
            {
                return Guid.Empty;
            }
            if (!(m_presetConfig.MockPresetInfo.TryGetValue(camname, out var mockPreset) && mockPreset.TryGetValue(presetname, out pst)))
            {
                return Guid.Empty;
            }

            if (pst != null)
            {
                Task.Run(async () =>
                {
                    OnCommandUpdate?.Invoke(camname, "STARTED", "DRIVE.ABSPOS", presetname, reqid.ToString());
                    OnCameraMoved?.Invoke(this, new CameraDriveEventArgs(associatedCam ?? camname, CameraMoveState.Moving, pst.Thumbnail, pst.RuntimeMS));
                    await Task.Delay(pst.RuntimeMS);
                    OnCommandUpdate?.Invoke(camname, "COMPLETED", "DRIVE.ABSPOS", presetname, "after 1 tries", reqid.ToString());
                    OnCameraMoved?.Invoke(this, new CameraDriveEventArgs(associatedCam ?? camname, CameraMoveState.Arrived, pst.Thumbnail, pst.RuntimeMS));
                });
            }
            return reqid;
        }

        public void FireZoomLevel(string camname, string presetname)
        {
            throw new NotImplementedException();
        }

        public Guid FireZoomLevel_Tracked(string camname, string presetname)
        {
            Guid reqid = Guid.NewGuid();

            m_presetConfig.CameraAssociations.TryGetValue(camname, out var associatedCam);

            if (!(m_presetConfig.KeyedZooms.TryGetValue(camname, out var camPresets) && camPresets.TryGetValue(presetname, out var pst)))
            {
                return Guid.Empty;
            }


            CameraZoomEventArgs.ZDir zdir = pst.Mode == "WIDE" ? CameraZoomEventArgs.ZDir.WIDE : CameraZoomEventArgs.ZDir.TELE;
            const int ZSETUP_MS = 3800;

            if (pst != null)
            {
                Task.Run(async () =>
                {
                    OnCommandUpdate?.Invoke(camname, "STARTED", "CHIRP", "duration ms", pst.Mode, reqid.ToString());

                    OnCameraMoved?.Invoke(this, new CameraZoomEventArgs(associatedCam ?? camname, CameraMoveState.Zomming, zdir, ZSETUP_MS, pst.ZoomMS));
                    await Task.Delay(pst.ZoomMS + ZSETUP_MS);
                    OnCommandUpdate?.Invoke(camname, "COMPLETED", "CHIRP", pst.Mode, "after 1 tries", reqid.ToString());
                    OnCameraMoved?.Invoke(this, new CameraZoomEventArgs(associatedCam ?? camname, CameraMoveState.Arrived, zdir, ZSETUP_MS, pst.ZoomMS));
                });
            }
            return reqid;
        }

        public Guid FireZoom_Tracked(string camname, int zoomdir, int msdur)
        {
            throw new NotImplementedException();
        }

        public void LoadConfig(CCPUConfig? cfg)
        {
            var ecfg = cfg as CCPUConfig_Extended;
            if (ecfg == null)
            {
                throw new Exception("FIX THIS!!!");
            }
            m_presetConfig = ecfg;
        }

        public void RemovePreset(string camname, string presetname)
        {
            throw new NotImplementedException();
        }

        public void RunZoom(string camname, int direction)
        {
            throw new NotImplementedException();
        }

        public void SavePreset(string camname, string presetname)
        {
            throw new NotImplementedException();
        }

        public void SaveZoomLevel(string camname, string presetname, int zlevel, string mode)
        {
            throw new NotImplementedException();
        }

        public void ShowUI()
        {
            if (UiWindow == null)
            {
                UiWindow = new MockCameraDriver(this);
            }
            UiWindow.Show();
        }

        public void Shutdown()
        {
            UiWindow.Close();
        }

        public void StartCamera(string name, IPEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        public void StopCamera(string name)
        {
            throw new NotImplementedException();
        }
    }
}
