using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

    public enum CameraMoveState
    {
        Moving,
        Arrived,
    }

    public class CameraMotionEventArgs : EventArgs
    {
        public string CameraName { get; set; }
        public string Thumbnail { get; set; }
        public CameraMoveState CamState { get; set; }
    }



    public class MockCCUMonitor : ICCPUPresetMonitor
    {
        public event CCUEvent OnCommandUpdate;
        public event EventHandler<CameraMotionEventArgs> OnCameraMoved;


        MockCameraDriver UiWindow;

        List<MockPreset> _knownPresets = new List<MockPreset>()
        {
            new MockPreset
            {
                CameraName = "center",
                AssociatedCamera = "center",
                PresetName = "FRONT",
                RunTimeMS = 5000,
                ThumbnailFile = "",
                ThumbnailPath = "",
                ZoomName = "zfront",
            },
        };




        public void ChirpZoom(string camname, int direction, int duration)
        {
            throw new NotImplementedException();
        }

        public CCPUConfig ExportStateToConfig()
        {
            throw new NotImplementedException();
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
            var pst = _knownPresets.FirstOrDefault(x => x.AssociatedCamera == camname && x.PresetName == presetname);
            if (pst != null)
            {
                Task.Run(async () =>
                {
                    OnCommandUpdate?.Invoke(camname, "STARTED", "DRIVE.ABSPOS", presetname, reqid.ToString());
                    OnCameraMoved?.Invoke(this, new CameraMotionEventArgs
                    {
                        CameraName = pst.AssociatedCamera,
                        CamState = CameraMoveState.Moving,
                        Thumbnail = Path.Combine(pst.ThumbnailPath, pst.ThumbnailFile), // perhaps we should have a moving icon? or just let a consumer ignore the thumbnail here
                        // or it could use it in some sort of blend???
                    });
                    await Task.Delay(pst.RunTimeMS);
                    OnCommandUpdate?.Invoke(camname, "COMPLETED", "DRIVE.ABSPOS", presetname, "after 1 tries", reqid.ToString());
                    OnCameraMoved?.Invoke(this, new CameraMotionEventArgs
                    {
                        CameraName = pst.AssociatedCamera,
                        CamState = CameraMoveState.Arrived,
                        Thumbnail = Path.Combine(pst.ThumbnailPath, pst.ThumbnailFile),
                    });
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
            var pst = _knownPresets.FirstOrDefault(x => x.AssociatedCamera == camname && x.ZoomName == presetname);
            if (pst != null)
            {
                Task.Run(async () =>
                {
                    OnCommandUpdate?.Invoke(camname, "STARTED", "CHIRP", "duration ms", "ZDIR-here", reqid.ToString());

                    OnCameraMoved?.Invoke(this, new CameraMotionEventArgs
                    {
                        CameraName = camname,
                        CamState = CameraMoveState.Moving,
                        Thumbnail = Path.Combine(pst.ThumbnailPath, pst.ThumbnailFile), // perhaps we should have a moving icon? or just let a consumer ignore the thumbnail here
                        // or it could use it in some sort of blend???
                    });
                    await Task.Delay(pst.RunTimeMS);
                    OnCommandUpdate?.Invoke(camname, "COMPLETED", "CHIRP", "ZDIR-here", "after 1 tries", reqid.ToString());
                    OnCameraMoved?.Invoke(this, new CameraMotionEventArgs
                    {
                        CameraName = camname,
                        CamState = CameraMoveState.Arrived,
                        Thumbnail = Path.Combine(pst.ThumbnailPath, pst.ThumbnailFile),
                    });
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
            throw new NotImplementedException();
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
                UiWindow = new MockCameraDriver();
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
