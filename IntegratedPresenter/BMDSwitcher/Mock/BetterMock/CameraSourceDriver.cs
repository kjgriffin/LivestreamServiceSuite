using CCUI_UI;

using CommonGraphics;

using IntegratedPresenter.Main;

using SharedPresentationAPI.Presentation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace IntegratedPresenter.BMDSwitcher.Mock
{


    class LiveCameraState
    {
        internal string Thumbnail { get; set; } = "";

        internal bool Moving { get; set; } = false;
        internal bool ReqMove { get; set; } = false;
        internal int MRunMS { get; set; } = 0;

        internal bool Zooming { get; set; } = false;
        internal bool ReqZoom { get; set; } = false;

        /// <summary>
        /// -1 WIDE, 1 TELE, 0 none
        /// </summary>
        internal int ZDir { get; set; } = 0;
        internal int ZSetupMS { get; set; } = 0;
        internal int ZRunMS { get; set; } = 0;
    }


    class CameraSourceDriver : ICameraSourceProvider, IManualMoveCameraDriver
    {

        const string DEFAULT_SOURCE = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png";
        Dictionary<int, string> m_defaultSourceMap = new Dictionary<int, string>()
        {
            [0] = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png", // always black
            [1] = "pack://application:,,,/BMDSwitcher/Mock/Images/backcam.PNG",
            [2] = "pack://application:,,,/BMDSwitcher/Mock/Images/powerpoint.png",
            [3] = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png",
            [4] = "pack://application:,,,/BMDSwitcher/Mock/Images/black.png",
            [5] = "pack://application:,,,/BMDSwitcher/Mock/Images/organshot1.PNG",
            [6] = "pack://application:,,,/BMDSwitcher/Mock/Images/rightshot.PNG",
            [7] = "pack://application:,,,/BMDSwitcher/Mock/Images/centershot.PNG",
            [8] = "pack://application:,,,/BMDSwitcher/Mock/Images/leftshot1.PNG",
            [(int)BMDSwitcherVideoSources.ColorBars] = "pack://application:,,,/BMDSwitcher/Mock/Images/cbars.png",
        };

        Dictionary<int, LiveCameraState> m_liveCameras = new Dictionary<int, LiveCameraState>()
        {
            [8] = new LiveCameraState(),
            [7] = new LiveCameraState(),
            [6] = new LiveCameraState(),
        };

        List<BitmapImage> _customThumbnails = new List<BitmapImage>();


        public int SlideID { get; set; } = 4;
        public int AKeyID { get; set; } = 3;

        ISlide m_slideSource;
        public void UpdateSlideSource(ISlide slide)
        {
            m_slideSource = slide;
        }

        public bool TryGetSourceImage(int PhysicalInputID, out BitmapImage source)
        {
            source = null;

            if (PhysicalInputID == SlideID)
            {
                return m_slideSource?.TryGetPrimaryImage(out source) ?? false;
            }
            else if (PhysicalInputID == AKeyID)
            {
                return m_slideSource?.TryGetKeyImage(out source) ?? false;
            }
            else if (m_liveCameras.TryGetValue(PhysicalInputID, out var cam))
            {
                /*
                if (File.Exists(cam.Thumbnail))
                {
                    source = new BitmapImage(new Uri(cam.Thumbnail));
                    return true;
                }
                */
                if (!string.IsNullOrWhiteSpace(cam.Thumbnail))
                {
                    try
                    {
                        source = CommonGraphicsHelpers.ToBitmapImage(cam.Thumbnail);
                        return true;
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (m_defaultSourceMap.TryGetValue(PhysicalInputID, out string spath))
                {
                    source = new BitmapImage(new Uri(spath));
                    return true;
                }


            }
            else if (m_defaultSourceMap.TryGetValue(PhysicalInputID, out string spath))
            {
                source = new BitmapImage(new Uri(spath));
                return true;
            }

            return false;
        }

        public bool TryGetSourceVideo(int PhysicalInputID, out string source)
        {
            source = null;

            if (PhysicalInputID == SlideID)
            {
                return m_slideSource?.TryGetPrimaryVideoPath(out source) ?? false;
            }
            else if (PhysicalInputID == AKeyID)
            {
                return m_slideSource?.TryGetKeyVideoPath(out source) ?? false;
            }

            return false;
        }

        bool ICameraSourceProvider.TryGetLiveCamState(int PhysicalInputID, out LiveCameraState camState)
        {
            camState = null;
            return m_liveCameras?.TryGetValue(PhysicalInputID, out camState) ?? false;
        }

        internal void UpdateLiveCamera(int sid, CameraUpdateEventArgs e)
        {
            int ZDirTranslation(CameraZoomEventArgs.ZDir zoomDirection)
            {
                switch (zoomDirection)
                {
                    case CameraZoomEventArgs.ZDir.WIDE:
                        return -1;
                    case CameraZoomEventArgs.ZDir.TELE:
                        return 1;
                    default:
                        return 0;
                }
            }

            var zoomargs = e as CameraZoomEventArgs;
            if (zoomargs != null)
            {
                if (zoomargs.CamState == CameraMoveState.Arrived)
                {
                    // zoom animation is completed...

                    if (m_liveCameras.TryGetValue(sid, out var live))
                    {
                        live.ReqZoom = false;
                        live.Zooming = false;
                        live.ZDir = 0;
                        live.ZSetupMS = 0;
                        live.ZRunMS = 0;
                    }
                }
                else if (zoomargs.CamState == CameraMoveState.Zomming)
                {
                    // figure out how to request camera begins zoom animation

                    if (m_liveCameras.TryGetValue(sid, out var live))
                    {
                        live.ReqZoom = true;
                        live.Zooming = true;
                        live.ZDir = ZDirTranslation(zoomargs.ZoomDirection);
                        live.ZSetupMS = zoomargs.ZSetupDur;
                        live.ZRunMS = zoomargs.ZDriveDur;
                    }
                }
            }
            var driveargs = e as CameraDriveEventArgs;
            if (driveargs != null)
            {
                if (driveargs.CamState == CameraMoveState.Arrived)
                {
                    if (m_liveCameras.TryGetValue(sid, out var live))
                    {
                        live.ReqMove = false;
                        live.Moving = false;
                        live.MRunMS = 0;
                        live.Thumbnail = driveargs.Thumbnail; // todo? is this necessary
                    }
                }
                else if (driveargs.CamState == CameraMoveState.Moving)
                {
                    if (m_liveCameras.TryGetValue(sid, out var live))
                    {
                        live.ReqMove = true;
                        live.Moving = true;
                        live.MRunMS = driveargs.RunTimeMS;
                        live.Thumbnail = driveargs.Thumbnail;
                    }
                }
            }

        }

        void IManualMoveCameraDriver.ResetCamerasToDefaults()
        {
            foreach (var lcam in m_liveCameras.Keys)
            {
                m_liveCameras[lcam] = new LiveCameraState();
            }
        }

        public void ManualMoveCamera(int camPhyID, string base64png)
        {
            if (m_liveCameras.TryGetValue(camPhyID, out var live))
            {

                live.Thumbnail = base64png;
            }
            else
            {
                // make it live now???
                m_liveCameras[camPhyID] = new LiveCameraState()
                {
                    Thumbnail = base64png
                };
            }
        }

        public List<BitmapImage> GetCamChoices()
        {
            // pick up all defaults
            List<BitmapImage> choices = new List<BitmapImage>();

            foreach (var img in m_defaultSourceMap.GroupBy(x => x.Value).Select(x => x.First()))
            {
                choices.Add(new BitmapImage(new Uri(img.Value)));
            }

            choices.AddRange(_customThumbnails);

            return choices;
        }

        void IManualMoveCameraDriver.AddLocalThumbnails(List<BitmapImage> thumbnails)
        {
            _customThumbnails.AddRange(thumbnails);
        }
    }
}
